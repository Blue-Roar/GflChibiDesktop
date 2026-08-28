using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using GflChibiDesktop.Views;
using Microsoft.Xna.Framework;
using GflChibiDesktop.Windows;
using System.Runtime.InteropServices;
using System.Windows.Interop;
using System.Text;
using static GflChibiDesktop.WebAPI;
using static GflChibiDesktop.WebVerification;
using Newtonsoft.Json;
using Microsoft.Win32;
using System.Security.Principal;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Linq;
using System.Windows.Data;
using System.IO;
using HandyControl.Data;

namespace GflChibiDesktop
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    /// 

    public partial class MainWindow : Window
    {
        [DllImport("user32", EntryPoint = "SetWindowLong")]
        private static extern uint SetWindowLong(IntPtr hwnd, int nIndex, long dwNewLong);

        [DllImport("user32", EntryPoint = "GetWindowLong")]
        private static extern uint GetWindowLong(IntPtr hwnd, int nIndex);

        public long OldLong;

        public static MainWindow MasterMain;
        public static ContentControl MasterControl;
        public static UCPlayer UC_Player;
        public static OptionsWindow _options;
        public static DataManagerWindow dataManagerWindow;
        public static SettingsWindow _settings;
        
        public readonly string productName = ((AssemblyProductAttribute)Attribute.GetCustomAttribute(Assembly.GetExecutingAssembly(), typeof(AssemblyProductAttribute))).Product.ToString();
        public readonly string productTitle = ((AssemblyTitleAttribute)Attribute.GetCustomAttribute(Assembly.GetExecutingAssembly(), typeof(AssemblyTitleAttribute))).Title.ToString();
        public readonly string productDescription = ((AssemblyDescriptionAttribute)Attribute.GetCustomAttribute(Assembly.GetExecutingAssembly(), typeof(AssemblyDescriptionAttribute))).Description.ToString();
        public readonly string productCopyright = ((AssemblyCopyrightAttribute)Attribute.GetCustomAttribute(Assembly.GetExecutingAssembly(), typeof(AssemblyCopyrightAttribute))).Copyright.ToString();
        public readonly string productCompany = ((AssemblyCompanyAttribute)Attribute.GetCustomAttribute(Assembly.GetExecutingAssembly(), typeof(AssemblyCompanyAttribute))).Company.ToString();
        public readonly Version productVersion = new Version(((AssemblyFileVersionAttribute)Attribute.GetCustomAttribute(Assembly.GetExecutingAssembly(), typeof(AssemblyFileVersionAttribute))).Version);
        public readonly Version productBuild = Assembly.GetExecutingAssembly().GetName().Version;
        public readonly string currentBuild = ((AssemblyInformationalVersionAttribute)Attribute.GetCustomAttribute(Assembly.GetExecutingAssembly(), typeof(AssemblyInformationalVersionAttribute))).InformationalVersion;
        public string homepageLink = "https://projects.brightsu.cn/GflChibiDesktop/V1/";
        public string repoLink = "https://github.com/Blue-Roar/GflChibiDesktop";
        public string updateLink = "https://projects.brightsu.cn/GflChibiDesktop/V1/download";
        public string donateLink = "https://projects.brightsu.cn/GflChibiDesktop/donate";
        public string extraStr = string.Empty;
        public string announcementMsg = string.Empty;
        public string chibiListLink = "https://api.brightsu.cn/GFL/chibi_list";

        /// <summary>当前正在使用的模型目录键（"spine/xxx" 或 "spine_external/xxx"），用于禁止删除正在使用的数据。</summary>
        private string _loadedPathKey;

        System.Windows.Threading.DispatcherTimer dispatcherTimer = new System.Windows.Threading.DispatcherTimer();

        System.Windows.Threading.DispatcherTimer timerEventsSimulation = new System.Windows.Threading.DispatcherTimer();
        System.Windows.Threading.DispatcherTimer timerSimulationMoveX = new System.Windows.Threading.DispatcherTimer();
        System.Windows.Threading.DispatcherTimer timerSimulationS = new System.Windows.Threading.DispatcherTimer();
        System.Windows.Threading.DispatcherTimer timerSimulationVictory = new System.Windows.Threading.DispatcherTimer();

        public MainWindow()
        {

            //HandyControl.Controls.NotifyIcon.MouseDoubleClickEvent;
            InitializeComponent();

            Game game = new Game();
            _options = new OptionsWindow(this);
            _options.Show();
            Title = $"{productTitle}";
            MasterMain = this;
            dispatcherTimer.Tick += dispatcherTimer_Tick;
            dispatcherTimer.Interval = new TimeSpan(0, 0, 0, 0, 100);
            dispatcherTimer.Start();

            App.globalValues.EnableInteraction = true;

            timerEventsSimulation.Interval = new TimeSpan(0, 0, 0, 30);
            timerEventsSimulation.Tick += timerEventsSimulation_Tick;

            timerSimulationMoveX.Interval = new TimeSpan(0, 0, 0, 0, 10);
            timerSimulationMoveX.Tick += timerSimulationMoveX_Tick;

            timerSimulationS.Interval = new TimeSpan(0, 0, 0, (int)App.globalValues.AnimeDuration);
            timerSimulationS.Tick += timerSimulationS_Tick;

            timerSimulationVictory.Interval = new TimeSpan(0, 0, 0, (int)App.globalValues.AnimeDuration);
            timerSimulationVictory.Tick += timerSimulationVictory_Tick;

            UpdateSpine();

            bool StartupPost = false;
            string StartupStr = HttpRequestHelper.PostWebRequest("https://api.brightsu.cn/GflChibiDesktop/startup", $"build={productBuild}", Encoding.UTF8, ref StartupPost);
            if (StartupPost)
            {
                StartupRoot rt = JsonConvert.DeserializeObject<StartupRoot>(StartupStr);
                if (rt.ret != 200)
                {
                    HandyControl.Controls.Growl.WarningGlobal($"API接口调用失败。部分功能可能会受到影响。\n错误：API 接口返回了 HTTP 状态码 {rt.ret}");
                }
            }
            else
            {
                HandyControl.Controls.Growl.WarningGlobal($"API接口调用失败。部分功能可能会受到影响。\n{StartupStr}");
            }

            IntPtr hwnd = new WindowInteropHelper(this).Handle;
            OldLong = GetWindowLong(hwnd, -20);
            LoadSettings();
            _options.LoadSetting();
            string[] pargs = Environment.GetCommandLineArgs();
            if (pargs.Contains("-silent")) { silentRun = true; }
            if (pargs.Contains("-autorun")) { startupAutoRun = true; }
            UpdateLinks();
            CheckForUpdates();


            SetBinding(OpacityProperty, new Binding() { Source = App.globalValues, Path = new PropertyPath("Opacity"), Mode = BindingMode.OneWay });
            //SetBinding(WidthProperty, new Binding() { Source = App.globalValues, Path = new PropertyPath("FrameWidth"), Mode = BindingMode.OneWay });
            //SetBinding(HeightProperty, new Binding() { Source = App.globalValues, Path = new PropertyPath("FrameHeight"), Mode = BindingMode.OneWay });
            GridPlayer.SetBinding(BackgroundProperty, new Binding() { Source = App.globalValues, Path = new PropertyPath("CanvasBackground"), Mode = BindingMode.OneWay });

        }

        public double GetDPI()
        {
            PresentationSource source = PresentationSource.FromVisual(this);

            double dpiX = 1, dpiY = 1;

            if (source != null)
            {
                dpiX = source.CompositionTarget.TransformToDevice.M11;
                dpiY = source.CompositionTarget.TransformToDevice.M22;
            }

            if (dpiX != 1)
            {
                HandyControl.Controls.Growl.InfoGlobal($"当前 DPI 缩放大小为 {Math.Round(dpiX, 2) * 100}%。\n画布显示可能异常。");
            }

            return dpiX;
        }

        bool startupAutoRun = false;
        bool silentRun = false;

        public static bool DataManagerWindowIsOpen = false;
        public static void DataManagerWindowState(bool state)
        {
            DataManagerWindowIsOpen = state;
        }

        public static bool AboutWindowIsOpen = false;
        public static void AboutWindowState(bool state)
        {
            AboutWindowIsOpen = state;
        }

        public static bool SettingsWindowIsOpen = false;
        public static void SettingsWindowState(bool state)
        {
            SettingsWindowIsOpen = state;
        }

        private void UpdateLinks()
        {
            try
            {
                bool IndexPost = false;
                string IndexStr = HttpRequestHelper.PostWebRequest("https://api.brightsu.cn/GflChibiDesktop", string.Empty, Encoding.UTF8, ref IndexPost);
                if (IndexPost)
                {
                    IndexRoot rt = JsonConvert.DeserializeObject<IndexRoot>(IndexStr);
                    if (rt.ret == 200)
                    {
                        if (CheckIsUrlFormat(rt.data.homepage_link)) { homepageLink = rt.data.homepage_link; }
                        if (CheckIsUrlFormat(rt.data.update_link)) { updateLink = rt.data.update_link; }
                        if (CheckIsUrlFormat(rt.data.donate_link)) { donateLink = rt.data.donate_link; }
                        if (CheckIsUrlFormat(rt.data.repo_link)) { repoLink = rt.data.repo_link; }
                        if (CheckIsUrlFormat(rt.data.chibi_list_link)) { chibiListLink = rt.data.chibi_list_link; }
                        extraStr = rt.data.extra_str;
                    }
                    else
                    {
                        HandyControl.Controls.Growl.ErrorGlobal($"API 接口调用失败，链接更新失败。\n部分功能可能会受到影响。\n错误：API 接口返回了状态码 {rt.ret}");
                    }
                }
                else
                {
                    HandyControl.Controls.Growl.ErrorGlobal($"API 接口调用失败，链接更新失败。\n部分功能可能会受到影响。\n错误：{IndexStr}");
                }
            }
            catch (Exception ex)
            {
                HandyControl.Controls.Growl.ErrorGlobal($"API 接口调用失败，链接更新失败。\n部分功能可能会受到影响。\n错误：{ex}");
                return;
            }
        }

        private void CheckForUpdates()
        {
            try
            {
                bool UpdatePost = false;
                string UpdateStr = HttpRequestHelper.PostWebRequest("https://api.brightsu.cn/GflChibiDesktop/update", string.Empty, Encoding.UTF8, ref UpdatePost);
                if (!UpdatePost)
                {
                    return;
                }
                UpdateRoot rt = JsonConvert.DeserializeObject<UpdateRoot>(UpdateStr);

                if (rt.ret != 200)
                {
                    return;
                }
                if (rt.data.version != null)
                {
                    Version latestBuild = new Version(rt.data.buildver);
                    bool urgentUpdate = false;
                    if (rt.data.urgent == 1) { urgentUpdate = true; }
                    if (latestBuild > productBuild)
                    {
                        if (urgentUpdate)
                        {
                            if (silentRun)
                            {
                                //new PopupWindow().DisplayDialog("autorun", "info");
                            }
                            else if (startupAutoRun)
                            {
                                HandyControl.Controls.Growl.AskGlobal(new HandyControl.Data.GrowlInfo
                                {
                                    Message = $"主程序有新版本可用。\n当前版本：{productVersion}\n最新版本：{rt.data.version}\n\n是否前往更新？",
                                    CancelStr = "不用了",
                                    ConfirmStr = "前往更新",
                                    ActionBeforeClose = isConfirmed =>
                                    {
                                        if (isConfirmed)
                                        {
                                            ShowAbout();
                                        }
                                        return true;
                                    },
                                });
                            }
                            else
                            {
                                ShowAbout();
                            }
                        }
                    }
                }
            }
            catch (Exception)
            {
                return;
            }
        }



        public void LoadDummy()
        {
            if (!DataManagerWindowIsOpen)
            {
                dataManagerWindow = new DataManagerWindow();
                dataManagerWindow.OwnerMainWindow = this;
                dataManagerWindow.LoadedPaths = GetLoadedPaths();
                dataManagerWindow.ModelLoadRequested += DataManagerWindow_ModelLoadRequested;
                dataManagerWindow.homepageLink = homepageLink;
                dataManagerWindow.repoLink = repoLink;
                dataManagerWindow.updateLink = updateLink;
                dataManagerWindow.chibiListLink = chibiListLink;
                dataManagerWindow.announcementMsg = string.IsNullOrEmpty(announcementMsg) ? productTitle : announcementMsg;
                dataManagerWindow.Closed += (s, e) => DataManagerWindowState(false);
                DataManagerWindowState(true);
                dataManagerWindow.Show();
            }
            else
            {
                dataManagerWindow.Show();
                dataManagerWindow.WindowState = WindowState.Normal;
                dataManagerWindow.Focus();
            }
        }

        /// <summary>
        /// 数据管理器触发模型加载：自动检测 Spine 版本并加载到桌宠。
        /// </summary>
        private void DataManagerWindow_ModelLoadRequested(ChibiModelData data)
        {
            try
            {
                string appDir = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resources");
                string atlas = data.AtlasFile;
                string skel = data.SkeletonFile;
                if (!System.IO.Path.IsPathRooted(atlas)) atlas = System.IO.Path.Combine(appDir, atlas);
                if (!System.IO.Path.IsPathRooted(skel)) skel = System.IO.Path.Combine(appDir, skel);

                if (!System.IO.File.Exists(atlas) || !System.IO.File.Exists(skel))
                {
                    HandyControl.Controls.Growl.ErrorGlobal("加载失败：骨骼数据文件不存在。");
                    return;
                }

                App.globalValues.SelectAtlasFile = atlas;
                App.globalValues.SelectSpineFile = skel;
                App.globalValues.DummyDisplayName = data.DisplayName;
                App.globalValues.IsDormMode = System.IO.Path.GetFileNameWithoutExtension(atlas).StartsWith("r");

                // 持久化所选模型，供启动时恢复
                string dir = new System.IO.DirectoryInfo(System.IO.Path.GetDirectoryName(atlas)).Name;
                string file = System.IO.Path.GetFileNameWithoutExtension(atlas);
                bool isDorm = file.StartsWith("r");
                string baseName = isDorm ? file.Substring(1) : file;
                Properties.Settings.Default.DummyPath = dir;
                Properties.Settings.Default.DummyFilename = baseName;
                Properties.Settings.Default.DummyFilenameR = "r" + baseName;
                Properties.Settings.Default.DummyDormMode = isDorm;
                Properties.Settings.Default.DummyName = dir;
                Properties.Settings.Default.DummyDisplayName = data.DisplayName;
                Properties.Settings.Default.Save();

                App.globalValues.Dummy = dir;
                App.isNew = true;
                _loadedPathKey = ComputeLoadedKey(atlas);

                LoadPlayer("2.1.25");
            }
            catch (Exception ex)
            {
                HandyControl.Controls.Growl.ErrorGlobal($"加载失败。\n{ex.Message}");
            }
        }

        /// <summary>
        /// 计算模型目录键（"spine/xxx" 或 "spine_external/xxx"）。
        /// </summary>
        private static string ComputeLoadedKey(string atlasPath)
        {
            try
            {
                // 形如 ...\Resources\spine\path\file.atlas 或 ...\Resources\spine_external\path\file.atlas
                string[] parts = atlasPath.Split('\\', '/');
                for (int i = 0; i < parts.Length - 2; i++)
                {
                    if ((parts[i] == "spine" || parts[i] == "spine_external") && i + 1 < parts.Length)
                        return $"{parts[i]}/{parts[i + 1]}";
                }
            }
            catch
            {
            }
            return null;
        }

        /// <summary>
        /// 当前正在使用的模型目录键集合。
        /// </summary>
        public HashSet<string> GetLoadedPaths()
        {
            var set = new HashSet<string>();
            if (!string.IsNullOrEmpty(_loadedPathKey))
            {
                set.Add(_loadedPathKey);
            }
            return set;
        }

        private void Move_MouseMove(object sender, MouseEventArgs e)
        {
            string oldSelectedAnime = App.globalValues.SelectAnimeName;
            if (oldSelectedAnime == string.Empty) { oldSelectedAnime = "wait"; }
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                if (App.globalValues.AnimeList.Contains("pick"))
                {
                    App.globalValues.SelectAnimeName = "pick";
                    App.globalValues.SetAnime = true;
                    App.globalValues.Simulation_Moving = false;
                    this.DragMove();
                    if (App.globalValues.Simulation == true)
                    {
                        App.globalValues.SelectAnimeName = "wait";
                        App.globalValues.SetAnime = true;
                    }
                    else
                    {
                        App.globalValues.SelectAnimeName = oldSelectedAnime;
                        App.globalValues.SetAnime = true;
                    }
                }
                else
                {
                    this.DragMove();
                }
            }
        }


        private void LoadSettings()
        {
            if (App.globalValues.Scale == 0)
                App.globalValues.Scale = 1;

            if (Properties.Settings.Default.LastSelectDir == string.Empty)
            {
                App.lastDir = App.rootDir;
            }
            else
            {
                App.lastDir = Properties.Settings.Default.LastSelectDir;
            }
            string[] tagString = new string[12];
            tagString[6] = Properties.Settings.Default.DummyPath;
            tagString[7] = Properties.Settings.Default.DummyFilename;
            tagString[10] = Properties.Settings.Default.DummyFilenameR;

            App.globalValues.FrameWidth = Properties.Settings.Default.CanvasSize.Width;
            App.globalValues.FrameHeight = Properties.Settings.Default.CanvasSize.Height;
            App.canvasWidth = Properties.Settings.Default.CanvasSize.Width;
            App.canvasHeight = Properties.Settings.Default.CanvasSize.Height;

            App.globalValues.Dummy = Properties.Settings.Default.DummyName;
            App.globalValues.DummyDisplayName = Properties.Settings.Default.DummyDisplayName;
            App.globalValues.IsDormMode = Properties.Settings.Default.DummyDormMode;

            App.globalValues.Alpha = true;
            App.globalValues.PreMultiplyAlpha = true;
            App.globalValues.IsLoop = true;

            string spineDir = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resources", "spine");
            string atlas = $@"{spineDir}\{tagString[6]}\{tagString[7]}.atlas";
            string skel = $@"{spineDir}\{tagString[6]}\{tagString[7]}.skel";
            if (App.globalValues.IsDormMode)
            {
                if (File.Exists($@"{spineDir}\{tagString[6]}\{tagString[10]}.atlas"))
                { atlas = $@"{spineDir}\{tagString[6]}\{tagString[10]}.atlas"; }
                if (File.Exists($@"{spineDir}\{tagString[6]}\{tagString[10]}.skel"))
                { skel = $@"{spineDir}\{tagString[6]}\{tagString[10]}.skel"; }
            }
            App.globalValues.SelectAtlasFile = atlas;
            App.globalValues.SelectSpineFile = skel;

            // 仅当数据存在时加载上次选择的模型（避免数据缺失导致启动失败）
            if (File.Exists(atlas) && File.Exists(skel))
            {
                _loadedPathKey = ComputeLoadedKey(atlas);
                LoadPlayer("2.1.25");
            }

            // LoadPlayer 内 Common.Reset() 会清空以下设置，需在加载之后设置
            App.globalValues.SelectSkin = "default";
            App.globalValues.SetSkin = true;
            App.globalValues.SelectedAnime = Properties.Settings.Default.DummyAnime;
            App.globalValues.SelectAnimeName = Properties.Settings.Default.DummyAnime;
            App.globalValues.SetAnime = true;

            App.globalValues.Scale = Properties.Settings.Default.DummyScale;
            App.globalValues.FilpX = false;
            App.globalValues.FilpY = false;
            App.globalValues.PosX = Properties.Settings.Default.DummyPos.X;
            App.globalValues.PosY = Properties.Settings.Default.DummyPos.Y;
            App.globalValues.DownloadSource = Properties.Settings.Default.DownloadSource;

            App.mainWidth = ActualWidth;
            App.mainHeight = ActualHeight;
        }

        public void SaveSettings()
        {
            Properties.Settings.Default.LastSelectDir = App.lastDir;

            Properties.Settings.Default.Topmost = Topmost;
            Properties.Settings.Default.DummyOpacity = Opacity;
            Properties.Settings.Default.Pos = new System.Drawing.Point((int)Left, (int)Top);
            Properties.Settings.Default.CanvasSize = new System.Drawing.Size((int)App.globalValues.FrameWidth, (int)App.globalValues.FrameHeight);

            if (App.globalValues.Dummy != "(external)")
            {
                Properties.Settings.Default.DummyName = App.globalValues.Dummy;
                Properties.Settings.Default.DummyDisplayName = App.globalValues.DummyDisplayName;
                Properties.Settings.Default.DummyDormMode = App.globalValues.IsDormMode;
                Properties.Settings.Default.DummyAnime = App.globalValues.SelectAnimeName;
            }

            Properties.Settings.Default.DummyScale = App.globalValues.Scale;
            //App.globalValues.FilpX = false;
            //App.globalValues.FilpY = false;
            Properties.Settings.Default.DummyPos = new System.Drawing.Point((int)App.globalValues.PosX, (int)App.globalValues.PosY);
            Properties.Settings.Default.DummySimulation = App.globalValues.Simulation;
            Properties.Settings.Default.DownloadSource = App.globalValues.DownloadSource;
            //Properties.Settings.Default.DummyTag = App.globalValues.DummyTag;

            Properties.Settings.Default.Save();
        }

        public void UpdateSpine()
        {
            if (UC_Player != null)
            {
                UC_Player.ChangeSet();
            }
        }

        public static void SetCBAnimeName()
        {
            var brush = new SolidColorBrush();
            brush.Opacity = 0;
            App.appXC.Background = brush;
            _options.cb_AnimeList.Items.Clear();
            for (int i = 0; i < App.globalValues.AnimeList.Count; i++)
            {
                _options.cb_AnimeList.Items.Add(App.globalValues.AnimeList[i]);
            }
            //for (int i = 0; i < App.globalValues.SkinList.Count; i++)
            //{
            //    _options.cb_SkinList.Items.Add(App.globalValues.SkinList[i]);
            //}
        }

        private void Window_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            WindowState = WindowState.Normal;

            App.mainWidth = ActualWidth;
            App.mainHeight = ActualHeight;
            //Player.Width = Math.Round(GridPlayer.ColumnDefinitions[1].ActualWidth + 60 - 2, 2);
            //Player.Height = Math.Round(this.ActualHeight - 60, 2); 
            //Player.Width = 384;
            //Player.Height = 384;
        }

        public void LoadPlayer(string spineVersion)
        {
            Common.Reset();

            _options.cb_AnimeList.Items.Clear();
            //_options.cb_SkinList.Items.Clear();
            Player.Width = App.canvasWidth;
            Player.Height = App.canvasHeight;
            Width = Player.Width / matrixDPI.M11;
            Height = Player.Height / matrixDPI.M22;

            if (Player.Content != null)
            {
                if (App.globalValues.SelectSpineVersion != spineVersion)
                {
                    App.globalValues.SelectSpineVersion = spineVersion;
                    App.isNew = true;
                    App.appXC.ContentManager.Dispose();
                    App.appXC.Initialize = null;
                    App.appXC.Update = null;
                    App.appXC.LoadContent = null;
                    App.appXC.Draw = null;

                    DependencyObject xnaParent = ((UserControl)Player.Content).Parent;
                    if (xnaParent != null)
                    {
                        xnaParent.SetValue(ContentPresenter.ContentProperty, null);
                    }
                    Canvas oldCanvas = (Canvas)App.appXC.Parent;
                    if (oldCanvas != null)
                    {
                        oldCanvas.Children.Clear();
                    }
                    Player.Content = null;
                    UC_Player = new UCPlayer();
                    Player.Content = UC_Player;
                    App.appXC.RequestReload();
                }
                else
                {
                    UC_Player.Reload();
                }
            }
            else
            {
                App.globalValues.SelectSpineVersion = spineVersion;
                UC_Player = new UCPlayer();
                Player.Content = UC_Player;
            }

        }

        private void Window_Closed(object sender, EventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            e.Cancel = true;
        }

        public void Shutdown()
        {
            Application.Current.Shutdown();
        }

        private void Window_GotFocus(object sender, RoutedEventArgs e)
        {
            if (App.graphicsDevice != null && App.graphicsDevice.GraphicsDeviceStatus == Microsoft.Xna.Framework.Graphics.GraphicsDeviceStatus.NotReset)
            {
                App.graphicsDevice.Reset();
            }
        }


        private void dispatcherTimer_Tick(object sender, EventArgs e)
        {
            if (App.graphicsDevice != null && App.graphicsDevice.GraphicsDeviceStatus == Microsoft.Xna.Framework.Graphics.GraphicsDeviceStatus.NotReset)
            {
                App.graphicsDevice.Reset();
            }
        }

        private void Window_IsHitTestVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            try
            {
                if (IsHitTestVisible)
                {
                    MasterGrid.IsHitTestVisible = true;
                    MainContent.IsHitTestVisible = true;
                    GridPlayer.IsHitTestVisible = true;
                    Player.IsHitTestVisible = true;

                    IntPtr hwnd = new WindowInteropHelper(this).Handle;
                    SetWindowLong(hwnd, (-20), OldLong);
                }
                else
                {
                    MasterGrid.IsHitTestVisible = false;
                    MainContent.IsHitTestVisible = false;
                    GridPlayer.IsHitTestVisible = false;
                    Player.IsHitTestVisible = false;

                    IntPtr hwnd = new WindowInteropHelper(this).Handle;
                    SetWindowLong(hwnd, (-20), 0x20);
                }
            }
            catch (Exception)
            {

            }
        }

        public void ShowOptions()
        {
            _options.Show();
            //_options.Left = Left + Mouse.GetPosition(this).X - (_options.ActualWidth / 2);
            //_options.Top = Top + Mouse.GetPosition(this).Y - (_options.ActualHeight / 2);
            _options.Focus();
        }

        public void ShowSettings()
        {
            if (!SettingsWindowIsOpen)
            {
                _settings = new SettingsWindow(this);
                _settings.Show();
            }
            else
            {
                _settings.Show();
                _settings.WindowState = WindowState.Normal;
                _settings.Focus();
            }
        }

        private void Window_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            ShowOptions();
        }

        public void toggleSimulation(bool toggleSwitch)
        {
            if (toggleSwitch)
            {
                App.globalValues.Simulation = true;
                timerEventsSimulation.Start();
                timerEventsSimulation_Tick(this, EventArgs.Empty);
                App.globalValues.Speed = 30;
            }
            else
            {
                App.globalValues.Simulation = false;
                timerEventsSimulation.Stop();
                StopMove();
            }
        }

        public void setSimulationInterval(double intervalsec)
        {
            timerEventsSimulation.Interval = new TimeSpan(0, 0, 0, (int)intervalsec);
        }

        private void timerEventsSimulation_Tick(object sender, EventArgs e)
        {
            StopMove();
            timerSimulationMoveX.Stop();
            timerSimulationReload.Stop();
            timerSimulationS.Stop();
            timerSimulationVictory.Stop();
            Random rand = new Random();
            if (App.globalValues.IsDormMode)
            {
                App.globalValues.IsLoop = true;
                UpdateSpine();
                int i = rand.Next(1, 11);
                //Text = $"{i}";
                switch (i)
                {
                    // universal
                    case 1:
                        eventSimulation_wait();
                        break;
                    case 2:
                        eventSimulation_forward();
                        break;
                    case 3:
                        eventSimulation_wait();
                        break;
                    case 4:
                        eventSimulation_backward();
                        break;
                    case 5:
                        eventSimulation_wait();
                        break;
                    // dormitory
                    case 6:
                        eventSimulation_lie();
                        break;
                    case 7:
                        eventSimulation_sit();
                        break;
                    case 8:
                        eventSimulation_lie();
                        break;
                    case 9:
                        eventSimulation_sit();
                        break;
                    case 10:
                        eventSimulation_lie();
                        break;

                    default:
                        eventSimulation_wait();
                        break;
                }
            }
            else
            {
                int i = rand.Next(1, 16);
                //Text = $"{i}";
                switch (i)
                {
                    // universal
                    case 1:
                        eventSimulation_wait();
                        break;
                    case 2:
                        eventSimulation_forward();
                        break;
                    case 3:
                        eventSimulation_wait();
                        break;
                    case 4:
                        eventSimulation_backward();
                        break;
                    case 5:
                        eventSimulation_wait();
                        break;
                    // normal
                    case 6:
                        eventSimulation_attack();
                        break;
                    case 7:
                        eventSimulation_victory();
                        break;
                    case 8:
                        eventSimulation_s();
                        break;
                    case 9:
                        eventSimulation_skill();
                        break;
                    case 10:
                        eventSimulation_die();
                        break;
                    case 11:
                        eventSimulation_attack2();
                        break;
                    case 12:
                        eventSimulation_wait();
                        break;
                    case 13:
                        eventSimulation_reload();
                        break;
                    case 14:
                        eventSimulation_victory();
                        break;
                    case 15:
                        eventSimulation_wait();
                        break;

                    default:
                        eventSimulation_wait();
                        break;
                }
            }
        }

        //private int i;
        private int moveDistanceX;
        private int movedDistanceX;
        private bool moveXDirection;
        bool DummyReverse = false;
        private void eventSimulation_forward()
        {
            if (App.globalValues.AnimeList.Contains("move"))
            {
                if (DummyReverse)
                {
                    App.globalValues.FilpX = true;
                }
                else
                {
                    App.globalValues.FilpX = false;
                }
                App.globalValues.IsLoop = true;
                UpdateSpine();
                if (((int)(SystemParameters.PrimaryScreenWidth - Left - Width) >= 10) && ((SystemParameters.PrimaryScreenWidth - Left - (int)(Width / 2)) > 100))
                {
                    Random rand = new Random();
                    moveDistanceX = rand.Next(10, (int)(SystemParameters.PrimaryScreenWidth - Left - Width));
                    App.globalValues.SelectedAnime = App.globalValues.SelectAnimeName;
                    App.globalValues.SelectAnimeName = "move";
                    App.globalValues.SetAnime = true;
                    App.globalValues.Simulation_Moving = true;

                    movedDistanceX = 0;
                    moveXDirection = true;
                    timerSimulationMoveX.Start();
                }
                else
                {
                    eventSimulation_backward();
                }
            }
        }

        private void eventSimulation_backward()
        {
            if (App.globalValues.AnimeList.Contains("move"))
            {
                if (DummyReverse)
                {
                    App.globalValues.FilpX = false;
                }
                else
                {
                    App.globalValues.FilpX = true;
                }
                App.globalValues.IsLoop = true;
                UpdateSpine();
                if (Left > 100)
                {
                    Random rand = new Random();
                    moveDistanceX = rand.Next(10, (int)Left);
                    App.globalValues.SelectedAnime = App.globalValues.SelectAnimeName;
                    App.globalValues.SelectAnimeName = "move";
                    App.globalValues.SetAnime = true;
                    App.globalValues.Simulation_Moving = true;
                    movedDistanceX = 0;
                    moveXDirection = false;
                    timerSimulationMoveX.Start();
                }
                else
                {
                    eventSimulation_forward();
                }
            }
        }

        private void eventSimulation_sit()
        {
            if (App.globalValues.AnimeList.Contains("sit"))
            {
                App.globalValues.IsLoop = true;
                App.globalValues.SelectAnimeName = "sit";
                App.globalValues.SetAnime = true;
                App.globalValues.IsLoop = true;
                UpdateSpine();
            }
        }
        private void eventSimulation_wait()
        {
            if (App.globalValues.AnimeList.Contains("wait"))
            {
                App.globalValues.SelectAnimeName = "wait";
                App.globalValues.SetAnime = true;
                App.globalValues.IsLoop = true;
                UpdateSpine();
            }
        }
        private void eventSimulation_attack()
        {
            if (App.globalValues.AnimeList.Contains("attack"))
            {
                App.globalValues.SelectAnimeName = "attack";
                App.globalValues.SetAnime = true;
                App.globalValues.IsLoop = true;
                UpdateSpine();
            }
            else
            {
                eventSimulation_wait();
            }
        }
        private void eventSimulation_attack2()
        {
            if (App.globalValues.AnimeList.Contains("attack2"))
            {
                App.globalValues.SelectAnimeName = "attack2";
                App.globalValues.SetAnime = true;
                App.globalValues.IsLoop = true;
                UpdateSpine();
            }
            else
            {
                eventSimulation_attack();
            }
        }

        private void eventSimulation_s()
        {
            if (App.globalValues.AnimeList.Contains("s"))
            {
                App.globalValues.SelectAnimeName = "s";
                App.globalValues.SetAnime = true;
                App.globalValues.IsLoop = false;
                UpdateSpine();
                //timerSimulations.Interval = (int)(App.globalValues.AnimeDuration * 1000);
                //timerSimulations.Enabled = true;

                timerSimulationS.Interval = new TimeSpan(0, 0, 0, (int)App.globalValues.AnimeDuration);
                timerSimulationS.Start();
            }
            else
            {
                eventSimulation_attack();
            }
        }

        private void timerSimulationS_Tick(object sender, EventArgs e)
        {
            eventSimulation_attack2();
            timerSimulationS.Stop();
        }

        System.Windows.Threading.DispatcherTimer timerSimulationReload = new System.Windows.Threading.DispatcherTimer();
        private void eventSimulation_reload()
        {
            if (App.globalValues.AnimeList.Contains("reload"))
            {
                App.globalValues.SelectAnimeName = "reload";
                App.globalValues.SetAnime = true;
                App.globalValues.IsLoop = false;
                UpdateSpine();
                timerSimulationReload.Interval = new TimeSpan(0, 0, 0, (int)App.globalValues.AnimeDuration, 0);
                timerSimulationReload.Tick += timerSimulationReload_Tick;
                timerSimulationReload.Start();
            }
            else
            {
                eventSimulation_attack2();
            }
        }
        private void timerSimulationReload_Tick(object sender, EventArgs e)
        {
            eventSimulation_attack2();
            timerSimulationReload.Stop();
        }

        private void eventSimulation_skill()
        {
            if (App.globalValues.AnimeList.Contains("skill"))
            {
                App.globalValues.SelectAnimeName = "skill";
                App.globalValues.SetAnime = true;
                App.globalValues.IsLoop = true;
                UpdateSpine();
            }
            else
            {
                eventSimulation_attack();
            }
        }
        private void eventSimulation_die()
        {
            if (App.globalValues.AnimeList.Contains("die"))
            {
                App.globalValues.SelectAnimeName = "die";
                App.globalValues.SetAnime = true;
                App.globalValues.IsLoop = false;
                UpdateSpine();
            }
        }

        private void eventSimulation_victory()
        {
            if (App.globalValues.AnimeList.Contains("victory"))
            {
                if (App.globalValues.AnimeList.Contains("victoryloop"))
                {
                    App.globalValues.SelectAnimeName = "victory";
                    App.globalValues.SetAnime = true;
                    App.globalValues.IsLoop = false;
                    UpdateSpine();

                    timerSimulationVictory.Interval = new TimeSpan(0, 0, 0, (int)App.globalValues.AnimeDuration);
                    timerSimulationVictory.Start();
                }
                else
                {
                    App.globalValues.SelectAnimeName = "victory";
                    App.globalValues.SetAnime = true;
                    App.globalValues.IsLoop = true;
                    UpdateSpine();
                }
            }
            else
            {
                eventSimulation_wait();
            }
        }

        private void timerSimulationVictory_Tick(object sender, EventArgs e)
        {
            timerSimulationVictory.Stop();
            if (App.globalValues.AnimeList.Contains("victoryloop"))
            {
                App.globalValues.SelectAnimeName = "victoryloop";
                App.globalValues.SetAnime = true;
                App.globalValues.IsLoop = true;
                UpdateSpine();
            }
            else
            {
                eventSimulation_wait();
            }
        }
        private void eventSimulation_lie()
        {
            if (App.globalValues.AnimeList.Contains("lying"))
            {
                App.globalValues.SelectAnimeName = "lying";
                App.globalValues.SetAnime = true;
                App.globalValues.IsLoop = true;
                UpdateSpine();
            }
        }

        private void timerSimulationMoveX_Tick(object sender, EventArgs e)
        {
            //Text = $"{movedDistanceX} | {moveDistanceX}";
            if (App.globalValues.Simulation_Moving == true)
            {
                if (movedDistanceX < moveDistanceX)
                {
                    if (moveXDirection == true)
                    {
                        if ((SystemParameters.PrimaryScreenWidth - Left - Width) > 100)
                        {
                            Left += 1;
                            movedDistanceX += 1;
                        }
                        else
                        {
                            StopMove();
                        }
                    }
                    else
                    {
                        if (Left > 100)
                        {
                            Left -= 1;
                            movedDistanceX += 1;
                        }
                        else
                        {
                            StopMove();
                        }
                    }
                }
                else
                {
                    StopMove();
                }
            }
            else
            {
                StopMove();
            }
        }

        public void StopMove()
        {
            timerSimulationMoveX.Stop();
            App.globalValues.Simulation_Moving = false;
            eventSimulation_wait();
        }

        private void menuItem_LoadDummy_Click(object sender, RoutedEventArgs e)
        {
            LoadDummy();
        }

        private void menuItem_Options_Click(object sender, RoutedEventArgs e)
        {
            ShowOptions();
        }

        private void mi_about_Click(object sender, RoutedEventArgs e)
        {
            ShowAbout();
        }

        public void ShowAbout()
        {
            if (!AboutWindowIsOpen)
            {
                new AboutWindow(this).Show();
            }
        }

        public void DownloadSources()
        {
            LoadDummy();
            dataManagerWindow.DownloadSources();
        }

        private void mi_disableInteraction_Unchecked(object sender, RoutedEventArgs e)
        {
            App.globalValues.EnableInteraction = true;
            IsHitTestVisible = true;
        }

        private void mi_disableInteraction_Checked(object sender, RoutedEventArgs e)
        {
            App.globalValues.EnableInteraction = false;
            IsHitTestVisible = false;
        }

        private void mi_resetDummy_Click(object sender, RoutedEventArgs e)
        {
            ResetDummy();
        }

        public void ResetDummy()
        {
            WindowState = WindowState.Normal;
            Left = (int)((SystemParameters.WorkArea.Width - Width) / 2);
            Top = (int)((SystemParameters.WorkArea.Height - Height) / 2);
            App.globalValues.PosX = 224;
            App.globalValues.PosY = 224;
            mi_hideDummy.IsChecked = false;
            mi_disableInteraction.IsChecked = false;
            App.globalValues.Scale = 1;
        }

        private void mi_hideDummy_Checked(object sender, RoutedEventArgs e)
        {
            Visibility = Visibility.Collapsed;
        }

        private void mi_hideDummy_Unchecked(object sender, RoutedEventArgs e)
        {
            Visibility = Visibility.Visible;
        }

        private void mi_donate_Click(object sender, RoutedEventArgs e)
        {
            HttpRequestHelper.OpenUrl(donateLink);
        }

        public bool IsAdministrator()
        {
            WindowsIdentity current = WindowsIdentity.GetCurrent();
            WindowsPrincipal windowsPrincipal = new WindowsPrincipal(current);
            return windowsPrincipal.IsInRole(WindowsBuiltInRole.Administrator);
        }

        System.Windows.Media.Matrix matrixDPI;
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            matrixDPI = PresentationSource.FromVisual(this).CompositionTarget.TransformToDevice;
            ScaleTransform dpiTransform = new ScaleTransform(1 / matrixDPI.M11, 1 / matrixDPI.M22);
            if (dpiTransform.CanFreeze)
                dpiTransform.Freeze();
            Player.LayoutTransform = dpiTransform;
            Width = Player.Width / matrixDPI.M11;
            Height = Player.Height / matrixDPI.M22;

            //CheckDPI();

            Left = Properties.Settings.Default.Pos.X;
            Top = Properties.Settings.Default.Pos.Y;
            Topmost = Properties.Settings.Default.Topmost;
            //Opacity = Properties.Settings.Default.DummyOpacity;

            if (Properties.Settings.Default.DummySimulation)
            {
                App.globalValues.Simulation = true;
                timerEventsSimulation.Start();
                App.globalValues.Speed = 30;
            }
            else
            {
                App.globalValues.Simulation = false;
            }
        }

        private void mi_Exit_Click(object sender, RoutedEventArgs e)
        {
            SaveSettings();
            Application.Current.Shutdown();
        }

        private void NotifyIcon_MouseDoubleClick(object sender, RoutedEventArgs e)
        {
            bool prevTopmostState = Topmost;
            Topmost = false;
            Topmost = true;
            Topmost = prevTopmostState;
            mi_hideDummy.IsChecked = false;
        }

        private void mi_saveSettings_Click(object sender, RoutedEventArgs e)
        {
            SaveSettings();
        }

        private void mi_settings_Click(object sender, RoutedEventArgs e)
        {
            ShowSettings();
        }
    }
}
