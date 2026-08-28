using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace GflChibiDesktop
{
    /// <summary>
    /// 本地实现的最小 ViewModelBase，替代 GalaSoft.MvvmLight.ViewModelBase，
    /// 使项目可以脱离 MvvmLight 依赖迁移到 .NET 6。
    /// </summary>
    public abstract class ViewModelBase : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        protected bool Set<T>(ref T field, T value, [CallerMemberName] string propertyName = null)
        {
            if (EqualityComparer<T>.Default.Equals(field, value))
                return false;
            field = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            return true;
        }

        protected void RaisePropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
