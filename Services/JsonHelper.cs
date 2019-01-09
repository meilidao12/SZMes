using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Services
{
    public class JsonHelper
    {
        IniHelper ini = new IniHelper(System.AppDomain.CurrentDomain.BaseDirectory + @"\Set.ini");
        string jsonpath = "";
        //构成配置文件路径 
        //static string  jsonpath =  serverAppPath + "config.json";

        public JsonHelper()
        {
            jsonpath = ini.ReadIni("Config", "JsonUrl");
        }

        /// <summary>
        /// 追加写入
        /// </summary>
        /// <typeparam name="Model"></typeparam>
        /// <param name="model"></param>
        /// <returns></returns>
        public  bool AppendWrite<Model>(string name ,Model model)
        {
            string path = jsonpath + name;
            string js1 = JsonConvert.SerializeObject(model);
            if (!File.Exists(path))
            {
                File.Create(path).Close();
            }
            File.AppendAllText(path, js1);
            return true;
        }

        public  bool Write<Model>(Model model)
        {
            string js1 = JsonConvert.SerializeObject(model);
            if (!File.Exists(jsonpath))
            {
                File.Create(jsonpath).Close();
            }
            File.WriteAllText(jsonpath, js1);
            ////把模型数据写到文件 
            //using (StreamWriter sw = new StreamWriter(jsonpath))
            //{
            //    JsonSerializer serializer = new JsonSerializer();
            //    serializer.Converters.Add(new JavaScriptDateTimeConverter());
            //    serializer.NullValueHandling = NullValueHandling.Ignore;
            //    //构建Json.net的写入流 
            //    JsonWriter writer = new JsonTextWriter(sw);
            //    //把模型数据序列化并写入Json.net的JsonWriter流中 

            //    serializer.Serialize(writer, model);
            //    //ser.Serialize(writer, ht); 
            //    writer.Close();
            //    sw.Close();
            //}
            return true;
        }

    }
}
