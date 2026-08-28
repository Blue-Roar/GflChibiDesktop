using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;

namespace GflChibiDesktop
{
    /// <summary>
    /// OptionsWindow.xaml 的交互逻辑
    /// </summary>
    public partial class OptionsWindow : Window
    {

        private MainWindow _window;

        public OptionsWindow(MainWindow main)
        {
            InitializeComponent();
            _window = main;
            Visibility = Visibility.Collapsed;
        }

        /*
        private void cb_SkinList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cb_SkinList.SelectedIndex != -1)
            {
                if (cb_SkinList.SelectedItem.ToString() != string.Empty)
                {
                    App.globalValues.SelectSkin = cb_SkinList.SelectedItem.ToString();
                    App.globalValues.SetSkin = true;
                }
            }
        }
        */

        private void chb_IsLoop_Click(object sender, RoutedEventArgs e)
        {
            App.globalValues.SetAnime = true;
        }
        //private void chb_PreMultiplyAlpha_Click(object sender, RoutedEventArgs e)
        //{
        //    _window.UpdateSpine();
        //}

        private void cb_AnimeList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cb_AnimeList.SelectedIndex != -1)
            {
                if (cb_AnimeList.SelectedItem.ToString() != string.Empty)
                {
                    App.globalValues.SelectAnimeName = cb_AnimeList.SelectedItem.ToString();
                    App.globalValues.SetAnime = true;
                }
            }
        }

        public void LoadSetting()
        {
            lbl_Dummy.SetBinding(ContentProperty, new Binding() { Source = App.globalValues, Path = new PropertyPath("DummyDisplayName"), Mode = BindingMode.OneWay });
            //tb_Fps.SetBinding(TextBox.TextProperty, new Binding() { Source = App.globalValues, Path = new PropertyPath("Speed") });
            sld_Fps.SetBinding(System.Windows.Controls.Primitives.RangeBase.ValueProperty, new Binding() { Source = App.globalValues, Path = new PropertyPath("Speed") });
            //tb_Spine_Scale.SetBinding(TextBox.TextProperty, new Binding() { Source = App.globalValues, Path = new PropertyPath("Scale") });
            sld_Spine_Scale.SetBinding(System.Windows.Controls.Primitives.RangeBase.ValueProperty, new Binding() { Source = App.globalValues, Path = new PropertyPath("Scale") });
            //lb_ViewScale.SetBinding(ContentProperty, new Binding() { Source = App.globalValues, Path = new PropertyPath("ViewScale") });
            //lb_ViewScale.ContentStringFormat = $"ViewScale：{0 * 100}%";
            //tb_PosX.SetBinding(TextBox.TextProperty, new Binding() { Source = App.globalValues, Path = new PropertyPath("PosX") });
            tb_PosX.SetBinding(HandyControl.Controls.NumericUpDown.ValueProperty, new Binding() { Source = App.globalValues, Path = new PropertyPath("PosX") });
            tb_PosX.SetBinding(HandyControl.Controls.NumericUpDown.MaximumProperty, new Binding() { Source = App.globalValues, Path = new PropertyPath("FrameWidth"), Mode = BindingMode.OneWay });
            //tb_PosY.SetBinding(TextBox.TextProperty, new Binding() { Source = App.globalValues, Path = new PropertyPath("PosY") });
            tb_PosY.SetBinding(HandyControl.Controls.NumericUpDown.ValueProperty, new Binding() { Source = App.globalValues, Path = new PropertyPath("PosY") });
            tb_PosY.SetBinding(HandyControl.Controls.NumericUpDown.MaximumProperty, new Binding() { Source = App.globalValues, Path = new PropertyPath("FrameHeight"), Mode = BindingMode.OneWay });

            //tb_Rotation.SetBinding(TextBox.TextProperty, new Binding() { Source = App.globalValues, Path = new PropertyPath("Rotation") });
            sld_Rotation.SetBinding(System.Windows.Controls.Primitives.RangeBase.ValueProperty, new Binding() { Source = App.globalValues, Path = new PropertyPath("Rotation") });

            chb_Simulate.SetBinding(System.Windows.Controls.Primitives.ToggleButton.IsCheckedProperty, new Binding() { Source = App.globalValues, Path = new PropertyPath("Simulation") });
            //chb_Alpha.SetBinding(CheckBox.IsCheckedProperty, new Binding() { Source = App.globalValues, Path = new PropertyPath("Alpha") });
            App.globalValues.Alpha = true;
            App.globalValues.PreMultiplyAlpha = true;
            chb_IsLoop.SetBinding(System.Windows.Controls.Primitives.ToggleButton.IsCheckedProperty, new Binding() { Source = App.globalValues, Path = new PropertyPath("IsLoop") });
            //chb_PreMultiplyAlpha.SetBinding(CheckBox.IsCheckedProperty, new Binding() { Source = App.globalValues, Path = new PropertyPath("PreMultiplyAlpha") });
            App.globalValues.Opacity = 1;
            sld_Opacity.SetBinding(System.Windows.Controls.Primitives.RangeBase.ValueProperty, new Binding() { Source = App.globalValues, Path = new PropertyPath("Opacity") });
            chb_FilpX.SetBinding(System.Windows.Controls.Primitives.ToggleButton.IsCheckedProperty, new Binding() { Source = App.globalValues, Path = new PropertyPath("FilpX") });
            chb_FilpY.SetBinding(System.Windows.Controls.Primitives.ToggleButton.IsCheckedProperty, new Binding() { Source = App.globalValues, Path = new PropertyPath("FilpY") });

            cb_AnimeList.SetBinding(System.Windows.Controls.Primitives.Selector.SelectedValueProperty, new Binding() { Source = App.globalValues, Path = new PropertyPath("SelectAnimeName"), Mode = BindingMode.OneWay});

            //TextCompositionManager.AddPreviewTextInputStartHandler(sld_Fps, textBox_PreviewTextInput);

            //sl_Loading.SetBinding(Slider.ValueProperty, new Binding() { Source = App.globalValues, Path = new PropertyPath("Lock") });
            //lb_Loading.SetBinding(ContentProperty, new Binding() { Source = App.globalValues, Path = new PropertyPath("LoadingProcess") });
            //GridAttributes.ColumnDefinitions[0].Width = new GridLength(34);
        }

        private void textBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            Regex regExp = new Regex(@"\d");
            string singleValue = e.Text;
            e.Handled = !regExp.Match(singleValue).Success;
        }
        
        private void Border_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                this.DragMove();
            }
        }

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            e.Cancel = true;
            Visibility = Visibility.Collapsed;
        }

        private void chb_TopMost_Click(object sender, RoutedEventArgs e)
        {
            _window.Topmost = chb_TopMost.IsChecked.Value;
        }

        private void chb_Simulate_Click(object sender, RoutedEventArgs e)
        {
            _window.toggleSimulation(chb_Simulate.IsChecked.Value);
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            Visibility = Visibility.Collapsed;

            chb_TopMost.IsChecked = Properties.Settings.Default.Topmost;
            sld_Opacity.Value = Properties.Settings.Default.DummyOpacity;
            chb_Simulate.IsChecked = Properties.Settings.Default.DummySimulation;
        }

        private void btnLoad_Click(object sender, RoutedEventArgs e)
        {
            _window.LoadDummy();
        }

        private void Window_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            Close();
        }

        private void sld_Simulation_Interval_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (sld_Simulation_Interval.IsEnabled && chb_Simulate.IsChecked.Value)
            {
                _window.setSimulationInterval(sld_Simulation_Interval.Value);
            }
        }

    }
}
