using MesToPlc.Models;
using Services;
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
using System.Windows.Shapes;

namespace MesToPlc
{
    /// <summary>
    /// MenuSet.xaml 的交互逻辑
    /// </summary>
    public partial class MenuSet : Window
    {
        IniHelper ini = new IniHelper(System.AppDomain.CurrentDomain.BaseDirectory + @"\Set.ini");
        public MenuSet()
        {
            InitializeComponent();
            this.Loaded += MenuSet_Loaded;
        }

        private void MenuSet_Loaded(object sender, RoutedEventArgs e)
        {
            this.txtPLCIP.Text = ini.ReadIni(Set.ConfigTCP, Set.PLCIP);
            this.txtPLCPort.Text = ini.ReadIni(Set.ConfigTCP, Set.PLCPort);
            this.txtYiBiaoIP.Text = ini.ReadIni(Set.ConfigTCP, Set.YiBiaoIP);
            this.txtYiBiaoPort.Text = ini.ReadIni(Set.ConfigTCP, Set.YiBiaoPort);
        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            ini.WriteIni(Set.ConfigTCP, Set.PLCIP, this.txtPLCIP.Text);
            ini.WriteIni(Set.ConfigTCP, Set.PLCPort, this.txtPLCPort.Text);
            ini.WriteIni(Set.ConfigTCP, Set.YiBiaoIP, this.txtYiBiaoIP.Text);
            ini.WriteIni(Set.ConfigTCP, Set.YiBiaoPort, this.txtYiBiaoPort.Text);
            MessageBox.Show("保存成功");
            this.Close();
        }
    }
}
