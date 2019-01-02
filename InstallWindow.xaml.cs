using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.IO;
using System.Management;
using Newtonsoft.Json;
using System.Diagnostics;

namespace SKProCHLauncher
{
    /// <summary>
    /// Логика взаимодействия для InstallWindow.xaml
    /// </summary>
    public partial class InstallWindow : Window
    {
        private static InstallWindow form;
        private static bool ISAlreadyShown = false;
        public InstallWindow()
        {
            if (!ISAlreadyShown)
            {
                ISAlreadyShown = true;
                InitializeComponent();
                form = this;
                var usersSearcher = new ManagementObjectSearcher(@"SELECT * FROM Win32_UserAccount");
                var users = usersSearcher.Get();
                //List<string> ConfigList = new List<string>();
                foreach (var item in users)
                {
                    if (File.Exists($@"C:\Users\{Convert.ToString(item).Remove(Convert.ToString(item).Length - 1).Remove(0, Convert.ToString(item).Remove(Convert.ToString(item).Length - 1).LastIndexOf('"') + 1)}\AppData\Roaming\SKProCH's Launcher\MainConfig.json"))
                        OtherUsersCFG.Items.Add(Convert.ToString(item).Remove(Convert.ToString(item).Length - 1));
                }

                if (OtherUsersCFG.Items.Count == 0)
                {
                    IsNotFound.Visibility = Visibility.Visible;
                    IsFound.Visibility = Visibility.Collapsed;
                }
                else
                {
                    IsFound.Visibility = Visibility.Visible;
                    IsNotFound.Visibility = Visibility.Collapsed;
                }
                if (!this.IsVisible)
                {
                    this.Show();
                }
            }

        }

        private void ChooseFolder_Click(object sender, RoutedEventArgs e)
        {
            FolderBrowserDialog FB = new FolderBrowserDialog();
            if (FB.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                InstallPathBox.Text = FB.SelectedPath + @"\SKProCH's Laucnher\";
            }
        }

        private static bool Confurmed = false;
        private void Install_Click(object sender, RoutedEventArgs e)
        {
            if (!Confurmed)
            {
                Confurmed = true;
                Install.Content = "Нажмите для подтверждения установки";
            }
            else
            {
                //Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData)

                
                File.AppendAllLines(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData) + "SKProCH's Launcher - URL Handler.reg", new List<string>() {
                @"Windows Registry Editor Version 5.00".Replace('#', '"'),
                @"".Replace('#', '"'),
                @"[HKEY_CLASSES_ROOT\skplr]".Replace('#', '"'),
                @"@=#SKProCH's Launcher URL Handler#".Replace('#', '"'),
                @"#URL Protocol#=##".Replace('#', '"'),
                @"".Replace('#', '"'),
                @"[HKEY_CLASSES_ROOT\slx\DefaultIcon]".Replace('#', '"'),
                $"@=#{InstallPathBox.Text}\\SKProCH's Launcher.exe#".Replace('#', '"'),
                @"".Replace('#', '"'),
                @"[HKEY_CLASSES_ROOT\slx\shell]".Replace('#', '"'),
                @"".Replace('#', '"'),
                @"[HKEY_CLASSES_ROOT\slx\shell\open]".Replace('#', '"'),
                @"".Replace('#', '"'),
                @"[HKEY_CLASSES_ROOT\slx\shell\open\command]".Replace('#', '"'),
                @"@=#\#" + InstallPathBox.Text + "\\SKProCH's Launcher.exe# %1#".Replace('#', '"')
                });
                Process regeditProcess = Process.Start("regedit.exe", "/s \"" + Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData) + "SKProCH's Launcher - URL Handler.reg" + "\"");
                regeditProcess.WaitForExit();
                File.Delete(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData) + "SKProCH's Launcher - URL Handler.reg");
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog FB = new OpenFileDialog();
            if (FB.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                
            }
        }

        private void BOUNDCFG_Click(object sender, RoutedEventArgs e)
        {
            if(OtherUsersCFG.SelectedItem != null)
            {
                this.Hide();
                UserConfig tempcfg = JsonConvert.DeserializeObject<UserConfig>($@"C:\Users\{OtherUsersCFG.SelectedItem as string}\AppData\Roaming\SKProCH's Launcher\MainConfig.json");
                tempcfg.ReferenceToConfig = $@"C:\Users\{OtherUsersCFG.SelectedItem as string}\AppData\Roaming\SKProCH's Launcher\MainConfig.json";
                File.WriteAllText(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData) + @"SKProCH's Launcher\MainConfig.json", JsonConvert.SerializeObject(tempcfg));
                if (!File.Exists(tempcfg.InstallFolder + @"\SKProCH's Launcher.exe"))
                {
                    
                }
            }
        }

        private void ImportCFG_Click(object sender, RoutedEventArgs e)
        {
            if (OtherUsersCFG.SelectedItem != null)
            {
                this.Hide();
                File.Copy($@"C:\Users\{OtherUsersCFG.SelectedItem as string}\AppData\Roaming\SKProCH's Launcher\MainConfig.json", Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData) + @"SKProCH's Launcher\MainConfig.json");
                UserConfig tempcfg = JsonConvert.DeserializeObject<UserConfig>($@"C:\Users\{OtherUsersCFG.SelectedItem as string}\AppData\Roaming\SKProCH's Launcher\MainConfig.json");
                if(!File.Exists(tempcfg.InstallFolder + @"\SKProCH's Launcher.exe"))
                {
                    
                }
            }
        }
    }
}
