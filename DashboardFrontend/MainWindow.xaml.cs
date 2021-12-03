using DashboardFrontend.ViewModels;
using DashboardFrontend.DetachedWindows;
using Model;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Threading.Tasks;
using System.Windows.Media;
using DashboardFrontend.Charts;
using LiveChartsCore.SkiaSharpView.WPF;

namespace DashboardFrontend
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            ViewModel = new(ListViewLog);
            DataContext = ViewModel;
        }

        public MainWindowViewModel ViewModel { get; }

        public void ButtonStartStopClick(object sender, RoutedEventArgs e)
        {
            ViewModel.Controller.OnStartPressed();
        }

        //Detach window events
        public void ButtonSettingsClick(object sender, RoutedEventArgs e)
        {
            SettingsWindow settingsWindow = new(ViewModel.Controller);
            settingsWindow.Owner = this;
            settingsWindow.ShowDialog();
        }

        public void DetachManagerButtonClick(object sender, RoutedEventArgs e)
        {
            ManagerViewModel detachedManagerViewModel = ViewModel.Controller.CreateManagerViewModel();
            ManagerListDetached detachManager = new(detachedManagerViewModel);
            detachManager.Show();
            detachManager.Closed += delegate
            {
                // Ensures that the ViewModel is only removed from the controller after its data has been modified, preventing an InvalidOperationException.
                _ = Task.Run(() =>
                {
                    while (ViewModel.Controller.IsUpdatingManagers) { }
                    ViewModel.Controller.ManagerViewModels.Remove(detachedManagerViewModel);
                });
            };
        }

        public void DetachLogButtonClick(object sender, RoutedEventArgs e)
        {
            LogViewModel detachedLogViewModel = ViewModel.Controller.CreateLogViewModel();
            LogDetached detachLog = new(detachedLogViewModel);
            detachLog.Show();
            detachLog.Closed += delegate
            {
                _ = Task.Run(() =>
                {
                    while (ViewModel.Controller.IsUpdatingLog) { }
                    ViewModel.Controller.LogViewModels.Remove(detachedLogViewModel);
                });
            };
        }

        public void DetachValidationReportButtonClick(object sender, RoutedEventArgs e)
        {
            ValidationReportViewModel detachedValidationReportViewModel =
                ViewModel.Controller.CreateValidationReportViewModel();
            ValidationReportDetached detachVr = new(detachedValidationReportViewModel);
            detachVr.Show();
            detachVr.Closed += delegate
            {
                _ = Task.Run(() =>
                {
                    while (ViewModel.Controller.IsUpdatingLog) { }
                    ViewModel.Controller.ValidationReportViewModels.Remove(detachedValidationReportViewModel);
                });
            };
        }

        public void DetachHealthReportButtonClick(object sender, RoutedEventArgs e)
        {
            HealthReportViewModel detachedHealthReportViewModel = ViewModel.Controller.CreateHealthReportViewModel();
            HealthReportDetached detachHr = new(detachedHealthReportViewModel);
            detachHr.Show();
            detachHr.Closed += delegate
            {
                _ = Task.Run(() =>
                {
                    while (ViewModel.Controller.IsUpdatingLog) { }
                    ViewModel.Controller.HealthReportViewModels.Remove(detachedHealthReportViewModel);
                });
            };
        }

        //OnWindowClosing events
        private void OnSettingsWindowClosing(object? sender, CancelEventArgs e)
        {
            ButtonSettings.IsEnabled = true;
        }

        private void OnManagerWindowClosing(object sender, CancelEventArgs e)
        {
            ButtonDetachManager.IsEnabled = true;
        }

        private void ValidationsDataGrid_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            ButtonLogDetach.IsEnabled = true;
        }

        private void OnValidationWindowClosing(object? sender, CancelEventArgs e)
        {
            ButtonValidationReportDetach.IsEnabled = true;
        }

        private void OnHealthWindowClosing(object? sender, CancelEventArgs e)
        {
            ButtonHealthReportDetach.IsEnabled = true;
        }

        private void CommandBinding_CanExecute_1(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
        }

        private void CommandBinding_Executed_1(object sender, ExecutedRoutedEventArgs e)
        {
            SystemCommands.CloseWindow(this);
        }

        private void CommandBinding_Executed_2(object sender, ExecutedRoutedEventArgs e)
        {
            System.Drawing.Rectangle rec = System.Windows.Forms.Screen.FromHandle(new System.Windows.Interop.WindowInteropHelper(this).Handle).WorkingArea;
            MaxHeight = rec.Height;
            MaxWidth = rec.Width;
            ResizeMode = ResizeMode.NoResize;
            WindowState = WindowState.Maximized;
            this.ButtonMaximize.Visibility = Visibility.Collapsed;
            this.ButtonRestore.Visibility = Visibility.Visible;
        }

        private void CommandBinding_Executed_3(object sender, ExecutedRoutedEventArgs e)
        {
            SystemCommands.MinimizeWindow(this);
        }

        private void CommandBinding_Executed_4(object sender, ExecutedRoutedEventArgs e)
        {
            MaxHeight = double.PositiveInfinity;
            MaxWidth = double.PositiveInfinity;
            ResizeMode = ResizeMode.CanResize;
            SystemCommands.RestoreWindow(this);
            this.ButtonMaximize.Visibility = Visibility.Visible;
            this.ButtonRestore.Visibility = Visibility.Collapsed;
        }

        private void DraggableGrid(object sender, MouseButtonEventArgs e)
        {
            this.DragMove();
        }

        //Performance events

        private void CartesianChart_MouseLeave(object sender, MouseEventArgs e)
        {
            DataChart? chart = (DataChart)(sender as CartesianChart)?.DataContext!;
            chart?.AutoFocusOn();
        }

        private void CartesianChart_MouseEnter(object sender, MouseEventArgs e)
        {
            DataChart? chart = (DataChart)(sender as CartesianChart)?.DataContext!;
            chart?.AutoFocusOff();
        }

        private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ViewModel is null) return;
            _ = int.TryParse(((FrameworkElement)ComboBoxMaxView.SelectedItem).Tag as string, out int comboBoxItemValue);
            ViewModel.HealthReportViewModel.SystemLoadChart.ChangeMaxView(comboBoxItemValue);
            ViewModel.HealthReportViewModel.NetworkChart.ChangeMaxView(comboBoxItemValue);
            ViewModel.HealthReportViewModel.NetworkDeltaChart.ChangeMaxView(comboBoxItemValue);
            ViewModel.HealthReportViewModel.NetworkSpeedChart.ChangeMaxView(comboBoxItemValue);
        }

        private void ListViewLog_MouseOverChanged(object sender, MouseEventArgs e)
        {
            ViewModel.LogViewModel.DoAutoScroll = !ViewModel.LogViewModel.DoAutoScroll;
        }

        /// <summary>
        /// Called once a TreeViewItem is expanded. Gets the item's ManagerValidationsWrapper, and adds the manager name to a list of expanded TreeViewItems in the Validation Report viewmodel.
        /// </summary>
        /// <remarks>This ensures that the items stay expanded when the data is updated/refreshed.</remarks>
        private void TreeViewValidations_Expanded(object sender, RoutedEventArgs e)
        {
            TreeView tree = (TreeView)sender;
            TreeViewItem item = (TreeViewItem)e.OriginalSource;
            if (tree.ItemContainerGenerator.ItemFromContainer(item) is ManagerValidationsWrapper wrapper)
            {
                if (!ViewModel.ValidationReportViewModel.ExpandedManagerNames.Contains(wrapper.ManagerName))
                {
                    ViewModel.ValidationReportViewModel.ExpandedManagerNames.Add(wrapper.ManagerName);
                }
            }
        }

        /// <summary>
        /// Called once a TreeViewItem is collapsed. Gets the item's ManagerValidationsWrapper, and removes the manager name to a list of expanded TreeViewItems in the Validation Report viewmodel.
        /// </summary>
        private void TreeViewValidations_Collapsed(object sender, RoutedEventArgs e)
        {
            TreeView tree = (TreeView)sender;
            TreeViewItem item = (TreeViewItem)e.OriginalSource;
            item.IsSelected = false;
            if (tree.ItemContainerGenerator.ItemFromContainer(item) is ManagerValidationsWrapper wrapper)
            {
                if (!ViewModel.ValidationReportViewModel.ExpandedManagerNames.Contains(wrapper.ManagerName))
                {
                    ViewModel.ValidationReportViewModel.ExpandedManagerNames.Remove(wrapper.ManagerName);
                }
            }
        }

        private void CopySrcSql_Click(object sender, RoutedEventArgs e)
        {
            var button = (Button)sender;
            if (button.DataContext is ValidationTest test)
            {
                Clipboard.SetText(test.SrcSql);
            }
        }

        private void CopyDestSql_Click(object sender, RoutedEventArgs e)
        {
            var button = (Button)sender;
            if (button.DataContext is ValidationTest test)
            {
                Clipboard.SetText(test.DstSql);
            }
        }
    }
}