using Newtonsoft.Json;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Controls;
using static GflChibiDesktop.HttpRequestHelper;
using static GflChibiDesktop.WebAPI;
using static GflChibiDesktop.WebVerification;

namespace GflChibiDesktop
{
    /// <summary>
    /// DownloadSourcesDialog.xaml 的交互逻辑
    /// </summary>
    public partial class DownloadSourcesDialog
    {
        public DownloadSourcesDialog()
        {
            InitializeComponent();
            tb_downloadSource.Text = App.globalValues.DownloadSource;
            btn_UpdateSources_Click(null, null);
        }

        private void canvasBackgroundColorPicker_Canceled(object sender, System.EventArgs e)
        {
            btn_Cancel.Command.Execute(null);
        }

        private void btn_OK_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            App.globalValues.DownloadSource = tb_downloadSource.Text;
            // 同时写回持久化设置，保证立即生效（下载/预览读取当前生效值）
            Properties.Settings.Default.DownloadSource = tb_downloadSource.Text;
            Properties.Settings.Default.Save();
            btn_Cancel.Command.Execute(null);
        }

        private void btn_UpdateSources_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            bool sourcesPost = false;
            string sourcesStr = HttpRequestHelper.PostWebRequest("https://api.brightsu.cn/GflChibiDesktop/sources", string.Empty, Encoding.UTF8, ref sourcesPost);
            if (!sourcesPost)
            {
                HandyControl.Controls.Growl.ErrorGlobal($"获取下载源列表失败。API 接口调用失败。\n错误：{sourcesStr}");
                return;
            }
            SourcesRoot rt = JsonConvert.DeserializeObject<SourcesRoot>(sourcesStr);
            if (rt.ret != 200)
            {
                HandyControl.Controls.Growl.ErrorGlobal($"获取下载源列表失败。API 接口调用失败。\n错误：API 接口返回了 HTTP 状态码 {rt.ret}");
                return;
            }

            lb_sources.Items.Clear();
            foreach (SourcesItem item in rt.data.sources)
            {
                ListBoxItem listBoxItem = new ListBoxItem();
                listBoxItem.Name = $"lbiSource{item.id}";
                listBoxItem.Content = $"[{item.id}] {item.name}";
                listBoxItem.IsEnabled = System.Convert.ToBoolean(item.enabled);
                listBoxItem.ToolTip = item.desc;

                string[] tagString = new string[5];
                tagString[0] = item.id;
                tagString[1] = item.name;
                tagString[2] = item.desc;
                tagString[3] = item.url;
                tagString[4] = item.enabled;
                listBoxItem.Tag = tagString;

                lb_sources.Items.Add(listBoxItem);
            }

            // 默认选中与当前下载源配置一致的项
            string currentSource = tb_downloadSource.Text;
            foreach (object item in lb_sources.Items)
            {
                if (item is ListBoxItem listBoxItem && listBoxItem.Tag is string[] tagString && tagString.Length > 3 && tagString[3] == currentSource)
                {
                    lb_sources.SelectedItem = listBoxItem;
                    break;
                }
            }
        }

        private void lb_sources_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (lb_sources.SelectedItem != null)
            {
                ListBoxItem item = (ListBoxItem)lb_sources.SelectedItem;
                string[] tagString = (string[])item.Tag;
                tb_downloadSource.Text = tagString[3];
                lbl_source.Content = tagString[1];
                tb_sourceInfo.Text = tagString[2];
            }
        }
    }
}
