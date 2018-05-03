using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace sendXmlToAmon
{
    class Program
    {
        static string FilePath = @"\\192.168.0.33\Source\sendXmlToAmon\Amon\wfl.xml";
        static string CrEventsPath = @"C:\Users\gumbarm\Desktop\Amon\log_table_2017.xml";
        static string CrEventsPathMongular = @"C:\Users\gumbarm\Desktop\Amon\cr_audit_logs_2018.xml";
        static string EventFilePath = @"\\192.168.0.33\Source\sendXmlToAmon\Amon\wf_mon.xml";

        static void Main(string[] args)
        {
            //SendWorkflow();
            //SendEvent();
            //SendLogFile();
            //SendCrEvents();
            SendCrEventsToMongular();
            Console.ReadLine();
        }

        public static void SendLogFile()
        {
            var targetDirectory = @"C:\Users\hp_envy\Downloads\log\rcsappkiid02fp";
            string[] folderEntries = Directory.GetFileSystemEntries(targetDirectory);
            int counter = 50;
            foreach (string folderPath in folderEntries)
            {
                Console.WriteLine(folderPath);
                string[] fileEntries = Directory.GetFiles(folderPath);
                foreach (string filePath in fileEntries)
                {
                    if(!Path.GetFileName(filePath).Contains("2017"))
                    {
                        Console.WriteLine("---" + filePath);
                        SendFile(filePath, counter);
                        counter++;
                    }
                }
            }
        }

        public static void SendFile(string filePath, int counter)
        {
            string url = @"http://localhost/api/processFile/coreact/" + counter.ToString();
            var httpWebRequest = (HttpWebRequest)WebRequest.Create(url);
            //httpWebRequest.ContentType = "application/json; charset=utf-8";
            //httpWebRequest.Method = "POST";
            httpWebRequest.Accept = "application/json; charset=utf-8";

            httpWebRequest.ContentType = "text/json";
            httpWebRequest.Method = "POST";
            string json = "{\"jobName\":\"" + Path.GetFileName(filePath) + "\",\"startDate\":\"01/07/2017 00:00:01\",\"payload\":{\"filePath\":\"" + filePath.Replace("\\", "\\\\") + "\",}}";
            using (var streamWriter = new StreamWriter(httpWebRequest.GetRequestStream()))
            {
                streamWriter.Write(json);
                streamWriter.Flush();
                streamWriter.Close();
            }
            var httpResponse = (HttpWebResponse)httpWebRequest.GetResponse();
            Console.WriteLine(httpResponse.ToString());
        }

        public static void SendEvent()
        {
            XmlDocument doc = new XmlDocument();
            doc.Load(EventFilePath);
            foreach (XmlNode node in doc.DocumentElement.ChildNodes)
            {
                string text = node.InnerText; //or loop through its children as well
                foreach (XmlNode locNode in node)
                {
                    var wfMonId = locNode.Name + locNode.InnerText;
                    if (locNode.Name == "XML_PARAM")
                    {
                        try
                        {
                            SendXmlEvent(locNode.InnerText, node["WF_INST_ID"].InnerText);
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine("Exception: " + e.ToString());
                        }
                    }
                    Console.WriteLine(wfMonId);
                }
                Console.WriteLine("-----------------");
            }
        }

        public static void SendXmlEvent(string json, string ClientKey)
        {
            string url = @"http://localhost/api/event/co-react/" + ClientKey;
            var httpWebRequest = (HttpWebRequest)WebRequest.Create(url);
            //httpWebRequest.ContentType = "application/json; charset=utf-8";
            //httpWebRequest.Method = "POST";
            httpWebRequest.Accept = "application/json; charset=utf-8";

            httpWebRequest.ContentType = "text/json";
            httpWebRequest.Method = "POST";
            using (var streamWriter = new StreamWriter(httpWebRequest.GetRequestStream()))
            {
                streamWriter.Write(json);
                streamWriter.Flush();
                streamWriter.Close();
            }
            var httpResponse = (HttpWebResponse)httpWebRequest.GetResponse();
        }

        public static void SendCrEvents()
        {
            XmlDocument doc = new XmlDocument();
            Int64 counter = 0;
            doc.Load(CrEventsPath);
            Console.Write("Start: " + DateTime.Now.ToString());
            foreach (XmlNode node in doc.DocumentElement.ChildNodes)
            {
                var logMsg = node["LOG_TYP"].InnerText + "; " + node["CAT"].InnerText + "; " + node["MSG"].InnerText;
                //Console.WriteLine(logMsg);

                string url = @"http://localhost/api/cr/events/";

                var httpWebRequest = (HttpWebRequest)WebRequest.Create(url);
                //httpWebRequest.ContentType = "application/json; charset=utf-8";
                //httpWebRequest.Method = "POST";
                httpWebRequest.Accept = "application/json; charset=utf-8";

                httpWebRequest.ContentType = "text/json";
                httpWebRequest.Method = "POST";
                string json = "{\"process\":\"" + node["LOG_ID"].InnerText + "\",\"dateTime\":\"" + node["DTE"].InnerText + "\",\"data\":\"" + logMsg.Replace("\\", "\\\\") + "\"}";
                using (var streamWriter = new StreamWriter(httpWebRequest.GetRequestStream()))
                {
                    streamWriter.Write(json);
                    streamWriter.Flush();
                    streamWriter.Close();
                }
                try
                {
                    using (var httpResponse = (HttpWebResponse)httpWebRequest.GetResponse())
                    {

                    }

                    //if (httpResponse.ToString().Trim() != "")
                    //{
                    //    Console.WriteLine(counter.ToString() + ": " + httpResponse.ToString() + "=>" + logMsg);
                    //}
                    //Console.WriteLine(httpResponse.ToString());
                }
                catch (Exception e)
                {
                    counter++;
                    Console.Write("Error " + counter.ToString() + ":" + e.Message);
                }
            }
            Console.Write("End: " + DateTime.Now.ToString());
        }

        public static void SendCrEventsToMongular()
        {
            System.Net.ServicePointManager.Expect100Continue = false;
            XmlDocument doc = new XmlDocument();
            Int64 counter = 0;
            doc.Load(CrEventsPathMongular);
            Console.Write("Start: " + DateTime.Now.ToString());
            var serializerSettings = new JsonSerializerSettings()
            {
                StringEscapeHandling = StringEscapeHandling.EscapeHtml
            };
            foreach (XmlNode node in doc.DocumentElement.ChildNodes)
            {
                var logMsg = node["LOG_TYP"].InnerText + "; " + node["CAT"].InnerText + "; " + node["MSG"].InnerText;
                //Console.WriteLine(logMsg);

                string url = @"http://89.159.180.74:80/api/logs/LogCoreactEvents/";
                //string url = @"http://localhost:8000/api/logs/LogCoreactEvents/";

                var httpWebRequest = (HttpWebRequest)WebRequest.Create(url);
                //httpWebRequest.ContentType = "application/json; charset=utf-8";
                //httpWebRequest.Method = "POST";
                httpWebRequest.Accept = "application/json; charset=utf-8";

                httpWebRequest.ContentType = "text/json";
                httpWebRequest.Method = "POST";
                httpWebRequest.KeepAlive = false;
                //string json = "{\"logId\":\"" + node["LOG_ID"].InnerText + 
                //               "\",\"logType\":\"" + node["LOG_TYP"].InnerText +
                //               "\",\"category\":\"" + node["CAT"].InnerText +
                //               "\",\"date\":\"" + node["DTE"].InnerText +
                //               "\",\"userId\":\"" + node["USR_ID"].InnerText +
                //               "\",\"userName\":\"" + node["NME"].InnerText +
                //               "\",\"details\":\"" + node["DET"].InnerText.Replace("\\", "\\\\") +
                //               "\",\"message\":\"" + System.Net.WebUtility.HtmlEncode(node["MSG"].InnerText) +
                //               "\",\"entId\":\"" + node["ENT_ID"].InnerText +
                //               "\",\"entProdId\":\"" + node["ENT_PRO_ID"].InnerText +
                //                "\"}";
                //CoreactAuditLog jsonRequest = ;

                var json = JsonConvert.SerializeObject(new CoreactAuditLog{
                                                                            LogId = Convert.ToInt32(node["LOG_ID"].InnerText),
                                                                            LogTyp = String.IsNullOrEmpty(node["LOG_TYP"].InnerText) ? "" : node["LOG_TYP"].InnerText,
                                                                            Cat = String.IsNullOrEmpty(node["CAT"].InnerText) ? "" : node["CAT"].InnerText,
                                                                            UserId = String.IsNullOrEmpty(node["USR_ID"].InnerText) ? 0 : Convert.ToInt32(node["USR_ID"].InnerText),
                                                                            UserName = String.IsNullOrEmpty(node["NME"].InnerText) ? "" : node["NME"].InnerText,
                                                                            Dte = node["DTE"].InnerText,
                                                                            Det = String.IsNullOrEmpty(node["DET"].InnerText) ? "" : node["DET"].InnerText,
                                                                            Msg = String.IsNullOrEmpty(node["MSG"].InnerText) ? "" : node["MSG"].InnerText,
                                                                            EntId = String.IsNullOrEmpty(node["ENT_ID"].InnerText) ? 0 : Convert.ToInt32(node["ENT_ID"].InnerText),
                                                                            EndProdId = String.IsNullOrEmpty(node["ENT_PRO_ID"].InnerText) ? 0 : Convert.ToInt32(node["ENT_PRO_ID"].InnerText)
                                                                        });
                using (var streamWriter = new StreamWriter(httpWebRequest.GetRequestStream()))
                {
                    streamWriter.Write(json);
                    streamWriter.Flush();
                    streamWriter.Close();
                }
                try
                {
                    using (var httpResponse = (HttpWebResponse)httpWebRequest.GetResponse())
                    {
                        if (httpResponse.StatusCode.ToString() != "OK")
                        {
                            Console.WriteLine(counter.ToString() + ": " + node["LOG_ID"].InnerText);
                        }

                    }

                    //if (httpResponse.ToString().Trim() != "")
                    //{
                    //    Console.WriteLine(counter.ToString() + ": " + httpResponse.ToString() + "=>" + logMsg);
                    //}
                    //Console.WriteLine(httpResponse.ToString());
                }
                catch (Exception e)
                {
                    counter++;
                    Console.Write("Error " + node["LOG_ID"].InnerText + ":" + e.Message);
                }
            }
            Console.Write("End: " + DateTime.Now.ToString());
        }
        public static void SendWorkflow()
        {
            XmlDocument doc = new XmlDocument();
            doc.Load(FilePath);
            foreach (XmlNode node in doc.DocumentElement.ChildNodes)
            {
                string text = node.InnerText; //or loop through its children as well
                foreach (XmlNode locNode in node)
                {
                    var wfMonId = locNode.Name + locNode.InnerText;
                    if (locNode.Name == "XML_PARAM")
                    {
                        SendXmlWorkflow(locNode.InnerText, node["WF_INST_ID"].InnerText);
                    }
                    Console.WriteLine(wfMonId);
                }
                Console.WriteLine("-----------------");
            }
        }

        public static void SendXmlWorkflow(string json, string ClientKey)
        {
            string url = @"http://localhost/api/" + ClientKey;
            var httpWebRequest = (HttpWebRequest)WebRequest.Create(url);
            //httpWebRequest.ContentType = "application/json; charset=utf-8";
            //httpWebRequest.Method = "POST";
            httpWebRequest.Accept = "application/json; charset=utf-8";

            httpWebRequest.ContentType = "text/json";
            httpWebRequest.Method = "POST";
            using (var streamWriter = new StreamWriter(httpWebRequest.GetRequestStream()))
            {
                streamWriter.Write(json);
                streamWriter.Flush();
                streamWriter.Close();
            }
            var httpResponse = (HttpWebResponse)httpWebRequest.GetResponse();
        }
    }

    public class CoreactAuditLog
    {
        [JsonProperty("logId")]
        public int? LogId { get; set; }
        [JsonProperty("logType")]
        public string LogTyp { get; set; }
        [JsonProperty("category")]
        public string Cat { get; set; }
        [JsonProperty("userName")]
        public string UserName { get; set; }
        [JsonProperty("userId")]
        public int? UserId { get; set; }
        [JsonProperty("date")]
        public string Dte { get; set; }
        [JsonProperty("details")]
        public string Det { get; set; }
        [JsonProperty("message")]
        public string Msg { get; set; }
        [JsonProperty("entId")]
        public int? EntId { get; set; }
        [JsonProperty("entProdId")]
        public int? EndProdId { get; set; }
        [JsonProperty("application_name")]
        public string ApplicationName { get; set; }

    }
}
