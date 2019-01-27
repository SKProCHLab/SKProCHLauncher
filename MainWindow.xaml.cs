using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Net;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using Ionic.Zip;
using Microsoft.Win32;
using Newtonsoft.Json;
using Exception = System.Exception;
using MessageBox = System.Windows.MessageBox;
using Path = System.IO.Path;

namespace SKProCHLauncher
{
    public partial class MainWindow : Window
    {
        public static  string     InstallPath = "";
        private static MainWindow form;
        public static  UserConfig CFG;

        private static Rectangle _сurrentChoosenTab;

        public MainWindow() {

            #region ReadConfigFromRegistry

            var registry = Registry.LocalMachine;
            registry = registry.OpenSubKey("SOFTWARE", true);
            registry = registry.CreateSubKey("SKProCH's Launcher", true);
            if (Convert.ToString(registry.GetValue("PATH")) == ""){
                registry.SetValue("PATH", Path.GetDirectoryName(Environment.GetCommandLineArgs()[0]));

                SetURLProcessing();

                CFG = new UserConfig();
                registry.SetValue("GlobalConfig", JsonConvert.SerializeObject(CFG));
            }
            else if (Convert.ToString(registry.GetValue("PATH")) != Path.GetDirectoryName(Environment.GetCommandLineArgs()[0])){
                if (File.Exists(Path.Combine(registry.GetValue("PATH") + @"\SKProCH's Launcher.exe"))){
                    Process.Start(Path.Combine(registry.GetValue("PATH") + @"\SKProCH's Launcher.exe"));
                    Environment.Exit(11);
                }
                else{
                    var result = MessageBox.Show(
                                                 "Неведомая сила переместила программу в другую директорию. \nЕсли выберите Да, то мы привяжемся к этой директории. \nЕсли выберете Нет, то мы скачаем последнюю версию в старую директорию",
                                                 "Смена директории", MessageBoxButton.YesNo);
                    if (result == MessageBoxResult.Yes){
                        registry.SetValue("PATH", Path.GetDirectoryName(Environment.GetCommandLineArgs()[0]));
                        SetURLProcessing();
                    }
                    else if (result == MessageBoxResult.No){
                        //
                    }
                }
            }

                #endregion

            InstallPath = Path.GetDirectoryName(Environment.GetCommandLineArgs()[0]);

            InitializeComponent();
            form = this;

            var iconManager = new IconManager();

            _сurrentChoosenTab =  ChooseMainModpacks;
            form.Activated     += Form_Activated;
        }

        private static void SetURLProcessing() {
            var URLHandler = Registry.ClassesRoot;
            URLHandler = URLHandler.CreateSubKey("skpmclaucnher");
            URLHandler.SetValue("",             "SKProCH's Launcher");
            URLHandler.SetValue("URL Protocol", "");
            var iconURLHandler = URLHandler.CreateSubKey("DefaultIcon");
            iconURLHandler.SetValue("", Environment.GetCommandLineArgs()[0]);
            URLHandler = URLHandler.CreateSubKey("shell");
            URLHandler = URLHandler.CreateSubKey("open");
            URLHandler = URLHandler.CreateSubKey("command");
            URLHandler.SetValue("", '"' + Environment.GetCommandLineArgs()[0] + '"' + " " + '"' + "%1" + '"');
        }

        private void Button_Folder_Click(object sender, RoutedEventArgs e) {
            Process.Start(InstallPath);
        }

        private void ChooseMainModpacks_Event(object sender, MouseButtonEventArgs e) {
            _сurrentChoosenTab = ChooseMainModpacks;
            UpdateChoosenTab(0.3);
            ExportModpack.Visibility     = Visibility.Collapsed;
            AvailableModpacks.Visibility = Visibility.Collapsed;
            MainModpacks.Visibility      = Visibility.Visible;
        }

        private void ChooseExportModpacks_Event(object sender, MouseButtonEventArgs e) {
            _сurrentChoosenTab = ChooseExportModpacks;
            UpdateChoosenTab(0.3);
            MainModpacks.Visibility      = Visibility.Collapsed;
            AvailableModpacks.Visibility = Visibility.Collapsed;
            ExportModpack.Visibility     = Visibility.Visible;
        }

        private void ChooseAvailableModpacks_Event(object sender, MouseButtonEventArgs e) {
            _сurrentChoosenTab = ChooseAvailableModpacks;
            UpdateChoosenTab(0.3);
            ExportModpack.Visibility     = Visibility.Collapsed;
            MainModpacks.Visibility      = Visibility.Collapsed;
            AvailableModpacks.Visibility = Visibility.Visible;

            var AvailableModpacksRegistry = Registry.LocalMachine;
            AvailableModpacksRegistry = AvailableModpacksRegistry.OpenSubKey("SOFTWARE", true);
            AvailableModpacksRegistry = AvailableModpacksRegistry.CreateSubKey("SKProCHsLauncher", true);

            var AllAvailableModpacks =
                JsonConvert.DeserializeObject<List<AvailableModpack>>(
                                                                      Convert.ToString(AvailableModpacksRegistry.GetValue("AvailableModpacks", null)));
            if (!(AllAvailableModpacks == null || AllAvailableModpacks.Count == 0)){
                NoModpacksAvailable.Visibility = Visibility.Collapsed;
                LB_AvailableModpacks.Items.Clear();
                foreach (var item in AllAvailableModpacks){
                    LB_AvailableModpacks.Items.Add(item);
                }
            }
            else{
                AllAvailableModpacks = new List<AvailableModpack>();
                AvailableModpacksRegistry.SetValue("AvailableModpacks",
                                                   JsonConvert.SerializeObject(AllAvailableModpacks));
                NoModpacksAvailable.Visibility = Visibility.Visible;
            }
        }

        private void LB_MainModpack_Event(object sender, MouseButtonEventArgs e) {
        }

        private void LB_ExportModpack_Event(object sender, MouseButtonEventArgs e) {
        }

        private void LB_AvailableModpacks_Event(object sender, MouseButtonEventArgs e) {
        }

        private void Button_AddModpack_Click(object sender, RoutedEventArgs e) {
            ChooseAvailableModpacks_Event(form, null);
        }

        #region TabsAnimation

        private void Form_Activated(object sender, EventArgs e) {
            UpdateChoosenTab(0.00000001);
            form.Activated -= Form_Activated;
        }

        public static void UpdateChoosenTab(double duration) {
            var relativePoint = _сurrentChoosenTab.TransformToAncestor(form)
                                                  .Transform(new Point(0, 0));

            form.LeftBackground.BeginAnimation(MarginProperty,
                                               new ThicknessAnimation(new Thickness(relativePoint.X - 25, 0, 0, 0), TimeSpan.FromSeconds(duration)));
            form.RightBackground.BeginAnimation(MarginProperty,
                                                new ThicknessAnimation(new Thickness(relativePoint.X + _сurrentChoosenTab.ActualWidth, 0, 0, 0),
                                                                       TimeSpan.FromSeconds(duration)));
            form.LeftFill.BeginAnimation(WidthProperty,
                                         new DoubleAnimation(form.LeftFill.ActualWidth, relativePoint.X - 15, TimeSpan.FromSeconds(duration)));
            form.RightFill.BeginAnimation(MarginProperty,
                                          new ThicknessAnimation(new Thickness(relativePoint.X + _сurrentChoosenTab.ActualWidth + 10, 0, 0, 0),
                                                                 TimeSpan.FromSeconds(duration)));
            form.DarkBackground.BeginAnimation(WidthProperty,
                                               new DoubleAnimation(_сurrentChoosenTab.ActualWidth, TimeSpan.FromSeconds(duration)));
            form.DarkBackground.BeginAnimation(MarginProperty,
                                               new ThicknessAnimation(new Thickness(relativePoint.X, 0, 0, 0), TimeSpan.FromSeconds(duration)));
        }

        [DllImport("user32.dll")]
        public static extern IntPtr SendMessage(IntPtr hWnd, uint Msg, IntPtr wParam, ref COPYDATASTRUCT lParam);

        [StructLayout(LayoutKind.Sequential)]
        public struct COPYDATASTRUCT
        {
            public IntPtr dwData;
            public int    cbData;
            public IntPtr lpData;
        }

        const uint WM_COPYDATA = 0x004A;


        public static IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled) {
            if (msg == WM_COPYDATA){
                COPYDATASTRUCT data = new COPYDATASTRUCT();
                data = (COPYDATASTRUCT) Marshal.PtrToStructure(lParam, data.GetType());
                MessageBox.Show("Received command: " + Marshal.PtrToStringUni(data.lpData));
            }

            return IntPtr.Zero;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e) {
            WindowInteropHelper h      = new WindowInteropHelper(this);
            HwndSource          source = HwndSource.FromHwnd(h.Handle);
            source.AddHook(new HwndSourceHook(WndProc)); //регистрируем обработчик сообщений            
        }

        #endregion
    }

    public class TopMarginConverter : IValueConverter
    {
        #region IValueConverter Members

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            var sliderValue = (double) value;
            return new Thickness(0, sliderValue - 1, 0, 0);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            throw new Exception("The method or operation is not implemented.");
        }

        #endregion
    }

}
