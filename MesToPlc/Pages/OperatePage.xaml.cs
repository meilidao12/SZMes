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
        DispatcherTimer VerifyTimer = new DispatcherTimer();
        JsonHelper jsonHelper = new JsonHelper();
       public OperatePage()
        {
            InitializeComponent();
            this.Loaded += OperatePage_Loaded;
            this.Unloaded += OperatePage_Unloaded;
        }

        private void OperatePage_Unloaded(object sender, RoutedEventArgs e)
        {
            
        }

        private void OperatePage_Loaded(object sender, RoutedEventArgs e)
        {
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
        }

        private void AddLog(string log)
        {
            Dispatcher.BeginInvoke(new Action(() =>
            {
                log = DateTime.Now + ": " + log;
                this.lstInfoLog.Items.Add(log);
                Decorator decorator = (Decorator)VisualTreeHelper.GetChild(lstInfoLog, 0);
                ScrollViewer scrollViewer = (ScrollViewer)decorator.Child;
                scrollViewer.ScrollToEnd();
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
            if (this.txtBianHao.Text == "") return;
            //结束老的
            FinishNow();
            //开始新的
            StartAnother();
        }

        /// <summary>
        ///  开始新的
        /// </summary>
        private void StartAnother()
        {

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
                    string commandtext = string.Format("Select * FROM welddata_spot where NO >= '{0}'", id);
                    DataTable dt = sql.GetDataTable1(commandtext);
                    if (dt != null)
                    {
                        AddLog("共获取" + dt.Rows.Count + "记录");
                        foreach (DataRow dr in dt.Rows)
                        {
                            InstrumentParameters par = new InstrumentParameters()
                            {
                                Current = dr["Current(1)"].ToString(),
                                Voltage = dr["Voltage(1)"].ToString(),
                                Temperature = dr["Temperature"].ToString(),
                                Pressure = dr["Pressure"].ToString(),
                                WeldTime = dr["WeldTime(1)"].ToString()
                            };
                            this.jsonHelper.AppendWrite<InstrumentParameters>(index + ".json", par);
                        }
                    }
                }
            }
            catch(Exception ex)
            {
                SimpleLogHelper.Instance.WriteLog(LogType.Info, ex);
            }
        }
    }
}
