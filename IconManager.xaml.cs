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
using System.Windows.Shapes;
using System.Net;
using Newtonsoft.Json;
using System.Windows.Forms;
using System.IO;
using System.Threading;
using System.ComponentModel;
using Path = System.IO.Path;
using MessageBox = System.Windows.MessageBox;

namespace SKProCHLauncher
{
    /// <summary>
    /// Логика взаимодействия для IconManager.xaml
    /// </summary>
    public partial class IconManager
    {
        public static IconManager form;
        public IconManager()
        {
            InitializeComponent();
            form = this;
            this.DataContext = this;
            InitializeIcons();
        }

        #region ImageLoader
        string _IconPathString = "";
        ImageLoader Loader = null;

        public BitmapImage IconImageSource
        {
            get
            {
                if (Loader != null) return Loader.Image;
                else return null;
            }
        }

        string _ImageStatusText = "";
        public string ImageStatusText
        {
            get { return _ImageStatusText; }
            set
            {
                _ImageStatusText = value;
                OnPropertyChanged("ImageStatusText");
            }
        }

        public string IconPathString
        {
            get { return _IconPathString; }

            set
            {
                try
                {
                    if (Loader != null)
                    {
                        Loader.LoadCompleted -= ImageLoadCompleted;
                        Loader = null;
                    }

                    if (String.IsNullOrEmpty(value))
                    {
                        _IconPathString = "";
                        OnPropertyChanged("IconImageSource");
                        return;
                    }

                    Uri uri = new Uri(value);
                    Loader = new ImageLoader(uri);
                    Loader.LoadCompleted += ImageLoadCompleted;
                    Loader.Run();
                    _IconPathString = value;
                    ImageStatusText = "Подождите...";
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine(ex.ToString());
                    _IconPathString = "";
                    OnPropertyChanged("IconImageSource");
                    ImageIsLoaded = false;
                    CheckInstallAvailable(form, null);
                }
            }
        }

        void ImageLoadCompleted(object sender, EventArgs e)
        {
            this.Dispatcher.Invoke(new Action(() =>
            {
                if ((sender as ImageLoader).Image != null)
                {
                    ImageIsLoaded = true;
                    CheckInstallAvailable(form, null);
                }
                else
                {
                    _IconPathString = "";
                    ImageIsLoaded = false;
                    CheckInstallAvailable(form, null);
                }

                OnPropertyChanged("IconImageSource");
                (sender as ImageLoader).LoadCompleted -= ImageLoadCompleted;
            }));
        }

        public event PropertyChangedEventHandler PropertyChanged;

        void OnPropertyChanged(string name)
        {
            var handler = PropertyChanged;
            if (handler != null) handler(this, new PropertyChangedEventArgs(name));
        }

        private static bool ImageIsLoaded = false;

        #endregion
        #region ListBoxIcons Class
        class ListBoxIcons
        {
            public string IconPath { get; set; }
            public string IconName { get; set; }
        }
        #endregion

        private void InitializeIcons()
        {
            AllIcons.Visibility = Visibility.Visible;
            Dictionary<string, string> Default;
            Dictionary<string, string> Custom;

            try
            {
                using (WebClient wc = new WebClient())
                {
                    string JSON = wc.DownloadString(@"https://gdurl.com/u9FJ");
                    Default = JsonConvert.DeserializeObject<Dictionary<string, string>>(JSON);
                }
            }
            catch (Exception)
            {
                MessageBox.Show("Видимо, нет подключения к базе данных иконок. Скорее всего нет подключения к GoogleDrive или GDURL.");
                Default = new Dictionary<string, string>() { };
                foreach (var item in Directory.GetFiles(Path.Combine(MainWindow.InstallPath, @"\Icons\DefaultIcons\")))
                {
                    Default.Add(item, Path.GetFileNameWithoutExtension(item));
                }
            }

            Custom = new Dictionary<string, string>();

            if (Directory.Exists(MainWindow.InstallPath + @"\Icons\CustomIcons\"))
            {
                var CustomIconsPath = Directory.GetFiles(MainWindow.InstallPath + @"\Icons\CustomIcons\");
                foreach (var item in CustomIconsPath)
                {
                    Custom.Add(item, System.IO.Path.GetFileNameWithoutExtension(item));
                }
            }
            else
                Directory.CreateDirectory(MainWindow.InstallPath + @"\Icons\CustomIcons\");

            form.Dispatcher.Invoke(new Action(() => {
                form.DefaultIconsListBox.Items.Clear();
                form.CustomIconsListBox.Items.Clear();

                form.AllIcons.Visibility = Visibility.Visible;

                foreach (var defitem in Default)
                {
                    form.DefaultIconsListBox.Items.Add(new ListBoxIcons() { IconName = defitem.Value, IconPath = defitem.Key });
                }
                foreach (var cusitem in Custom)
                {
                    form.CustomIconsListBox.Items.Add(new ListBoxIcons() { IconName = cusitem.Value, IconPath = cusitem.Key });
                }
                form.CustomIconsListBox.Items.Add(new ListBoxIcons() { IconPath = @"https://gdurl.com/M8Ps", IconName = "Add icon" });
            }));
            if (!form.IsVisible)
            {
                form.Show();
            }
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
                    wc.DownloadFile(URL, MainWindow.InstallPath + @"\Icons\DefaultIcons\" + NAME + "." + URL.Remove(0, URL.LastIndexOf("." + 1)));
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
            bool temp_IsError = false;
            try
            {
                File.Copy(form.IconPath.Text, MainWindow.InstallPath + @"\Icons\CustomIcons\" + form.IconName);
            }
            catch (Exception)
            {
                try
                {
                    using (WebClient wc = new WebClient())
                    {
                        wc.DownloadFile(new Uri(form.IconPath.Text), Path.Combine(MainWindow.InstallPath, @"Icons\CustomIcons", form.IconName.Text + ".skplaucnhericon"));
                    }
                }
                catch (Exception)
                {
                    MessageBox.Show("Что-то не так с файлом. Проверьте еще раз");
                    form.AllIcons.Visibility = Visibility.Collapsed;
                    form.AddCustomIconForm.Visibility = Visibility.Visible;
                    temp_IsError = true;
                }
            }

            if (!temp_IsError)
            {
                form.AddCustomIconForm.Visibility = Visibility.Collapsed;
                InitializeIcons();
            }
        }

        private void CheckInstallAvailable(object sender, TextChangedEventArgs e)
        {
            if (ImageIsLoaded)
            {
                if (IconName.Text != "" && IconName.Text != null)
                {
                    AddCustomIconButton.IsEnabled = true;
                }
                else
                    AddCustomIconButton.IsEnabled = false;
            }
            else
                AddCustomIconButton.IsEnabled = false;
        }
    }

    public class ImageLoader
    {
        Uri _ImageUri;
        BitmapImage _bi;

        public BitmapImage Image
        {
            get { return _bi; }
        }

        public event EventHandler LoadCompleted;

        void OnLoadCompleted()
        {
            if (LoadCompleted != null) LoadCompleted(this, new EventArgs());
        }

        public ImageLoader(Uri uri)
        {
            this._ImageUri = uri;
        }

        void LoadImage()
        {
            BitmapImage bi;
            try
            {
                byte[] data;

                if (_ImageUri.IsFile)
                {
                    data = System.IO.File.ReadAllBytes(_ImageUri.LocalPath);
                }
                else
                {
                    var client = new System.Net.WebClient();
                    using (client)
                    {
                        data = client.DownloadData(_ImageUri);
                    }
                }

                var ms = new System.IO.MemoryStream(data);

                bi = new BitmapImage();
                bi.BeginInit();
                bi.CacheOption = BitmapCacheOption.OnLoad;
                bi.StreamSource = ms;
                bi.EndInit();
                bi.Freeze();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.ToString());
                bi = null;
            }
            this._bi = bi;
            OnLoadCompleted();
        }

        public void Run()
        {
            Task t = new Task(() => LoadImage());
            t.Start();
        }
    }
}
