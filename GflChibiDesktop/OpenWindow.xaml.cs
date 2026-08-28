using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Win32;
using System.Windows.Media;
using Newtonsoft.Json;
using static GflChibiDesktop.DummyListReader;
using MessageBox = HandyControl.Controls.MessageBox;
using static GflChibiDesktop.WebAPI;
using System.Text.RegularExpressions;
using System.Collections.Generic;

namespace GflChibiDesktop.Windows
{

    public partial class OpenWindow : Window
    {
        private MainWindow _window;

        List<ComponentModel> initializeDataSet = new List<ComponentModel>();

        public OpenWindow(MainWindow main)
        {
            InitializeComponent();
            _window = main;
            MainWindow.OpenWindowState(true);

            lblVersion.Content = $"{Environment.MachineName}\\{Environment.UserName}\n{_window.currentBuild} {_window.productBuild}";

            if (App.globalValues.SelectAtlasFile != string.Empty)
            {
                tb_Atlas_File.Text = App.globalValues.SelectAtlasFile;
            }
            if (App.globalValues.SelectSpineFile != string.Empty)
            {
                tb_JS_file.Text = App.globalValues.SelectSpineFile;
            }

            nud_canvas_x.Value = App.canvasWidth;
            nud_canvas_y.Value = App.canvasHeight;

            LoadDummyList();
        }

        private void btn_Altas_Open_Click(object sender, RoutedEventArgs e)
        {
            bool isSelect = SelectFile("Spine Altas 文件 (*.atlas)|*.atlas;", tb_Atlas_File);

            if (isSelect)
            {
                App.globalValues.SelectAtlasFile = tb_Atlas_File.Text;
                if (!Common.CheckSpineFile(App.globalValues.SelectAtlasFile))
                {
                    HandyControl.Controls.Growl.ErrorGlobal("找不到 Spine Json 或二进制文件！");

                    bool isSelectSp = SelectFile("Spine Json 文件 (*.json)|*.json|Spine 二进制文件 (*.skel)|*.skel", tb_JS_file);
                    if (isSelectSp)
                    {
                        App.globalValues.SelectSpineFile = tb_JS_file.Text;
                    }
                }
                else
                {
                    tb_JS_file.Text = App.globalValues.SelectSpineFile;
                }
            }
        }

        private void btn_JS_Open_Click(object sender, RoutedEventArgs e)
        {
            bool isSelect = SelectFile("Spine Json 文件 (*.json)|*.json|Spine 二进制文件 (*.skel)|*.skel", tb_JS_file);
            if (isSelect)
            {
                tb_JS_file.Text = App.globalValues.SelectSpineFile;
            }
        }

        private bool SelectFile(string filter, TextBox textBox)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            if (Directory.Exists(App.lastDir))
            {
                openFileDialog.InitialDirectory = App.lastDir;
            }
            else
            {
                openFileDialog.InitialDirectory = Environment.CurrentDirectory;
            }
            openFileDialog.Filter = filter; ;
            if (openFileDialog.ShowDialog() == true)
            {
                textBox.Text = openFileDialog.FileName;
                App.lastDir = Common.GetDirName(openFileDialog.FileName);
                return true;
            }
            return false;

        }

        private void btn_OpenExternal_Click(object sender, RoutedEventArgs e)
        {
            if (rb_External.IsChecked.Value)
            {
                if (tb_Atlas_File.Text.Trim() == string.Empty)
                {
                    HandyControl.Controls.Growl.ErrorGlobal("请选择配置文件！");
                    return;
                }
                if (tb_JS_file.Text.Trim() == string.Empty)
                {
                    HandyControl.Controls.Growl.ErrorGlobal("请选择骨骼文件！");
                    return;
                }

                SetCanvasSize();
                App.isNew = true;
                App.globalValues.Dummy = "(external)";
                App.globalValues.DummyDisplayName = "(外部数据)";
                _window.LoadPlayer(cb_Version.SelectedValue.ToString());

                App.globalValues.PosX = (int)(nud_canvas_x.Value / 2);
                App.globalValues.PosY = (int)(nud_canvas_y.Value / 2);
            }
        }

        SolidColorBrush defaultColor = new SolidColorBrush(Color.FromRgb(255, 255, 255));
        SolidColorBrush type2color = new SolidColorBrush(Color.FromRgb(234, 234, 234));
        SolidColorBrush type3color = new SolidColorBrush(Color.FromRgb(107, 218, 199));
        SolidColorBrush type4color = new SolidColorBrush(Color.FromRgb(209, 223, 91));
        SolidColorBrush type5color = new SolidColorBrush(Color.FromRgb(254, 179, 0));
        SolidColorBrush type6color = new SolidColorBrush(Color.FromRgb(252, 79, 0));
        SolidColorBrush type7color = new SolidColorBrush(Color.FromRgb(222, 182, 255));

        public void LoadDummyList()
        {
            KillEmptyDirectory($@"{AppDomain.CurrentDomain.BaseDirectory}Resources\spine");

            initializeDataSet.Clear();

            initializeDataSet.Add(new ComponentModel() { ComponentID = 1, ComponentName = "HGclass", Level = 1, ParentID = 0, ToolTip = "手枪人形", Header = "手枪(HG)", Foreground = defaultColor });
            initializeDataSet.Add(new ComponentModel() { ComponentID = 2, ComponentName = "HG2class", Level = 2, ParentID = 1, ToolTip = "初始二星手枪人形", Header = "★★", Foreground = type2color });
            initializeDataSet.Add(new ComponentModel() { ComponentID = 3, ComponentName = "HG3class", Level = 2, ParentID = 1, ToolTip = "初始三星手枪人形", Header = "★★★", Foreground = type3color });
            initializeDataSet.Add(new ComponentModel() { ComponentID = 4, ComponentName = "HG4class", Level = 2, ParentID = 1, ToolTip = "初始四星手枪人形", Header = "★★★★", Foreground = type4color });
            initializeDataSet.Add(new ComponentModel() { ComponentID = 5, ComponentName = "HG5class", Level = 2, ParentID = 1, ToolTip = "初始五星手枪人形", Header = "★★★★★", Foreground = type5color });
            initializeDataSet.Add(new ComponentModel() { ComponentID = 7, ComponentName = "HG7class", Level = 2, ParentID = 1, ToolTip = "特典手枪人形", Header = "★EXTRA", Foreground = type7color });

            initializeDataSet.Add(new ComponentModel() { ComponentID = 11, ComponentName = "SMGclass", Level = 1, ParentID = 0, ToolTip = "冲锋枪人形", Header = "冲锋枪(SMG)", Foreground = defaultColor });
            initializeDataSet.Add(new ComponentModel() { ComponentID = 12, ComponentName = "SMG2class", Level = 2, ParentID = 11, ToolTip = "初始二星冲锋枪人形", Header = "★★", Foreground = type2color });
            initializeDataSet.Add(new ComponentModel() { ComponentID = 13, ComponentName = "SMG3class", Level = 2, ParentID = 11, ToolTip = "初始三星冲锋枪人形", Header = "★★★", Foreground = type3color });
            initializeDataSet.Add(new ComponentModel() { ComponentID = 14, ComponentName = "SMG4class", Level = 2, ParentID = 11, ToolTip = "初始四星冲锋枪人形", Header = "★★★★", Foreground = type4color });
            initializeDataSet.Add(new ComponentModel() { ComponentID = 15, ComponentName = "SMG5class", Level = 2, ParentID = 11, ToolTip = "初始五星冲锋枪人形", Header = "★★★★★", Foreground = type5color });
            initializeDataSet.Add(new ComponentModel() { ComponentID = 17, ComponentName = "SMG7class", Level = 2, ParentID = 11, ToolTip = "特典冲锋枪人形", Header = "★EXTRA", Foreground = type7color });

            initializeDataSet.Add(new ComponentModel() { ComponentID = 21, ComponentName = "RFclass", Level = 1, ParentID = 0, ToolTip = "步枪人形", Header = "步枪(RF)", Foreground = defaultColor });
            initializeDataSet.Add(new ComponentModel() { ComponentID = 22, ComponentName = "RF2class", Level = 2, ParentID = 21, ToolTip = "初始二星步枪人形", Header = "★★", Foreground = type2color });
            initializeDataSet.Add(new ComponentModel() { ComponentID = 23, ComponentName = "RF3class", Level = 2, ParentID = 21, ToolTip = "初始三星步枪人形", Header = "★★★", Foreground = type3color });
            initializeDataSet.Add(new ComponentModel() { ComponentID = 24, ComponentName = "RF4class", Level = 2, ParentID = 21, ToolTip = "初始四星步枪人形", Header = "★★★★", Foreground = type4color });
            initializeDataSet.Add(new ComponentModel() { ComponentID = 25, ComponentName = "RF5class", Level = 2, ParentID = 21, ToolTip = "初始五星步枪人形", Header = "★★★★★", Foreground = type5color });
            initializeDataSet.Add(new ComponentModel() { ComponentID = 27, ComponentName = "RF7class", Level = 2, ParentID = 21, ToolTip = "特典步枪人形", Header = "★EXTRA", Foreground = type7color });

            initializeDataSet.Add(new ComponentModel() { ComponentID = 31, ComponentName = "ARclass", Level = 1, ParentID = 0, ToolTip = "突击步枪人形", Header = "突击步枪(AR)", Foreground = defaultColor });
            initializeDataSet.Add(new ComponentModel() { ComponentID = 32, ComponentName = "AR2class", Level = 2, ParentID = 31, ToolTip = "初始二星突击步枪人形", Header = "★★", Foreground = type2color });
            initializeDataSet.Add(new ComponentModel() { ComponentID = 33, ComponentName = "AR3class", Level = 2, ParentID = 31, ToolTip = "初始三星突击步枪人形", Header = "★★★", Foreground = type3color });
            initializeDataSet.Add(new ComponentModel() { ComponentID = 34, ComponentName = "AR4class", Level = 2, ParentID = 31, ToolTip = "初始四星突击步枪人形", Header = "★★★★", Foreground = type4color });
            initializeDataSet.Add(new ComponentModel() { ComponentID = 35, ComponentName = "AR5class", Level = 2, ParentID = 31, ToolTip = "初始五星突击步枪人形", Header = "★★★★★", Foreground = type5color });
            initializeDataSet.Add(new ComponentModel() { ComponentID = 37, ComponentName = "AR7class", Level = 2, ParentID = 31, ToolTip = "特典突击步枪人形", Header = "★EXTRA", Foreground = type7color });

            initializeDataSet.Add(new ComponentModel() { ComponentID = 41, ComponentName = "MGclass", Level = 1, ParentID = 0, ToolTip = "机枪人形", Header = "机枪(MG)", Foreground = defaultColor });
            initializeDataSet.Add(new ComponentModel() { ComponentID = 42, ComponentName = "MG2class", Level = 2, ParentID = 41, ToolTip = "初始二星机枪人形", Header = "★★", Foreground = type2color });
            initializeDataSet.Add(new ComponentModel() { ComponentID = 43, ComponentName = "MG3class", Level = 2, ParentID = 41, ToolTip = "初始三星机枪人形", Header = "★★★", Foreground = type3color });
            initializeDataSet.Add(new ComponentModel() { ComponentID = 44, ComponentName = "MG4class", Level = 2, ParentID = 41, ToolTip = "初始四星机枪人形", Header = "★★★★", Foreground = type4color });
            initializeDataSet.Add(new ComponentModel() { ComponentID = 45, ComponentName = "MG5class", Level = 2, ParentID = 41, ToolTip = "初始五星机枪人形", Header = "★★★★★", Foreground = type5color });
            initializeDataSet.Add(new ComponentModel() { ComponentID = 47, ComponentName = "MG7class", Level = 2, ParentID = 41, ToolTip = "特典机枪人形", Header = "★EXTRA", Foreground = type7color });

            initializeDataSet.Add(new ComponentModel() { ComponentID = 51, ComponentName = "SGclass", Level = 1, ParentID = 0, ToolTip = "霰弹枪人形", Header = "霰弹枪(SG)", Foreground = defaultColor });
            initializeDataSet.Add(new ComponentModel() { ComponentID = 52, ComponentName = "SG2class", Level = 2, ParentID = 51, ToolTip = "初始二星霰弹枪人形", Header = "★★", Foreground = type2color });
            initializeDataSet.Add(new ComponentModel() { ComponentID = 53, ComponentName = "SG3class", Level = 2, ParentID = 51, ToolTip = "初始三星霰弹枪人形", Header = "★★★", Foreground = type3color });
            initializeDataSet.Add(new ComponentModel() { ComponentID = 54, ComponentName = "SG4class", Level = 2, ParentID = 51, ToolTip = "初始四星霰弹枪人形", Header = "★★★★", Foreground = type4color });
            initializeDataSet.Add(new ComponentModel() { ComponentID = 55, ComponentName = "SG5class", Level = 2, ParentID = 51, ToolTip = "初始五星霰弹枪人形", Header = "★★★★★", Foreground = type5color });
            initializeDataSet.Add(new ComponentModel() { ComponentID = 57, ComponentName = "SG7class", Level = 2, ParentID = 51, ToolTip = "特典霰弹枪人形", Header = "★EXTRA", Foreground = type7color });

            initializeDataSet.Add(new ComponentModel() { ComponentID = 61, ComponentName = "OTHERclass", Level = 1, ParentID = 0, ToolTip = "其它人形", Header = "其它", Foreground = defaultColor });
            initializeDataSet.Add(new ComponentModel() { ComponentID = 62, ComponentName = "HOCclass", Level = 2, ParentID = 61, ToolTip = "重装部队人形", Header = "重装部队", Foreground = defaultColor });
            initializeDataSet.Add(new ComponentModel() { ComponentID = 63, ComponentName = "HUMANclass", Level = 2, ParentID = 61, ToolTip = "人类", Header = "人类", Foreground = defaultColor });
            initializeDataSet.Add(new ComponentModel() { ComponentID = 64, ComponentName = "UNKNOWNclass", Level = 2, ParentID = 61, ToolTip = "未分类的数据", Header = "未分类", Foreground = defaultColor });

            //initializeDataSet.Add(new ComponentModel() { ComponentID = 71, ComponentName = "COALITIONclass", Level = 1, ParentID = 0, ToolTip = "融合势力人形", Header = "融合势力", Foreground = defaultColor });
            //initializeDataSet.Add(new ComponentModel() { ComponentID = 72, ComponentName = "SANGVISFERRIclass", Level = 2, ParentID = 71, ToolTip = "铁血工造人形", Header = "铁血工造", Foreground = defaultColor });
            //initializeDataSet.Add(new ComponentModel() { ComponentID = 73, ComponentName = "KCCOclass", Level = 2, ParentID = 71, ToolTip = "正规军人形", Header = "正规军", Foreground = defaultColor });
            //initializeDataSet.Add(new ComponentModel() { ComponentID = 74, ComponentName = "PARADEUSclass", Level = 2, ParentID = 71, ToolTip = "帕拉蒂斯人形", Header = "帕拉蒂斯", Foreground = defaultColor });
            //initializeDataSet.Add(new ComponentModel() { ComponentID = 75, ComponentName = "ETCclass", Level = 2, ParentID = 71, ToolTip = "其他势力人形", Header = "其他势力", Foreground = defaultColor });


            try
            {
                string str = File.ReadAllText($"{AppDomain.CurrentDomain.BaseDirectory}dummy_list.json");
                RootObject rb = JsonConvert.DeserializeObject<RootObject>(str);
                gb_Internal.Header = $"内置数据（战术人形数据列表版本：{rb.meta.version}）";
                int total = rb.content.Count;
                pb_loader.IsIndeterminate = false;
                pb_loader.Maximum = total;
                pb_loader.Value = 0;
                tii.ProgressState = System.Windows.Shell.TaskbarItemProgressState.Normal;
                tii.ProgressValue = 0;

                int counter = 0;

                foreach (Content content in rb.content)
                {
                    counter++;

                    lbl_loader.Content = $"正在处理：{counter} / {total}";
                    pb_loader.Value ++;
                    tii.ProgressValue = pb_loader.Value / pb_loader.Maximum;
                    try
                    {
                        bool displaySwitch = true;
                        ComponentModel node = new ComponentModel();
                        node.ComponentName = $"dummy_{content.name.Replace(" ", string.Empty)}";
                        node.Header = content.display_name;
                        node.ComponentID = 100 + counter;
                        string[] tagString = new string[12];
                        tagString[0] = $"{displaySwitch}";
                        tagString[1] = content.name;
                        tagString[2] = content.parent;
                        tagString[3] = content.type;
                        tagString[4] = content.display_name;
                        tagString[5] = content.fullname;
                        tagString[6] = content.path;
                        tagString[7] = content.filename;
                        tagString[8] = content.cg;
                        tagString[9] = content.cg_d;
                        tagString[10] = content.filename_r;
                        tagString[11] = content.files;
                        node.Tag = tagString;
                        //node.ImageKey = content.type;
                        //node.SelectedImageKey = content.type;
                        node.Foreground = defaultColor;
                        node.ToolTip = content.fullname;
                        if (content.type.Contains("2")) { node.Foreground = type2color; }
                        if (content.type.Contains("3")) { node.Foreground = type3color; }
                        if (content.type.Contains("4")) { node.Foreground = type4color; }
                        if (content.type.Contains("5")) { node.Foreground = type5color; }
                        if (content.type.Contains("6")) { node.Foreground = type6color; }
                        if (content.type.Contains("7")) { node.Foreground = type7color; }

                        node.ParentID = 64;
                        if (content.name == content.parent)
                        {
                            node.Level = 3;
                            switch (content.type)
                            {
                                case "HG2":
                                    node.ParentID = 2;
                                    break;
                                case "HG3":
                                    node.ParentID = 3;
                                    break;
                                case "HG4":
                                    node.ParentID = 4;
                                    break;
                                case "HG5":
                                    node.ParentID = 5;
                                    break;
                                case "HG7":
                                    node.ParentID = 7;
                                    break;
                                case "SMG2":
                                    node.ParentID = 12;
                                    break;
                                case "SMG3":
                                    node.ParentID = 13;
                                    break;
                                case "SMG4":
                                    node.ParentID = 14;
                                    break;
                                case "SMG5":
                                    node.ParentID = 15;
                                    break;
                                case "SMG7":
                                    node.ParentID = 17;
                                    break;
                                case "RF2":
                                    node.ParentID = 22;
                                    break;
                                case "RF3":
                                    node.ParentID = 23;
                                    break;
                                case "RF4":
                                    node.ParentID = 24;
                                    break;
                                case "RF5":
                                    node.ParentID = 25;
                                    break;
                                case "RF7":
                                    node.ParentID = 27;
                                    break;
                                case "AR2":
                                    node.ParentID = 32;
                                    break;
                                case "AR3":
                                    node.ParentID = 33;
                                    break;
                                case "AR4":
                                    node.ParentID = 34;
                                    break;
                                case "AR5":
                                    node.ParentID = 35;
                                    break;
                                case "AR7":
                                    node.ParentID = 37;
                                    break;
                                case "MG2":
                                    node.ParentID = 42;
                                    break;
                                case "MG3":
                                    node.ParentID = 43;
                                    break;
                                case "MG4":
                                    node.ParentID = 44;
                                    break;
                                case "MG5":
                                    node.ParentID = 45;
                                    break;
                                case "MG7":
                                    node.ParentID = 47;
                                    break;
                                case "SG2":
                                    node.ParentID = 52;
                                    break;
                                case "SG3":
                                    node.ParentID = 53;
                                    break;
                                case "SG4":
                                    node.ParentID = 54;
                                    break;
                                case "SG5":
                                    node.ParentID = 55;
                                    break;
                                case "SG7":
                                    node.ParentID = 57;
                                    break;
                                case "HOC":
                                    node.ParentID = 62;
                                    break;
                                case "HUMAN":
                                    node.ParentID = 63;
                                    break;
                                //case "SANGVISFERRI":
                                //    node.ParentID = 72;
                                //    break;
                                //case "KCCO":
                                //    node.ParentID = 73;
                                //    break;
                                //case "PARADEUS":
                                //    node.ParentID = 74;
                                //    break;
                                //case "ETC":
                                //    node.ParentID = 75;
                                //    break;
                                default:
                                    node.ParentID = 64;
                                    break;
                            }
                            initializeDataSet.Add(node);
                        }
                        else
                        {
                            node.Level = 4;
                            //if (content.type == "HUMAN") { node.Level = 3; }
                            node.ParentID = 64;
                            foreach (ComponentModel item in initializeDataSet)
                            {
                                if (item.ComponentName == $"dummy_{content.parent.Replace(" ", string.Empty)}")
                                {
                                    //node.IsExpanded = true;
                                    node.ParentID = item.ComponentID;
                                }
                            }
                            initializeDataSet.Add(node);
                        }
                    }
                    catch (Exception ex)
                    {
                        HandyControl.Controls.Growl.ErrorGlobal($"构建战术人形数据列表时出错。\n{ex}");
                    }
                }


                //加载数据
                tv_InternalSelector.ItemsSource = LoadTreeView(0);

                List<ComponentModel> LoadTreeView(int id)
                {
                    List<ComponentModel> node = initializeDataSet.FindAll(s => s.ParentID.Equals(id));
                    foreach (var item in node)
                    {
                        item.Children = LoadTreeView(item.ComponentID);
                    }
                    return node;
                }

                //});
                lbl_loader.Content = $"已加载 {counter} 条数据";
                pb_loader.IsIndeterminate = true;
                tii.ProgressValue = 100;
                tii.ProgressState = System.Windows.Shell.TaskbarItemProgressState.Indeterminate;
                //tv_InternalSelector.Items.Add(treeViewItemTemp);
            }
            catch (Exception ex)
            {
                HandyControl.Controls.Growl.ErrorGlobal($"加载战术人形数据列表时出错。\n{ex}");
            }
            tvAfterSelect();
        }
        private ComponentModel SelectedItem;
        private void tv_InternalSelector_Selected(object sender, RoutedEventArgs e)
        {
            lbl_InternalSelected.Content = "请选择要加载的战术人形";
            lbl_InternalSelected.Foreground = defaultColor;
            lblSelectedItem.Content = "未选择";
            lblSelectedItem.Foreground = defaultColor;

            img_Preview.Source = null;

            TreeViewItem tvi = e.OriginalSource as TreeViewItem;
            ComponentModel item = (ComponentModel)tvi.Header;
            SelectedItem = item;

            tvAfterSelect();
        }

        private void tvAfterSelect()
        {
            ComponentModel item = SelectedItem;
            //TreeViewItem item = (TreeViewItem)tv_InternalSelector.SelectedItem;

            btn_downloadData.IsEnabled = false;
            btn_downloadData.Visibility = Visibility.Visible;
            btn_deleteData.IsEnabled = false;
            btn_deleteData.Visibility = Visibility.Collapsed;
            btn_loadCG.IsEnabled = false;
            chb_save_cg.IsEnabled = false;
            btn_loadData.IsEnabled = false;
            btn_loadDefaultData.IsEnabled = false;
            btn_loadDormData.IsEnabled = false;
            btng_loadData.Visibility = Visibility.Collapsed;

            if (item != null)
            {
                lbl_InternalSelected.Content = item.ToolTip;
                lbl_InternalSelected.Foreground = item.Foreground;
                if (!item.ComponentName.Contains("class"))
                {
                    KillEmptyDirectory($@"{AppDomain.CurrentDomain.BaseDirectory}Resources\spine");

                    string[] tagString = new string[12];
                    tagString[0] = item.Tag[0];//displaySwitch
                    tagString[1] = item.Tag[1];//content.name;
                    tagString[2] = item.Tag[2];//content.parent;
                    tagString[3] = item.Tag[3];//content.type;
                    tagString[4] = item.Tag[4];//content.display_name;
                    tagString[5] = item.Tag[5];//content.fullname;
                    tagString[6] = item.Tag[6];//content.path;
                    tagString[7] = item.Tag[7];//content.filename;
                    tagString[8] = item.Tag[8];//content.cg;
                    tagString[9] = item.Tag[9];//content.cg_d;
                    tagString[10] = item.Tag[10];//content.filename_r;
                    tagString[11] = item.Tag[11];//content.files;

                    lblSelectedItem.Content = tagString[1].Replace("_", "__");
                    lblSelectedItem.Foreground = item.Foreground;

                    if (tagString[8] != null)
                    {
                        btn_loadCG.IsEnabled = true;
                        chb_save_cg.IsEnabled = true;
                        string cg_filename = tagString[8];
                        if ((bool)chb_preview_d.IsChecked) //默认大破立绘
                        {
                            if (tagString[9] != null)
                            {
                                cg_filename = tagString[9];
                            }
                        }
                        string cgURL = $@"{AppDomain.CurrentDomain.BaseDirectory}Resources\pic\{cg_filename}";
                        if (File.Exists(cgURL))
                        {
                            img_Preview.Source = new System.Windows.Media.Imaging.BitmapImage(new Uri(cgURL, UriKind.Absolute));
                        }
                        else
                        {
                            if ((bool)chb_preview.IsChecked)
                            {
                                try
                                {
                                    img_Preview.Source = new System.Windows.Media.Imaging.BitmapImage(new Uri($"{App.globalValues.DownloadSource}pic/{cg_filename}", UriKind.Absolute));
                                }
                                catch (Exception)
                                {

                                }
                            }
                        }
                    }

                    if ((tagString[6] != null) && (tagString[7] != null) && (tagString[11]!=null)) //存在数据
                    {
                        btn_downloadData.IsEnabled = true;
                        btn_downloadData.Visibility = Visibility.Visible;

                        if (Directory.Exists($@"{AppDomain.CurrentDomain.BaseDirectory}Resources\spine\{tagString[6]}"))
                        {
                            bool checkResult = true;
                            foreach (string filename in tagString[11].Split('|'))
                            {
                                if (!File.Exists($@"{AppDomain.CurrentDomain.BaseDirectory}Resources\spine\{tagString[6]}\{filename}"))
                                {
                                    checkResult = false;
                                }
                            }
                            if (checkResult)
                            {
                                btn_downloadData.IsEnabled = false;
                                btn_downloadData.Visibility = Visibility.Visible;
                                if ((tagString[1] != Properties.Settings.Default.DummyName) && (tagString[1] != App.globalValues.Dummy))
                                {
                                    btn_deleteData.IsEnabled = true;
                                    btn_deleteData.Visibility = Visibility.Visible;
                                    btn_downloadData.Visibility = Visibility.Collapsed;
                                }

                                btn_loadData.IsEnabled = true;
                                btn_loadDefaultData.IsEnabled = true;
                                if (tagString[10] != null)
                                {
                                    btng_loadData.Visibility = Visibility.Visible;
                                    btn_loadDormData.IsEnabled = true;
                                }
                            }
                        }
                    }

                }
            }
        }

        private void btn_LoadDummyList_Click(object sender, RoutedEventArgs e)
        {
            btn_LoadDummyList.IsEnabled = false;
            bool DummyListPost = false;
            string DummyListStr = HttpRequestHelper.PostWebRequest("https://api.brightsu.cn/GflChibiDesktop/dummy_list", string.Empty, Encoding.UTF8, ref DummyListPost);
            if (DummyListPost)
            {
                DummyListRoot rt = JsonConvert.DeserializeObject<DummyListRoot>(DummyListStr);

                if (rt.ret != 200) //API请求失败
                {
                    if (File.Exists($"{AppDomain.CurrentDomain.BaseDirectory}dummy_list.json"))//本地存在即加载本地
                    {
                        LoadDummyList();
                    }
                    else
                    {
                        MessageBoxResult downloadListResult = MessageBox.Show("本地战术人形数据表不存在，且 API 接口调用失败。加载进程已中止。\n是否重试？", "数据表加载失败", MessageBoxButton.YesNo, MessageBoxImage.Exclamation, MessageBoxResult.Yes);
                        if (downloadListResult == MessageBoxResult.Yes)
                        {
                            btn_LoadDummyList_Click(this, null);
                        }
                    }
                    return;
                }

                if (File.Exists($"{AppDomain.CurrentDomain.BaseDirectory}dummy_list.json"))//API请求成功，本地存在
                {
                    string str = File.ReadAllText($"{AppDomain.CurrentDomain.BaseDirectory}dummy_list.json");
                    RootObject rb = JsonConvert.DeserializeObject<RootObject>(str);
                    if (rt.data.uuid != rb.meta.uuid)//有新版本
                    {
                        sp_downloader.Visibility = Visibility.Visible;
                        lbl_loader.Content = "正在更新战术人形数据表";
                        HttpClass.DownloadFile(rt.data.url, $"{AppDomain.CurrentDomain.BaseDirectory}dummy_list.json", pb_downloader, lbl_downloader);
                        sp_downloader.Visibility = Visibility.Collapsed;
                        btn_LoadDummyList_Click(this, null);
                        return;
                    }
                    else//相同则加载本地
                    {
                        LoadDummyList();
                    }
                }
                else//API成功，本地不存在
                {
                    try
                    {
                        bool downloaded = HttpClass.DownloadFile(rt.data.url, $"{AppDomain.CurrentDomain.BaseDirectory}dummy_list.json", pb_loader, lbl_loader);
                        if (downloaded)
                        {
                            btn_LoadDummyList_Click(this, null);
                        }
                        else
                        {
                            HandyControl.Controls.Growl.ErrorGlobal("数据表下载失败");
                        }
                    }
                    catch (Exception ex)
                    {
                        HandyControl.Controls.Growl.ErrorGlobal($"获取与更新数据表时出错。\n{ex.Message}");
                    }
                }
            }
            else //API请求失败
            {
                if (File.Exists($"{AppDomain.CurrentDomain.BaseDirectory}dummy_list.json"))//存在本地即加载本地
                {
                    LoadDummyList();
                }
                else
                {
                    MessageBoxResult downloadListResult = MessageBox.Show("本地战术人形数据表不存在，且 API 接口调用失败。加载进程已中止。\n是否重试？", "数据表加载失败", MessageBoxButton.YesNo, MessageBoxImage.Exclamation, MessageBoxResult.Yes);
                    if (downloadListResult == MessageBoxResult.Yes)
                    {
                        btn_LoadDummyList_Click(this, null);
                    }
                }
            }
            btn_LoadDummyList.IsEnabled = true;
        }
        

        private void btn_loadCG_Click(object sender, RoutedEventArgs e)
        {
            ComponentModel item = SelectedItem;
            string[] tagString = new string[10];
            tagString[0] = item.Tag[0];//displaySwitch
            tagString[1] = item.Tag[1];//content.name;
            tagString[2] = item.Tag[2];//content.parent;
            tagString[3] = item.Tag[3];//content.type;
            tagString[4] = item.Tag[4];//content.display_name;
            tagString[5] = item.Tag[5];//content.fullname;
            tagString[6] = item.Tag[6];//content.path;
            tagString[7] = item.Tag[7];//content.filename;
            tagString[8] = item.Tag[8];//content.cg;
            tagString[9] = item.Tag[9];//content.cg_d;

            bool cg = false;
            bool cg_d = false;
            bool local_cg = false;
            bool local_cg_d = false;
            
            if (tagString[8] != null)
            { 
                cg = true; 
                local_cg = File.Exists($@"{AppDomain.CurrentDomain.BaseDirectory}Resources\pic\{tagString[8]}");
            }
            if (tagString[9] != null)
            {
                cg_d = true;
                local_cg_d = File.Exists($@"{AppDomain.CurrentDomain.BaseDirectory}Resources\pic\{tagString[9]}");
            }

            if (cg) //存在立绘
            {
                if (cg && cg_d) //同时存在两种立绘
                {
                    if (local_cg && local_cg_d) //本地同时存在两种立绘，直接加载
                    {
                        new WindowCG().LoadCG(tagString[5], $@"{AppDomain.CurrentDomain.BaseDirectory}Resources\pic\", tagString[8], tagString[9]);
                    }
                    else
                    {
                        if ((bool)chb_save_cg.IsChecked)
                        {
                            DownloadCG(tagString);
                        }
                        else
                        {
                            new WindowCG().LoadCG(tagString[5], $@"{App.globalValues.DownloadSource}/pic/", tagString[8], tagString[9]);
                        }
                    }
                }
                else //只有一种立绘
                {
                    if (local_cg) //本地存在，直接加载
                    {
                        new WindowCG().LoadCG(tagString[5], $@"{AppDomain.CurrentDomain.BaseDirectory}Resources\pic\", tagString[8]);
                    }
                    else
                    {
                        if ((bool)chb_save_cg.IsChecked)
                        {
                            DownloadCG(tagString);
                        }
                        else
                        {
                            new WindowCG().LoadCG(tagString[5], $@"{App.globalValues.DownloadSource}/pic/", tagString[8]);
                        }
                    }
                }
            }
            else
            {
                HandyControl.Controls.Growl.InfoGlobal($"{tagString[5]} ({tagString[1]}) 没有对应的立绘数据。");
            }
        }

        private void DownloadCG(string[] tagString)
        {
            //tagString[0] = item.Tag[0];//displaySwitch
            //tagString[1] = item.Tag[1];//content.name;
            //tagString[2] = item.Tag[2];//content.parent;
            //tagString[3] = item.Tag[3];//content.type;
            //tagString[4] = item.Tag[4];//content.display_name;
            //tagString[5] = item.Tag[5];//content.fullname;
            //tagString[6] = item.Tag[6];//content.path;
            //tagString[7] = item.Tag[7];//content.filename;
            //tagString[8] = item.Tag[8];//content.cg;
            //tagString[9] = item.Tag[9];//content.cg_d;

            tv_InternalSelector.IsEnabled = false;
            btn_loadCG.IsEnabled = false;
            chb_save_cg.IsEnabled = false;
            btn_downloadData.IsEnabled = false;

            string cg_url = App.globalValues.DownloadSource + "pic/";

            if (!Directory.Exists($@"{AppDomain.CurrentDomain.BaseDirectory}Resources\pic"))
            {
                Directory.CreateDirectory($@"{AppDomain.CurrentDomain.BaseDirectory}Resources\pic");
            }
            sp_downloader.Visibility = Visibility.Visible;
            pb_loader.IsIndeterminate = false;
            pb_loader.Value = 0;
            pb_loader.Maximum = 1;
            if (tagString[9] != null)
            {
                lbl_loader.Content = $"正在下载 {tagString[5]} 的大破立绘数据";
                pb_loader.Maximum = 2;
                HttpClass.DownloadFile($"{cg_url}/{tagString[9]}", $@"{AppDomain.CurrentDomain.BaseDirectory}Resources\pic\{tagString[9]}", pb_downloader, lbl_downloader);
                pb_loader.Value++;
            }
            lbl_loader.Content = $"正在下载 {tagString[5]} 的立绘数据";
            HttpClass.DownloadFile($"{cg_url}/{tagString[8]}", $@"{AppDomain.CurrentDomain.BaseDirectory}Resources\pic\{tagString[8]}", pb_downloader, lbl_downloader);
            pb_loader.Value++;
            sp_downloader.Visibility = Visibility.Collapsed;
            lbl_loader.Content = "准备就绪";
            pb_loader.IsIndeterminate = true;

            if (tagString[9] != null)
            {
                new WindowCG().LoadCG(tagString[5], $@"{AppDomain.CurrentDomain.BaseDirectory}Resources\pic\", tagString[8], tagString[9]);
            }
            else
            {
                new WindowCG().LoadCG(tagString[5], $@"{AppDomain.CurrentDomain.BaseDirectory}Resources\pic\", tagString[8]);
            }

            tv_InternalSelector.IsEnabled = true;
            btn_loadCG.IsEnabled = true;
            chb_save_cg.IsEnabled = true;
            tvAfterSelect();
        }

        /// <summary>
        /// 检测串值是否为合法的网址格式
        /// </summary>
        /// <param name="strValue">要检测的String值</param>
        /// <returns>成功返回true 失败返回false</returns>
        public static bool CheckIsUrlFormat(string strValue)
        {
            return CheckIsFormat(@"(http://)?([\w-]+\.)+[\w-]+(/[\w- ./?%&=]*)?", strValue);
        }

        /// <summary>
        /// 检测串值是否为合法的格式
        /// </summary>
        /// <param name="strRegex">正则表达式</param>
        /// <param name="strValue">要检测的String值</param>
        /// <returns>成功返回true 失败返回false</returns>
        public static bool CheckIsFormat(string strRegex, string strValue)
        {
            if (strValue != null && strValue.Trim() != string.Empty)
            {
                Regex re = new Regex(strRegex);
                if (re.IsMatch(strValue))
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            return false;
        }



        /// <summary>
        /// 删除掉空文件夹
        /// 所有没有子“文件系统”的都将被删除
        /// </summary>
        /// <param name="storagepath"></param>
        private void KillEmptyDirectory(String storagepath)
        {
            DirectoryInfo dir = new DirectoryInfo(storagepath);
            DirectoryInfo[] subdirs = dir.GetDirectories("*.*", SearchOption.AllDirectories);
            foreach (DirectoryInfo subdir in subdirs)
            {
                FileSystemInfo[] subFiles = subdir.GetFileSystemInfos();
                if (subFiles.Count() == 0)
                {
                    subdir.Delete();
                }
            }
        }

        private void btn_loadData_Click(object sender, RoutedEventArgs e)
        {
            ComponentModel item = SelectedItem;
            LoadInternalSpine(item.Tag, false);
        }

        private void btn_loadDormData_Click(object sender, RoutedEventArgs e)
        {
            ComponentModel item = SelectedItem;
            LoadInternalSpine(item.Tag, true);
        }

        private void SetCanvasSize()
        {
            double CanvasWidth = nud_canvas_x.Value;
            double CanvasHeight = nud_canvas_y.Value;
            App.globalValues.FrameWidth = CanvasWidth;
            App.globalValues.FrameHeight = CanvasHeight;
            App.canvasWidth = CanvasWidth;
            App.canvasHeight = CanvasHeight;
            _window.Player.Width = CanvasWidth;
            _window.Player.Height = CanvasHeight;
            _window.Width = CanvasWidth;
            _window.Height = CanvasHeight;
        }

        private void LoadInternalSpine(string[] tagString, bool dormMode)
        {
            _window.StopMove();
            //tagString[0] = $"{displaySwitch}";
            //tagString[1] = content.name;
            //tagString[2] = content.parent;
            //tagString[3] = content.type;
            //tagString[4] = content.display_name;
            //tagString[5] = content.fullname;
            //tagString[6] = content.path;
            //tagString[7] = content.filename;
            //tagString[8] = content.cg;
            //tagString[9] = content.cg_d;
            //tagString[10] = content.filename_r;
            //tagString[11] = content.files;

            SetCanvasSize();
            App.globalValues.IsDormMode = dormMode;
            App.globalValues.SelectAtlasFile = $@"{AppDomain.CurrentDomain.BaseDirectory}Resources\spine\{tagString[6]}\{tagString[7]}.atlas";
            App.globalValues.SelectSpineFile = $@"{AppDomain.CurrentDomain.BaseDirectory}Resources\spine\{tagString[6]}\{tagString[7]}.skel";
            if (App.globalValues.IsDormMode)
            {
                if (File.Exists($@"{AppDomain.CurrentDomain.BaseDirectory}Resources\spine\{tagString[6]}\{tagString[10]}.atlas"))
                { App.globalValues.SelectAtlasFile = $@"{AppDomain.CurrentDomain.BaseDirectory}Resources\spine\{tagString[6]}\{tagString[10]}.atlas"; }
                if (File.Exists($@"{AppDomain.CurrentDomain.BaseDirectory}Resources\spine\{tagString[6]}\{tagString[10]}.skel"))
                { App.globalValues.SelectSpineFile = $@"{AppDomain.CurrentDomain.BaseDirectory}Resources\spine\{tagString[6]}\{tagString[10]}.skel"; }
                App.globalValues.DummyDisplayName = $"[宿舍] {tagString[5]}";
            }
            else
            {
                //App.globalValues.SelectAtlasFile = $@"{AppDomain.CurrentDomain.BaseDirectory}Resources\spine\{tagString[6]}\{tagString[7]}.atlas";
                //App.globalValues.SelectSpineFile = $@"{AppDomain.CurrentDomain.BaseDirectory}Resources\spine\{tagString[6]}\{tagString[7]}.skel";
                App.globalValues.DummyDisplayName = tagString[5];
            }
            App.globalValues.Alpha = true;
            App.globalValues.PreMultiplyAlpha = true;
            App.isNew = true;
            _window.LoadPlayer("2.1.25");

            App.globalValues.Dummy = tagString[1];

            App.globalValues.PosX = (int)(nud_canvas_x.Value / 2);
            App.globalValues.PosY = (int)(nud_canvas_y.Value / 2);
            App.globalValues.SelectSkin = "default";
            App.globalValues.SelectAnimeName = "wait";

            Properties.Settings.Default.DummyFilename = tagString[7];
            Properties.Settings.Default.DummyPath = tagString[6];
            Properties.Settings.Default.DummyFilenameR = tagString[10];

            tvAfterSelect();
        }

        private void btn_downloadData_Click(object sender, RoutedEventArgs e)
        {
            ComponentModel item = SelectedItem;
            DownloadData(item.Tag);
        }


        private void DownloadData(string[] tagString)
        {
            ComponentModel item = SelectedItem;
            btn_downloadData.IsEnabled = false;
            btn_downloadData.Visibility = Visibility.Visible;
            btn_deleteData.IsEnabled = false;
            btn_deleteData.Visibility = Visibility.Collapsed;
            btn_loadCG.IsEnabled = false;
            chb_save_cg.IsEnabled = false;
            tv_InternalSelector.IsEnabled = false;

            //tagString[0] = $"{displaySwitch}";
            //tagString[1] = content.name;
            //tagString[2] = content.parent;
            //tagString[3] = content.type;
            //tagString[4] = content.display_name;
            //tagString[5] = content.fullname;
            //tagString[6] = content.path;
            //tagString[7] = content.filename;
            //tagString[8] = content.cg;
            //tagString[9] = content.cg_d;
            //tagString[10] = content.filename_r;
            //tagString[11] = content.files;
            string downloadSource = string.Empty;
            if (App.globalValues.DownloadSource != string.Empty)
            {
                downloadSource = App.globalValues.DownloadSource + "spine/" + tagString[6];
            }

            if (tagString[11].Split('|').Count() > 0)
            {
                if (CheckIsUrlFormat(downloadSource))
                {
                    int total_files = tagString[11].Split('|').Count();
                    pb_loader.Value = 0;
                    pb_loader.Maximum = total_files;
                    tii.ProgressState = System.Windows.Shell.TaskbarItemProgressState.Normal;

                    sp_downloader.Visibility = Visibility.Visible;
                    pb_loader.IsIndeterminate = false;
                    lbl_loader.Content = $"正在下载 {tagString[5]}";

                    KillEmptyDirectory($@"{AppDomain.CurrentDomain.BaseDirectory}Resources\spine");

                    foreach (string filename in tagString[11].Split('|'))
                    {
                        lbl_loader.Content = $"正在下载 {tagString[5]}：{filename} ({pb_loader.Value}/{total_files})";

                        if (!Directory.Exists($@"{AppDomain.CurrentDomain.BaseDirectory}Resources\spine\{tagString[6]}"))
                        {
                            Directory.CreateDirectory($@"{AppDomain.CurrentDomain.BaseDirectory}Resources\spine\{tagString[6]}");
                        }

                        HttpClass.DownloadFile($"{downloadSource}/{filename}", $@"{AppDomain.CurrentDomain.BaseDirectory}Resources\spine\{tagString[6]}\{filename}", pb_downloader, lbl_downloader);

                        pb_loader.Value++;
                        tii.ProgressValue = pb_loader.Value / pb_loader.Maximum;
                    }

                    lbl_loader.Content = $"已完成下载 {tagString[5]}，等待下一步操作...";
                    pb_loader.IsIndeterminate = true;
                    sp_downloader.Visibility = Visibility.Collapsed;
                    tii.ProgressState = System.Windows.Shell.TaskbarItemProgressState.Indeterminate;

                }
                else
                {
                    HandyControl.Controls.Growl.ErrorGlobal("战术人形数据下载失败。\nURL 无效。请检查下载源设置。");
                }
            }
            else
            {
                HandyControl.Controls.Growl.ErrorGlobal("战术人形数据下载失败。\n服务器端未包含该人形的有效数据。");
            }

            tv_InternalSelector.IsEnabled = true;
            
            tvAfterSelect();
        }

        private void btn_deleteData_Click(object sender, RoutedEventArgs e)
        {
            ComponentModel item = SelectedItem;
            string[] tagString = new string[8];
            tagString[0] = item.Tag[0];//displaySwitch
            tagString[1] = item.Tag[1];//content.name;
            tagString[2] = item.Tag[2];//content.parent;
            tagString[3] = item.Tag[3];//content.type;
            tagString[4] = item.Tag[4];//content.display_name;
            tagString[5] = item.Tag[5];//content.fullname;
            tagString[6] = item.Tag[6];//content.path;
            tagString[7] = item.Tag[7];//content.filename;
            DeleteData(tagString[1]);
        }

        private void DeleteData(string dummy)
        {
            if (Directory.Exists($@"{AppDomain.CurrentDomain.BaseDirectory}Resources\spine\{dummy}"))
            {
                Directory.Delete($@"{AppDomain.CurrentDomain.BaseDirectory}Resources\spine\{dummy}", true);
            }
            tvAfterSelect();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            //e.Cancel = true;
            //Hide();
        }

        private void tv_InternalSelector_Unselected(object sender, RoutedEventArgs e)
        {
            SelectedItem = null;
            btn_downloadData.IsEnabled = false;
            btn_downloadData.Visibility = Visibility.Visible;
            btn_deleteData.IsEnabled = false;
            btn_deleteData.Visibility = Visibility.Collapsed;

            btn_loadCG.IsEnabled = false;
            chb_save_cg.IsEnabled = false;
            btn_loadData.IsEnabled = false;
            btn_loadDefaultData.IsEnabled = false;
            btn_loadDormData.IsEnabled = false;
            btng_loadData.Visibility = Visibility.Collapsed;
            img_Preview.Source = null;
        }

        private void btn_sources_Click(object sender, RoutedEventArgs e)
        {
            DownloadSources();
        }
        DownloadSourcesDialog _downloadSources = new DownloadSourcesDialog();
        public void DownloadSources()
        {
            HandyControl.Controls.Dialog.Show(_downloadSources);
        }

        private void SearchBar_SearchStarted(object sender, HandyControl.Data.FunctionEventArgs<string> e)
        {
            if (sbQuery.Text != string.Empty)
            {
                initializeDataSet.Clear();
                try
                {
                    string str = File.ReadAllText($"{AppDomain.CurrentDomain.BaseDirectory}dummy_list.json");
                    RootObject rb = JsonConvert.DeserializeObject<RootObject>(str);
                    gb_Internal.Header = $"内置数据（战术人形数据列表版本：{rb.meta.version}）";
                    int total = rb.content.Count;

                    int counter = 0;

                    foreach (Content content in rb.content)
                    {
                        counter++;
                        if (content.name.Contains(sbQuery.Text) || content.parent.Contains(sbQuery.Text) || content.display_name.Contains(sbQuery.Text) || content.fullname.Contains(sbQuery.Text))
                        {
                            try
                            {
                                bool displaySwitch = true;
                                ComponentModel node = new ComponentModel();
                                node.ComponentName = $"dummy_{content.name.Replace(" ", string.Empty)}";
                                node.Header = content.display_name;
                                node.ComponentID = 100 + counter;
                                string[] tagString = new string[12];
                                tagString[0] = $"{displaySwitch}";
                                tagString[1] = content.name;
                                tagString[2] = content.parent;
                                tagString[3] = content.type;
                                tagString[4] = content.display_name;
                                tagString[5] = content.fullname;
                                tagString[6] = content.path;
                                tagString[7] = content.filename;
                                tagString[8] = content.cg;
                                tagString[9] = content.cg_d;
                                tagString[10] = content.filename_r;
                                tagString[11] = content.files;
                                node.Tag = tagString;
                                //node.ImageKey = content.type;
                                //node.SelectedImageKey = content.type;
                                node.Foreground = defaultColor;
                                node.ToolTip = content.fullname;
                                if (content.type.Contains("2")) { node.Foreground = type2color; }
                                if (content.type.Contains("3")) { node.Foreground = type3color; }
                                if (content.type.Contains("4")) { node.Foreground = type4color; }
                                if (content.type.Contains("5")) { node.Foreground = type5color; }
                                if (content.type.Contains("6")) { node.Foreground = type6color; }
                                if (content.type.Contains("7")) { node.Foreground = type7color; }

                                node.ParentID = 0;
                                if (content.name == content.parent)
                                {
                                    node.Level = 1;
                                    node.ParentID = 0;
                                    initializeDataSet.Add(node);
                                }
                                else
                                {
                                    node.Level = 2;
                                    node.ParentID = 0;
                                    foreach (ComponentModel item in initializeDataSet)
                                    {
                                        if (item.ComponentName == $"dummy_{content.parent.Replace(" ", string.Empty)}")
                                        {
                                            //node.IsExpanded = true;
                                            node.ParentID = item.ComponentID;
                                        }
                                    }
                                    initializeDataSet.Add(node);
                                }
                            }
                            catch (Exception ex)
                            {
                                HandyControl.Controls.Growl.ErrorGlobal($"构建战术人形数据列表时出错。\n{ex}");
                            }
                        }

                    }

                    //加载数据
                    tv_InternalSelector.ItemsSource = LoadTreeView(0);

                    List<ComponentModel> LoadTreeView(int id)
                    {
                        List<ComponentModel> node = initializeDataSet.FindAll(s => s.ParentID.Equals(id));
                        foreach (var item in node)
                        {
                            item.Children = LoadTreeView(item.ComponentID);
                        }
                        return node;
                    }

                    //});
                    //tv_InternalSelector.Items.Add(treeViewItemTemp);
                }
                catch (Exception ex)
                {
                    HandyControl.Controls.Growl.ErrorGlobal($"加载战术人形数据列表时出错。\n{ex}");
                }
                tvAfterSelect();
            }
            else
            {
                LoadDummyList();
            }
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            MainWindow.OpenWindowState(false);
        }

        private void chb_thinList_Click(object sender, RoutedEventArgs e)
        {
            //ResourceDictionary resourceDictionary = new ResourceDictionary();
            //Application.LoadComponent(resourceDictionary, new Uri("pack://application:,,,/HandyControl;component/Themes/Theme.xaml", UriKind.Relative));
            //Application.Current.Resources.MergedDictionaries.Add(resourceDictionary);
            if ((bool)chb_thinList.IsChecked)
            {
                tv_InternalSelector.SetValue(StyleProperty, Application.Current.Resources["TreeView.Small"]);
            }
            else
            {
                tv_InternalSelector.SetValue(StyleProperty, Application.Current.Resources["TreeViewBaseStyle"]);
            }
        }
    }
}
