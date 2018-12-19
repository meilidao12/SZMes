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
        SocketClient socPlc;  //连接plc的socket
        SocketClient socInstrument; //连接仪表的socket
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
            string port = ini.ReadIni("Config", "Port");
            
            
            
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
    }
}
