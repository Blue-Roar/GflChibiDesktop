using HandyControl.Data;

namespace GflChibiDesktop
{
    /// <summary>
    /// 全局提示封装，转发到 HandyControl Growl。
    /// </summary>
    public static class GrowlHelper
    {
        public static void InfoGlobal(string message)
        {
            HandyControl.Controls.Growl.InfoGlobal(message);
        }

        public static void InfoGlobal(GrowlInfo info)
        {
            HandyControl.Controls.Growl.InfoGlobal(info);
        }

        public static void WarningGlobal(string message)
        {
            HandyControl.Controls.Growl.WarningGlobal(message);
        }

        public static void WarningGlobal(GrowlInfo info)
        {
            HandyControl.Controls.Growl.WarningGlobal(info);
        }

        public static void ErrorGlobal(string message)
        {
            HandyControl.Controls.Growl.ErrorGlobal(message);
        }

        public static void SuccessGlobal(string message)
        {
            HandyControl.Controls.Growl.SuccessGlobal(message);
        }

        public static void AskGlobal(GrowlInfo info)
        {
            HandyControl.Controls.Growl.AskGlobal(info);
        }
    }
}
