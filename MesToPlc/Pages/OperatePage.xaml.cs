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
        //AccessHelper accessHelper = new AccessHelper();
        bool blClear = false;
        ExcelHelper ex;
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
            ex = new ExcelHelper();
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
            string commandtext = string.Format("Select VOLTAGE1,CURRENT1,PRESSURE,TEMPERATURE,WELDTIME1 FROM welddata_spot");
            DataTable dt = sql.GetDataTable1(commandtext);
            SimpleLogHelper.Instance.WriteLog(LogType.Info, dt.Rows[0]["VOLTAGE1"].ToString());
            //
            //InstrumentParameters par = new InstrumentParameters();
            //foreach (DataRow item in dt.Rows)
            //{
            //    par.Current = item["CURRENT1"].ToString();
            //    par.Voltage = item["VOLTAGE1"].ToString();
            //    par.Pressure = item["PRESSURE"].ToString();
            //    par.Temperature = item["TEMPERATURE"].ToString();
            //    par.WeldTime = item["WELDTIME1"].ToString();
            //    string a = string.Format("CURRENT1:{0},VOLTAGE1:{1},PRESSURE:{2},TEMPERATURE:{3},WELDTIME1:{4}",par.Current,par.Voltage,par.Pressure,par.Temperature,par.WeldTime);
            //    SimpleLogHelper.Instance.WriteLog(LogType.Info, a);
            //}
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
            else
            {
                SimpleLogHelper.Instance.WriteLog(LogType.Info, "检索条件：Select * FROM welddata_spot ORDER BY `ID` DESC LIMIT 1" + "，搜索数据为空");
            }
        }

        private void AddLog(string log)
        {
            Dispatcher.BeginInvoke(new Action(() =>
            {
                try
                {
                    log = DateTime.Now + ": " + log;
                    this.lstInfoLog.Items.Add(log);
                    SimpleLogHelper.Instance.WriteLog(LogType.Info, log);
                    //Decorator decorator = (Decorator)VisualTreeHelper.GetChild(lstInfoLog, 0);
                    //ScrollViewer scrollViewer = (ScrollViewer)decorator.Child;
                    //scrollViewer.ScrollToEnd();
                    if (this.lstInfoLog.Items.Count >= 60)
                    {
                        int ClearLstCount = lstInfoLog.Items.Count - 60;
                        this.lstInfoLog.Items.RemoveAt(ClearLstCount);
                    }
                }
                catch(Exception ex)
                {
                    SimpleLogHelper.Instance.WriteLog(LogType.Error, ex);
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
            //检查是否是重复插入
            //string commandText = string.Format("Select * from Model Where BianHao='{0}'", this.txtBianHaoHandInput.Text);
            //List<Model> models = accessHelper.GetDataTable<Model>(commandText);
            //if (models.Count == 0)
            //{
                this.txtBianHao.Text = this.txtBianHaoHandInput.Text;
                this.txtBianHaoHandInput.Text = "";
            //}
            //else
            //{
            //    AddLog("该编号已存在");
            //}
        }

        private void txtBianHao_TextChanged(object sender, TextChangedEventArgs e)
        {
            var d = e.OriginalSource as TextBox;
            if (d.Name == "txtBianHaoShow")
            {
                return;
            }
            if (this.txtBianHao.Text.Length <= 2) return;
            //检查是否是重复插入
            //string commandText = string.Format("Select * from Model Where BianHao='{0}'", this.txtBianHaoHandInput.Text);
            //accessHelper.GetDataTable<Model>(commandText);
            //List<Model> models = accessHelper.GetDataTable<Model>(commandText);
            //SimpleLogHelper.Instance.WriteLog(LogType.Info, models);
            //SimpleLogHelper.Instance.WriteLog(LogType.Info, models.Count);
            //if (models.Count == 0)
            //{
            //    if (blClear)
            //    {
            //        blClear = false;
            //        return;
            //    }
            //    e.Handled = true;
            //    //结束老的
            FinishNow();
            //开始新的
            StartAnother();
            //blClear = false;
            //}
            //else
            //{
            //    this.txtBianHao.Clear();
            //    AddLog("该编号已存在");
            //}
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
                    if(sql.TestConn != true)
                    {
                        sql.Open();
                    }
                    string commandtext = string.Format("Select VOLTAGE1,CURRENT1,PRESSURE,TEMPERATURE,WELDTIME1 FROM welddata_spot where ID >= '{0}'", id);
                    DataTable dt = sql.GetDataTable1(commandtext);
                    if (dt != null)
                    {
                        AddLog("共获取" + dt.Rows.Count + "记录");
                        InstrumentParameters par = new InstrumentParameters();
                        JsonHelper js = new JsonHelper();
                        foreach (DataRow item in dt.Rows)
                        {
                            par.Current = item["CURRENT1"].ToString();
                            par.Voltage = item["VOLTAGE1"].ToString();
                            par.Pressure = item["PRESSURE"].ToString();
                            par.Temperature = item["TEMPERATURE"].ToString();
                            par.WeldTime = item["WELDTIME1"].ToString();
                            //ex.Open(System.AppDomain.CurrentDomain.BaseDirectory + @"\Test.xlsx");
                            //ex.InsertTable(dt, "Sheet1", 2, 1);
                            //string infoFilePath = ini.ReadIni("Config", "JsonUrl");
                            //ex.SaveAs(infoFilePath + index + ".xlsx");
                            //ex.Close();
                            js.AppendWrite<InstrumentParameters>(this.txtBianHaoShow.Text + ".json", par);
                        }
                        
                    }
                    //commandtext = string.Format("insert into model ([BianHao],[AddTime1]) values ('{0}','{1}')",txtBianHaoShow.Text,DateTime.Now);
                    //if(accessHelper.Execute(commandtext))
                    //{
                    //    //foreach(var item in dt.Rows)
                    //    //{
                    //    //    commandtext = string.Format("insert into modeldetail ([VOLTAGE1],[CURRENT1],[PRESSURE],[TEMPERATURE],[WELDTIME1],[BianHao]) values ('{0}','{1}','{2}','{3}','{4}','{5}')",item["VOLTAGE"].ToString(), txtBianHaoShow.Text);
                    //    //}
                    AddLog("数据保存成功");
                    //}
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
