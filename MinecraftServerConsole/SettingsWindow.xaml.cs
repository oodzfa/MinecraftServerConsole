using MahApps.Metro.Controls;
using MahApps.Metro.Controls.Dialogs;
using Microsoft.Win32;
using Newtonsoft.Json.Linq;
using System;
using System.Diagnostics;
using System.IO;
using System.Windows;

namespace MinecraftServerConsole
{
    /// <summary>
    /// SettingsWindow.xaml 的交互逻辑
    /// </summary>
    public partial class SettingsWindow : MetroWindow
    {
        private class addon
        {
            public string file_name { get; set; }
            public string type { get; set; }
        }

        private void updataAddonGrid()
        {
            try
            {
                string[] files = Directory.GetFiles("server\\plugins");
                foreach (string file in files)
                {
                    addonGrid.Items.Add(new addon { file_name = file, type = "plugin" });
                }
            }
            catch { }
            try
            {
                string[] files = Directory.GetFiles("server\\mods");
                foreach (string file in files)
                {
                    addonGrid.Items.Add(new addon { file_name = file, type = "mod" });
                }
            }
            catch { }
        }

        public SettingsWindow()
        {
            InitializeComponent();
            javaBox.Text = App.java_path;
            fileBox.Text = App.server_file;
            memBox.Text = App.mem;
            updataAddonGrid();
        }

        private void javaButton_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog
            {
                Filter = "java可执行文件|java.exe"
            };
            if (ofd.ShowDialog() == true)
            {
                javaBox.Text = ofd.FileName;
            }
        }

        private void importPluginButton_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog
            {
                Filter = "jar文件|*.jar"
            };
            if (ofd.ShowDialog() == true)
            {
                try
                {
                    File.Copy(ofd.FileName, "server\\plugins\\" + ofd.SafeFileName);
                    updataAddonGrid();
                }
                catch (Exception ex)
                {
                    this.ShowMessageAsync("导入插件失败", ex.Message);
                }
            }
        }

        private void importModButton_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog
            {
                Filter = "jar文件|*.jar"
            };
            if (ofd.ShowDialog() == true)
            {
                try
                {
                    File.Copy(ofd.FileName, "server\\mods\\" + ofd.SafeFileName);
                    updataAddonGrid();
                }
                catch (Exception ex)
                {
                    this.ShowMessageAsync("导入mod失败", ex.Message);
                }
            }
        }

        private void delAddonButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                File.Delete(((addon) addonGrid.SelectedItem).file_name);
                addonGrid.Items.Remove(addonGrid.SelectedItem);
            }
            catch (Exception ex)
            {
                this.ShowMessageAsync("组件删除失败", ex.Message);
            }
        }

        private void MetroWindow_Closed(object sender, EventArgs e)
        {
            try
            {
                App.java_path = javaBox.Text;
                App.server_file = fileBox.Text;
                App.mem = memBox.Text;
                JObject obj = new JObject
                {
                    ["java_path"] = App.java_path,
                    ["server_file"] = App.server_file,
                    ["mem"] = App.mem
                };
                File.WriteAllText("config.json", obj.ToString());
            } catch {}
            App.settings_window_opened = false;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Process process = new Process();
                process.StartInfo.FileName = "cmd";
                process.StartInfo.Arguments = "/c start server\\server.properties";
                process.Start();
                process.Close();
            }
            catch (Exception ex)
            {
                this.ShowMessageAsync("打开properties文件失败", ex.Message);
            }
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            try
            {
                Process process = new Process();
                process.StartInfo.FileName = "explorer";
                process.StartInfo.Arguments = "server";
                process.Start();
                process.Close();
            }
            catch (Exception ex)
            {
                this.ShowMessageAsync("无法打开服务器目录", ex.Message);
            }
        }
    }
}
