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
namespace MesToPlc.Pages
{
    

    /// <summary>
    /// OperatePage.xaml 的交互逻辑
    /// </summary>
    public partial class OperatePage : Page
    {
        SqlHelper sql = new SqlHelper();
        SocketClientEx socPlc;  //连接plc的socket
        SocketClientEx socInstrument; //连接仪表的socket
        WorkHelper WorkHelper = new WorkHelper();
        IniHelper ini = new IniHelper(System.AppDomain.CurrentDomain.BaseDirectory + @"\Set.ini");
        CharacterConversion characterConversion;
        DispatcherTimer LinkToMesTimer = new DispatcherTimer();
        DispatcherTimer LinkToMesStateTimer = new DispatcherTimer();
        DispatcherTimer LinkToPlcTimer = new DispatcherTimer();
        DispatcherTimer VerifyTimer = new DispatcherTimer();
       public OperatePage()
        {
            InitializeComponent();
            this.Loaded += OperatePage_Loaded;
            this.Unloaded += OperatePage_Unloaded;          
            this.MesState.Source = ConnectResult.Normal;
        }

        private void SetMesState(BitmapImage connectResult)
        {
            this.LinkToMesStateTimer.Start();
            Dispatcher.BeginInvoke(new Action(() =>
            {
                this.MesState.Source = connectResult;
            }));
        }

        private void OperatePage_Unloaded(object sender, RoutedEventArgs e)
        {
            
        }

        private void OperatePage_Loaded(object sender, RoutedEventArgs e)
        {
            //---
            VerifyTimer.Interval = TimeSpan.FromSeconds(2);
            VerifyTimer.Tick += VerifyTimer_Tick;
            //---定义连接plc的socket
            string plcPort = ini.ReadIni(Set.ConfigTCP, Set.PLCPort);
            string plcIp = ini.ReadIni(Set.ConfigTCP, Set.PLCIP);
            socPlc = new SocketClientEx();
            socPlc.NewMessageEvent += SocPlc_NewMessageEvent;
            socPlc.ServerDisconnectedEvent += SocPlc_ServerDisconnectedEvent;
            socPlc.Connnect(plcPort, plcIp);
            //---定义连接仪表的socket
            string YiBiaoPort = ini.ReadIni(Set.ConfigTCP, Set.YiBiaoPort);
            string YiBiaoIP = ini.ReadIni(Set.ConfigTCP, Set.YiBiaoIP);
            socInstrument = new SocketClientEx();
            socInstrument.ServerDisconnectedEvent += SocInstrument_ServerDisconnectedEvent;
            socInstrument.NewMessageEvent += SocInstrument_NewMessageEvent;
            socInstrument.Connnect(YiBiaoPort, YiBiaoIP);
            VerifyTimer.Start();
            
        }

        private void SocInstrument_NewMessageEvent(Socket socket, byte[] Message)
        {
            
        }

        private void SocInstrument_ServerDisconnectedEvent(Socket socket)
        {
            AddLog("与仪表连接中断");
        }

        private void SocPlc_NewMessageEvent(Socket socket, byte[] Message)
        {
            AddLog(socPlc.ByteConvertToString(Message));
            AddLog(Message.Length.ToString());
        }

        private void SocPlc_ServerDisconnectedEvent(Socket socket)
        {
            AddLog("与PLC连接中断");
        }

        /// <summary>
        /// 检测与plc和仪表的通讯状态，如果通讯中断，则重新连接
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void VerifyTimer_Tick(object sender, EventArgs e)
        {
            try
            {
                if (!socPlc.IsConnected())
                {
                    AddLog("与plc连接中断，重新开始连接");
                    string plcPort = ini.ReadIni(Set.ConfigTCP, Set.PLCPort);
                    string plcIp = ini.ReadIni(Set.ConfigTCP, Set.PLCIP);
                    socPlc.Connnect(plcPort, plcIp);
                }
                else
                {
                    AddLog("plc目前状态：连通");
                }
                if (!socInstrument.IsConnected())
                {
                    AddLog("与仪表连接中断，重新开始连接");
                    string YiBiaoPort = ini.ReadIni(Set.ConfigTCP, Set.YiBiaoPort);
                    string YiBiaoIP = ini.ReadIni(Set.ConfigTCP, Set.YiBiaoIP);
                    socInstrument.Connnect(YiBiaoPort, YiBiaoIP);
                }
                else
                {
                    AddLog("仪表目前连接状态：连通");
                }
            }
            catch (Exception ex)
            {
                SimpleLogHelper.Instance.WriteLog(LogType.Error, ex);
            }
                
        }

        private void SocketServer_NewMessage1Event(Socket socket, string Message)
        {
            try
            {
                SimpleLogHelper.Instance.WriteLog(LogType.Info, Message);
                if (Message.Length != 24) return;
                AddLog("PLC数据请求" + Message);
                ModbusTcpServer modbusTcpServer = new ModbusTcpServer();
                modbusTcpServer.AffairID = Message.Substring(0, 4);
                modbusTcpServer.ProtocolID = Message.Substring(4, 4);
                int requestDataLength = MathHelper.HexToDec(Message.Substring(Message.Length - 4, 4)); //请求数据长度
                modbusTcpServer.BackDataLength = MathHelper.DecToHex((requestDataLength * 2).ToString()).PadLeft(2, '0');
                modbusTcpServer.SlaveId = Message.Substring(12, 2);
                modbusTcpServer.Length = MathHelper.DecToHex((3 + requestDataLength * 2).ToString()).PadLeft(4, '0');
                string backdata = modbusTcpServer.AffairID + modbusTcpServer.ProtocolID + modbusTcpServer.Length + modbusTcpServer.SlaveId + ModbusFunction.ReadHoldingRegisters + modbusTcpServer.BackDataLength;
                string chengxuhao = MathHelper.DecToHex(ini.ReadIni("Request", "Index")).PadLeft(4, '0');
                string xinghao = MathHelperEx.StrToASCII1(ini.ReadIni("Request", "ModelNum"));
                string xinghaolength = MathHelper.DecToHex((ini.ReadIni("Request", "ModelNum").Length * 2).ToString()).PadLeft(4, '0');
                backdata += (chengxuhao + xinghaolength + xinghao).PadRight(104, 'F');
                //if (this.socketServer.IsConnected(this.socketClient))
                //{
                //    characterConversion = new CharacterConversion();
                //    this.socketServer.Send(this.socketClient, characterConversion.HexConvertToByte(backdata));
                //    AddLog("返回PLC数据：" + backdata);
                //}
            }
            catch
            {
                AddLog("PLC请求数据失败");
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
                SimpleLogHelper.Instance.WriteLog(LogType.Info, log);
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

        private void txtBianHao_TextChanged(object sender, TextChangedEventArgs e)
        {
            if(string.IsNullOrEmpty(this.txtBianHao.Text))
            {
                return;
            }
            if(this.WorkHelper.HandInput)
            {
                this.WorkHelper.HandInput = false;
                this.txtBianHao.Text = "";
                return;
            }
            this.WorkHelper.WorkFlag = true;
            AddLog("开始焊接新的工件");
        }

        private void txtBianHao_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            this.WorkHelper.HandInput = true;
            MessageBox.Show("禁止直接在文本框中输入");
        }

        private void btnClearBianHao_Click(object sender, RoutedEventArgs e)
        {
            this.txtBianHao.Text = "";
        }
    }
}
