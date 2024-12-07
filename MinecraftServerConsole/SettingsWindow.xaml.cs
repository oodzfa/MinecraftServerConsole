using MahApps.Metro.Controls;
using Microsoft.Win32;
using Newtonsoft.Json.Linq;
using System;
using System.IO;
using System.Windows;

namespace MinecraftServerConsole
{
    /// <summary>
    /// SettingsWindow.xaml 的交互逻辑
    /// </summary>
    public partial class SettingsWindow : MetroWindow
    {
        public SettingsWindow()
        {
            InitializeComponent();
            javaBox.Text = App.java_path;
            fileBox.Text = App.server_file;
            memBox.Text = App.mem;
        }

        private void javaButton_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Filter = "java可执行文件|java.exe";
            if (ofd.ShowDialog() == true)
            {
                javaBox.Text = ofd.FileName;
            }
        }

        private void MetroWindow_Closed(object sender, EventArgs e)
        {
            try
            {
                App.java_path = javaBox.Text;
                App.server_file = fileBox.Text;
                App.mem = memBox.Text;
                JObject obj = new JObject();
                obj["java_path"] = App.java_path;
                obj["server_file"] = App.server_file;
                obj["mem"] = App.mem;
                File.WriteAllText("config.json", obj.ToString());
            } catch {}
            App.settings_window_opened = false;
        }
    }
}
