using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Diagnostics;

namespace LiveMirror
{
    public class Config
    {
        Setttings settings;

        public Config()
        {
            settings = new Setttings();
        }
        public void Load(string filename)
        {
            try
            {
                if (File.Exists(filename))
                    settings.FromXML(File.ReadAllText(filename));
            }
            catch (Exception ex)
            {
                Debug.Fail(String.Format("Could not load settings file '{0}' : {1}", filename, ex.Message));
            }
        }
        public void Save(string filename)
        {
            try
            {
                File.WriteAllText(filename, settings.ToXML(), Encoding.UTF8);
            }
            catch (Exception ex)
            {
                Debug.Fail(String.Format("Could not save settings file '{0}' : {1}",filename,ex.Message));
            }
        }

        public string Get(string name)
        {
            string value;
            lock (settings)
            {
                value = settings[name];
            }
            return value;
        }
        public TResult Get<TResult>(string name, bool resetOnFail, TResult fallback)// where TResult : IConvertible
        {
            if (!settings.Contains(name))
                settings[name] = fallback.ToString();
            string value = Get(name);
            try
            {
                settings.SetTypeHint(name, typeof(TResult));

                //Convert enums with Enum.Parse, since Convert.ChangeType cant (it sucks)
                if (typeof(TResult).IsSubclassOf(typeof(Enum)))
                    return (TResult)Enum.Parse(typeof(TResult), value);

                return (TResult)Convert.ChangeType(value, typeof(TResult));
            }
            catch
            {
                if (resetOnFail)
                    Set(name, fallback.ToString());
                return fallback;
            }
        }
        public void Set(string name, object value)
        {
            Set(name, value.ToString());
        }
        public void Set(string name, string value)
        {
            lock (settings)
            {
                settings[name] = value;
            }
        }
        public void Remove(string name)
        {
            lock (settings) settings.Remove(name);
        }
    }
}