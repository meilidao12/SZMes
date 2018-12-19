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
        Socket socketClient;
        SocketServer socketServer;
        IniHelper ini = new IniHelper(System.AppDomain.CurrentDomain.BaseDirectory + @"\Set.ini");
        CharacterConversion characterConversion;
        DispatcherTimer LinkToMesTimer = new DispatcherTimer();
        DispatcherTimer LinkToMesStateTimer = new DispatcherTimer();
        DispatcherTimer LinkToPlcTimer = new DispatcherTimer();
        DispatcherTimer VerifyTimer = new DispatcherTimer();
        int PlcDelayCount;
        int MesDelayCount;
        public OperatePage()
        {
            InitializeComponent();
            this.Loaded += OperatePage_Loaded;
            this.Unloaded += OperatePage_Unloaded;
            this.LinkToMesTimer.Interval = new TimeSpan(0,0,10);
            this.LinkToMesTimer.IsEnabled = true;
            this.LinkToMesTimer.Tick += LinkToMesTimer_Tick;
            this.LinkToPlcTimer.Interval = new TimeSpan(0,0,1);
            this.LinkToPlcTimer.Tick += LinkToPlcTimer_Tick;
            this.VerifyTimer.Interval = new TimeSpan(0, 0, 1);
            this.VerifyTimer.Tick += VerifyTimer_Tick;
            this.VerifyTimer.IsEnabled = true;
            this.LinkToMesStateTimer.Interval = new TimeSpan(0, 0, 1);
            this.LinkToMesStateTimer.Tick += LinkToMesStateTimer_Tick;
            this.PlcState.Source = ConnectResult.Normal;
            this.MesState.Source = ConnectResult.Normal;
            this.PlcDelayCount = 1;
            this.MesDelayCount = 1;
        }

        private void LinkToMesStateTimer_Tick(object sender, EventArgs e)
        {
            if (this.MesDelayCount-- == 0)
            {
                this.MesDelayCount = 1;
                this.MesState.Source = ConnectResult.Normal;
                this.LinkToMesStateTimer.IsEnabled = false;
                this.LinkToMesStateTimer.Stop();
            }
        }

        private void VerifyTimer_Tick(object sender, EventArgs e)
        {
            HandInputVerify();
        }

        private void LinkToPlcTimer_Tick(object sender, EventArgs e)
        {
            if (this.PlcDelayCount-- == 0)
            {
                this.PlcDelayCount = 1;
                this.PlcState.Source = ConnectResult.Normal;
                this.LinkToPlcTimer.IsEnabled = false;
                this.LinkToPlcTimer.Stop();
            }
        }

        /// <summary>
        ///  访问Mes定时器
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void LinkToMesTimer_Tick(object sender, EventArgs e)
        {
            if(this.txtSerialNum.Text == "") return;
            MesLoad mesload = new MesLoad();
            MesLoadResult mesLoadResult = new MesLoadResult();
            switch (mesload.GetData(out mesLoadResult, ini.ReadIni("Config", "MesInterface") + this.txtSerialNum.Text.Trim()))
            {
                case RequestResult.Success:
                    SetMesState(ConnectResult.Success);
                    this.AddLog("请求MES成功");
                    break;
                case RequestResult.Fail:
                    SetMesState(ConnectResult.Fail);
                    this.AddLog("请求MES系统没有响应");
                    return;
                default:
                    break;
            }
            switch (mesLoadResult.Type)
            {
                case "S":
                    this.txtModelNum.Text = mesLoadResult.MaterialCode;
                    this.AddLog("MES返回型号：" + mesLoadResult.MaterialCode);
                    this.GetChengXuHao(this.txtModelNum.Text);
                    break;
                case "E":
                    SetMesState(ConnectResult.Fail);
                    this.AddLog("未找到该序列号对应型号");
                    break;
                default:
                    break;
            }
            HandInputVerify();
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
            socketClient.Close();
        }

        private void OperatePage_Loaded(object sender, RoutedEventArgs e)
        {
            this.btnSure.IsEnabled = false;
            string port = ini.ReadIni("Config", "Port");
            socketServer = new SocketServer();
            this.txtSerialNumR.Text = ini.ReadIni("Response", "SerialNum");
            this.txtModelNumR.Text = ini.ReadIni("Response", "ModelNum");
            this.txtIndexR.Text = ini.ReadIni("Response", "Index");
            bool listenResult = socketServer.Listen(port);
            if(listenResult == false)
            {
                MessageBox.Show(string.Format("监听{0}口失败", port));
            }
            SimpleLogHelper.Instance.WriteLog(LogType.Info,"监听端口：" +  listenResult);
            socketServer.NewConnnectionEvent += SocketServer_NewConnnectionEvent;
            socketServer.NewMessage1Event += SocketServer_NewMessage1Event;
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
                if (this.socketServer.IsConnected(this.socketClient))
                {
                    characterConversion = new CharacterConversion();
                    this.socketServer.Send(this.socketClient, characterConversion.HexConvertToByte(backdata));
                    AddLog("返回PLC数据：" + backdata);
                    CopyMesToPlc();
                }
                this.LinkToPlcTimer.Start();
                Dispatcher.BeginInvoke(new Action(() =>
                {
                    this.PlcState.Source = ConnectResult.Success;
                }));
            }
            catch
            {
                AddLog("PLC请求数据失败");
                this.LinkToPlcTimer.Start();
                Dispatcher.BeginInvoke(new Action(() =>
                {
                    this.PlcState.Source = ConnectResult.Fail;
                }));
            }
        }

        private void CopyMesToPlc()
        {
            Dispatcher.BeginInvoke(new Action(() =>
            {
                this.txtIndex.Clear();
                this.txtModelNum.Clear();
                this.txtSerialNum.Clear();
                this.txtSerialNumR.Text = ini.ReadIni("Request", "SerialNum");
                this.txtModelNumR.Text = ini.ReadIni("Request", "ModelNum");
                this.txtIndexR.Text = ini.ReadIni("Request", "Index");
                ini.WriteIni("Response", "SerialNum", this.txtSerialNumR.Text);
                ini.WriteIni("Response", "ModelNum", this.txtModelNumR.Text);
                ini.WriteIni("Response", "Index", this.txtIndexR.Text);
            }));
        }

        private void SocketServer_NewConnnectionEvent(Socket socket)
        {
            socketClient = socket;
        }

        private void HandInputVerify()
        {
            string commandText = "SELECT * FROM [User] Where Authority = '0'";
            List<UserModel> users = sql.GetDataTable<UserModel>(commandText);
            if (users == null) return;
            foreach (var item in users)
            {
                if (ini.ReadIni("Config", "UserName") == item.UserName)
                {
                    this.HandInput.IsEnabled = true;
                    return;
                }
            }
            this.HandInput.IsEnabled = false;
            //this.txtIndex.IsEnabled = false;
            //this.txtModelNum.IsEnabled = false;
        }

        private void AddChengXuHao_DataBackEvent(ChengXuHaoModel chengXuHaoModel)
        {
            Dispatcher.BeginInvoke(new Action(() =>
            {
                this.txtIndex.Text = chengXuHaoModel.ChengXuHao;
                this.txtModelNum.Text = chengXuHaoModel.XingHao;
                GetChengXuHao(chengXuHaoModel.XingHao);
            }));
        }

        private void btnSure_Click(object sender, RoutedEventArgs e)
        {
            if (this.txtModelNum.Text == "") return;
            this.GetChengXuHao(this.txtModelNum.Text);
        }

        private void GetChengXuHao(string xinghao)
        {
            List<ChengXuHaoModel> chengXuHaoModels = sql.GetDataTable<ChengXuHaoModel>("select * from ChengXuHao");
            foreach (var item in chengXuHaoModels)
            {
                if (this.txtModelNum.Text == item.XingHao)
                {
                    this.txtIndex.Text = item.ChengXuHao;
                    ini.WriteIni("Request", "SerialNum", this.txtSerialNum.Text);
                    ini.WriteIni("Request", "ModelNum", item.XingHao);
                    ini.WriteIni("Request", "Index", item.ChengXuHao);
                    AddLog("型号与成序号映射成功并保存");
                    AddLog("型号：" + this.txtModelNum.Text);
                    AddLog("程序号：" + item.ChengXuHao);
                    return;
                }
            }
            AddLog("未找到对应程序号");
        }

        private void HandInput_Click(object sender, RoutedEventArgs e)
        {
            if (this.HandInput.IsChecked == true)
            {
                AddLog("已切换到手动输入");
                this.btnSure.IsEnabled = true;
                this.LinkToMesTimer.IsEnabled = false;
                AddChengXuHao addChengXuHao = new AddChengXuHao();
                addChengXuHao.DataBackEvent += AddChengXuHao_DataBackEvent;
                ini.WriteIni("Config", "AddWindowShow", WindowShowState.ShowState.Select.ToString());
                addChengXuHao.Show();
            }
            else
            {
                AddLog("已切换到自动请求");
                //this.btnSure.IsEnabled = false;
                this.LinkToMesTimer.IsEnabled = true;
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

        private void btnClear_Click(object sender, RoutedEventArgs e)
        {
            this.lstInfoLog.Items.Clear();
        }

        
    }
}
