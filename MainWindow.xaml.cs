using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Web;
using System.Net;
using System.IO;
using Newtonsoft.Json;
using System.Runtime.InteropServices;
using Ionic.Zip;

namespace SKProCHLauncher
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public class UserConfig
    {
        public string InstallFolder { get; set; }
        public string ReferenceToConfig { get; set; }
        public List<ModPack> Modpacks { get; set; } 
        public List<Account> Accounts { get; set; }
        public UserConfig() {
            Modpacks = new List<ModPack>();
            Accounts = new List<Account>();
        }
        #region WinApi
        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool DeleteFile(string lpFileName);
        #endregion 

        public class Account{
            public string ID { get; set; }
            public string NickName { get; set; }
            public string UUID { get; set; }

            public bool IsNeedToLogin { get; set; }

            public string Login { get; set; }
            public string Password { get; set; }
            public string AuthServer { get; set; }
        }
        
        public class ModPack{
            public bool IsLocalLauncher { get; set; }
            public bool IsOtherLauncher { get; set; }
            public string Path { get; set; }
            public string UpdatesURL { get; set; }
            public string CurrentVersion { get; set; }
            public Image Icon { get; set; }
            public string ID { get; set; }//Minecraft Version
            public string ForgeID { get; set; }//Forge Version
            public string ForgeURL { get; set; }//Forge version.json URL

            public string Name { get; set; }
            public string ProjectName { get; set; }

            private string NewVersion;
            private void CheckUpdates()
            {
                try
                {
                    WebClient wc = new WebClient();
                    wc.DownloadFile(UpdatesURL, Environment.GetFolderPath(Environment.SpecialFolder.InternetCache) + @"\SKProCH's Version File.tmp");
                    string VersionsContent = File.ReadAllText(Environment.GetFolderPath(Environment.SpecialFolder.InternetCache) + @"\SKProCH's Version File.tmp");
                    try
                    {
                        File.Delete(Environment.GetFolderPath(Environment.SpecialFolder.InternetCache) + @"\SKProCH's Version File.tmp");
                    }
                    catch (Exception)
                    {}

                    try
                    {
                        ModpackInfo MU = JsonConvert.DeserializeObject<ModpackInfo>(VersionsContent);
                        if (Convert.ToInt32((MU.Versions[MU.Versions.Count - 1].VersionNumber).Replace(".", "")) > Convert.ToInt32(CurrentVersion.Replace(".", "")))
                        {
                            NewVersion = (MU.Versions[MU.Versions.Count - 1].VersionNumber);
                            //

                            //Построение общих различий
                            List<ModpackInfo.Version.Download> ToDownload = new List<ModpackInfo.Version.Download>(); //Финальные файлы
                            List<ModpackInfo.Version.Delete> ToDelete = new List<ModpackInfo.Version.Delete>();

                            foreach (var item in MU.Versions)
                            {
                                if (Convert.ToInt32((item.VersionNumber).Replace(".", "")) > Convert.ToInt32(CurrentVersion.Replace(".", "")))
                                {
                                    foreach (var deleteitem in item.ListToDelete)
                                    {
                                        for (int i = 0; i < ToDownload.Count; i++)
                                        {
                                            if (deleteitem.Path == ToDownload[i].Path && !ToDownload[i].IsArchive)
                                            {
                                                ToDownload.RemoveAt(i);
                                            }
                                        }
                                    }

                                    ToDownload.AddRange(item.ListToDownload);
                                    ToDelete.AddRange(item.ListToDelete);
                                }
                                else break;
                            }

                            //Удаление всего
                            foreach (var item in ToDelete)
                            {
                                if (item.Path.Remove(6) == "%BASE%" && File.Exists(item.Path.Replace("%BASE%", Path)))
                                {
                                    DeleteFile(item.Path.Replace("%BASE%", Path));
                                }
                            }

                            WebClient WC = new WebClient();
                            WC.DownloadFileCompleted += WC_DownloadFileCompleted;
                            foreach (var item in ToDownload)
                            {
                                WC.DownloadFileAsync(new Uri(item.URL), item.Path);
                                lock(locker){
                                    ActiveDownloadCount++;
                                }

                                if (item.IsArchive)
                                    ZipArchieves.Add(item);
                            }
                            DownLoadsScheduled = true;
                        }
                    }
                    catch (Exception)
                    {
                        //
                    }
                }
                catch (Exception)
                {}
            }

            private bool DownLoadsScheduled = false;
            private object locker = new object();
            private int ActiveDownloadCount = 0;
            private List<ModpackInfo.Version.Download> ZipArchieves = new List<ModpackInfo.Version.Download>();

            private void WC_DownloadFileCompleted(object sender, System.ComponentModel.AsyncCompletedEventArgs e)
            {
                lock(locker){
                    ActiveDownloadCount--;
                }
                if (ActiveDownloadCount == 0 && DownLoadsScheduled) //Заканчиваем скачивать всё
                {
                    foreach (var item in ZipArchieves)//Распаковываем
                    {
                        using (ZipFile zip = ZipFile.Read(item.Path))
                        {
                            foreach (ZipEntry y in zip)
                            {
                                y.Extract(Path);
                            }
                        }
                        File.Delete(item.Path);
                    }
                    CurrentVersion = NewVersion;
                }
            }
        }
    }

    public class ModpackInfo{
        public string ModpackName { get; set; }
        public string ModpackServer { get; set; }
        public string URL { get; set; }
        public Image Icon { get; set; }
        public string ID { get; set; }//Minecraft Version
        public string ForgeID { get; set; }//Forge Version
        public string ForgeURL { get; set; }//Forge version.json URL
        
        public List<Version> Versions { get; set; }
        public ModpackInfo(){
            Versions = new List<Version>();
        }

        public class Version{
            public string VersionNumber { get; set; }

            public List<Delete> ListToDelete { get; set; }
            public List<Download> ListToDownload { get; set; }
            public Version(){
                ListToDelete = new List<Delete>();
                ListToDownload = new List<Download>();
            }

            public class Delete{
                public string Path { get; set; }
            }
            public class Download{
                public string Path { get; set; }
                public string URL { get; set; }
                public bool IsArchive{ get; set; }
            }
        }
    }



    public partial class MainWindow : Window
    {
        private static MainWindow form = new MainWindow();
        public static UserConfig CFG;
        public MainWindow()
        {
            InitializeComponent();
            form = this;
            InitializeIcons();
            /*if (File.Exists(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData) + @"SKProCH's Launcher\MainConfig.json"))
            {
                CFG = JsonConvert.DeserializeObject<UserConfig>(File.ReadAllText(Environment.SpecialFolder.CommonApplicationData + @"SKProCH's Launcher\MainConfig.json"));
                string FinalReference = "";
                while (CFG.ReferenceToConfig != null)
                {
                    if (File.Exists(CFG.ReferenceToConfig))
                    {
                        FinalReference = CFG.ReferenceToConfig;
                        CFG = JsonConvert.DeserializeObject<UserConfig>(File.ReadAllText(CFG.ReferenceToConfig));
                    }
                    else
                        break;
                }

                if (FinalReference != "")
                {
                    File.Copy(FinalReference, Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData) + @"SKProCH's Launcher\MainConfig.json");
                }
            }
            else
            {
                try{
                    //this.Close();
                }
                catch (Exception)
                {}
            }*/
        }

        //Добавить модпак
        private void Rectangle_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            
        }

        #region Icons Class
        public class IconSet
        {
            public Dictionary<string, string> iconsset { get; set; }
            public IconSet()
            {
                iconsset = new Dictionary<string, string>();
            }
        }
        #endregion 
        #region ListBoxIcons Class
        class ListBoxIcons{
            public string IconPath { get; set; }
            public string IconName { get; set; }
        }
        #endregion

        public static void InitializeIcons()
        {
            IconSet Default;
            IconSet Custom;
            using (WebClient wc = new WebClient())
            {
                string JSON = wc.DownloadString(@"https://gdurl.com/u9FJ");
                Default = JsonConvert.DeserializeObject<IconSet>(JSON);
            }
            Custom = new IconSet();

            /*var CustomIconsPath = Directory.GetFiles(CFG.InstallFolder + @"\Icons\CustonIcons\");
            foreach (var item in CustomIconsPath)
            {
                Custom.iconsset.Add(item, System.IO.Path.GetFileNameWithoutExtension(item));
            }*/

            form.Dispatcher.Invoke(new Action(() => {
                form.DefaultIconsListBox.Items.Clear();
                form.CustomIconsListBox.Items.Clear();

                form.AllIcons.Visibility = Visibility.Visible;

                foreach (var defitem in Default.iconsset)
                {
                    form.DefaultIconsListBox.Items.Add(new ListBoxIcons() { IconName = defitem.Value, IconPath = defitem.Key });
                }
                foreach (var cusitem in Custom.iconsset)
                {
                    form.CustomIconsListBox.Items.Add(new ListBoxIcons() { IconName = cusitem.Value, IconPath = cusitem.Key });
                }
                form.CustomIconsListBox.Items.Add(new ListBoxIcons() { IconPath = @"https://gdurl.com/M8Ps", IconName = "Add icon" });
            } ));

            

        }

        private void DefaultIconsListBox_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            CustomIconsListBox.SelectedIndex = -1;
            if (e.ClickCount == 2)
                CloseIconsForm((DefaultIconsListBox.SelectedItem as ListBoxIcons).IconPath, (DefaultIconsListBox.SelectedItem as ListBoxIcons).IconName, false);
        }

        private void CustomIconsListBox_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            DefaultIconsListBox.SelectedIndex = -1;
            if (e.ClickCount >= 2)
            {
                if (CustomIconsListBox.SelectedIndex != CustomIconsListBox.Items.Count - 1)
                    CloseIconsForm((DefaultIconsListBox.SelectedItem as ListBoxIcons).IconPath, (DefaultIconsListBox.SelectedItem as ListBoxIcons).IconName, false);
                else
                    AddCustomIcon();
            }
        }

        private static void CloseIconsForm(string URL, string NAME, bool IsCustom)
        {
            form.AllIcons.Visibility = Visibility.Collapsed;

            if (IsCustom)
            {
                //
            }
            else
            {
                using (WebClient wc = new WebClient())
                {
                    wc.DownloadFile(URL, CFG.InstallFolder + @"\Icons\DefaultIcons\" + NAME + "." + URL.Remove(0, URL.LastIndexOf("." + 1)));
                }
            }
            //
        }

        private static void AddCustomIcon()
        {
            form.AllIcons.Visibility = Visibility.Collapsed;
            form.AddCustomIconForm.Visibility = Visibility.Visible;
            form.IconName.Text = "";
            form.IconPath.Text = "";
        }

        //Добавление новой иконки
        private void AddCustomIconButton_Click(object sender, RoutedEventArgs e)
        {
            form.AddCustomIconForm.Visibility = Visibility.Collapsed;
            try
            {
                File.Copy(form.IconPath.Text, CFG.InstallFolder + @"\Icons\CustomIcons\" + form.IconName);
            }
            catch (Exception)
            {
                try
                {
                    using (WebClient wc = new WebClient())
                    {
                        wc.DownloadFile(form.IconPath.Text, CFG.InstallFolder + @"\Icons\CustomIcons\" + form.IconName);
                    }
                }
                catch (Exception)
                {
                    MessageBox.Show("Упс, что-то пошло не так. Проверьте путь к файлу!");
                }
            }
        }

        private void CheckInstallAvailable(object sender, TextChangedEventArgs e)
        {
            if (IconName.Text != "" && IconPath.Text != "")
            {
                AddCustomIconButton.IsEnabled = true;
            }
            else
                AddCustomIconButton.IsEnabled = false;
        }
    }
}
