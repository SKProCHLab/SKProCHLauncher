using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using Microsoft.Win32;
using Newtonsoft.Json;

namespace SKProCHLauncher
{
    public partial class MainWindow : Window
    {
        public static  string     InstallPath = "";
        private static MainWindow form;
        public static  UserConfig CFG;

        private static Rectangle _сurrentChoosenTab;


        public static List<AvailableModpack> AvailableModpacksList { get; set; }

        public MainWindow() {
            InitializeComponent();
            form        = this;
            DataContext = this;





            var iconManager = new IconManager();

            _сurrentChoosenTab =  ChooseMainModpacks;
            form.Activated     += Form_Activated;
            
        }

        private void Button_Folder_Click(object sender, RoutedEventArgs e) {
            var relativePoint = Button_Folder.TransformToAncestor(form)
                                             .Transform(new Point(0, 0));
            form.FoldersMenuStackPanel.Margin = new Thickness(relativePoint.X - 5, relativePoint.Y + Button_Folder.ActualHeight, 0, 0);

            form.FoldersMenu.Visibility = Visibility.Visible;
        }

        private void FoldersMenuClose(object sender, MouseButtonEventArgs e) {
            form.FoldersMenu.Visibility = Visibility.Collapsed;
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
            }
            else{
                AllAvailableModpacks = new List<AvailableModpack>();
                AvailableModpacksRegistry.SetValue("AvailableModpacks",
                                                   JsonConvert.SerializeObject(AllAvailableModpacks));
                NoModpacksAvailable.Visibility = Visibility.Visible;
            }
            AvailableModpacksList = AllAvailableModpacks;
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

        public static void BringToFrond(MainWindow mw = null) {
            if (mw == null){
                mw = form;
            }
            bool temp = form.Topmost;
            form.Topmost = true;
            form.Topmost = temp;
            // Set focus to the window.
            form.Activate();
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

        private const uint WM_COPYDATA = 0x004A;


        public static IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled) {
            if (msg == WM_COPYDATA){
                var data = new COPYDATASTRUCT();
                data = (COPYDATASTRUCT) Marshal.PtrToStructure(lParam, data.GetType());
                MessageBox.Show("Received command: " + Marshal.PtrToStringUni(data.lpData));
                switch (Marshal.PtrToStringUni(data.lpData))
                {
                    case "UpdateAvailablesModpacks":
                        form.ChooseAvailableModpacks_Event(form, null);

                    break;
                }
            }
            return IntPtr.Zero;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e) {
            var h      = new WindowInteropHelper(this);
            var source = HwndSource.FromHwnd(h.Handle);
            source.AddHook(WndProc); //регистрируем обработчик сообщений
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
