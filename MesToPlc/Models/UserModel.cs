using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MesToPlc.Models
{
    public class UserModel
    {
        private string userName;
        private string passWord;
        private string authority;

        /// <summary>
        /// 用户名
        /// </summary>
        public string UserName
        {
            get
            {
                return userName;
            }

            set
            {
                userName = value;
            }
        }

        /// <summary>
        /// 密码
        /// </summary>
        public string PassWord
        {
            get
            {
                return passWord;
            }

            set
            {
                passWord = value;
            }
        }

        /// <summary>
        /// 权限
        /// </summary>
        public string Authority
        {
            get
            {
                return authority;
            }

            set
            {
                authority = value;
            }
        }
    }
}
