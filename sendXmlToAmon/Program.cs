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
        static string EventFilePath = @"\\192.168.0.33\Source\sendXmlToAmon\Amon\wf_mon.xml";

        static void Main(string[] args)
        {
            //SendWorkflow()
            //SendEvent();
            SendLogFile();
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
            string url = @"http://localhost:60864/api/processFile/coreact/" + counter.ToString();
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
            string url = @"http://localhost:60864/api/event/co-react/" + ClientKey;
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
            string url = @"http://localhost:60864/api/" + ClientKey;
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
}
