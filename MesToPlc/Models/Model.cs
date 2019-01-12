using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MesToPlc.Models
{
    public class Model
    {
        private string iD;
        private string bianHao;
        private string addTime1;

        public string ID
        {
            get
            {
                return iD;
            }

            set
            {
                iD = value;
            }
        }

        public string BianHao
        {
            get
            {
                return bianHao;
            }

            set
            {
                bianHao = value;
            }
        }

        public string AddTime1
        {
            get
            {
                return addTime1;
            }

            set
            {
                addTime1 = value;
            }
        }
    }
}
