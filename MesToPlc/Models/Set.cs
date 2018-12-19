using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MesToPlc.Models
{
    public class Set
    {
        public static string RS485
        {
            get
            {
                return "RS485";
            }
        }

        public static string TCP
        {
            get
            {
                return "TCP/IP";
            }
        }

        public static string Config
        {
            get
            {
                return "Config";
            }
        }

        public static string Select
        {
            get
            {
                return "Select";
            }
        }

        public static string ConfigTCP
        {
            get
            {
                return "ConfigTCP";
            }
        }

        public static string PLCIP
        {
            get
            {
                return "PLCIP";
            }
        }

        public static string PLCPort
        {
            get
            {
                return "PLCPort";
            }
        }

        public static string YiBiaoIP
        {
            get
            {
                return "YiBiaoIP";
            }
        }

        public static string YiBiaoPort
        {
            get
            {
                return "YiBiaoPort";
            }
        }

        public static string Config485
        {
            get
            {
                return "Config485";
            }
        }
    }
}
