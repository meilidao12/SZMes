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
using MesToPlc.Models;
using System.Collections.ObjectModel;
using System.Data;
using Services.DataBase;
using System.Diagnostics;
using Services;

namespace MesToPlc
{
    public delegate void DataBack(ChengXuHaoModel chengXuHaoModel);
    /// <summary>
    /// AddChengXuHao.xaml 的交互逻辑
    /// </summary>
    
     public partial class AddChengXuHao : Window
    {
        IniHelper ini = new IniHelper(System.AppDomain.CurrentDomain.BaseDirectory + @"\Set.ini");
        public  event DataBack DataBackEvent;
        public PagingModel<ChengXuHaoModel> PagingModel;
        public ObservableCollection<ChengXuHaoModel> ChengXuHaoModels = new ObservableCollection<ChengXuHaoModel>();
        public ObservableCollection<ChengXuHaoModel> ChengXuHaoModelsBranch = new ObservableCollection<ChengXuHaoModel>();
        SqlHelper sql = new SqlHelper();
        ChengXuHaoModel SelectModel = new ChengXuHaoModel();
        public AddChengXuHao()
        {
            InitializeComponent();
            this.Loaded += AddChengXuHao_Loaded;
        }

        private void AddChengXuHao_Loaded(object sender, RoutedEventArgs e)
        {
            IniChengXuHaoModels();
            PagingModel = new PagingModel<ChengXuHaoModel>(ChengXuHaoModels, 30);
            PagingModel.GetPageData(JumpOperation.GoHome);
            //绑定
            this.txtCurrentPage.SetBinding(TextBlock.TextProperty, new Binding("CurrentIndex") { Source = PagingModel, Mode = BindingMode.TwoWay });
            this.txtTotalPage.SetBinding(TextBlock.TextProperty, new Binding("PageCount") { Source = PagingModel, Mode = BindingMode.TwoWay });
            this.txtTargetPage.SetBinding(TextBox.TextProperty, new Binding("JumpIndex") { Source = PagingModel, Mode = BindingMode.TwoWay });
            this.Record.SetBinding(DataGrid.ItemsSourceProperty, new Binding("ShowDataSource") { Source = PagingModel, Mode = BindingMode.TwoWay });
            if (ini.ReadIni("Config", "AddWindowShow") == WindowShowState.ShowState.Select.ToString())
            {
                this.Title = "选择型号与程序号";
                this.btnSure.Visibility = Visibility.Visible;
                this.btnAdd.Visibility = Visibility.Collapsed;
                this.btnDelete.Visibility = Visibility.Collapsed;
                this.btnChange.Visibility = Visibility.Collapsed;
                this.btnSearch.Visibility = Visibility.Collapsed;
                this.btnRefresh.Visibility = Visibility.Collapsed;

            }
        }

        private void IniChengXuHaoModels()
        {
           List<ChengXuHaoModel>  chengXuHaoModels = sql.GetDataTable<ChengXuHaoModel>("select * from ChengXuHao");
           if (chengXuHaoModels == null) return;
           ChengXuHaoModels.Clear();
           chengXuHaoModels.ForEach(p => ChengXuHaoModels.Add(p));
        }

        private void btnAdd_Click(object sender, RoutedEventArgs e)
        {
            List<ChengXuHaoModel> chengXuHaoModels = sql.GetDataTable<ChengXuHaoModel>("select * from ChengXuHao");
            foreach (var item in ChengXuHaoModels)
            {
                if (item.XingHao == this.txtXingHao.Text)
                {
                    MessageBox.Show("程序号已存在");
                    return;
                }
            }
            string commandtext = string.Format("insert into ChengXuHao (ChengXuHao,XingHao,AddTime) values ('{0}','{1}','{2}')", this.txtChengXuHao.Text, this.txtXingHao.Text, DateTime.Now.ToString());
            if (sql.Execute(commandtext))
            {
                MessageBox.Show("添加成功");
            }
            else
            {
                MessageBox.Show("添加失败");
            }
        }

        private void btnChange_Click(object sender, RoutedEventArgs e)
        {
            if (this.txtXingHao.Text != "" || this.txtChengXuHao.Text != "")
            {
                string commandtext = string.Format("update ChengXuHao set ChengXuHao='{0}',XingHao='{1}',AddTime='{2}' where XingHao='{3}'", this.txtChengXuHao.Text, this.txtXingHao.Text, DateTime.Now.ToString(),SelectModel.XingHao);
                if (sql.Execute(commandtext))
                {
                    MessageBox.Show("修改成功");
                }
                else
                {
                    MessageBox.Show("修改失败");
                }
            }
        }

        private void PageOperationClick(object sender, RoutedEventArgs e)
        {

            Button btn = (Button)e.Source;
            switch (btn.Name)
            {
                case "btnHomePage":
                    PagingModel.GetPageData(JumpOperation.GoHome);
                    break;
                case "btnPreviousPage":
                    PagingModel.GetPageData(JumpOperation.GoPrevious);
                    break;
                case "btnNextPage":
                    PagingModel.GetPageData(JumpOperation.GoNext);
                    break;
                case "btnEndPage":
                    PagingModel.GetPageData(JumpOperation.GoEnd);
                    break;
                case "btnJmpPage":
                    PagingModel.JumpPageData(Convert.ToInt32(txtTargetPage.Text));
                    break;
            }
        }

        private void ShowData(string displayName)
        {
            List<ChengXuHaoModel> lst = ChengXuHaoModels.Where(m => m.XingHao == displayName).ToList();
            if(lst == null)
            {
                MessageBox.Show("未找到该型号");
                return;
            }
            ListToObservable(lst, ChengXuHaoModelsBranch);
            PagingModel.DataSource = ChengXuHaoModelsBranch;  //记录
            PagingModel.Refresh();
        }

        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            if(this.txtXingHao.Text == "")
            {
                MessageBox.Show("型号不能为空");
                return;
            }
            ShowData(this.txtXingHao.Text);
        }

        private void ListToObservable(List<ChengXuHaoModel> lst, ObservableCollection<ChengXuHaoModel> obc)
        {
            obc.Clear();
            lst.ForEach(p => obc.Add(p));
        }

        private void btnRefresh_Click(object sender, RoutedEventArgs e)
        {
            IniChengXuHaoModels();
            PagingModel.DataSource = ChengXuHaoModels;
            PagingModel.GetPageData(JumpOperation.GoHome);
            PagingModel.Refresh();
        }

        private void Record_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (this.Record.SelectedItem != null)
            {
                var a = this.Record.SelectedItem;
                SelectModel = a as ChengXuHaoModel;
                this.txtXingHao.Text = SelectModel.XingHao;
                this.txtChengXuHao.Text = SelectModel.ChengXuHao;
            }
        }

        private void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            if (this.Record.SelectedItem != null)
            {
                var a = this.Record.SelectedItem;
                SelectModel = a as ChengXuHaoModel;
                string commandtext = string.Format("delete from ChengXuHao where XingHao='{0}'",SelectModel.XingHao);
                if (MessageBox.Show("是否删除?", "提示", MessageBoxButton.OKCancel)== MessageBoxResult.OK)
                {
                    sql.Execute(commandtext);
                }
                IniChengXuHaoModels();
                PagingModel.DataSource = ChengXuHaoModels;
                PagingModel.GetPageData(JumpOperation.GoHome);
                PagingModel.Refresh();
                this.txtXingHao.Clear();
                this.txtChengXuHao.Clear();
            }
        }

        private void btnSure_Click(object sender, RoutedEventArgs e)
        {
            if (this.Record.SelectedItem != null)
            {
                var a = this.Record.SelectedItem;
                SelectModel = a as ChengXuHaoModel;
                DataBackEvent(SelectModel);
                this.Close();
            }
        }
    }
}
