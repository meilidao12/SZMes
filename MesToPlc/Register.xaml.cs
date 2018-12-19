using MesToPlc.Models;
using Services;
using Services.DataBase;
using System;
using System.Collections.Generic;
using System.Diagnostics;
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
    /// Register.xaml 的交互逻辑
    /// </summary>
    public partial class Register : Window
    {
        IniHelper ini = new IniHelper(System.AppDomain.CurrentDomain.BaseDirectory + @"\Set.ini");
        SqlHelper sql = new SqlHelper();
        public Register()
        {
            InitializeComponent();
            this.Loaded += Register_Loaded;
            WindowFactory.windowbackhome += HomeEvent;
        }

        private void Register_Loaded(object sender, RoutedEventArgs e)
        {
            //判断权限
            string commandText = "SELECT * FROM [User]";
            List<UserModel> users = sql.GetDataTable<UserModel>(commandText);
            foreach (var item in users)
            {
                if (item.UserName == ini.ReadIni("Config", "UserName"))
                {
                    List<ComboxModel> models = new List<ComboxModel>();
                    switch (item.Authority)
                    {
                        case "0":
                            models.Add(new ComboxModel { Name = "高级", Value = "0" });
                            models.Add(new ComboxModel { Name = "普通", Value = "1" });
                            break;
                        case "1":
                            models.Add(new ComboxModel { Name = "普通", Value = "1" });
                            break;
                    }
                    this.cmbVerify.ItemsSource = models;
                    this.cmbVerify.DisplayMemberPath = "Name";
                    this.cmbVerify.SelectedValuePath = "Value";
                    this.cmbVerify.SelectedIndex = 0;
                }
            }
        }

        private void HomeEvent()
        {
            this.DialogResult = false;
            WindowFactory.windowbackhome -= HomeEvent;
            WindowFactory.BackHome_Event();
        }

        private void Register_Click(object sender, RoutedEventArgs e)
        {
            if (this.UserName.Text == "" || this.PassWord.Text == "")
            {
                MessageBox.Show("用户名和密码不能为空");
            }
            string commandText = "SELECT * FROM [User]";
            List<UserModel> users = sql.GetDataTable<UserModel>(commandText);
            foreach (var item in users)
            {
                if (item.UserName == this.UserName.Text)
                {
                    MessageBox.Show("用户名已存在");
                    return;
                }
            }
            commandText = string.Format("insert into [User] (UserName,PassWord,Authority) values ('{0}','{1}','{2}')",this.UserName.Text,this.PassWord.Text,this.cmbVerify.SelectedValue.ToString());
            bool result = sql.Execute(commandText);
            if(result)
            {
                SimpleLogHelper.Instance.WriteLog(LogType.Info, "注册新用户成功");
                MessageBox.Show("注册新用户成功");
            }
        }
    }
}
