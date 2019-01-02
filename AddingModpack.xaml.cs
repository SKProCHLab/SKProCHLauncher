using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Net;
using Newtonsoft.Json;
using System.IO;
using System.Threading;
using System.Windows.Forms;

namespace SKProCHLauncher
{
    /// <summary>
    /// Логика взаимодействия для AddingModpack.xaml
    /// </summary>
    
    public class ToAddModpacks{
        public List<string> URLs { get; set; }
        public ToAddModpacks(){
            URLs = new List<string>();
        }
    }

    public partial class AddingModpack : Window
    {
        List<UserConfig.ModPack> addmodpacks = new List<UserConfig.ModPack>();
        private AddingModpack form;
        public AddingModpack(string URL)
        {
            InitializeComponent();
            form = this;
            if (URL == null || URL == "")
            {
                PasteURLHere.Visibility = Visibility.Visible;
            }
            else
            {

            }
        }

        private void URLBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            SubmitURL.Visibility = Visibility.Visible;
        }

        private void SubmitURL_Click(object sender, RoutedEventArgs e)
        {
            PasteURLHere.Visibility = Visibility.Collapsed;
            WebClient wc = new WebClient();
            try
            {
                wc.DownloadFile(URLBox.Text, Environment.GetFolderPath(Environment.SpecialFolder.InternetCache) + @"\SKProCH's Add Modpack File.tmp");
                ToAddModpacks temp = JsonConvert.DeserializeObject<ToAddModpacks>(File.ReadAllText(Environment.GetFolderPath(Environment.SpecialFolder.InternetCache) + @"\SKProCH's Add Modpack File.tmp"));
                File.Delete(Environment.GetFolderPath(Environment.SpecialFolder.InternetCache) + @"\SKProCH's Add Modpack File.tmp");
                for (int i = 0; i < temp.URLs.Count; i++)
                {
                    WebClient wc1 = new WebClient();
                    wc1.DownloadFile(temp.URLs[i], Environment.GetFolderPath(Environment.SpecialFolder.InternetCache) + @"\SKProCH's Add Modpack File.tmp");

                    ModpackInfo MP = JsonConvert.DeserializeObject<ModpackInfo>(File.ReadAllText(Environment.GetFolderPath(Environment.SpecialFolder.InternetCache) + @"\SKProCH's Add Modpack File.tmp"));
                    addmodpacks.Add(new UserConfig.ModPack() {
                        //CurrentVersion = MP.Versions[]
                        ForgeID = MP.ForgeID,
                        ID = MP.ID,
                        ForgeURL = MP.ForgeURL,
                        IsLocalLauncher = true,
                        Name = MP.ModpackName,
                        IsOtherLauncher = false,
                        CurrentVersion = "0",
                        Icon = MP.Icon,
                        ProjectName = MP.ModpackServer,
                        UpdatesURL = MP.URL
                    });
                    File.Delete(Environment.GetFolderPath(Environment.SpecialFolder.InternetCache) + @"\SKProCH's Add Modpack File.tmp");
                }

            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"Ошибка - {ex.Message}, обратитесь к тому, кто давал вам ссылку");
                this.Close();
            }
        }

        private void CancelAddingModpack_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void DeleteModPackOnSetup_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            Thread.Sleep(TimeSpan.FromMilliseconds(250));
            ModpacksListToAdd.Items.RemoveAt(ModpacksListToAdd.SelectedIndex);
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Thread.Sleep(TimeSpan.FromMilliseconds(200));
            int selectedindex = ModpacksListToAdd.SelectedIndex;
            FolderBrowserDialog FB = new FolderBrowserDialog();
            if (FB.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                (ModpacksListToAdd.Items[selectedindex] as UserConfig.ModPack).IsLocalLauncher = false;
                (ModpacksListToAdd.Items[selectedindex] as UserConfig.ModPack).IsOtherLauncher = true;
                (ModpacksListToAdd.Items[selectedindex] as UserConfig.ModPack).Path = FB.SelectedPath;
            }
        }
    }
}
