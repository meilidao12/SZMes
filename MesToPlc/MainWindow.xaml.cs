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
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using WpfServers;

namespace MesToPlc
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑a
    /// </summary>
    public partial class MainWindow : Window
    {
        private string loginUrl = "Login";
        private string operatepage = "Pages/OperatePage.xaml";
        IniHelper ini = new IniHelper(System.AppDomain.CurrentDomain.BaseDirectory + @"\Set.ini");
        public MainWindow()
        {
            InitializeComponent();
            this.Loaded += MainWindow_Loaded;
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            //Login(this.loginUrl);
            this.frm.Source = new Uri(operatepage, UriKind.Relative);
        }

        private void Login(string loginUrl)
        {
            Window NextWindow = WindowFactory.CreateWindow(loginUrl);
            WindowFactory.Show(this, NextWindow);
        }

        private void Menu_Click(object sender, RoutedEventArgs e)
        {
            MenuItem menuItem = (MenuItem)e.OriginalSource;
            switch (menuItem.Name)
            {
                case "RegisterUser":
                    Login("Register");
                    break;
                case "ChangeUser":
                    Login("ChangeUser");
                    break;
                case "MenuClose":
                    if (MessageBox.Show("是否关闭此程序?", "消息提示", MessageBoxButton.OKCancel) == MessageBoxResult.OK) Application.Current.Shutdown();
                    break;
                case "MenuMin":
                    this.WindowState = WindowState.Minimized;
                    break;
                case "AddChengXuHao":
                    ini.WriteIni("Config", "AddWindowShow", WindowShowState.ShowState.Add.ToString());
                    Login("AddChengXuHao");
                    break;
            }
        }

        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            User.MoveWindow(User.GetWindowHandle(this));
        }
    }
}
