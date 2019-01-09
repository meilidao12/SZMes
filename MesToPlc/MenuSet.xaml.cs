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
            
        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            
        }
    }
}
