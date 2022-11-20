using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text.RegularExpressions;
using System.Windows.Input;
using static OpenVR2OSC.MainWindow;

namespace OpenVR2OSC
{
    static class MainModel
    {
        #region bindings
        public static readonly string CONFIG_DEFAULT = "default";
        private static readonly object _bindingsLock = new object();
        public static List<BindingItem> _bindings { get; set; } = new List<BindingItem>();
        
        /**
         * Store key codes as virtual key codes.
         */
        static public void RegisterBinding(string actionKey, HashSet<Key> keys)
        {
            //var keysArr = new Key[keys.Count];
            //keys.CopyTo(keysArr);
            //var binding = MainUtils.ConvertKeys(keysArr);
            //lock (_bindingsLock)
           //{
           //     _bindings[actionKey] = binding;
           //     var config = new Dictionary<string, Key[]>();
           //     foreach (var key in _bindings.Keys)
           //     {
           //         config.Add(key, _bindings[key].Item1);
           //     }
           //     StoreConfig(config);
           // }
        }
        static private void RegisterBindings(List<BindingItem> config)
        {
            lock (_bindingsLock) { 
                _bindings = config;
                //return config;
            }
        }
        static public bool BindingExists(string actionKey)
        {
            lock (_bindingsLock)
           {
                return true;//_bindings.ContainsKey(actionKey);
           }
        }
        public static string GetBinding(string actionkey)
        {
            lock (_bindingsLock)
            {
                return actionkey;
            }
        }
        static public void ClearBindings()
        {
            lock (_bindingsLock)
            {
                _bindings.Clear();
            }
            StoreConfig();
        }
        static public void RemoveBinding(BindingItem actionKey)
        {
            lock (_bindingsLock)
            {
                _bindings.Remove(actionKey);
                var config = new List<BindingItem>();
                foreach (var key in _bindings)
                {
                    config.Add(key);
                }
                StoreConfig(config);
            }
        }
        #endregion

        #region config
        static private string _configName = CONFIG_DEFAULT;

        static public void SetConfigName(string configName)
        {
            CleanConfigName(ref configName);
            _configName = configName;
        }

        static public bool IsDefaultConfig()
        {
            return _configName == CONFIG_DEFAULT;
        }

        static private void CleanConfigName(ref string configName)
        {
            Regex rgx = new Regex(@"[^a-zA-Z0-9\.]");
            var cleaned = rgx.Replace(configName, String.Empty).Trim(new char[] { '.' });
            configName = cleaned == String.Empty ? CONFIG_DEFAULT : cleaned;
        }

        static public string GetConfigFolderPath()
        {
            return $"{Directory.GetCurrentDirectory()}\\config\\";
        }

        static public void StoreConfig(List<BindingItem> config = null, string configName = null)
        {
            //_bindings.Add("Tester123");
            //_bindings.Add("L4helloworld");
            if (config == null)
            {
                config = new List<BindingItem>();
                lock (_bindingsLock)
                {
                    foreach (var key in _bindings)
                    {
                        config.Add(key);
                    }
                }
            }
            if (configName == null) configName = _configName;
            var jsonString = JsonConvert.SerializeObject(config);
            var configDir = GetConfigFolderPath();
            var configFilePath = $"{configDir}{configName}.json";
            if (!Directory.Exists(configDir)) Directory.CreateDirectory(configDir);
            File.WriteAllText(configFilePath, jsonString);
        }


        static public void DeleteConfig(string configName = null)
        {
            if (configName == null) configName = _configName;
            var configDir = GetConfigFolderPath();
            var configFilePath = $"{configDir}{configName}.json";
            if(File.Exists(configFilePath))
            {
                File.Delete(configFilePath);
                _configName = CONFIG_DEFAULT;
            }
        }

        static public List<BindingItem> RetrieveConfig(string configName = null)
        {
            if (configName == null) configName = _configName;
            CleanConfigName(ref configName);
            var configDir = $"{Directory.GetCurrentDirectory()}\\config\\";
            var configFilePath = $"{configDir}{configName}.json";
            var jsonString = File.Exists(configFilePath) ? File.ReadAllText(configFilePath) : null;
            if (jsonString != null)
            {
                Debug.WriteLine(jsonString);
                var config = JsonConvert.DeserializeObject(jsonString, typeof(List<BindingItem>)) as List<BindingItem>;
                RegisterBindings(config);
                //Debug.WriteLine("Config: " + config);
                return config;
            }
            return null;
        }
        #endregion

        #region Settings
        public enum Setting
        {
            Minimize, Tray, Notification, Haptic, ExitWithSteam
        }

        private static readonly Properties.Settings p = Properties.Settings.Default;

        static public void UpdateSetting(Setting setting, bool value)
        {
            var propertyName = Enum.GetName(typeof(Setting), setting);
            p[propertyName] = value;
            p.Save();
        }

        static public bool LoadSetting(Setting setting)
        {
            var propertyName = Enum.GetName(typeof(Setting), setting);
            return (bool)p[propertyName];
        }

        static public string GetVersion()
        {
            return (string)Properties.Resources.Version;
        }
        #endregion

    }
}
