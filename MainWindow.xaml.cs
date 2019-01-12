using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Net;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using Ionic.Zip;
using Microsoft.Win32;
using Newtonsoft.Json;
using Path = System.IO.Path;

namespace SKProCHLauncher
{
    /// <summary>
    ///     Логика взаимодействия для MainWindow.xaml
    /// </summary>

    #region ConfigClass

    public class UserConfig
    {
        public UserConfig()
        {
            LocalModpacks = new List<ModPack>();
            ExportedModpacks = new List<ModPack>();
            Accounts = new List<Account>();
        }

        public List<ModPack> LocalModpacks { get; set; }
        public List<ModPack> ExportedModpacks { get; set; }
        public List<Account> Accounts { get; set; }

        public class Account
        {
            public string NickName { get; set; }
            public string UUID { get; set; }

            public bool IsLegacy { get; set; }

            public string Login { get; set; }
            public string Password { get; set; }
        }

        public class ModPack
        {
            private int ActiveDownloadCount;

            private bool DownLoadsScheduled;
            private readonly object locker = new object();


            private string NewVersion;
            private readonly List<ModpackInfo.Version.Download> ZipArchieves = new List<ModpackInfo.Version.Download>();
            public string Name { get; set; }
            public string ServerName { get; set; }
            public string UpdatesURL { get; set; }
            public string CurrentVersion { get; set; }
            public string Icon { get; set; }
            public string Path { get; set; }

            //OnlyLocal
            public string ID { get; set; } //Minecraft Version
            public string ForgeID { get; set; } //Forge Version
            public string ForgeURL { get; set; } //Forge version.json URL
            public List<Account> Accounts { get; set; }
            public string ChoosenAccount { get; set; }

            private void CheckUpdates()
            {
                try
                {
                    var wc = new WebClient();
                    wc.DownloadFile(UpdatesURL,
                        Environment.GetFolderPath(Environment.SpecialFolder.InternetCache) +
                        @"\SKProCH's Version File.tmp");
                    var VersionsContent =
                        File.ReadAllText(Environment.GetFolderPath(Environment.SpecialFolder.InternetCache) +
                                         @"\SKProCH's Version File.tmp");
                    try
                    {
                        File.Delete(Environment.GetFolderPath(Environment.SpecialFolder.InternetCache) +
                                    @"\SKProCH's Version File.tmp");
                    }
                    catch (Exception)
                    {
                    }

                    try
                    {
                        var MU = JsonConvert.DeserializeObject<ModpackInfo>(VersionsContent);
                        if (Convert.ToInt32(MU.Versions[MU.Versions.Count - 1].VersionNumber.Replace(".", "")) >
                            Convert.ToInt32(CurrentVersion.Replace(".", "")))
                        {
                            NewVersion = MU.Versions[MU.Versions.Count - 1].VersionNumber;
                            //

                            //Построение общих различий
                            var ToDownload = new List<ModpackInfo.Version.Download>(); //Финальные файлы
                            var ToDelete = new List<ModpackInfo.Version.Delete>();

                            foreach (var item in MU.Versions)
                                if (Convert.ToInt32(item.VersionNumber.Replace(".", "")) >
                                    Convert.ToInt32(CurrentVersion.Replace(".", "")))
                                {
                                    foreach (var deleteitem in item.ListToDelete)
                                        for (var i = 0; i < ToDownload.Count; i++)
                                            if (deleteitem.Path == ToDownload[i].Path && !ToDownload[i].IsArchive)
                                                ToDownload.RemoveAt(i);

                                    ToDownload.AddRange(item.ListToDownload);
                                    ToDelete.AddRange(item.ListToDelete);
                                }
                                else
                                {
                                    break;
                                }

                            //Удаление всего
                            foreach (var item in ToDelete)
                                if (item.Path.Remove(6) == "%BASE%" && File.Exists(item.Path.Replace("%BASE%", Path)))
                                    File.Delete(item.Path.Replace("%BASE%", Path));

                            var WC = new WebClient();
                            WC.DownloadFileCompleted += WC_DownloadFileCompleted;
                            foreach (var item in ToDownload)
                            {
                                WC.DownloadFileAsync(new Uri(item.URL), item.Path);
                                lock (locker)
                                {
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
                {
                }
            }

            private void WC_DownloadFileCompleted(object sender, AsyncCompletedEventArgs e)
            {
                lock (locker)
                {
                    ActiveDownloadCount--;
                }

                if (ActiveDownloadCount == 0 && DownLoadsScheduled) //Заканчиваем скачивать всё
                {
                    foreach (var item in ZipArchieves) //Распаковываем
                    {
                        using (var zip = ZipFile.Read(item.Path))
                        {
                            foreach (var y in zip) y.Extract(Path);
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

    public class ModpackInfo
    {
        public ModpackInfo()
        {
            Versions = new List<Version>();
        }

        public string ModpackName { get; set; }
        public string ModpackServer { get; set; }
        public string URL { get; set; }
        public string Icon { get; set; }
        public string MCVersion { get; set; } //Minecraft Version
        public string ForgeVersion { get; set; } //Forge Version
        public string ForgeURL { get; set; } //Forge version.json URL

        public List<Version> Versions { get; set; }

        public class Version
        {
            public Version()
            {
                ListToDelete = new List<Delete>();
                ListToDownload = new List<Download>();
            }

            public string VersionNumber { get; set; }

            public List<Delete> ListToDelete { get; set; }
            public List<Download> ListToDownload { get; set; }

            public class Delete
            {
                public string Path { get; set; }
            }

            public class Download
            {
                public string Path { get; set; }
                public string URL { get; set; }
                public bool IsArchive { get; set; }
            }
        }
    }

    #endregion

    #region AvailableModpacks

    public class AvailableModpack
    {
        public string ID { get; set; }
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
        private static MainWindow form;
        public static UserConfig CFG;

        public static Rectangle CurrentChoosenTab;

        public MainWindow()
        {
            #region ProcessingCommandlineArgs

            foreach (var item in Environment.GetCommandLineArgs())
                if (item.Contains("SKpLauncher:"))
                {
                    var temp = JsonConvert.DeserializeObject<List<string>>(item.Replace("SKpLauncher:", ""));
                    var AvailableModpacksRegistry = Registry.LocalMachine;
                    AvailableModpacksRegistry = AvailableModpacksRegistry.OpenSubKey("SOFTWARE", true);
                    AvailableModpacksRegistry = AvailableModpacksRegistry.CreateSubKey("SKProCHsLauncher", true);

                    var AllAvailableModpacks =
                        JsonConvert.DeserializeObject<List<AvailableModpack>>(
                            Convert.ToString(AvailableModpacksRegistry.GetValue("AvailableModpacks", null)));
                    if (AllAvailableModpacks == null)
                        AllAvailableModpacks = new List<AvailableModpack>();
                    foreach (var ManifestsURL in temp)
                    {
                        var MpInfoToAdd = new ModpackInfo();
                        try
                        {
                            using (var wc = new WebClient())
                            {
                                MpInfoToAdd =
                                    JsonConvert.DeserializeObject<ModpackInfo>(wc.DownloadString(ManifestsURL));
                            }

                            AllAvailableModpacks.Add(new AvailableModpack
                            {
                                PathToManifest = ManifestsURL, Icon = MpInfoToAdd.Icon,
                                McVersion = MpInfoToAdd.MCVersion, Name = MpInfoToAdd.ModpackName,
                                Server = MpInfoToAdd.ModpackServer,
                                ID = AllAvailableModpacks[AllAvailableModpacks.Count - 1].ID + 1
                            });
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show("При добавлении сборки возникла ошибка: " + ex.Message);
                        }
                    }

                    AvailableModpacksRegistry.SetValue("AvailableModpacks",
                        JsonConvert.SerializeObject(AllAvailableModpacks));
                }

            #endregion

            var registry = Registry.LocalMachine;
            registry = registry.OpenSubKey("SOFTWARE", true);
            registry = registry.CreateSubKey("SKProCH's Launcher", true);
            if (Convert.ToString(registry.GetValue("PATH")) == "" ||
                Convert.ToString(registry.GetValue("PATH")) == null)
            {
                registry.SetValue("PATH", Path.GetDirectoryName(Environment.GetCommandLineArgs()[0]));

                var URLHandler = Registry.ClassesRoot;
                URLHandler = URLHandler.CreateSubKey("skpmclaucnher");
                URLHandler.SetValue("@", "SKProCH's Launcher - Protocol Handler");
                URLHandler.SetValue("URL Protocol", "");
                var iconURLHandler = URLHandler.CreateSubKey("DefaultIcon");
                iconURLHandler.SetValue("@", Environment.GetCommandLineArgs()[0]);
                URLHandler = URLHandler.CreateSubKey("shell");
                URLHandler = URLHandler.CreateSubKey("open");
                URLHandler = URLHandler.CreateSubKey("command");
                URLHandler.SetValue("@", Environment.GetCommandLineArgs()[0] + "%1");

                CFG = new UserConfig();
                registry.SetValue("GlobalConfig", JsonConvert.SerializeObject(CFG));
            }
            else if (Convert.ToString(registry.GetValue("PATH")) !=
                     Path.GetDirectoryName(Environment.GetCommandLineArgs()[0]))
            {
                if (File.Exists(Path.Combine(registry.GetValue("PATH") + @"\SKProCH's Launcher.exe")))
                {
                    Process.Start(Path.Combine(registry.GetValue("PATH") + @"\SKProCH's Launcher.exe"));
                    Environment.Exit(11);
                }
                else
                {
                    var result = MessageBox.Show(
                        "Неведомая сила переместила программу в другую директорию. \nЕсли выберите Да, то мы привяжемся к этой директории. \nЕсли выберете Нет, то мы скачаем последнюю версию в старую директорию",
                        "Смена директории", MessageBoxButton.YesNo);
                    if (result == MessageBoxResult.Yes)
                    {
                        registry.SetValue("PATH", Path.GetDirectoryName(Environment.GetCommandLineArgs()[0]));

                        var URLHandler = Registry.ClassesRoot;
                        URLHandler = URLHandler.CreateSubKey("skpmclaucnher");
                        URLHandler.SetValue("@", "SKProCH's Launcher - Protocol Handler");
                        URLHandler.SetValue("URL Protocol", "");
                        var iconURLHandler = URLHandler.CreateSubKey("DefaultIcon");
                        iconURLHandler.SetValue("@", Environment.GetCommandLineArgs()[0]);
                        URLHandler = URLHandler.CreateSubKey("shell");
                        URLHandler = URLHandler.CreateSubKey("open");
                        URLHandler = URLHandler.CreateSubKey("command");
                        URLHandler.SetValue("@", Environment.GetCommandLineArgs()[0] + "%1");
                    }
                    else if (result == MessageBoxResult.No)
                    {
                    }
                }
            }

            InstallPath = Path.GetDirectoryName(Environment.GetCommandLineArgs()[0]);

            InitializeComponent();
            form = this;

            var iconManager = new IconManager();

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

            var AvailableModpacksRegistry = Registry.LocalMachine;
            AvailableModpacksRegistry = AvailableModpacksRegistry.OpenSubKey("SOFTWARE", true);
            AvailableModpacksRegistry = AvailableModpacksRegistry.CreateSubKey("SKProCHsLauncher", true);

            var AllAvailableModpacks =
                JsonConvert.DeserializeObject<List<AvailableModpack>>(
                    Convert.ToString(AvailableModpacksRegistry.GetValue("AvailableModpacks", null)));
            if (!(AllAvailableModpacks == null || AllAvailableModpacks.Count == 0))
            {
                NoModpacksAvailable.Visibility = Visibility.Collapsed;
                LB_AvailableModpacks.Items.Clear();
                foreach (var item in AllAvailableModpacks) LB_AvailableModpacks.Items.Add(item);
            }
            else
            {
                AllAvailableModpacks = new List<AvailableModpack>();
                AvailableModpacksRegistry.SetValue("AvailableModpacks",
                    JsonConvert.SerializeObject(AllAvailableModpacks));
                NoModpacksAvailable.Visibility = Visibility.Visible;
            }
        }

        private void LB_MainModpack_Event(object sender, MouseButtonEventArgs e)
        {
        }

        private void LB_ExportModpack_Event(object sender, MouseButtonEventArgs e)
        {
        }

        private void LB_AvailableModpacks_Event(object sender, MouseButtonEventArgs e)
        {
        }

        private void Button_AddModpack_Click(object sender, RoutedEventArgs e)
        {
            ChooseAvailableModpacks_Event(form, null);
        }

        #region TabsAnimation

        private void Form_Activated(object sender, EventArgs e)
        {
            UpdateChoosenTab(0.00000001);
        }

        public static void UpdateChoosenTab(double duration)
        {
            var relativePoint = CurrentChoosenTab.TransformToAncestor(form)
                .Transform(new Point(0, 0));

            form.LeftBackground.BeginAnimation(MarginProperty,
                new ThicknessAnimation(new Thickness(relativePoint.X - 25, 0, 0, 0), TimeSpan.FromSeconds(duration)));
            form.RightBackground.BeginAnimation(MarginProperty,
                new ThicknessAnimation(new Thickness(relativePoint.X + CurrentChoosenTab.ActualWidth, 0, 0, 0),
                    TimeSpan.FromSeconds(duration)));
            form.LeftFill.BeginAnimation(WidthProperty,
                new DoubleAnimation(form.LeftFill.ActualWidth, relativePoint.X - 15, TimeSpan.FromSeconds(duration)));
            form.RightFill.BeginAnimation(MarginProperty,
                new ThicknessAnimation(new Thickness(relativePoint.X + CurrentChoosenTab.ActualWidth + 10, 0, 0, 0),
                    TimeSpan.FromSeconds(duration)));
            form.DarkBackground.BeginAnimation(WidthProperty,
                new DoubleAnimation(CurrentChoosenTab.ActualWidth, TimeSpan.FromSeconds(duration)));
            form.DarkBackground.BeginAnimation(MarginProperty,
                new ThicknessAnimation(new Thickness(relativePoint.X, 0, 0, 0), TimeSpan.FromSeconds(duration)));
        }

        #endregion
    }

    public class TopMarginConverter : IValueConverter
    {
        #region IValueConverter Members

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var sliderValue = (double) value;
            return new Thickness(0, sliderValue - 1, 0, 0);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new Exception("The method or operation is not implemented.");
        }

        #endregion
    }
}