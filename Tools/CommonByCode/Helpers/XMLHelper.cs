using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace Monitor.Common
{
    /// <summary>
    /// xml文件帮助类
    /// </summary>
    public class XMLHelper
    {
        /// <summary>
        /// xml文件对象
        /// </summary>
        public XmlDocument XmlDoc { get; private set; }

        /// <summary>
        /// XMLHelper实例化
        /// 参数：
        /// xmlFilePath文件路径
        /// </summary>
        /// <param name="xmlFilePath"></param>
        public XMLHelper(string xmlFilePath)
        {
            XmlDoc = new XmlDocument();
            XmlDoc.Load(xmlFilePath);
        }

        #region 读取XML文件Add节点指定Key的Value

        /// <summary>
        /// 读取指定路径的XML配置文件(node.key:node.value)
        /// </summary>
        /// <param name="key">配置文件中的键</param>
        public string GetString(string key)
        {
            XmlNodeList elementsByTagName = XmlDoc.GetElementsByTagName("add");

            foreach (XmlNode item in elementsByTagName)
            {
                if (item.Attributes["key"] != null
                    && item.Attributes["value"] != null
                    && string.Equals(item.Attributes["key"].Value, key))
                {
                    var text = item.Attributes["value"].Value;
                    return text;
                }
            }
            return default;
        }

        /// <summary>
        /// 读取指定路径的XML配置文件(node.key:node.value)
        /// </summary>
        /// <param name="key">配置文件中的键</param>
        public int GetIntValue(string key)
        {
            XmlNodeList elementsByTagName = XmlDoc.GetElementsByTagName("add");

            foreach (XmlNode item in elementsByTagName)
            {
                if (item.Attributes["key"] != null
                    && item.Attributes["value"] != null
                    && string.Equals(item.Attributes["key"].Value, key))
                {
                    var text = item.Attributes["value"].Value;
                    return int.Parse(text);
                }
            }
            return default;
        }

        /// <summary>
        /// 读取指定路径的XML配置文件(node.key:node.value)
        /// </summary>
        /// <param name="key">配置文件中的键</param>
        public bool GetBoolValue(string key)
        {
            XmlNodeList elementsByTagName = XmlDoc.GetElementsByTagName("add");

            foreach (XmlNode item in elementsByTagName)
            {
                if (item.Attributes["key"] != null
                    && item.Attributes["value"] != null
                    && string.Equals(item.Attributes["key"].Value, key))
                {
                    var text = item.Attributes["value"].Value;
                    return bool.Parse(text);
                }
            }
            return default;
        }

        #endregion

        /// <summary>
        /// 将xml文件保存为hashtable对象(node.Name:node.InnerText)
        /// </summary>
        /// <param name="path">文件路径</param>
        /// <returns>Hashtable</returns>
        public static Hashtable GetXmlSetting(string path)
        {
            Hashtable hashtable = new Hashtable();
            XmlDocument xmlDocument = new XmlDocument();
            xmlDocument.Load(path);
            foreach (XmlNode childNode in xmlDocument.SelectSingleNode("/Root").ChildNodes)
            {
                if (!childNode.Name.Equals("#comment"))
                {
                    hashtable[childNode.Name] = childNode.InnerText;
                }
            }
            return hashtable;
        }

        /// <summary>
        /// 将hashtable对象保存为xml文件(node.Name:node.InnerText)
        /// </summary>
        /// <param name="path">文件路径</param>
        /// <param name="hash">Hashtable</param>
        public static void SetXmlSetting(string path, Hashtable hash)
        {
            XmlDocument xmlDocument = new XmlDocument();
            xmlDocument.Load(path);
            XmlNode xmlNode = xmlDocument.SelectSingleNode("Root");
            xmlNode.RemoveAll();
            foreach (object key in hash.Keys)
            {
                XmlElement xmlElement = xmlDocument.CreateElement(key.ToString());
                xmlElement.InnerText = hash[key.ToString()].ToString();
                xmlNode.AppendChild(xmlElement);
            }
            xmlDocument.Save(path);
        }
    }
}
