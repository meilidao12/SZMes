using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MesToPlc.Models
{
    /// <summary>
    /// 仪表参数类
    /// </summary>
    public class InstrumentParameters
    {
        private string current;
        private string voltage;
        private string cpk;
        private string pressure;
        private string temperature;
        private string heat;

        /// <summary>
        /// 电流
        /// </summary>
        public string Current
        {
            get
            {
                return current;
            }

            set
            {
                current = value;
            }
        }

        /// <summary>
        /// 电压
        /// </summary>
        public string Voltage
        {
            get
            {
                return voltage;
            }

            set
            {
                voltage = value;
            }
        }

        /// <summary>
        /// CPK
        /// </summary>
        public string Cpk
        {
            get
            {
                return cpk;
            }

            set
            {
                cpk = value;
            }
        }

        /// <summary>
        /// 压力
        /// </summary>
        public string Pressure
        {
            get
            {
                return pressure;
            }

            set
            {
                pressure = value;
            }
        }

        /// <summary>
        ///  温度
        /// </summary>
        public string Temperature
        {
            get
            {
                return temperature;
            }

            set
            {
                temperature = value;
            }
        }

        /// <summary>
        /// 热量
        /// </summary>
        public string Heat
        {
            get
            {
                return heat;
            }

            set
            {
                heat = value;
            }
        }
    }
}
