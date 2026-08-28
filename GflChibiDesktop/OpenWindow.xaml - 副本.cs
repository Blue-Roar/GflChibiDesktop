using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using Microsoft.Win32;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using static GflChibiDesktop.JsonReader;
using MessageBox = HandyControl.Controls.MessageBox;
using static GflChibiDesktop.WebAPI;
using System.Net;
using System.Diagnostics;
using System.Text.RegularExpressions;
using GflChibiDesktop.Windows;
using System.Collections.Generic;

namespace GflChibiDesktop.Windows
{

    public partial class OpenWindow : Window
    {
        private MainWindow _window;

        List<ComponentModel> initializeDataSet = new List<ComponentModel>();
        List<ComponentModel> HGclassDataSet = new List<ComponentModel>();
        List<ComponentModel> SMGclassDataSet = new List<ComponentModel>();
        List<ComponentModel> RFclassDataSet = new List<ComponentModel>();
        List<ComponentModel> ARclassDataSet = new List<ComponentModel>();
        List<ComponentModel> MGclassDataSet = new List<ComponentModel>();
        List<ComponentModel> SGclassDataSet = new List<ComponentModel>();

        public OpenWindow(MainWindow main)
        {
            InitializeComponent();

            _window = main;
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

            lbl_bottom.Content += $" ({_window.homepageLink})";

        }

        private void btn_Altas_Open_Click(object sender, RoutedEventArgs e)
        {
            bool isSelect = SelectFile("Spine Altas 文件 (*.atlas)|*.atlas;", tb_Atlas_File);

            if (isSelect)
            {
                App.globalValues.SelectAtlasFile = tb_Atlas_File.Text;
                if (!Common.CheckSpineFile(App.globalValues.SelectAtlasFile))
                {
                    MessageBox.Show($"找不到 Spine Json 或二进制文件！", "错误", MessageBoxButton.OK, MessageBoxImage.Exclamation);

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
                    MessageBox.Show($"请选择配置文件！", "加载外部数据", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                    return;
                }
                if (tb_JS_file.Text.Trim() == string.Empty)
                {
                    MessageBox.Show($"请选择骨骼文件！", "加载外部数据", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                    return;
                }

                double setWidth = nud_canvas_x.Value;
                double setHeight = nud_canvas_y.Value;
                //if (!double.TryParse(tb_Canvas_X.Text, out setWidth) || !double.TryParse(tb_Canvas_Y.Text, out setHeight))
                //{
                //    MessageBox.Show($"请设置画布大小！", "加载数据", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                //    return;
                //}
                App.globalValues.FrameWidth = setWidth;
                App.globalValues.FrameHeight = setHeight;
                App.canvasWidth = setWidth;
                App.canvasHeight = setHeight;
                App.isNew = true;
                App.globalValues.Dummy = "(external)";
                App.globalValues.DummyDisplayName = "(外部数据)";
                _window.LoadPlayer(cb_Version.SelectedValue.ToString());
            }
        }

        SolidColorBrush defaultColor = new SolidColorBrush(Color.FromRgb(255, 255, 255));
        SolidColorBrush type2color = new SolidColorBrush(Color.FromRgb(234, 234, 234));
        SolidColorBrush type3color = new SolidColorBrush(Color.FromRgb(107, 218, 199));
        SolidColorBrush type4color = new SolidColorBrush(Color.FromRgb(209, 223, 91));
        SolidColorBrush type5color = new SolidColorBrush(Color.FromRgb(254, 179, 0));
        SolidColorBrush type6color = new SolidColorBrush(Color.FromRgb(252, 79, 0));
        SolidColorBrush type7color = new SolidColorBrush(Color.FromRgb(222, 182, 255));

        string skin_connector = " ";
        public void LoadDummyList()
        {
            sb_tvis.IsEnabled = false;
            KillEmptyDirectory($@"{AppDomain.CurrentDomain.BaseDirectory}\Resources\spine");

            initializeDataSet.Clear();

            initializeDataSet.Add(new ComponentModel() { ComponentID = 1, ComponentName = "HGclass", Level = 1, ParentID = 0, ComponentToolTip = "手枪人形", ComponentHeader = "手枪(HG)", ComponentForeground = defaultColor });
            initializeDataSet.Add(new ComponentModel() { ComponentID = 2, ComponentName = "HG2class", Level = 2, ParentID = 1, ComponentToolTip = "初始二星手枪人形", ComponentHeader = "★★", ComponentForeground = type2color });
            initializeDataSet.Add(new ComponentModel() { ComponentID = 3, ComponentName = "HG3class", Level = 2, ParentID = 1, ComponentToolTip = "初始三星手枪人形", ComponentHeader = "★★★", ComponentForeground = type3color });
            initializeDataSet.Add(new ComponentModel() { ComponentID = 4, ComponentName = "HG4class", Level = 2, ParentID = 1, ComponentToolTip = "初始四星手枪人形", ComponentHeader = "★★★★", ComponentForeground = type4color });
            initializeDataSet.Add(new ComponentModel() { ComponentID = 5, ComponentName = "HG5class", Level = 2, ParentID = 1, ComponentToolTip = "初始五星手枪人形", ComponentHeader = "★★★★★", ComponentForeground = type5color });
            initializeDataSet.Add(new ComponentModel() { ComponentID = 7, ComponentName = "HG7class", Level = 2, ParentID = 1, ComponentToolTip = "特典手枪人形", ComponentHeader = "★EXTRA", ComponentForeground = type7color });

            initializeDataSet.Add(new ComponentModel() { ComponentID = 11, ComponentName = "SMGclass", Level = 1, ParentID = 0, ComponentToolTip = "冲锋枪人形", ComponentHeader = "冲锋枪(SMG)", ComponentForeground = defaultColor });
            initializeDataSet.Add(new ComponentModel() { ComponentID = 12, ComponentName = "SMG2class", Level = 2, ParentID = 11, ComponentToolTip = "初始二星冲锋枪人形", ComponentHeader = "★★", ComponentForeground = type2color });
            initializeDataSet.Add(new ComponentModel() { ComponentID = 13, ComponentName = "SMG3class", Level = 2, ParentID = 11, ComponentToolTip = "初始三星冲锋枪人形", ComponentHeader = "★★★", ComponentForeground = type3color });
            initializeDataSet.Add(new ComponentModel() { ComponentID = 14, ComponentName = "SMG4class", Level = 2, ParentID = 11, ComponentToolTip = "初始四星冲锋枪人形", ComponentHeader = "★★★★", ComponentForeground = type4color });
            initializeDataSet.Add(new ComponentModel() { ComponentID = 15, ComponentName = "SMG5class", Level = 2, ParentID = 11, ComponentToolTip = "初始五星冲锋枪人形", ComponentHeader = "★★★★★", ComponentForeground = type5color });
            initializeDataSet.Add(new ComponentModel() { ComponentID = 17, ComponentName = "SMG7class", Level = 2, ParentID = 11, ComponentToolTip = "特典冲锋枪人形", ComponentHeader = "★EXTRA", ComponentForeground = type7color });

            initializeDataSet.Add(new ComponentModel() { ComponentID = 21, ComponentName = "RFclass", Level = 1, ParentID = 0, ComponentToolTip = "步枪人形", ComponentHeader = "步枪(RF)", ComponentForeground = defaultColor });
            initializeDataSet.Add(new ComponentModel() { ComponentID = 22, ComponentName = "RF2class", Level = 2, ParentID = 21, ComponentToolTip = "初始二星步枪人形", ComponentHeader = "★★", ComponentForeground = type2color });
            initializeDataSet.Add(new ComponentModel() { ComponentID = 23, ComponentName = "RF3class", Level = 2, ParentID = 21, ComponentToolTip = "初始三星步枪人形", ComponentHeader = "★★★", ComponentForeground = type3color });
            initializeDataSet.Add(new ComponentModel() { ComponentID = 24, ComponentName = "RF4class", Level = 2, ParentID = 21, ComponentToolTip = "初始四星步枪人形", ComponentHeader = "★★★★", ComponentForeground = type4color });
            initializeDataSet.Add(new ComponentModel() { ComponentID = 25, ComponentName = "RF5class", Level = 2, ParentID = 21, ComponentToolTip = "初始五星步枪人形", ComponentHeader = "★★★★★", ComponentForeground = type5color });
            initializeDataSet.Add(new ComponentModel() { ComponentID = 27, ComponentName = "RF7class", Level = 2, ParentID = 21, ComponentToolTip = "特典步枪人形", ComponentHeader = "★EXTRA", ComponentForeground = type7color });

            initializeDataSet.Add(new ComponentModel() { ComponentID = 31, ComponentName = "ARclass", Level = 1, ParentID = 0, ComponentToolTip = "突击步枪人形", ComponentHeader = "突击步枪(AR)", ComponentForeground = defaultColor });
            initializeDataSet.Add(new ComponentModel() { ComponentID = 32, ComponentName = "AR2class", Level = 2, ParentID = 31, ComponentToolTip = "初始二星突击步枪人形", ComponentHeader = "★★", ComponentForeground = type2color });
            initializeDataSet.Add(new ComponentModel() { ComponentID = 33, ComponentName = "AR3class", Level = 2, ParentID = 31, ComponentToolTip = "初始三星突击步枪人形", ComponentHeader = "★★★", ComponentForeground = type3color });
            initializeDataSet.Add(new ComponentModel() { ComponentID = 34, ComponentName = "AR4class", Level = 2, ParentID = 31, ComponentToolTip = "初始四星突击步枪人形", ComponentHeader = "★★★★", ComponentForeground = type4color });
            initializeDataSet.Add(new ComponentModel() { ComponentID = 35, ComponentName = "AR5class", Level = 2, ParentID = 31, ComponentToolTip = "初始五星突击步枪人形", ComponentHeader = "★★★★★", ComponentForeground = type5color });
            initializeDataSet.Add(new ComponentModel() { ComponentID = 37, ComponentName = "AR7class", Level = 2, ParentID = 31, ComponentToolTip = "特典突击步枪人形", ComponentHeader = "★EXTRA", ComponentForeground = type7color });

            initializeDataSet.Add(new ComponentModel() { ComponentID = 41, ComponentName = "MGclass", Level = 1, ParentID = 0, ComponentToolTip = "机枪人形", ComponentHeader = "机枪(MG)", ComponentForeground = defaultColor });
            initializeDataSet.Add(new ComponentModel() { ComponentID = 42, ComponentName = "MG2class", Level = 2, ParentID = 41, ComponentToolTip = "初始二星机枪人形", ComponentHeader = "★★", ComponentForeground = type2color });
            initializeDataSet.Add(new ComponentModel() { ComponentID = 43, ComponentName = "MG3class", Level = 2, ParentID = 41, ComponentToolTip = "初始三星机枪人形", ComponentHeader = "★★★", ComponentForeground = type3color });
            initializeDataSet.Add(new ComponentModel() { ComponentID = 44, ComponentName = "MG4class", Level = 2, ParentID = 41, ComponentToolTip = "初始四星机枪人形", ComponentHeader = "★★★★", ComponentForeground = type4color });
            initializeDataSet.Add(new ComponentModel() { ComponentID = 45, ComponentName = "MG5class", Level = 2, ParentID = 41, ComponentToolTip = "初始五星机枪人形", ComponentHeader = "★★★★★", ComponentForeground = type5color });
            initializeDataSet.Add(new ComponentModel() { ComponentID = 47, ComponentName = "MG7class", Level = 2, ParentID = 41, ComponentToolTip = "特典机枪人形", ComponentHeader = "★EXTRA", ComponentForeground = type7color });

            initializeDataSet.Add(new ComponentModel() { ComponentID = 51, ComponentName = "SGclass", Level = 1, ParentID = 0, ComponentToolTip = "霰弹枪人形", ComponentHeader = "霰弹枪(SG)", ComponentForeground = defaultColor });
            initializeDataSet.Add(new ComponentModel() { ComponentID = 52, ComponentName = "SG2class", Level = 2, ParentID = 51, ComponentToolTip = "初始二星霰弹枪人形", ComponentHeader = "★★", ComponentForeground = type2color });
            initializeDataSet.Add(new ComponentModel() { ComponentID = 53, ComponentName = "SG3class", Level = 2, ParentID = 51, ComponentToolTip = "初始三星霰弹枪人形", ComponentHeader = "★★★", ComponentForeground = type3color });
            initializeDataSet.Add(new ComponentModel() { ComponentID = 54, ComponentName = "SG4class", Level = 2, ParentID = 51, ComponentToolTip = "初始四星霰弹枪人形", ComponentHeader = "★★★★", ComponentForeground = type4color });
            initializeDataSet.Add(new ComponentModel() { ComponentID = 55, ComponentName = "SG5class", Level = 2, ParentID = 51, ComponentToolTip = "初始五星霰弹枪人形", ComponentHeader = "★★★★★", ComponentForeground = type5color });
            initializeDataSet.Add(new ComponentModel() { ComponentID = 57, ComponentName = "SG7class", Level = 2, ParentID = 51, ComponentToolTip = "特典霰弹枪人形", ComponentHeader = "★EXTRA", ComponentForeground = type7color });

            initializeDataSet.Add(new ComponentModel() { ComponentID = 61, ComponentName = "OTHERclass", Level = 1, ParentID = 0, ComponentToolTip = "其它人形", ComponentHeader = "其它", ComponentForeground = defaultColor });
            initializeDataSet.Add(new ComponentModel() { ComponentID = 62, ComponentName = "HOCclass", Level = 2, ParentID = 61, ComponentToolTip = "重装部队人形", ComponentHeader = "重装部队", ComponentForeground = defaultColor });
            initializeDataSet.Add(new ComponentModel() { ComponentID = 63, ComponentName = "HUMANclass", Level = 2, ParentID = 61, ComponentToolTip = "人类", ComponentHeader = "人类", ComponentForeground = defaultColor });
            initializeDataSet.Add(new ComponentModel() { ComponentID = 64, ComponentName = "UNKNOWNclass", Level = 2, ParentID = 61, ComponentToolTip = "未分类的人形", ComponentHeader = "未分类", ComponentForeground = defaultColor });

            //initializeDataSet.Add(new ComponentModel() { ComponentID = 71, ComponentName = "COALITIONclass", Level = 1, ParentID = 0, ComponentToolTip = "融合势力人形", ComponentHeader = "融合势力", ComponentForeground = defaultColor });
            //initializeDataSet.Add(new ComponentModel() { ComponentID = 72, ComponentName = "SANGVISFERRIclass", Level = 2, ParentID = 71, ComponentToolTip = "铁血工造人形", ComponentHeader = "铁血工造", ComponentForeground = defaultColor });
            //initializeDataSet.Add(new ComponentModel() { ComponentID = 73, ComponentName = "KCCOclass", Level = 2, ParentID = 71, ComponentToolTip = "正规军人形", ComponentHeader = "正规军", ComponentForeground = defaultColor });
            //initializeDataSet.Add(new ComponentModel() { ComponentID = 74, ComponentName = "PARADEUSclass", Level = 2, ParentID = 71, ComponentToolTip = "帕拉蒂斯人形", ComponentHeader = "帕拉蒂斯", ComponentForeground = defaultColor });
            //initializeDataSet.Add(new ComponentModel() { ComponentID = 75, ComponentName = "ETCclass", Level = 2, ParentID = 71, ComponentToolTip = "其他势力人形", ComponentHeader = "其他势力", ComponentForeground = defaultColor });


            try
            {
                string str = File.ReadAllText($"{AppDomain.CurrentDomain.BaseDirectory}dummy_list.json");
                RootObject rb = JsonConvert.DeserializeObject<RootObject>(str);

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
                    pb_loader.Value += 1;
                    tii.ProgressValue = pb_loader.Value / pb_loader.Maximum;
                    try
                    {
                        bool dummyExistence = Directory.Exists($@"{AppDomain.CurrentDomain.BaseDirectory}Resources\spine\{content.Name}");
                        ComponentModel node = new ComponentModel();
                        node.ComponentName = $"dummy_{content.Name.Replace(" ", string.Empty)}";
                        node.ComponentHeader = content.DisplayName;
                        node.ComponentID = 100 + counter;
                        string[] tagString = new string[5];
                        tagString[0] = $"{dummyExistence}";
                        tagString[1] = content.Name;
                        tagString[2] = content.Parent;
                        tagString[3] = content.Type;
                        tagString[4] = content.DisplayName;
                        node.ComponentTag = tagString;
                        //node.ImageKey = content.Type;
                        //node.SelectedImageKey = content.Type;
                        node.ComponentForeground = defaultColor;
                        if (content.Type.Contains("2")) { node.ComponentForeground = type2color; }
                        if (content.Type.Contains("3")) { node.ComponentForeground = type3color; }
                        if (content.Type.Contains("4")) { node.ComponentForeground = type4color; }
                        if (content.Type.Contains("5")) { node.ComponentForeground = type5color; }
                        if (content.Type.Contains("6")) { node.ComponentForeground = type6color; }
                        if (content.Type.Contains("7")) { node.ComponentForeground = type7color; }

                        node.ParentID = 64;
                        if (content.Name == content.Parent)
                        {
                            node.ComponentToolTip = content.DisplayName;
                            node.Level = 3;
                            switch (content.Type)
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
                            }
                            initializeDataSet.Add(node);
                        }
                        else
                        {
                            node.Level = 4;
                            //if (content.Type == "HUMAN") { node.Level = 3; }
                            node.ParentID = 64;
                            foreach (ComponentModel item in initializeDataSet)
                            {
                                if (item.ComponentName == $"dummy_{content.Parent.Replace(" ", string.Empty)}")
                                {
                                    node.ComponentToolTip = $"{item.ComponentHeader}{skin_connector}{content.DisplayName}";
                                    //node.IsExpanded = true;
                                    node.ParentID = item.ComponentID;
                                }
                            }
                            initializeDataSet.Add(node);
                        }
                    }
                    catch (Exception)
                    {

                    }
                }


                //加载数据
                tv_InternalSelector.ItemsSource = LoadTreeView(0);

                foreach (ComponentModel item in initializeDataSet)
                {
                    item.IsExpanded = true;
                    item.IsExpanded = false;
                }

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
                sb_tvis.IsEnabled = true;
                //tv_InternalSelector.Items.Add(treeViewItemTemp);
            }
            catch (Exception)
            { }
        }
        private ComponentModel SelectedItem;
        private void tv_InternalSelector_Selected(object sender, RoutedEventArgs e)
        {
            lbl_InternalSelected.Content = "请选择要加载的战术人形";
            lbl_InternalSelected.Foreground = defaultColor;

            TreeViewItem tvi = e.OriginalSource as TreeViewItem;
            ComponentModel item = (ComponentModel)tvi.Header;
            SelectedItem = item;
            Console.WriteLine("activated");

            tvAfterSelect();
        }

        private void tvAfterSelect()
        {
            ComponentModel item = SelectedItem;
            //TreeViewItem item = (TreeViewItem)tv_InternalSelector.SelectedItem;

            btn_loadData.IsEnabled = false;
            btn_loadCG.IsEnabled = false;
            btn_downloadData.IsEnabled = false;
            btn_deleteData.IsEnabled = false;
            chb_dorm_data.IsChecked = false;
            chb_dorm_data.IsEnabled = false;

            if (item != null)
            {
                lbl_InternalSelected.Content = item.ComponentToolTip;
                lbl_InternalSelected.Foreground = item.ComponentForeground;
                if (!item.ComponentName.Contains("class"))
                {
                    btn_loadCG.IsEnabled = true;

                    string[] tagString = new string[5];
                    tagString[1] = item.ComponentTag[1];//content.Name;
                    tagString[2] = item.ComponentTag[2];//content.Parent;
                    tagString[3] = item.ComponentTag[3];//content.Type;
                    tagString[4] = item.ComponentTag[4];//content.DisplayName;

                    if (Directory.Exists($@"{AppDomain.CurrentDomain.BaseDirectory}Resources\spine\{tagString[1]}"))
                    {
                        btn_deleteData.IsEnabled = true;
                        btn_downloadData.IsEnabled = false;
                        btn_loadData.IsEnabled = true;
                        if (File.Exists($@"{AppDomain.CurrentDomain.BaseDirectory}Resources\spine\{tagString[1]}\r{tagString[1]}.skel"))
                        {
                            chb_dorm_data.IsEnabled = true;
                        }
                    }
                    else
                    {
                        btn_deleteData.IsEnabled = false;
                        btn_downloadData.IsEnabled = true;
                        btn_loadData.IsEnabled = false;
                    }
                }
            }
        }

        private void btn_LoadDummyList_Click(object sender, RoutedEventArgs e)
        {
            string DummyListStr = HttpRequestHelper.PostWebRequest("https://api.brightsu.cn/snqx/gfddd/", $"request=dummy", Encoding.UTF8);
            DummyListRoot rt = JsonConvert.DeserializeObject<DummyListRoot>(DummyListStr);
            if (File.Exists($"{AppDomain.CurrentDomain.BaseDirectory}dummy_list.json"))
            {
                string str = File.ReadAllText($"{AppDomain.CurrentDomain.BaseDirectory}dummy_list.json");
                RootObject rb = JsonConvert.DeserializeObject<RootObject>(str);
                if (rt.data.dummy_list_version != rb.meta.version)
                {
                    MessageBoxResult updateListResult = MessageBox.Show($"战术人形数据表有新版本可用。\n本地版本：{rb.meta.version}\n最新版本：{rt.data.dummy_list_version}\n更新日志：{rt.data.dummy_list_version_log}\n\n是否立即下载数据表？", "更新数据表", MessageBoxButton.YesNo, MessageBoxImage.Question, MessageBoxResult.Yes);
                    if (updateListResult == MessageBoxResult.Yes)
                    {
                        HttpClass.DownloadFile(rt.data.dummy_list_link, $"{AppDomain.CurrentDomain.BaseDirectory}dummy_list.json", pb_loader, lbl_loader);
                        btn_LoadDummyList_Click(this, null);
                    }
                    else
                    {
                        LoadDummyList();
                    }
                }
                else
                {
                    LoadDummyList();
                }
            }
            else
            {
                MessageBoxResult downloadListResult = MessageBox.Show("本地战术人形数据表不存在，无法继续加载。\n是否立即下载数据表？", "数据表加载失败", MessageBoxButton.YesNo, MessageBoxImage.Exclamation, MessageBoxResult.Yes);
                if (downloadListResult == MessageBoxResult.Yes)
                {
                    try
                    {
                        if (CheckIsUrlFormat(rt.data.dummy_list_link))
                        {
                            bool downloaded = HttpClass.DownloadFile(rt.data.dummy_list_link, $"{AppDomain.CurrentDomain.BaseDirectory}dummy_list.json", pb_loader, lbl_loader);
                            if (downloaded)
                            {
                                //MessageBox.Show($"数据表下载成功！\n请单击『加载』按钮开始读取战术人形数据列表", "数据表下载成功", MessageBoxButton.OK, MessageBoxImage.Information);
                                btn_LoadDummyList_Click(this, null);
                            }
                            else
                            {
                                MessageBox.Show($"数据表下载失败，请重试。", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                            }
                        }
                        else
                        {
                            MessageBox.Show($"获取数据表时出错。\n获取到的地址不合法。", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"获取与更新数据表时出错。\n{ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
        }

        private void btn_loadCG_Click(object sender, RoutedEventArgs e)
        {
            ComponentModel item = SelectedItem;
            string[] tagString = new string[5];
            tagString[0] = item.ComponentTag[0];//DummyExistence
            tagString[1] = item.ComponentTag[1];//content.Name;
            tagString[2] = item.ComponentTag[2];//content.Parent;
            tagString[3] = item.ComponentTag[3];//content.Type;
            tagString[4] = item.ComponentTag[4];//content.DisplayName;

            string CGStr = HttpRequestHelper.PostWebRequest($"https://api.brightsu.cn/snqx/gfddd/", $"request=cg&dummy={tagString[1].ToLowerInvariant()}", Encoding.UTF8);
            CGRoot rt = JsonConvert.DeserializeObject<CGRoot>(CGStr);
            string cg_url = rt.data.cg_url;
            if (chb_alt_download.IsChecked.Value) { cg_url = rt.data.cg_url_alt; }

            // 是否存在本地立绘
            bool local_cg = File.Exists($@"{AppDomain.CurrentDomain.BaseDirectory}Resources\pic\{rt.data.cg_filename}");
            bool local_cg_d = File.Exists($@"{AppDomain.CurrentDomain.BaseDirectory}Resources\pic\{rt.data.cg_d_filename}");

            if (rt.data.cg_exist)
            {
                if (local_cg && local_cg_d) // 本地存在普通及大破立绘
                {
                    new WindowCG().LoadCG(item.ComponentToolTip.ToString(), $@"{AppDomain.CurrentDomain.BaseDirectory}Resources\pic\", rt.data.cg_filename, rt.data.cg_d_filename);
                }
                else if (local_cg && !rt.data.cg_d_exist) // 本地存在普通立绘，且人形本身不存在大破立绘
                {
                    new WindowCG().LoadCG(item.ComponentToolTip.ToString(), $@"{AppDomain.CurrentDomain.BaseDirectory}Resources\pic\", rt.data.cg_filename);
                }
                else // 本地立绘不完整或没有立绘
                {
                    if (chb_download_cg.IsChecked.Value)
                    {
                        LocalCG(tagString[1].ToLowerInvariant(), item.ComponentToolTip.ToString());
                    }
                    else
                    {
                        if (rt.data.cg_d_exist)
                        {
                            new WindowCG().LoadCG(item.ComponentToolTip.ToString(), cg_url, rt.data.cg_filename, rt.data.cg_d_filename);
                        }
                        else
                        {
                            new WindowCG().LoadCG(item.ComponentToolTip.ToString(), cg_url, rt.data.cg_filename);
                        }
                    }
                }
            }
            else
            {
                MessageBox.Show($"{item.ComponentToolTip} ({tagString[1].ToLowerInvariant()}) 没有对应的立绘数据。", "立绘加载失败", MessageBoxButton.OK, MessageBoxImage.Exclamation);
            }
        }

        private void LocalCG(string dummy, string dummy_display)
        {
            tv_InternalSelector.IsEnabled = false;
            btn_loadCG.IsEnabled = false;
            sb_tvis.IsEnabled = false;
            string CGStr = HttpRequestHelper.PostWebRequest($"https://api.brightsu.cn/snqx/gfddd/", $"request=cg&dummy={dummy}", Encoding.UTF8);
            CGRoot rt = JsonConvert.DeserializeObject<CGRoot>(CGStr);
            string cg_url = rt.data.cg_url;
            if (chb_alt_download.IsChecked.Value) { cg_url = rt.data.cg_url_alt; }
            if (rt.data.cg_exist)
            {
                if (!Directory.Exists($@"{AppDomain.CurrentDomain.BaseDirectory}Resources\pic"))
                {
                    Directory.CreateDirectory($@"{AppDomain.CurrentDomain.BaseDirectory}Resources\pic");
                }
                sp_downloader.Visibility = Visibility.Visible;
                pb_loader.IsIndeterminate = false;
                pb_loader.Value = 0;
                pb_loader.Maximum = 1;
                if (rt.data.cg_d_exist)
                {
                    lbl_loader.Content = $"正在下载 {dummy_display} 的大破立绘数据";
                    pb_loader.Maximum = 2;
                    HttpClass.DownloadFile($"{cg_url}/{rt.data.cg_d_filename}", $@"{AppDomain.CurrentDomain.BaseDirectory}Resources\pic\{rt.data.cg_d_filename}", pb_downloader, lbl_downloader);
                    pb_loader.Value++;
                }
                lbl_loader.Content = $"正在下载 {dummy_display} 的立绘数据";
                HttpClass.DownloadFile($"{cg_url}/{rt.data.cg_filename}", $@"{AppDomain.CurrentDomain.BaseDirectory}Resources\pic\{rt.data.cg_filename}", pb_downloader, lbl_downloader);
                pb_loader.Value++;
                sp_downloader.Visibility = Visibility.Collapsed;
                lbl_loader.Content = "准备就绪";
                pb_loader.IsIndeterminate = true;

                if (rt.data.cg_d_exist)
                {
                    new WindowCG().LoadCG(dummy_display, $@"{AppDomain.CurrentDomain.BaseDirectory}Resources\pic\", rt.data.cg_filename, rt.data.cg_d_filename);
                }
                else
                {
                    new WindowCG().LoadCG(dummy_display, $@"{AppDomain.CurrentDomain.BaseDirectory}Resources\pic\", rt.data.cg_filename);
                }
            }
            else
            {
                MessageBox.Show($"{dummy_display} ({dummy}) 没有对应的立绘数据。", "立绘加载失败", MessageBoxButton.OK, MessageBoxImage.Exclamation);
            }
            tv_InternalSelector.IsEnabled = true;
            btn_loadCG.IsEnabled = true;
            sb_tvis.IsEnabled = true;
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
            string[] tagString = new string[5];
            tagString[0] = item.ComponentTag[0];//DummyExistence
            tagString[1] = item.ComponentTag[1];//content.Name;
            tagString[2] = item.ComponentTag[2];//content.Parent;
            tagString[3] = item.ComponentTag[3];//content.Type;
            tagString[4] = item.ComponentTag[4];//content.DisplayName;

            LoadInternalSpine(tagString[1].ToLowerInvariant(), item.ComponentToolTip);
        }


        private void LoadInternalSpine(string DollName, string DollDisplayName)
        {
            double CanvasWidth = nud_canvas_x.Value;
            double CanvasHeight = nud_canvas_y.Value;
            App.globalValues.FrameWidth = CanvasWidth;
            App.globalValues.FrameHeight = CanvasHeight;
            App.canvasWidth = CanvasWidth;
            App.canvasHeight = CanvasHeight;

            if (chb_dorm_data.IsChecked.Value)
            {
                if (File.Exists($@"{AppDomain.CurrentDomain.BaseDirectory}Resources\spine\{DollName}\r{ DollName}.atlas"))
                { App.globalValues.SelectAtlasFile = $@"{AppDomain.CurrentDomain.BaseDirectory}Resources\spine\{DollName}\r{DollName}.atlas"; }
                else
                { App.globalValues.SelectAtlasFile = $@"{AppDomain.CurrentDomain.BaseDirectory}Resources\spine\{DollName}\{DollName}.atlas"; }
                App.globalValues.SelectSpineFile = $@"{AppDomain.CurrentDomain.BaseDirectory}Resources\spine\{DollName}\r{DollName}.skel";
            }
            else
            {
                App.globalValues.SelectAtlasFile = $@"{AppDomain.CurrentDomain.BaseDirectory}Resources\spine\{DollName}\{DollName}.atlas";
                App.globalValues.SelectSpineFile = $@"{AppDomain.CurrentDomain.BaseDirectory}Resources\spine\{DollName}\{DollName}.skel";
            }
            App.globalValues.Alpha = true;
            App.globalValues.PreMultiplyAlpha = true;
            App.isNew = true;
            _window.LoadPlayer("2.1.25");

            App.globalValues.Dummy = DollName;

            if (chb_dorm_data.IsChecked.Value)
            {
                App.globalValues.DummyDisplayName = $"[宿舍] {DollDisplayName}";
            }
            else
            {
                App.globalValues.DummyDisplayName = DollDisplayName;
            }

            //App.globalValues.PosX = 224;
            //App.globalValues.PosY = 224;
            App.globalValues.SelectSkin = "default";
            App.globalValues.SelectAnimeName = "wait";
        }

        private void btn_downloadData_Click(object sender, RoutedEventArgs e)
        {
            ComponentModel item = SelectedItem;
            string[] tagString = new string[5];
            tagString[0] = item.ComponentTag[0];//DummyExistence
            tagString[1] = item.ComponentTag[1];//content.Name;
            tagString[2] = item.ComponentTag[2];//content.Parent;
            tagString[3] = item.ComponentTag[3];//content.Type;
            tagString[4] = item.ComponentTag[4];//content.DisplayName;
            //if (tagString[1] != tagString[2]) //子皮肤，非本体原皮
            //{
            //    if (!File.Exists($@"{AppDomain.CurrentDomain.BaseDirectory}Resources\spine\{tagString[2]}"))
            //    {
            //        DownloadData(tagString[2], item.ToolTip.ToString().Replace($"{skin_connector}{tagString[4]}", string.Empty));
            //    }
            //}
            DownloadData(tagString[1], item.ComponentToolTip);
        }


        private void DownloadData(string dummy, string dummy_display)
        {
            ComponentModel item = SelectedItem;
            chb_alt_download.IsEnabled = false;
            btn_downloadData.IsEnabled = false;
            tv_InternalSelector.IsEnabled = false;
            sb_tvis.IsEnabled = false;
            string DummyStr = HttpRequestHelper.PostWebRequest($"https://api.brightsu.cn/snqx/gfddd/", $"request=dummy&dummy={dummy}", Encoding.UTF8);
            DummyRoot rt = JsonConvert.DeserializeObject<DummyRoot>(DummyStr);
            string downloadSource = rt.data.dummy_dir;
            if (chb_alt_download.IsChecked.Value)
            {
                downloadSource = rt.data.dummy_dir_alt;
            }
            else
            {
                downloadSource = rt.data.dummy_dir;
            }

            if (rt.data.dummy == dummy)
            {
                if (rt.data.dummy_exist)
                {
                    if (rt.data.dummy_files_count > 0)
                    {
                        if (CheckIsUrlFormat(downloadSource))
                        {
                            int total_files = rt.data.dummy_files_count;
                            pb_loader.Value = 0;
                            pb_loader.Maximum = total_files;
                            tii.ProgressState = System.Windows.Shell.TaskbarItemProgressState.Normal;
                            
                            sp_downloader.Visibility = Visibility.Visible;
                            pb_loader.IsIndeterminate = false;
                            lbl_loader.Content = $"正在下载 {dummy_display}";

                            foreach (string filename in rt.data.dummy_files.Split('|'))
                            {
                                pb_loader.Value ++;
                                tii.ProgressValue = pb_loader.Value / pb_loader.Maximum;
                                lbl_loader.Content = $"正在下载 {dummy_display}：{filename} ({pb_loader.Value}/{total_files})";

                                if (!Directory.Exists($@"{AppDomain.CurrentDomain.BaseDirectory}Resources\spine\{dummy}"))
                                {
                                    Directory.CreateDirectory($@"{AppDomain.CurrentDomain.BaseDirectory}Resources\spine\{dummy}");
                                }
                                
                                HttpClass.DownloadFile($"{downloadSource}/{filename}", $@"{AppDomain.CurrentDomain.BaseDirectory}Resources\spine\{dummy}\{filename}", pb_downloader, lbl_downloader);
                                
                                KillEmptyDirectory($@"{AppDomain.CurrentDomain.BaseDirectory}\Resources\spine");
                            }
                            lbl_loader.Content = $"已完成下载 {dummy_display}，等待下一步操作...";
                            pb_loader.IsIndeterminate = true;
                            sp_downloader.Visibility = Visibility.Collapsed;
                            tii.ProgressState = System.Windows.Shell.TaskbarItemProgressState.Indeterminate;
                            
                        }
                        else
                        {
                            MessageBox.Show($"战术人形数据下载失败。\n服务器端返回了无效的路径。", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                    }
                    else
                    {
                        MessageBox.Show($"战术人形数据下载失败。\n服务器端未包含该人形的有效数据。", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
                else
                {
                    MessageBox.Show($"战术人形数据下载失败。\n服务器端不存在所请求的人形数据。", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            else
            {
                MessageBox.Show($"战术人形数据下载失败。\n服务器端返回的人形数据与所请求的不一致。", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            //btn_downloadData.IsEnabled = false;
            //btn_deleteData.IsEnabled = true;
            chb_alt_download.IsEnabled = true;
            //if (Directory.Exists($@"{AppDomain.CurrentDomain.BaseDirectory}Resources\spine\{item.Name.ToLowerInvariant()}"))
            //{
            //    btn_deleteData.IsEnabled = true;
            //    btn_downloadData.IsEnabled = false;
            //    btn_loadData.IsEnabled = true;
            //    if (File.Exists($@"{AppDomain.CurrentDomain.BaseDirectory}Resources\spine\{item.Name.ToLowerInvariant()}\r{item.Name.ToLowerInvariant()}.skel"))
            //    {
            //        chb_dorm_data.IsEnabled = true;
            //    }
            //}
            //else
            //{
            //    btn_deleteData.IsEnabled = false;
            //    btn_downloadData.IsEnabled = true;
            //    btn_loadData.IsEnabled = false;
            //}
            tv_InternalSelector.IsEnabled = true;
            sb_tvis.IsEnabled = true;
            //item.IsSelected = false;
            
            tvAfterSelect();
        }

        private void btn_deleteData_Click(object sender, RoutedEventArgs e)
        {
            ComponentModel item = SelectedItem;
            string[] tagString = new string[5];
            tagString[0] = item.ComponentTag[0];//DummyExistence
            tagString[1] = item.ComponentTag[1];//content.Name;
            tagString[2] = item.ComponentTag[2];//content.Parent;
            tagString[3] = item.ComponentTag[3];//content.Type;
            tagString[4] = item.ComponentTag[4];//content.DisplayName;
            //if (tagString[1] == tagString[2]) //本体原皮
            //{
            //    MessageBoxResult deleteConfirm = MessageBox.Show($"请注意，你正在尝试删除原型数据。{Environment.NewLine}当原型数据被删除，该人形的其它皮肤（如存在）将不会显示在加载列表中。{Environment.NewLine}确定要删除 {item.ToolTip}({tagString[1]}) 吗？", "警告", MessageBoxButton.YesNo, MessageBoxImage.Exclamation);
            //    if (deleteConfirm == MessageBoxResult.Yes) { DeleteData(tagString[1]); }
            //}
            //else
            //{
                DeleteData(tagString[1]);
            //}
        }

        private void DeleteData(string dummy)
        {
            if (Directory.Exists($@"{AppDomain.CurrentDomain.BaseDirectory}\Resources\spine\{dummy}"))
            {
                Directory.Delete($@"{AppDomain.CurrentDomain.BaseDirectory}\Resources\spine\{dummy}", true);
            }
            btn_downloadData.IsEnabled = true;
            btn_deleteData.IsEnabled = false;
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            e.Cancel = true;
            Hide();
        }

        private void sb_tvis_SearchStarted(object sender, HandyControl.Data.FunctionEventArgs<string> e)
        {
            
            if (sb_tvis.Text.Replace(" ", string.Empty) != string.Empty)
            {
                foreach (ComponentModel item in initializeDataSet)
                {
                    string itemheader = item.ComponentHeader.ToLowerInvariant();
                    if (itemheader.Contains(sb_tvis.Text.ToLowerInvariant()))
                    {
                        if (item.ComponentID > SelectedItem.ComponentID)
                        {
                            if (item.Level == 4)
                            {
                                ComponentModel level3parent = initializeDataSet.Find(delegate (ComponentModel model) { return model.ComponentID == item.ParentID; });
                                ComponentModel level2parent = initializeDataSet.Find(delegate (ComponentModel model) { return model.ComponentID == level3parent.ParentID; });
                                ComponentModel level1parent = initializeDataSet.Find(delegate (ComponentModel model) { return model.ComponentID == level2parent.ParentID; });
                                level1parent.IsExpanded = true;
                                level2parent.IsExpanded = true;
                                level3parent.IsExpanded = true;
                                item.IsSelected = true;
                                //SelectedItem = item;
                            }
                            else if (item.Level == 3)
                            {
                                //initializeDataSet.Find(obj => obj.ParentID == 1)
                                ComponentModel level2parent = initializeDataSet.Find(delegate (ComponentModel model) { return model.ComponentID == item.ParentID; });
                                ComponentModel level1parent = initializeDataSet.Find(delegate (ComponentModel model) { return model.ComponentID == level2parent.ParentID; });
                                level1parent.IsExpanded = true;
                                level2parent.IsExpanded = true;
                                item.IsSelected = true;
                                //(item.ParentID).IsExpanded = true;
                            }
                        }
                    }
                }
                tvAfterSelect();
            }
        }

        private void tv_InternalSelector_Unselected(object sender, RoutedEventArgs e)
        {
            SelectedItem = null;
            btn_downloadData.IsEnabled = false;
            btn_deleteData.IsEnabled = false;
            btn_loadCG.IsEnabled = false;
            btn_loadData.IsEnabled = false;
            chb_dorm_data.IsChecked = false;
            chb_dorm_data.IsEnabled = false;
        }
    }
}
