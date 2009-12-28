using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using System.IO;
using System.Diagnostics;

namespace LiveMirror
{
    public class Setttings
    {
        Dictionary<string, string> settings = new Dictionary<string, string>();
        Dictionary<string, Type> typeHints = new Dictionary<string, Type>();
        public Dictionary<string, string> Settings { get { return settings; } }

        public string this[string key] 
        {
            get
            {
                if (settings.ContainsKey(key))
                    return settings[key];
                return "";
            }
            set
            {
                if (settings.ContainsKey(key))
                    settings[key] = value;
                else
                    settings.Add(key, value);
            }
        }

        public Setttings()
        { }

        public void FromXML(string xml)
        {
            XmlDocument doc = new XmlDocument();
            doc.LoadXml(xml);
            FromXML(doc);
        }
        public void FromXML(XmlDocument doc)
        {
            XmlNamespaceManager nameMgr = new XmlNamespaceManager(doc.NameTable);
            nameMgr.AddNamespace("x", "Xnet");
            XmlNodeList nodes = doc.SelectNodes("/x:Settings/x:Setting",nameMgr);
            if (nodes == null)
                return;
            foreach (XmlNode node in nodes)
            {
                XmlAttribute name = node.Attributes["Name"];
                XmlAttribute value = node.Attributes["Value"];
                if (name == null || value == null)
                    continue;
                if (settings.ContainsKey(name.Value))
                    settings[name.Value] = value.Value;
                else
                    settings.Add(name.Value, value.Value);
            }
        }
        public string ToXML()
        {
            XmlDocument doc = new XmlDocument();
            XmlNode root = doc.CreateElement("", "Settings", "Xnet");
            root.InnerText = "";
            doc.AppendChild(root);
            doc.InsertBefore(doc.CreateXmlDeclaration("1.0", "UTF-8", null), doc.DocumentElement);

            foreach (var setting in settings)
            {
                XmlNode node = doc.CreateElement("Setting", "Xnet");

                XmlAttribute name = doc.CreateAttribute("Name");
                name.Value = setting.Key;
                node.Attributes.Append(name);

                XmlAttribute value = doc.CreateAttribute("Value");
                value.Value = setting.Value;
                node.Attributes.Append(value);

                if (typeHints.ContainsKey(setting.Key))
                {
                    XmlAttribute typeHint = doc.CreateAttribute("TypeHint");
                    typeHint.Value = GetTypeHint(setting.Key).FullName;
                    node.Attributes.Append(typeHint);
                }

                root.AppendChild(node);
            }
            
            StringWriter sw = new StringWriter();
            XmlTextWriter xtw = new XmlTextWriter(sw);
            xtw.Formatting = Formatting.Indented;
            doc.WriteTo(xtw);
            return sw.ToString();
        }
        public bool Contains(string name)
        {
            return settings.ContainsKey(name);
        }
        public bool Remove(string name)
        {
            typeHints.Remove(name);
            return settings.Remove(name);
        }
        public Type GetTypeHint(string key)
        {
            if (typeHints.ContainsKey(key))
                return typeHints[key];
            else return typeof(string);
        }
        public void SetTypeHint(string key, Type type)
        {
            if (typeHints.ContainsKey(key))
                typeHints[key] = type;
            else
                typeHints.Add(key, type);
        }

    }
}
