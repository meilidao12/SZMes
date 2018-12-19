using MesToPlc.Models;
using Services;
using Services.DataBase;
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
using WpfServers;

namespace MesToPlc
{
    /// <summary>
    /// ChangeUser.xaml 的交互逻辑
    /// </summary>
    public partial class ChangeUser : Window
    {
        IniHelper ini = new IniHelper(System.AppDomain.CurrentDomain.BaseDirectory + @"\Set.ini");
        SqlHelper sql = new SqlHelper();
        public ChangeUser()
        {
            InitializeComponent();
            this.Loaded += ChangeUser_Loaded;
            WindowFactory.windowbackhome += HomeEvent;
        }

        private void ChangeUser_Loaded(object sender, RoutedEventArgs e)
        {
            
        }
        private void HomeEvent()
        {
            this.DialogResult = false;
            WindowFactory.windowbackhome -= HomeEvent;
            WindowFactory.BackHome_Event();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            string commandText = "SELECT * FROM [User]";
            List<UserModel> users = sql.GetDataTable<UserModel>(commandText);
            foreach (var item in users)
            {
                if (item.UserName == this.txtUserName.Text)
                {
                    if (item.PassWord == this.txtPassWord.Text)
                    {
                        ini.WriteIni("Config", "UserName", this.txtUserName.Text);
                        MessageBox.Show("切换成功");
                        HomeEvent();
                        return;
                    }
                    MessageBox.Show("密码不正确");
                    return;
                }
            }
            MessageBox.Show("用户名不正确");
        }
    }
}
