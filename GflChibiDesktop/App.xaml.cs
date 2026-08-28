using Microsoft.Xna.Framework.Graphics;
using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Interop;

namespace GflChibiDesktop
{
    /// <summary>
    /// App.xaml 的互动逻辑
    /// </summary>
    public partial class App : Application
    {
        public static GlobalValue globalValues = new GlobalValue();
        public static string rootDir = Environment.CurrentDirectory;
        public static string lastDir;
        public static MonoGameControl appXC;
        public static Texture2D textureBG;

        public static bool isPress = false;
        public static bool isNew = true;
        public static Point mouseLocation;
        public static SpriteBatch spriteBatch;
        public static GraphicsDevice graphicsDevice;
        public static int recordImageCount;
        public static double canvasWidth = SystemParameters.WorkArea.Width;
        public static double canvasHeight = SystemParameters.WorkArea.Height;
        public static double mainWidth;
        public static double mainHeight;
    }

    [ValueConversion(typeof(bool), typeof(bool))]
    public class InverseBooleanConverter : IValueConverter
    {
        #region IValueConverter Members

        public object Convert(object value, Type targetType, object parameter,
             System.Globalization.CultureInfo culture)
        {
            if (targetType != typeof(bool))
                throw new InvalidOperationException("The target must be a boolean");

            return !(bool)value;
        }

        public object ConvertBack(object value, Type targetType, object parameter,
            System.Globalization.CultureInfo culture)
        {
            throw new NotSupportedException();
        }

        #endregion
    }
}
