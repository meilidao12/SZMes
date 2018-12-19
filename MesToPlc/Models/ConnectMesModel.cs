using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media.Imaging;

namespace MesToPlc.Models
{
    public class ConnectMesModel
    {
        //private string connectResult;
        //public string ConnectResult
        //{
        //    get
        //    {

        //    }
        //    set
        //    {

        //    }
        //}
    }
    public class ConnectResult
    {
        public static BitmapImage Normal
        {
            get
            {
                return new BitmapImage(new Uri("/Resource/Normal.png",UriKind.Relative));
            }
        }
        public static BitmapImage Success
        {
            get
            {
                return new BitmapImage(new Uri("/Resource/Success.png", UriKind.Relative));
            }
        }

        public static BitmapImage Fail
        {
            get
            {
                return new BitmapImage(new Uri("/Resource/Fail.png", UriKind.Relative));
            }
        }
    }
   
}
