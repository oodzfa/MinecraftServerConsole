using MahApps.Metro.Controls;
using MahApps.Metro.Controls.Dialogs;
using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Windows;
using System.Windows.Threading;

namespace MinecraftServerConsole
{
    public partial class MainWindow : MetroWindow
    {
        SettingsWindow settingsWindow;
        private Process process = new Process();

        public MainWindow()
        {
            InitializeComponent();
        }

        private void StandardOutput()
        {
            try
            {
                using (StreamReader reader = process.StandardOutput)
                {
                    string line;
                    while ((line = reader.ReadLine()) != null)
                    {
                        Dispatcher.Invoke(() =>
                        {
                            outputBox.AppendText(line + "\n");
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                Dispatcher.Invoke(() => outputBox.AppendText($"无法读取标准输出流: {ex.Message}\n"));
            }
        }

        private void StandardError()
        {
            try
            {
                using (StreamReader reader = process.StandardError)
                {
                    string line;
                    while ((line = reader.ReadLine()) != null)
                    {
                        Dispatcher.Invoke(() =>
                        {
                            outputBox.AppendText(line + "\n");
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                Dispatcher.Invoke(() => outputBox.AppendText($"无法读取标准错误流: {ex.Message}\n"));
            }
        }

        private void StartServer()
        {
            try
            {
                process.StartInfo.WorkingDirectory = "server";
                process.StartInfo.FileName = App.java_path;
                process.StartInfo.Arguments = "-Xms" + App.mem + "M -Xmx" + App.mem + "M -jar " + App.server_file + " --nogui";
                process.StartInfo.UseShellExecute = false;
                process.StartInfo.RedirectStandardInput = true;
                process.StartInfo.RedirectStandardOutput = true;
                process.StartInfo.RedirectStandardError = true;
                process.StartInfo.CreateNoWindow = true;
                process.Start();
                new Thread(StandardOutput).Start();
                new Thread(StandardError).Start();
                process.WaitForExit();
                Dispatcher.Invoke(() => outputBox.AppendText($"服务器已关闭，退出码: {process.ExitCode}\n"));
            }
            catch (Exception ex)
            {
                try {process.Kill();} catch {}
                Dispatcher.Invoke(() => outputBox.AppendText($"错误: {ex.Message}\n"));
            }
            process.Close();
            Dispatcher.Invoke(() =>
            {
                stopButton.IsEnabled = false;
                execButton.IsEnabled = false;
                startButton.IsEnabled = true;
            });
        }

        private async void startButton_Click(object sender, RoutedEventArgs e)
        {
            if (App.java_path == "" || App.server_file == "" || App.mem == "")
            {
                await this.ShowMessageAsync("提示", "请填写完整信息");
                return;
            }
            if (!File.Exists(App.java_path))
            {
                await this.ShowMessageAsync("提示", "java不存在");
                return;
            }
            if (!File.Exists("server\\" + App.server_file))
            {
                await this.ShowMessageAsync("提示", "服务器文件不存在");
                return;
            }
            startButton.IsEnabled = false;
            stopButton.IsEnabled = true;
            execButton.IsEnabled = true;
            outputBox.Clear();
            new Thread(StartServer).Start();
        }

        private void stopButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                process.StandardInput.WriteLine("stop");
                process.StandardInput.Flush();
            }
            catch (Exception ex)
            {
                this.ShowMessageAsync("错误", $"无法发送停止命令:\n{ex.Message}");
            }
        }
        private void settingsButton_Click(object sender, RoutedEventArgs e)
        {
            if (!App.settings_window_opened)
            {
                App.settings_window_opened = true;
                settingsWindow = new SettingsWindow();
                settingsWindow.Show();
            }
            else
            {
                try { settingsWindow.Activate(); } catch {}
            }
        }

        private void execButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                outputBox.AppendText($"> {inputBox.Text}\n");
                process.StandardInput.WriteLine(inputBox.Text);
                process.StandardInput.Flush();
                inputBox.Text = "";
            }
            catch (Exception ex)
            {
                this.ShowMessageAsync("错误", $"无法发送命令:\n{ex.Message}");
            }
        }

        private void MetroWindow_Closed(object sender, System.EventArgs e)
        {
            try { process.Kill(); } catch {}
            try { settingsWindow.Close(); } catch {}
        }

    }
}
