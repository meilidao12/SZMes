using Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Script.Serialization;
using Newtonsoft.Json;
using System.Diagnostics;

namespace MesToPlc.Models
{
    public enum RequestResult
    {
       Success,
       Fail
    }

    public class MesLoad
    {
        public JavaScriptSerializer js = new JavaScriptSerializer();
        public MesLoadResult bdata = new MesLoadResult();
        IniHelper ini = new IniHelper(System.AppDomain.CurrentDomain.BaseDirectory + @"\Set.ini");
        public RequestResult GetData(out MesLoadResult mesLoadResult,string url)
        {
            mesLoadResult = null;
            try
            {
                string jsonData = Http.HttpGet(url);
                //string jsonData = ini.ReadIni("Config", "MesDebug");
                var data1 = JsonConvert.DeserializeObject<MesLoadResult>(jsonData);
                mesLoadResult = data1;
                if (mesLoadResult == null)
                {
                    return RequestResult.Fail;
                }
                return RequestResult.Success;
            }
            catch
            {
                return RequestResult.Fail;
            }
        }
    }
    public class MesLoadResult
    {
        //{"MaterialCode":"CDX6081212R3204","Message":"查询成功","SerialNumber":"1YHP000002A4402","Type":"S"}
        public string MaterialCode { get; set; }
        public string Message { get; set; }
        public string SerialNumber { get; set; }
        public string Type { get; set; }

    }
}
