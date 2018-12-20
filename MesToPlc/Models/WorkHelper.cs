using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MesToPlc.Models
{
    public class WorkHelper
    {
        private bool workFlag;
        /// <summary>
        /// 判断是不是开始工作水
        /// </summary>
        public bool WorkFlag
        {
            set
            {
                workFlag = value;
            }
            get
            {
                return workFlag;
            }
        }

        /// <summary>
        /// 判断是不是手动输入
        /// </summary>
        public bool HandInput
        {
            get
            {
                return handInput;
            }

            set
            {
                handInput = value;
            }
        }

        private bool handInput;

    }
}
