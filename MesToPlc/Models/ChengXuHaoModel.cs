using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WpfServers.Notification;

namespace MesToPlc.Models
{
    public class ChengXuHaoModel : NotificationObject
    {
        private string xingHao;
        private string chengXuHao;
        private string addTime;

        public string XingHao
        {
            get
            {
                return xingHao;
            }

            set
            {
                xingHao = value;
                RaisePropertyChanged("XingHao");
            }
        }

        public string ChengXuHao
        {
            get
            {
                return chengXuHao;
            }

            set
            {
                chengXuHao = value;
                RaisePropertyChanged("ChengXuHao");
            }
        }

        public string AddTime
        {
            get
            {
                return addTime;
            }

            set
            {
                addTime = value;
                RaisePropertyChanged("AddTime");
            }
        }
    }
}
