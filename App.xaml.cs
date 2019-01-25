using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Interop;
using Microsoft.Win32;
using Newtonsoft.Json;
using Application = System.Windows.Application;
using MessageBox = System.Windows.MessageBox;

namespace SKProCHLauncher
{
    /// <summary>
    ///     Логика взаимодействия для App.xaml
    /// </summary>
    public partial class App : Application
    {
        [STAThread]
        public static void Main() {
            foreach (var item in Environment.GetCommandLineArgs()){
                if (item.Contains("skpmclaucnher:")){
                    var temp                      = JsonConvert.DeserializeObject<List<string>>(item.Replace("skpmclaucnher://", ""));
                    var AvailableModpacksRegistry = Registry.LocalMachine;
                    AvailableModpacksRegistry = AvailableModpacksRegistry.OpenSubKey("SOFTWARE", true);
                    AvailableModpacksRegistry = AvailableModpacksRegistry.CreateSubKey("SKProCHsLauncher", true);

                    var AllAvailableModpacks =
                        JsonConvert.DeserializeObject<List<AvailableModpack>>(
                                                                              Convert.ToString(AvailableModpacksRegistry.GetValue("AvailableModpacks", null)));
                    if (AllAvailableModpacks == null)
                        AllAvailableModpacks = new List<AvailableModpack>();
                    foreach (var ManifestsURL in temp){
                        var MpInfoToAdd = new ModpackInfo();
                        try{
                            using (var wc = new WebClient()){
                                MpInfoToAdd =
                                    JsonConvert.DeserializeObject<ModpackInfo>(wc.DownloadString(ManifestsURL));
                            }

                            AllAvailableModpacks.Add(new AvailableModpack
                                                     {
                                                         PathToManifest = ManifestsURL,
                                                         Icon           = MpInfoToAdd.Icon,
                                                         McVersion      = MpInfoToAdd.MCVersion,
                                                         Name           = MpInfoToAdd.ModpackName,
                                                         Server         = MpInfoToAdd.ModpackServer,
                                                         ID             = AllAvailableModpacks[AllAvailableModpacks.Count - 1].ID + 1
                                                     });
                        }
                        catch (Exception ex){
                            MessageBox.Show("При добавлении сборки возникла ошибка: " + ex.Message);
                        }
                    }

                    AvailableModpacksRegistry.SetValue("AvailableModpacks",
                                                       JsonConvert.SerializeObject(AllAvailableModpacks));
                }
            }

            bool   existed;
            string guid = Marshal.GetTypeLibGuidForAssembly(Assembly.GetExecutingAssembly()).ToString();

            Mutex mutexObj = new Mutex(true, guid, out existed);

            if (existed){
                var application = new App();
                application.InitializeComponent();
                application.Run();
            }
            else{
                Process this_process = Process.GetCurrentProcess();

                //найти все процессы с таким же именем
                Process[] other_processes =
                    Process.GetProcessesByName(this_process.ProcessName).Where(pr => pr.Id != this_process.Id).ToArray();

                foreach (var pr in other_processes)
                {
                    pr.WaitForInputIdle(1000); //на случай, если процесс еще не загрузился

                    //берем первый процесс с окном
                    IntPtr hWnd = pr.MainWindowHandle;
                    if (hWnd == IntPtr.Zero) continue;

                    //отправляем команду
                    string command = "Hello";
                    var    cds     = new COPYDATASTRUCT();
                    cds.dwData = (IntPtr)1;
                    cds.cbData = (command.Length + 1) * 2;
                    cds.lpData = Marshal.StringToHGlobalUni(command);
                    SendMessage(hWnd, WM_COPYDATA, IntPtr.Zero, ref cds);
                    Marshal.FreeHGlobal(cds.lpData);

                    //завершаем работу
                    MessageBox.Show("Отправлено!");
                    Environment.Exit(0);
                }
            }
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


        public static IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            if (msg == WM_COPYDATA)
            {
                COPYDATASTRUCT data = new COPYDATASTRUCT();
                data = (COPYDATASTRUCT)Marshal.PtrToStructure(lParam, data.GetType());
                MessageBox.Show("Received command: " + Marshal.PtrToStringUni(data.lpData));
            }

            return IntPtr.Zero;
        }

    }
}
