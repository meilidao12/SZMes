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
        private string weldTime;
        private string pressure;
        private string temperature;

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
        /// 焊接时间
        /// </summary>
        public string WeldTime
        {
            get
            {
                return weldTime;
            }

            set
            {
                weldTime = value;
            }
        }
    }
}
