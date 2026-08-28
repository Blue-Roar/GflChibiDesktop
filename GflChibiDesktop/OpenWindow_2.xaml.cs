using System;
using System.Collections.Generic;
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

namespace GflChibiDesktop.Windows
{
    /// <summary>
    /// Open.xaml 的互動邏輯
    /// </summary>
    public partial class OpenWindow : Window
    {
        private MainWindow _window;

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
                this.Close();
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
            tv_InternalSelector.Items.Clear();

            try
            {
                string str = File.ReadAllText($"{AppDomain.CurrentDomain.BaseDirectory}dummy_list.json");
                RootObject rb = JsonConvert.DeserializeObject<RootObject>(str);

                int total = rb.content.Count;
                pb_loader.Maximum = total;
                pb_loader.Value = 0;
                tii.ProgressState = System.Windows.Shell.TaskbarItemProgressState.Normal;
                tii.ProgressValue = 0;
                int counter = 0;

				foreach (Content content in rb.content)
				{
					try
					{
						bool dummyExistence = Directory.Exists($@"{AppDomain.CurrentDomain.BaseDirectory}Resources\spine\{content.Name}");
						TreeViewItem node = new TreeViewItem();
						node.Name = $"dummy_{content.Name.Replace(" ", string.Empty)}";
						node.Header = content.DisplayName;
						string[] tagString = new string[5];
						tagString[0] = $"{dummyExistence}";
						tagString[1] = content.Name;
						tagString[2] = content.Parent;
						tagString[3] = content.Type;
						tagString[4] = content.DisplayName;
						node.Tag = tagString;
						//node.ImageKey = content.Type;
						//node.SelectedImageKey = content.Type;
						if (content.Type.Contains("2")) { node.Foreground = type2color; }
						if (content.Type.Contains("3")) { node.Foreground = type3color; }
						if (content.Type.Contains("4")) { node.Foreground = type4color; }
						if (content.Type.Contains("5")) { node.Foreground = type5color; }
						if (content.Type.Contains("6")) { node.Foreground = type6color; }
						if (content.Type.Contains("7")) { node.Foreground = type7color; }

						if (content.Name == content.Parent)
						{
							node.ToolTip = content.DisplayName;
							//node.IsExpanded = true;
							tv_InternalSelector.Items.Add(node);
						}
						else
						{
							foreach (TreeViewItem item in tv_InternalSelector.Items)
							{
								if (item.Name == $"dummy_{content.Parent.Replace(" ", string.Empty)}")
								{
									node.ToolTip = $"{item.Header}{skin_connector}{content.DisplayName}";
									//node.IsExpanded = true;
									item.Items.Add(node);
								}
							}
						}
					}
					catch (Exception)
					{

					}
					//counter += 1;
					//lbl_loader.Content = $"正在处理：{counter} / {total}";
					//pb_loader.Value += 1;
					//tii.ProgressValue = pb_loader.Value / pb_loader.Maximum;
				}

                lbl_loader.Content = $"准备就绪";
                tii.ProgressValue = 100;
                tii.ProgressState = System.Windows.Shell.TaskbarItemProgressState.Indeterminate;
                sb_tvis.IsEnabled = true;
                //tv_InternalSelector.Items.Add(treeViewItemTemp);
            }
            catch (Exception)
            { }
        }

        private void tv_InternalSelector_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            lbl_InternalSelected.Content = "请选择要加载的战术人形";
            lbl_InternalSelected.Foreground = new SolidColorBrush(Color.FromRgb(255, 255, 255));

            TreeViewItem item = (TreeViewItem)tv_InternalSelector.SelectedItem;

            btn_loadData.IsEnabled = false;
            btn_loadCG.IsEnabled = false;
            btn_downloadData.IsEnabled = false;
            btn_deleteData.IsEnabled = false;
            chb_dorm_data.IsChecked = false;
            chb_dorm_data.IsEnabled = false;

            if (tv_InternalSelector.SelectedItem != null)
            {
                lbl_InternalSelected.Content = item.ToolTip;
                lbl_InternalSelected.Foreground = item.Foreground;
                if (!item.Name.Contains("class"))
                {
                    btn_loadCG.IsEnabled = true;

                    string[] tagString = new string[5];
                    tagString[1] = ((string[])item.Tag)[1];//content.Name;
                    tagString[2] = ((string[])item.Tag)[2];//content.Parent;
                    tagString[3] = ((string[])item.Tag)[3];//content.Type;
                    tagString[4] = ((string[])item.Tag)[4];//content.DisplayName;

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
            string DummyListStr = PostWebRequest("https://api.brightsu.cn/snqx/gfddd/", $"request=dummy", Encoding.UTF8);
            DummyListRoot rt = JsonConvert.DeserializeObject<DummyListRoot>(DummyListStr);
            if (File.Exists($"{AppDomain.CurrentDomain.BaseDirectory}dummy_list.json"))
            {
                string str = File.ReadAllText($"{AppDomain.CurrentDomain.BaseDirectory}dummy_list.json");
                RootObject rb = JsonConvert.DeserializeObject<RootObject>(str);
                if (rt.data.dummy_list_version != rb.meta.version)
                {
                    MessageBoxResult updateListResult = MessageBox.Show($"战术人形数据表有新版本可用。\n本地版本：{rb.meta.version}\n最新版本：{rt.data.dummy_list_version}\n\n是否立即下载数据表？", "更新数据表", MessageBoxButton.YesNo, MessageBoxImage.Question, MessageBoxResult.Yes);
                    if (updateListResult == MessageBoxResult.Yes)
                    {
                        DownloadFile(rt.data.dummy_list_link, $"{AppDomain.CurrentDomain.BaseDirectory}dummy_list.json");
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
                            bool downloaded = DownloadFile(rt.data.dummy_list_link, $"{AppDomain.CurrentDomain.BaseDirectory}dummy_list.json");
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
            TreeViewItem item = (TreeViewItem)tv_InternalSelector.SelectedItem;
            string[] tagString = new string[5];
            tagString[0] = ((string[])item.Tag)[0];//DummyExistence
            tagString[1] = ((string[])item.Tag)[1];//content.Name;
            tagString[2] = ((string[])item.Tag)[2];//content.Parent;
            tagString[3] = ((string[])item.Tag)[3];//content.Type;
            tagString[4] = ((string[])item.Tag)[4];//content.DisplayName;

            string CGStr = PostWebRequest($"https://api.brightsu.cn/snqx/gfddd/", $"request=cg&dummy={tagString[1].ToLowerInvariant()}", Encoding.UTF8);
            CGRoot rt = JsonConvert.DeserializeObject<CGRoot>(CGStr);
            string cg_url = rt.data.cg_url;
            if (chb_alt_cg.IsChecked.Value) { cg_url = rt.data.cg_url_alt; }
            if (rt.data.cg_exist)
            {
                new WindowCG().LoadCG(item.ToolTip.ToString(), cg_url, rt.data.cg_filename, rt.data.cg_d_exist, rt.data.cg_d_filename);
            }
            else
            {
                MessageBox.Show($"{item.ToolTip} ({tagString[1].ToLowerInvariant()}) 没有对应的立绘数据。", "立绘加载失败", MessageBoxButton.OK, MessageBoxImage.Exclamation);
            }
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

        private string PostWebRequest(string postUrl, string paramData, Encoding dataEncode)
        {
            string ret = string.Empty;
            try
            {
                byte[] byteArray = dataEncode.GetBytes(paramData); //转化
                HttpWebRequest webReq = (HttpWebRequest)WebRequest.Create(new Uri(postUrl));
                webReq.Method = "POST";
                webReq.ContentType = "application/x-www-form-urlencoded";

                webReq.ContentLength = byteArray.Length;
                Stream newStream = webReq.GetRequestStream();
                newStream.Write(byteArray, 0, byteArray.Length);//写入参数
                newStream.Close();
                HttpWebResponse response = (HttpWebResponse)webReq.GetResponse();
                StreamReader sr = new StreamReader(response.GetResponseStream(), Encoding.Default);
                ret = sr.ReadToEnd();
                sr.Close();
                response.Close();
                newStream.Close();
            }
            catch (Exception ex)
            {
                return $"ERROR: {ex.Message}";
            }
            return ret;
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

        ///<summary>
        /// 下载文件
        /// </summary>
        /// <param name="URL">下载文件地址</param>
        /// <param name="Filename">下载后另存为（全路径）</param>
        private bool DownloadFile(string URL, string filename)
        {
            try
            {
                HttpWebRequest Myrq = (System.Net.HttpWebRequest)System.Net.HttpWebRequest.Create(URL);
                HttpWebResponse myrp = (System.Net.HttpWebResponse)Myrq.GetResponse();
                Stream st = myrp.GetResponseStream();
                Stream so = new System.IO.FileStream(filename, System.IO.FileMode.Create);
                byte[] by = new byte[1024];
                int osize = st.Read(by, 0, (int)by.Length);
                while (osize > 0)
                {
                    so.Write(by, 0, osize);
                    osize = st.Read(by, 0, (int)by.Length);
                }
                so.Close();
                st.Close();
                myrp.Close();
                Myrq.Abort();
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }


        private void btn_loadData_Click(object sender, RoutedEventArgs e)
        {
            TreeViewItem item = (TreeViewItem)tv_InternalSelector.SelectedItem;
            string[] tagString = new string[5];
            tagString[0] = ((string[])item.Tag)[0];//DummyExistence
            tagString[1] = ((string[])item.Tag)[1];//content.Name;
            tagString[2] = ((string[])item.Tag)[2];//content.Parent;
            tagString[3] = ((string[])item.Tag)[3];//content.Type;
            tagString[4] = ((string[])item.Tag)[4];//content.DisplayName;

            LoadInternalSpine(tagString[1].ToLowerInvariant(), item.ToolTip.ToString());
        }


        private void LoadInternalSpine(string DollName, string DollDisplayName)
        {
            double CanvasWidth = nud_canvas_x.Value;
            double CanvasHeight = nud_canvas_y.Value;
            App.canvasWidth = CanvasWidth;
            App.canvasHeight = CanvasHeight;
            App.globalValues.FrameWidth = CanvasWidth;
            App.globalValues.FrameHeight = CanvasHeight;
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

            App.globalValues.PosX = 224;
            App.globalValues.PosY = 224;
            App.globalValues.SelectSkin = "default";
            App.globalValues.SelectAnimeName = "wait";
        }

        private void btn_downloadData_Click(object sender, RoutedEventArgs e)
        {
            bool wget = true;
            TreeViewItem item = (TreeViewItem)tv_InternalSelector.SelectedItem;
            string[] tagString = new string[5];
            tagString[0] = ((string[])item.Tag)[0];//DummyExistence
            tagString[1] = ((string[])item.Tag)[1];//content.Name;
            tagString[2] = ((string[])item.Tag)[2];//content.Parent;
            tagString[3] = ((string[])item.Tag)[3];//content.Type;
            tagString[4] = ((string[])item.Tag)[4];//content.DisplayName;
            if (!File.Exists($"{AppDomain.CurrentDomain.BaseDirectory}wget.exe"))
            {
                MessageBoxResult wgetErr = MessageBox.Show($"未找到下载组件(wget.exe)。{Environment.NewLine}单击『是』下载下载组件；{Environment.NewLine}单击『否』使用内置下载模块下载；\n单击『取消』取消下载。", "警告", MessageBoxButton.YesNoCancel, MessageBoxImage.Exclamation, MessageBoxResult.Yes);
                if (wgetErr == MessageBoxResult.Yes)
                {
                    string WgetStr = PostWebRequest($"https://api.brightsu.cn/snqx/gfddd/", $"request=wget", Encoding.UTF8);
                    WgetRoot rt = JsonConvert.DeserializeObject<WgetRoot>(WgetStr);
                    DownloadFile($"{rt.data.wget_link}", $@"{AppDomain.CurrentDomain.BaseDirectory}wget.exe");
                    btn_downloadData_Click(this, null);
                }
                else if (wgetErr == MessageBoxResult.No)
                {
                    wget = false;
                }
                else
                {
                    return;
                }
            }
            //if (tagString[1] != tagString[2]) //子皮肤，非本体原皮
            //{
            //    if (!File.Exists($@"{AppDomain.CurrentDomain.BaseDirectory}Resources\spine\{tagString[2]}"))
            //    {
            //        DownloadData(tagString[2], item.ToolTip.ToString().Replace($"{skin_connector}{tagString[4]}", string.Empty), wget);
            //    }
            //}
            DownloadData(tagString[1], item.ToolTip.ToString(), wget);
        }


        private void DownloadData(string dummy, string dummy_display, bool wget)
        {
            TreeViewItem item = (TreeViewItem)tv_InternalSelector.SelectedItem;
            chb_alt_download.IsEnabled = false;
            btn_downloadData.IsEnabled = false;
            string DummyStr = PostWebRequest($"https://api.brightsu.cn/snqx/gfddd/", $"request=dummy&dummy={dummy}", Encoding.UTF8);
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
                                
                                if (wget)
                                {
                                    try
                                    {
                                        Process pro = new Process();
                                        pro.StartInfo.FileName = $"{AppDomain.CurrentDomain.BaseDirectory}wget.exe";
                                        pro.StartInfo.UseShellExecute = false;        //是否使用操作系统shell启动
                                        //pro.StartInfo.RedirectStandardInput = true;   //接受来自调用程序的输入信息
                                        //pro.StartInfo.RedirectStandardOutput = true;  //由调用程序获取输出信息
                                        //pro.StartInfo.RedirectStandardError = true;   //重定向标准错误输出
                                        pro.StartInfo.Arguments = $@"-P .\Resources\spine\{dummy} {downloadSource}/{filename}";   //参数
                                        pro.StartInfo.CreateNoWindow = chb_hide_download.IsChecked.Value;          //不显示程序窗口

                                        pro.Start();
                                        pro.WaitForExit((int)nud_downloadTimeout.Value);
                                        pro.Close();
                                    }
                                    catch (Exception)
                                    {

                                    }
                                }
                                else
                                {
                                    DownloadFile($"{downloadSource}/{filename}", $@"{AppDomain.CurrentDomain.BaseDirectory}Resources\spine\{dummy}\{filename}");
                                }
                                KillEmptyDirectory($@"{AppDomain.CurrentDomain.BaseDirectory}\Resources\spine");
                            }
                            lbl_loader.Content = $"已完成下载 {dummy_display}，等待下一步操作...";

                            pb_loader.IsIndeterminate = true;
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
            if (Directory.Exists($@"{AppDomain.CurrentDomain.BaseDirectory}Resources\spine\{item.Name.ToLowerInvariant()}"))
            {
                btn_deleteData.IsEnabled = true;
                btn_downloadData.IsEnabled = false;
                btn_loadData.IsEnabled = true;
                if (File.Exists($@"{AppDomain.CurrentDomain.BaseDirectory}Resources\spine\{item.Name.ToLowerInvariant()}\r{item.Name.ToLowerInvariant()}.skel"))
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

        private void btn_deleteData_Click(object sender, RoutedEventArgs e)
        {
            TreeViewItem item = (TreeViewItem)tv_InternalSelector.SelectedItem;
            string[] tagString = new string[5];
            tagString[0] = ((string[])item.Tag)[0];//DummyExistence
            tagString[1] = ((string[])item.Tag)[1];//content.Name;
            tagString[2] = ((string[])item.Tag)[2];//content.Parent;
            tagString[3] = ((string[])item.Tag)[3];//content.Type;
            tagString[4] = ((string[])item.Tag)[4];//content.DisplayName;
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
            //MessageBox.Show("此搜索功能仍在编写过程中，暂时不可用。\n此功能就绪后将可以直接用于搜索（筛选）战术人形数据。", "抱歉……", MessageBoxButton.OK, MessageBoxImage.Information);
            foreach (TreeViewItem item in tv_InternalSelector.Items)
            {
                string itemheader = item.Header.ToString().ToLowerInvariant();
                if (itemheader.Length == 0)
                {
                    item.Visibility = Visibility.Visible;
                }
                else
                {
                    if (itemheader.Contains(sb_tvis.Text.ToLowerInvariant()))
                    {
                        item.Visibility = Visibility.Visible;
                        item.IsSelected = true;//设置有关键字的Item被选中
                        item.BringIntoView();
                    }
                    else
                    {
                        item.Visibility = Visibility.Collapsed;
                    }
                }
            }
        }
    }
}
