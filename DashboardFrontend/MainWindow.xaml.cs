using DashboardFrontend;
using DashboardFrontend.ViewModels;
using DashboardFrontend.DetachedWindows;
using DashboardFrontend.Settings;
using DashboardBackend;
using DashboardBackend.Database;
using Model;
using System;
using System.ComponentModel;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace DashboardFrontend
{
    public partial class MainWindow : Window
    {
        public PerformanceViewModel PerformanceViewModel { get; private set; } = new();
        public LiveChartViewModel LiveChartViewModel { get; private set; }

        public MainWindow()
        {
            LiveChartViewModel = new(PerformanceViewModel.Series, PerformanceViewModel.PerformanceData, PerformanceViewModel.XAxis, PerformanceViewModel.YAxis);

            InitializeComponent();
            TryLoadUserSettings();
            
            ViewModel = new(UserSettings, Log, ValidationReport, LiveChartViewModel);
            DataContext = ViewModel;
        }

        private UserSettings UserSettings { get; } = new();
        public ValidationReport ValidationReport { get; set; } = new();
        public Log Log { get; set; } = new();
        public MainWindowViewModel ViewModel { get; }
        
        private void TryLoadUserSettings()
        {
            try
            {
                UserSettings.LoadFromFile();
            }
            catch (System.IO.FileNotFoundException)
            {
                // Configuration file was not found, possibly first time setup
            }
            catch (System.Text.Json.JsonException ex)
            {
                DisplayGeneralError("Failed to parse contents of UserSettings.json", ex);
            }
            catch (System.IO.IOException ex)
            {
                DisplayGeneralError("An unexpected problem occured while loading user settings", ex);
            }
        }

        private void DisplayGeneralError(string message, Exception ex)
        {
            MessageBox.Show($"{message}\n\nDetails\n{ex.Message}");
        }
        
        public void ButtonStartStopClick(object sender, RoutedEventArgs e)
        {
            ViewModel.OnStartPressed();
        }

        //Detach window events
        public void ButtonSettingsClick(object sender, RoutedEventArgs e)
        {
            SettingsWindow settingsWindow = new(UserSettings);
            //settingsWindow.Closing += OnSettingsWindowClosing;
            //settingsWindow.IsEnabled = false;
            //settingsWindow.Owner = Application.Current.MainWindow;
            settingsWindow.ShowDialog();
        }

        public void DetachManagerButtonClick(object sender, RoutedEventArgs e)
        {
            
            //ManagerWindow detachManager = new();
            //detachManager.Closing += OnManagerWindowClosing;
            //buttonDetachManager.IsEnabled = false;
            //detachManager.Show();
        }
         
        public void DetachLogButtonClick(object sender, RoutedEventArgs e)
        {
            LogDetached detachLog = new(ViewModel.LogViewModel);
            detachLog.Closing += OnLogWindowClosing;
            ButtonLogDetach.IsEnabled = false;
            detachLog.Show();
        }

        public void DetachValidationReportButtonClick(object sender, RoutedEventArgs e)
        {            
            ValidationReportDetached detachVR = new(ValidationReport);
            detachVR.Closing += OnValidationWindowClosing;
            buttonValidationReportDetach.IsEnabled = false;
            detachVR.Show();
        }

        public void DetachHealthReportButtonClick(object sender, RoutedEventArgs e)
        {
            HealthReportDetached expandHr = new();
            

            ButtonHealthReportDetach.IsEnabled = false;
            expandHr.Closing += OnHealthWindowClosing;
            
            expandHr.Show();
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

        private void OnLogWindowClosing(object? sender, CancelEventArgs e)
        {
            ButtonLogDetach.IsEnabled = true;
        }

        private void OnValidationWindowClosing(object? sender, CancelEventArgs e)
        {
            buttonValidationReportDetach.IsEnabled = true;
        }

        private void OnHealthWindowClosing(object? sender, CancelEventArgs e)
        {
            ButtonHealthReportDetach.IsEnabled = true;
        }

        private void MenuItem_SrcSql_Click(object sender, RoutedEventArgs e)
        {
            var menuItem = (MenuItem)sender;
            if (menuItem.DataContext is ValidationTest test)
            {
                Clipboard.SetText(test.SrcSql ?? "");
            }
        }

        private void MenuItem_DstSql_Click(object sender, RoutedEventArgs e)
        {
            var menuItem = (MenuItem)sender;
            if (menuItem.DataContext is ValidationTest test)
            {
                Clipboard.SetText(test.DstSql ?? "");
            }
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
            SystemCommands.MaximizeWindow(this);
            this.ButtonMaximize.Visibility = Visibility.Collapsed;
            this.ButtonRestore.Visibility = Visibility.Visible;
        }

        private void CommandBinding_Executed_3(object sender, ExecutedRoutedEventArgs e)
        {
            SystemCommands.MinimizeWindow(this);
        }

        private void CommandBinding_Executed_4(object sender, ExecutedRoutedEventArgs e)
        {
            SystemCommands.RestoreWindow(this);
            this.ButtonMaximize.Visibility = Visibility.Visible;
            this.ButtonRestore.Visibility = Visibility.Collapsed;
        }

        private void DraggableGrid(object sender, MouseButtonEventArgs e)
        {
            this.DragMove();
        }

        private void CartesianChart_MouseLeave(object sender, MouseEventArgs e)
        {
            LiveChartViewModel.AutoFocusOn();

        }

        private void CartesianChart_MouseEnter(object sender, MouseEventArgs e)
        {
            LiveChartViewModel.AutoFocusOff();
        }

        private void ComboBox_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            _=int.TryParse(((FrameworkElement)comboBoxMaxView.SelectedItem).Tag as string, out int comboBoxItemValue);
            LiveChartViewModel.ChangeMaxView(comboBoxItemValue);
        }
    }
}