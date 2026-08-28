using GalaSoft.MvvmLight;
using System.Collections.ObjectModel;
using System.Windows.Media;

namespace GflChibiDesktop.Model
{
    public class DataModel : ViewModelBase
    {
        public int Index { get; set; }

        //public string Name { get; set; }
        private string name;
        public string Name
        {
            get => name;
            set => Set(ref name, value);
        }

        public bool IsSelected { get; set; }

        public ObservableCollection<DataModel> DataList { get; set; }

        public string Header { get; set; }

        public string Content { get; set; }
        public string ToolTip { get; set; }
        public string[] Tag { get; set; }
        public SolidColorBrush Foreground { get; set; }

    }
}
