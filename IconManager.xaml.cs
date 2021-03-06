﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using Newtonsoft.Json;

namespace SKProCHLauncher
{
    /// <summary>
    ///     Логика взаимодействия для IconManager.xaml
    /// </summary>
    internal class MagicAttribute : Attribute
    {
    }

    [Magic]
    public abstract class PropertyChangedBase : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void RaisePropertyChanged(string propName) {
            var e = PropertyChanged;
            if (e != null)
                e(this, new PropertyChangedEventArgs(propName));
        }
    }


    public partial class IconManager : INotifyPropertyChanged
    {
        public static IconManager form;
        public IconManager() {
            InitializeComponent();
            form        = this;
            DataContext = this;
            InitializeIcons();
        }

        //Load all icons
        private void InitializeIcons() {
            AllIcons.Visibility = Visibility.Visible;
            Dictionary<string, string> Default;
            Dictionary<string, string> Custom;

            try{
                using (var wc = new WebClient()){
                    var JSON = wc.DownloadString(@"https://gdurl.com/u9FJ");
                    Default = JsonConvert.DeserializeObject<Dictionary<string, string>>(JSON);
                }
            }
            catch (Exception){
                MessageBox.Show("Видимо, нет подключения к базе данных иконок. Скорее всего нет подключения к GoogleDrive или GDURL.");
                Default = new Dictionary<string, string>();
                foreach (var item in Directory.GetFiles(Path.Combine(MainWindow.InstallPath, @"\Icons\DefaultIcons\"))){
                    Default.Add(item, Path.GetFileNameWithoutExtension(item));
                }
            }

            Custom = new Dictionary<string, string>();

            if (Directory.Exists(MainWindow.InstallPath + @"\Icons\CustomIcons\")){
                var CustomIconsPath = Directory.GetFiles(MainWindow.InstallPath + @"\Icons\CustomIcons\");
                foreach (var item in CustomIconsPath){
                    Custom.Add(item, Path.GetFileNameWithoutExtension(item));
                }
            }
            else{
                Directory.CreateDirectory(MainWindow.InstallPath + @"\Icons\CustomIcons\");
            }

            form.Dispatcher.Invoke(() => {
                                       form.DefaultIconsListBox.Items.Clear();
                                       form.CustomIconsListBox.Items.Clear();

                                       foreach (var defitem in Default){
                                           form.DefaultIconsListBox.Items.Add(new ListBoxIcons { IconName = defitem.Value, IconPath = defitem.Key });
                                       }
                                       foreach (var cusitem in Custom){
                                           form.CustomIconsListBox.Items.Add(new ListBoxIcons { IconName = cusitem.Value, IconPath = cusitem.Key });
                                       }
                                       form.CustomIconsListBox.Items.Add(new ListBoxIcons { IconPath = @"https://gdurl.com/M8Ps", IconName = "Add icon" });

                                       form.AllIcons.Visibility = Visibility.Visible;
                                   });
            if (!form.IsVisible) form.Show();
        }

        private void DefaultIconsListBox_MouseLeftButtonDown(object sender, MouseButtonEventArgs e) {
            CustomIconsListBox.SelectedIndex = -1;
            if (e.ClickCount == 2)
                CloseIconsForm((DefaultIconsListBox.SelectedItem as ListBoxIcons).IconPath, (DefaultIconsListBox.SelectedItem as ListBoxIcons).IconName, false);
        }

        private void CustomIconsListBox_MouseLeftButtonDown(object sender, MouseButtonEventArgs e) {
            DefaultIconsListBox.SelectedIndex = -1;
            if (e.ClickCount >= 2){
                if (CustomIconsListBox.SelectedIndex != CustomIconsListBox.Items.Count - 1)
                    CloseIconsForm((DefaultIconsListBox.SelectedItem as ListBoxIcons).IconPath, (DefaultIconsListBox.SelectedItem as ListBoxIcons).IconName, false);
                else
                    AddCustomIcon();
            }
        }

        private static void CloseIconsForm(string url, string name, bool isCustom) {
            form.AllIcons.Visibility = Visibility.Collapsed;

            if (isCustom){
                //
            }
            else{
                using (var wc = new WebClient()){
                    wc.DownloadFile(url, MainWindow.InstallPath + @"\Icons\DefaultIcons\" + name + "." + url.Remove(0, url.LastIndexOf("." + 1)));
                }
            }
            //
        }

        private static void AddCustomIcon() {
            form.AllIcons.Visibility          = Visibility.Collapsed;
            form.AddCustomIconForm.Visibility = Visibility.Visible;
            form.IconName.Text                = "";
            form.IconPath.Text                = "";
        }

        //Добавление новой иконки
        private void AddCustomIconButton_Click(object sender, RoutedEventArgs e) {
            var temp_IsError = false;
            try{
                File.Copy(form.IconPath.Text, MainWindow.InstallPath + @"\Icons\CustomIcons\" + form.IconName);
            }
            catch (Exception){
                try{
                    using (var wc = new WebClient()){
                        wc.DownloadFile(new Uri(form.IconPath.Text), Path.Combine(MainWindow.InstallPath, @"Icons\CustomIcons", form.IconName.Text + ".skplaucnhericon"));
                    }
                }
                catch (Exception){
                    MessageBox.Show("Что-то не так с файлом. Проверьте еще раз");
                    form.AllIcons.Visibility          = Visibility.Collapsed;
                    form.AddCustomIconForm.Visibility = Visibility.Visible;
                    temp_IsError                      = true;
                }
            }

            if (!temp_IsError){
                form.AddCustomIconForm.Visibility = Visibility.Collapsed;
                InitializeIcons();
            }
        }

        private void CheckInstallAvailable(object sender, TextChangedEventArgs e) {
            if (IsImageLoaded){
                if (IconName.Text != "")
                    AddCustomIconButton.IsEnabled = true;
                else
                    AddCustomIconButton.IsEnabled = false;
            }
            else{
                AddCustomIconButton.IsEnabled = false;
            }
        }

        private class ListBoxIcons
        {
            public string IconPath { get; set; }
            public string IconName { get; set; }
        }

        #region ImageLoading

        private bool IsImageLoaded
        {
            set
            {
                _IsImageLoaded = value;
                CheckInstallAvailable(form, null);
            }
            get => _IsImageLoaded;
        }
        private bool        _IsImageLoaded;
        private string      _IconPathString = "";
        private ImageLoader Loader;

        public BitmapImage IconImageSource
        {
            get
            {
                if (Loader != null) return Loader.Image;
                return null;
            }
        }

        private string _ImageStatusText = "";
        public string ImageStatusText
        {
            get => _ImageStatusText;
            set
            {
                _ImageStatusText = value;
                OnPropertyChanged("ImageStatusText");
            }
        }

        public string IconPathString
        {
            get => _IconPathString;

            set
            {
                try{
                    if (Loader != null){
                        Loader.LoadCompleted -= ImageLoadCompleted;
                        Loader               =  null;
                    }

                    if (string.IsNullOrEmpty(value)){
                        _IconPathString = "";
                        OnPropertyChanged("IconImageSource");
                        IsImageLoaded = false;
                        return;
                    }

                    var uri = new Uri(value);
                    Loader               =  new ImageLoader(uri);
                    Loader.LoadCompleted += ImageLoadCompleted;
                    Loader.Run();
                    _IconPathString = value;
                    IsImageLoaded   = false;
                }
                catch (Exception ex){
                    Debug.WriteLine(ex.ToString());
                    _IconPathString = "";
                    OnPropertyChanged("IconImageSource");
                    IsImageLoaded = false;
                }
            }
        }

        private void ImageLoadCompleted(object sender, EventArgs e) {
            Dispatcher.Invoke(() => {
                                  if ((sender as ImageLoader).Image != null){
                                      IsImageLoaded = true;
                                  }
                                  else{
                                      _IconPathString = "";
                                      IsImageLoaded   = false;
                                  }

                                  OnPropertyChanged("IconImageSource");
                                  (sender as ImageLoader).LoadCompleted -= ImageLoadCompleted;
                              });
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged(string name) {
            var handler = PropertyChanged;
            if (handler != null) handler(this, new PropertyChangedEventArgs(name));
        }

        private void IconPath_TextChanged(object sender, TextChangedEventArgs e) {
            //IconPath.Text = IconPath.Text.Remove(0, 20);
        }
    }

    public class ImageLoader
    {
        private Uri _ImageUri;

        public ImageLoader(Uri uri) {
            _ImageUri = uri;
        }

        public BitmapImage Image { get; private set; }

        public event EventHandler LoadCompleted;

        private void OnLoadCompleted() {
            if (LoadCompleted != null) LoadCompleted(this, new EventArgs());
        }

        private void LoadImage() {
            BitmapImage bi;
            try{
                byte[] data;

                if (_ImageUri.IsFile){
                    data = File.ReadAllBytes(_ImageUri.LocalPath);
                }
                else{
                    var client = new WebClient();
                    using (client){
                        data = client.DownloadData(_ImageUri);
                    }
                }

                var ms = new MemoryStream(data);

                bi = new BitmapImage();
                bi.BeginInit();
                bi.CacheOption  = BitmapCacheOption.OnLoad;
                bi.StreamSource = ms;
                bi.EndInit();
                bi.Freeze();
            }
            catch (Exception ex){
                Debug.WriteLine(ex.ToString());
                bi = null;
            }
            Image = bi;
            OnLoadCompleted();
        }

        public void Run() {
            var t = new Task(() => LoadImage());
            t.Start();
        }
    }

    #endregion
}
