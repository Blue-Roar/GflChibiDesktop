using GalaSoft.MvvmLight;
using GflChibiDesktop.Model;
using System.Collections.ObjectModel;

namespace GflChibiDesktop.ViewModel
{
    public class TreeViewViewModel : ViewModelBase
    {
        private ObservableCollection<DataModel> dataList;
        public ObservableCollection<DataModel> DataList
        {
            get => dataList;
            set => Set(ref dataList, value);
        }

        public TreeViewViewModel()
        {
            DataList = GetDataList();
        }

        private ObservableCollection<DataModel> GetDataList()
        {
            return new ObservableCollection<DataModel>
            {
                new DataModel{ Name = "Name1", DataList = new ObservableCollection<DataModel>{ new DataModel { Name = "Name1-1", DataList = null},
                                                                                               new DataModel { Name = "Name1-2", DataList = null},
                                                                                               new DataModel { Name = "Name1-2", DataList = null},
                                                                                               new DataModel { Name = "Name1-2", DataList = null},
                                                                                               new DataModel { Name = "Name1-2", DataList = null},
                                                                                               new DataModel { Name = "Name1-2", DataList = null},} },
                new DataModel{ Name = "Name2", DataList = new ObservableCollection<DataModel>{ new DataModel { Name = "Name2-1", DataList = null},
                                                                                               new DataModel { Name = "Name1-2", DataList = null},
                                                                                               new DataModel { Name = "Name1-2", DataList = null},
                                                                                               new DataModel { Name = "Name1-2", DataList = null},
                                                                                               new DataModel { Name = "Name1-2", DataList = null},
                                                                                               new DataModel { Name = "Name2-2", DataList = null},} },
                new DataModel{ Name = "Name3", DataList = null},
            };
        }
    }
}
