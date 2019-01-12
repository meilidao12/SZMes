using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using CommunicationServers.Sockets;
using Services;
using ProtocolFamily.Modbus;
using System.Net.Sockets;
using System.Windows.Threading;
using MesToPlc.Models;
using Services.DataBase;
using System.Diagnostics;
using MesToPlc;
using System.Data;
using System.Windows.Interop;

namespace MesToPlc.Pages
{
    

    /// <summary>
    /// OperatePage.xaml 的交互逻辑
    /// </summary>
    public partial class OperatePage : Page
    {
        MysqlHelper sql = new MysqlHelper();
        WorkHelper WorkHelper = new WorkHelper();
        IniHelper ini = new IniHelper(System.AppDomain.CurrentDomain.BaseDirectory + @"\Set.ini");
        CharacterConversion characterConversion;
        DispatcherTimer CollectTimer = new DispatcherTimer();
        JsonHelper jsonHelper = new JsonHelper();
        IntPtr txtBianHaoHwnd;
        IntPtr txtShoudongHwnd;
        bool blClear = false;
       public OperatePage()
        {
            InitializeComponent();
            this.Loaded += OperatePage_Loaded;
            Reset();
        }

        private void Reset()
        {
            ini.WriteIni(Set.Config, Set.Index, "");
            this.txtBianHao.Text = "";
            this.CollectTimer.Stop();
            this.txtWeldTime.Text = "";
            this.txtWenDu.Text = "";
            this.txtYaLi.Clear();
            this.txtDianYa.Clear();
            this.txtDianLiu.Clear();
            this.txtBianHaoShow.Clear();
            AddLog("重置成功");
        }

        private void OperatePage_Loaded(object sender, RoutedEventArgs e)
        {
            string infoFilePath = ini.ReadIni("Config", "JsonUrl");
            if (!System.IO.Directory.Exists(System.IO.Path.GetFullPath(infoFilePath)))
                System.IO.Directory.CreateDirectory(System.IO.Path.GetFullPath(infoFilePath));
            if (sql.Open())
            {
                AddLog("连接数据库成功");
                SimpleLogHelper.Instance.WriteLog(LogType.Info, "连接数据库成功");
            }
            else
            {
                AddLog("连接数据库失败");
                SimpleLogHelper.Instance.WriteLog(LogType.Info, "连接数据库失败");
            }
            this.CollectTimer.Interval = TimeSpan.FromSeconds(1);
            this.CollectTimer.Tick += CollectTimer_Tick;
        }

        private void CollectTimer_Tick(object sender, EventArgs e)
        {
            string commandtext = string.Format("Select * FROM welddata_spot ORDER BY `ID` DESC LIMIT 1");
            DataTable dt = sql.GetDataTable1(commandtext);
            if (dt != null)
            {
                int i = int.Parse(dt.Rows[0]["ID"].ToString());
                if(i>=int.Parse(ini.ReadIni(Set.Config,Set.NO)))
                {
                    this.txtDianLiu.Text = dt.Rows[0]["CURRENT1"].ToString();
                    this.txtDianYa.Text = dt.Rows[0]["VOLTAGE1"].ToString();
                    this.txtWenDu.Text = dt.Rows[0]["TEMPERATURE"].ToString();
                    this.txtYaLi.Text = dt.Rows[0]["PRESSURE"].ToString();
                    this.txtWeldTime.Text = dt.Rows[0]["WELDTIME1"].ToString();
                }
            }
        }

        private void AddLog(string log)
        {
            Dispatcher.BeginInvoke(new Action(() =>
            {
                log = DateTime.Now + ": " + log;
                this.lstInfoLog.Items.Add(log);
                SimpleLogHelper.Instance.WriteLog(LogType.Info, log);
                Decorator decorator = (Decorator)VisualTreeHelper.GetChild(lstInfoLog, 0);
                ScrollViewer scrollViewer = (ScrollViewer)decorator.Child;
                scrollViewer.ScrollToEnd();
                if (this.lstInfoLog.Items.Count >= 60)
                {
                    int ClearLstCount = lstInfoLog.Items.Count - 60;
                    this.lstInfoLog.Items.RemoveAt(ClearLstCount);
                }
            }));
        }

        /// <summary>
        /// 清除日志
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnClear_Click(object sender, RoutedEventArgs e)
        {
            this.lstInfoLog.Items.Clear();
        }

        private void btnAddBianHao_Click(object sender, RoutedEventArgs e)
        {
            this.txtBianHao.Text = this.txtBianHaoHandInput.Text;
            this.txtBianHaoHandInput.Text = "";
            //---
        }

        private void txtBianHao_TextChanged(object sender, TextChangedEventArgs e)
        {
            var d = e.OriginalSource as TextBox;
            if (d.Name == "txtBianHaoShow")
            {
                return;
            }
            if (this.txtBianHao.Text.Length <= 2) return;
            if (blClear)
            {
                blClear = false;
                return;
            }
                
            e.Handled = true;
            //结束老的
            FinishNow();
            //开始新的
            StartAnother();
            blClear = false;
        }

        /// <summary>
        ///  开始新的
        /// </summary>
        private void StartAnother()
        {
            try
            {
                string commandtext = string.Format("Select * FROM welddata_spot ORDER BY `ID` DESC LIMIT 1");
                DataTable dt = sql.GetDataTable1(commandtext);
                if (dt != null)
                {
                    ini.WriteIni(Set.Config, Set.Index, this.txtBianHao.Text);
                    int i = int.Parse(dt.Rows[0]["ID"].ToString()) + 1;
                    ini.WriteIni(Set.Config, Set.NO, i.ToString());
                }
                else
                {
                    ini.WriteIni(Set.Config, Set.Index, this.txtBianHao.Text);
                    ini.WriteIni(Set.Config, Set.NO, "1");
                }
            }
            catch(Exception ex)
            {
                SimpleLogHelper.Instance.WriteLog(LogType.Info, ex);
            }
            this.CollectTimer.Start();
            this.txtBianHaoShow.Text = this.txtBianHao.Text;
            this.blClear = true;
            this.txtBianHao.Clear();
        }

        /// <summary>
        /// 结束当前的
        /// </summary>
        private void FinishNow()
        {
            try
            {
                string index = ini.ReadIni(Set.Config, Set.Index);
                string id = ini.ReadIni(Set.Config, Set.NO);
                if (string.IsNullOrEmpty(index)) return;
                //读取数据库数据并保存的记录
                if (!string.IsNullOrEmpty(id))
                {
                    string commandtext = string.Format("Select * FROM welddata_spot where ID >= '{0}'", id);
                    DataTable dt = sql.GetDataTable1(commandtext);
                    if (dt != null)
                    {
                        AddLog("共获取" + dt.Rows.Count + "记录");
                        foreach (DataRow dr in dt.Rows)
                        {
                            InstrumentParameters par = new InstrumentParameters()
                            {
                                Current = dr["CURRENT1"].ToString(),
                                Voltage = dr["VOLTAGE1"].ToString(),
                                Temperature = dr["TEMPERATURE"].ToString(),
                                Pressure = dr["PRESSURE"].ToString(),
                                WeldTime = dr["WELDTIME1"].ToString()
                            };
                            this.jsonHelper.AppendWrite<InstrumentParameters>(index + ".json", par);
                        }
                    }
                }
                this.txtWeldTime.Text = "";
                this.txtWenDu.Text = "";
                this.txtYaLi.Clear();
                this.txtDianYa.Clear();
                this.txtDianLiu.Clear();
            }
            catch(Exception ex)
            {
                SimpleLogHelper.Instance.WriteLog(LogType.Info, ex);
            }
        }

        private void txbHandInput_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if(this.spHandInput.Visibility == Visibility.Collapsed)
            {
                this.spHandInput.Visibility = Visibility.Visible;
                this.txbHandInput.Text = "\xe664";
            }
            else
            {
                this.spHandInput.Visibility = Visibility.Collapsed;
                this.txbHandInput.Text = "\xe665";
            }
        }

        private void btnReset_Click(object sender, RoutedEventArgs e)
        {
            Reset();
        }

        private void StackPanel_TextChanged(object sender, RoutedEventArgs e)
        {
            var d = e.OriginalSource as TextBox;
            if(d.Name == "txtBianHao")
            {
                if(d.Text.Length <=2)
                e.Handled = true;
            }
        }
    }
}
