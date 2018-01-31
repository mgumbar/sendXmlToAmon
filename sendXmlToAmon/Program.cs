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
        static string FilePath = @"C:\Users\gumbarm\Desktop\Amon\wf_xml_one.xml";

        static void Main(string[] args)
        {
            XmlDocument doc = new XmlDocument();
            doc.Load(FilePath);
            foreach (XmlNode node in doc.DocumentElement.ChildNodes)
            {
                string text = node.InnerText; //or loop through its children as well
                foreach (XmlNode locNode in node)
                {
                    var wfMonId = locNode.Name + locNode.InnerText ;
                    if (locNode.Name == "XML_PARAM")
                    {
                        SendXml(locNode.InnerText);
                    }
                    Console.WriteLine(wfMonId);
                }
                Console.WriteLine("-----------------");

            }

            Console.ReadLine();
        }

        public static void SendXml(string json)
        {
            string url = @"http://192.168.0.27:60864/api/amon/postJson/coreact";
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
