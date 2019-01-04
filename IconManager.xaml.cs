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

namespace SKProCHLauncher
{
    /// <summary>
    /// Логика взаимодействия для IconManager.xaml
    /// </summary>
    public partial class IconManager : Window
    {
        public static IconManager form;
        public IconManager()
        {
            InitializeComponent();
            form = this;
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
        class ListBoxIcons
        {
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
            }));



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
                    System.Windows.MessageBox.Show("Упс, что-то пошло не так. Проверьте путь к файлу!");
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
