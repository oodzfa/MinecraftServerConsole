using Newtonsoft.Json.Linq;
using System;
using System.IO;
using System.Windows;

namespace MinecraftServerConsole
{
    /// <summary>
    /// App.xaml 的交互逻辑
    /// </summary>
    public partial class App : Application
    {
        public static string java_path { get; set; }
        public static string server_file { get; set; }
        public static string mem { get; set; }
        public static bool settings_window_opened { get; set; }

        public App()
        {
            try
            {
                if (!Directory.Exists("server"))
                {
                    Directory.CreateDirectory("server");
                }
                JObject config = JObject.Parse(File.ReadAllText("config.json"));
                java_path = config["java_path"].ToString();
                server_file = config["server_file"].ToString();
                mem = config["mem"].ToString();
            }
            catch { }
        }
    }
}
