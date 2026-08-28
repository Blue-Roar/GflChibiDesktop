using Microsoft.Win32;
using System;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Media;
using HandyControl.Controls;
using System.Windows.Data;

namespace GflChibiDesktop
{
    /// <summary>
    /// SettingsWindow.xaml 的交互逻辑
    /// </summary>
    public partial class SettingsWindow : GlowWindow
    {
        private MainWindow _window;
        public SettingsWindow(MainWindow main)
        {
            InitializeComponent();
            _window = main;
            MainWindow.SettingsWindowState(true);
            CheckStartupLaunch();
            //chbSimulation.IsChecked = App.globalValues.Simulation;
            chbSimulation.SetBinding(System.Windows.Controls.Primitives.ToggleButton.IsCheckedProperty, new Binding() { Source = App.globalValues, Path = new PropertyPath("Simulation") });
        }

        private void btn_sources_Click(object sender, RoutedEventArgs e)
        {
            _window.DownloadSources();
        }

        private void btn_CanvasBackground_Click(object sender, RoutedEventArgs e)
        {
            var picker = HandyControl.Tools.SingleOpenHelper.CreateControl<ColorPicker>();
            picker.SelectedBrush = new SolidColorBrush(Color.FromArgb(0, 255, 255, 255));
            if (App.globalValues.CanvasBackground != null)
            {
                picker.SelectedBrush = (SolidColorBrush)App.globalValues.CanvasBackground;
            }
            var window = new PopupWindow
            {
                PopupElement = picker,
                WindowStartupLocation = WindowStartupLocation.CenterScreen,
                AllowsTransparency = true,
                WindowStyle = WindowStyle.None,
                MinWidth = 0,
                MinHeight = 0,
                Title = "画布底色设置"
            };
            picker.Confirmed += delegate { App.globalValues.CanvasBackground = picker.SelectedBrush; window.Close(); };
            picker.Canceled += delegate { window.Close(); };
            window.Show();
        }

        private void CheckStartupLaunch()
        {
            App.globalValues.StartupLaunch = false;
            /* ---检查开机自启动--- */
            try
            {
                if (RegistryKey.OpenBaseKey(RegistryHive.CurrentUser, RegistryView.Default).OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Run").GetValueNames().Contains("GflChibiDesktop"))
                {
                    App.globalValues.StartupLaunch = true;
                }
                else
                {
                    App.globalValues.StartupLaunch = false;
                }
            }
            catch (Exception)
            {

            }
            chbStartupLaunch.IsChecked = App.globalValues.StartupLaunch;
        }

        private void chbStartupLaunch_Checked(object sender, RoutedEventArgs e)
        {
            try
            {
                RegistryKey rk = Registry.CurrentUser;
                RegistryKey rk2 = rk.CreateSubKey(@"Software\Microsoft\Windows\CurrentVersion\Run");
                rk2.SetValue("GflChibiDesktop", $@"{Assembly.GetEntryAssembly().Location} -autorun");
                rk2.Close();
                rk.Close();
            }
            catch (Exception ex)
            {
                Growl.ErrorGlobal($"设置开机启动项时发生错误。{Environment.NewLine}{ex.Message}");
            }
        }

        private void chbStartupLaunch_Unchecked(object sender, RoutedEventArgs e)
        {
            try
            {
                RegistryKey rk = Registry.CurrentUser;
                RegistryKey rk2 = rk.CreateSubKey(@"Software\Microsoft\Windows\CurrentVersion\Run");
                rk2.DeleteValue("GflChibiDesktop", false);
                rk2.Close();
                rk.Close();
            }
            catch (Exception ex)
            {
                Growl.ErrorGlobal($"设置开机启动项时发生错误。{Environment.NewLine}{ex.Message}");
            }
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            MainWindow.SettingsWindowState(false);
        }

        private void pbDevMode_Completed(object sender, RoutedEventArgs e)
        {
            if (pbDevMode.Password == "7355608")
            {
                pbDevMode.IsEnabled = false;
                chbAdvancedSimulation.IsEnabled = true;
            }
            else
            {
                pbDevMode.Password = "";
            }
        }

        private void btn_ResetDummy_Click(object sender, RoutedEventArgs e)
        {
            _window.ResetDummy();
        }

        private void btn_SaveSettings_Click(object sender, RoutedEventArgs e)
        {
            _window.SaveSettings();
        }

        private void chbSimulation_Click(object sender, RoutedEventArgs e)
        {
            _window.toggleSimulation(chbSimulation.IsChecked.Value);
        }
    }
}
