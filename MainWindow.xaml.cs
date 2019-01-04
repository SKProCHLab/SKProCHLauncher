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
using System.Web;
using System.Net;
using System.IO;
using Newtonsoft.Json;
using System.Runtime.InteropServices;
using Ionic.Zip;
using Microsoft.Win32;
using System.Diagnostics;
using System.Windows.Shapes;
using System.Windows.Media.Animation;

namespace SKProCHLauncher
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    #region ConfigClass
    public class UserConfig
    {
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
    #endregion


    #region ModpackInfo
    public class ModpackInfo{
        public string ModpackName { get; set; }
        public string ModpackServer { get; set; }
        public string URL { get; set; }
        public string Icon { get; set; }
        public string MCVersion { get; set; }//Minecraft Version
        public string ForgeVersion { get; set; }//Forge Version
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
    #endregion
    
    #region AvailableModpacks
    public class AvailableModpack{
        public string Name { get; set; }
        public string Icon { get; set; }
        public string Server { get; set; }
        public string McVersion { get; set; }
        public string PathToManifest { get; set; }
    }
    #endregion

    public partial class MainWindow : Window
    {
        public static string InstallPath = "";
        private static MainWindow form = new MainWindow();
        public static UserConfig CFG;

        public static Rectangle CurrentChoosenTab;
        public MainWindow()
        {

            #region ProcessingCommandlineArgs
            foreach (var item in Environment.GetCommandLineArgs())
            {
                if (item.Contains("SKpLauncher:"))
                {
                    List<string> temp = JsonConvert.DeserializeObject<List<string>>(item.Replace("SKpLauncher:", ""));
                    RegistryKey AvailableModpacksRegistry = Registry.LocalMachine;
                    AvailableModpacksRegistry = AvailableModpacksRegistry.OpenSubKey("SOFTWARE", true);
                    AvailableModpacksRegistry = AvailableModpacksRegistry.CreateSubKey("SKProCHsLauncher", true);

                    List<AvailableModpack> AllAvailableModpacks = JsonConvert.DeserializeObject<List<AvailableModpack>>(Convert.ToString(AvailableModpacksRegistry.GetValue("AvailableModpacks", null)));
                    if (AllAvailableModpacks == null)
                        AllAvailableModpacks = new List<AvailableModpack>();
                    foreach (var ManifestsURL in temp)
                    {
                        ModpackInfo MpInfoToAdd = new ModpackInfo();
                        try
                        {
                            using (WebClient wc = new WebClient())
                            {
                                MpInfoToAdd = JsonConvert.DeserializeObject<ModpackInfo>(wc.DownloadString(ManifestsURL));
                            }
                            AllAvailableModpacks.Add(new AvailableModpack() { PathToManifest = ManifestsURL, Icon = MpInfoToAdd.Icon, McVersion = MpInfoToAdd.MCVersion, Name = MpInfoToAdd.ModpackName, Server = MpInfoToAdd.ModpackServer });
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show("При добавлении сборки возникла ошибка: " + ex.Message);
                        }
                    }
                    AvailableModpacksRegistry.SetValue("AvailableModpacks", JsonConvert.SerializeObject(AllAvailableModpacks));
                }
            }
            #endregion
            
            RegistryKey registry = Registry.LocalMachine;
            registry = registry.OpenSubKey("SOFTWARE", true);
            registry = registry.CreateSubKey("SKProCHsLauncher", true);
            if (Convert.ToString(registry.GetValue("PATH")) == "" || Convert.ToString(registry.GetValue("PATH")) == null)
            {
                registry.SetValue("PATH", System.IO.Path.GetPathRoot(Environment.GetCommandLineArgs()[0]));
                CFG = new UserConfig();
                registry.SetValue("GlobalConfig", JsonConvert.SerializeObject(CFG));
            }else if(Convert.ToString(registry.GetValue("PATH")) != System.IO.Path.GetPathRoot(Environment.GetCommandLineArgs()[0]))
            {
                Process.Start(System.IO.Path.Combine(registry.GetValue("PATH") + @"\SKProCH's Launcher.exe"));
                Environment.Exit(11);
            }

            InitializeComponent();
            form = this;

            CurrentChoosenTab = ChooseMainModpacks;
            form.Activated += Form_Activated;
        }

        private void Button_Folder_Click(object sender, RoutedEventArgs e)
        {
            Process.Start(InstallPath);
        }

        private void ChooseMainModpacks_Event(object sender, MouseButtonEventArgs e)
        {
            CurrentChoosenTab = ChooseMainModpacks;
            UpdateChoosenTab(0.3);
            ExportModpack.Visibility = Visibility.Collapsed;
            AvailableModpacks.Visibility = Visibility.Collapsed;
            MainModpacks.Visibility = Visibility.Visible;
        }

        private void ChooseExportModpacks_Event(object sender, MouseButtonEventArgs e)
        {
            CurrentChoosenTab = ChooseExportModpacks;
            UpdateChoosenTab(0.3);
            MainModpacks.Visibility = Visibility.Collapsed;
            AvailableModpacks.Visibility = Visibility.Collapsed;
            ExportModpack.Visibility = Visibility.Visible;
        }

        private void ChooseAvailableModpacks_Event(object sender, MouseButtonEventArgs e)
        {
            CurrentChoosenTab = ChooseAvailableModpacks;
            UpdateChoosenTab(0.3);
            ExportModpack.Visibility = Visibility.Collapsed;
            MainModpacks.Visibility = Visibility.Collapsed;
            AvailableModpacks.Visibility = Visibility.Visible;
        }

        private void Form_Activated(object sender, EventArgs e)
        {
            UpdateChoosenTab(0.00000001);
        }

        public static void UpdateChoosenTab(double duration)
        {
            Point relativePoint = CurrentChoosenTab.TransformToAncestor(form)
                                          .Transform(new Point(0, 0));
            
            form.LeftBackground.BeginAnimation(MarginProperty, new ThicknessAnimation(new Thickness(relativePoint.X - 25,0,0,0), TimeSpan.FromSeconds(duration)));
            form.RightBackground.BeginAnimation(MarginProperty, new ThicknessAnimation(new Thickness(relativePoint.X + CurrentChoosenTab.ActualWidth, 0, 0, 0), TimeSpan.FromSeconds(duration)));
            form.LeftFill.BeginAnimation(WidthProperty, new DoubleAnimation(form.LeftFill.ActualWidth, relativePoint.X - 15, TimeSpan.FromSeconds(duration)));
            form.RightFill.BeginAnimation(MarginProperty, new ThicknessAnimation(new Thickness(relativePoint.X + CurrentChoosenTab.ActualWidth + 10, 0, 0, 0), TimeSpan.FromSeconds(duration)));
            form.DarkBackground.BeginAnimation(WidthProperty, new DoubleAnimation(CurrentChoosenTab.ActualWidth, TimeSpan.FromSeconds(duration)));
            form.DarkBackground.BeginAnimation(MarginProperty, new ThicknessAnimation(new Thickness(relativePoint.X, 0, 0, 0), TimeSpan.FromSeconds(duration)));
        }
    }

    public class TopMarginConverter : IValueConverter
    {
        #region IValueConverter Members

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            double sliderValue = (double)value;
            return new Thickness(0, sliderValue - 1, 0, 0);
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new Exception("The method or operation is not implemented.");
        }

        #endregion
    }
}
