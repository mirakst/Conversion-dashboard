﻿using DashboardBackend.Database;
using DashboardFrontend.DetachedWindows;
using DashboardFrontend.Settings;
using DashboardFrontend.ViewModels;
using Model;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using DU = DashboardBackend.DataUtilities;

namespace DashboardFrontend
{
    public class Controller
    {
        public Controller(MainWindowViewModel viewModel)
        {
            TryLoadUserSettings();
            _vm = viewModel;
            Conversion = new();
            _timers = new List<Timer>();
        }

        public bool IsUpdatingExecutions { get; private set; }
        public bool IsUpdatingLog { get; private set; }
        public bool IsUpdatingValidations { get; private set; }
        public bool IsUpdatingManagers { get; private set; }
        public bool IsUpdatingHealthReport { get; private set; }

        private readonly MainWindowViewModel _vm;
        private readonly List<Timer> _timers;
        public Conversion Conversion { get; set; }
        public readonly List<HealthReportViewModel> HealthReportViewModels = new();
        public readonly List<LogViewModel> LogViewModels = new();
        public readonly List<ValidationReportViewModel> ValidationReportViewModels = new();
        public readonly List<ManagerViewModel> ManagerViewModels = new();
        public UserSettings UserSettings { get; set; } = new();

        /// <summary>
        /// Initializes the view models in the <see cref="Controller"/>.
        /// </summary>
        public void InitializeViewModels(ListView listViewLog)
        {
            _vm.LogViewModel = new LogViewModel(listViewLog);
            LogViewModels.Add(_vm.LogViewModel);

            _vm.ValidationReportViewModel = new ValidationReportViewModel();
            ValidationReportViewModels.Add(_vm.ValidationReportViewModel);

            _vm.HealthReportViewModel = new HealthReportViewModel();
            HealthReportViewModels.Add(_vm.HealthReportViewModel);

            _vm.ManagerViewModel = new ManagerViewModel(Conversion.HealthReport);
            ManagerViewModels.Add(_vm.ManagerViewModel);
        }

        public LogViewModel CreateLogViewModel()
        {
            LogViewModel result = new();
            result.UpdateData(Conversion.ActiveExecution.Log);
            LogViewModels.Add(result);
            return result;
        }

        public ValidationReportViewModel CreateValidationReportViewModel()
        {
            ValidationReportViewModel result = new();
            if (Conversion.ActiveExecution != null)
            {
                result.UpdateData(Conversion.ActiveExecution.ValidationReport);
            }
            ValidationReportViewModels.Add(result);
            return result;
        }

        public HealthReportViewModel CreateHealthReportViewModel()
        {
            HealthReportViewModel result = new();
            result.SystemLoadChart.UpdateData(Conversion.HealthReport.Ram, Conversion.HealthReport.Cpu);
            result.NetworkChart.UpdateData(Conversion.HealthReport.Network);
            result.NetworkDeltaChart.UpdateData(Conversion.HealthReport.Network);
            result.NetworkSpeedChart.UpdateData(Conversion.HealthReport.Network);
            HealthReportViewModels.Add(result);
            return result;
        }

        public ManagerViewModel CreateManagerViewModel()
        {
            ManagerViewModel result = new(Conversion.HealthReport);
            result.UpdateData(Conversion.ActiveExecution.Managers);
            ManagerViewModels.Add(result);
            return result;
        }

        /// <summary>
        /// Updates the messages in the log.
        /// </summary>
        public void UpdateLog()
        {
            if (IsUpdatingLog || Conversion.ActiveExecution?.Log is null)
            {
                return;
            }
            IsUpdatingLog = true;

            List<LogMessage> newData = DU.GetLogMessages(Conversion.LastLogUpdate);
            Conversion.LastLogUpdate = DateTime.Now;

            if (newData.Count > 0)
            {
                newData.ForEach(m => ParseLogMessage(m));

                foreach (LogViewModel vm in LogViewModels)
                {
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        vm.UpdateData(Conversion.ActiveExecution.Log);
                    });
                }
            }
            IsUpdatingLog = false;
        }

        /// <summary>
        /// Gets (or creates, if it does not yet exist) the execution associated with the specified log message.
        /// If the message contains "Starting manager: [NAME]" or "Manager execution done." the method will try to assign the context ID to the appropriate manager and set its status.
        /// </summary>
        /// <param name="message">The log message to parse.</param>
        private void ParseLogMessage(LogMessage message)
        {
            Execution? exec = Conversion.Executions.Find(e => e.Id == message.ExecutionId);
            if (exec is null)
            {
                // The first log message of an execution was logged before the execution was created
                exec = new Execution(message.ExecutionId, message.Date);
                Conversion.Executions.Add(exec);
            }
            exec.Log.Messages.Add(message);

            if (message.Content.StartsWith("Starting manager:"))
            {
                Match match = Regex.Match(message.Content, @"^Starting manager: (?<Name>[\w.]*)");
                if (match.Success)
                {
                    string name = match.Groups["Name"].Value;
                    // If the manager has already been set up, simply change its status
                    if (Conversion.AllManagers.Find(m => m.Name == name && m.ContextId == 0) is Manager mgr)
                    {
                        if (mgr.Status != Manager.ManagerStatus.Ok)
                        {
                            mgr.Status = Manager.ManagerStatus.Running;
                        }
                        mgr.ContextId = message.ContextId;
                        if (!exec.Managers.Contains(mgr))
                        {
                            exec.Managers.Add(mgr);
                        }
                    }
                    // Otherwise, create the manager and set its name, context ID and status
                    else
                    {
                        Manager manager = new()
                        {
                            Name = name,
                            ContextId = message.ContextId,
                            Status = Manager.ManagerStatus.Running
                        };
                        exec.Managers.Add(manager);
                        Conversion.AllManagers.Add(manager);
                    }
                }
            }
            // Check if a manager has finished its execution
            else if (message.Content == "Manager execution done.")
            {
                if (exec.Managers.Find(m => m.ContextId == message.ContextId) is Manager mgr)
                {
                    mgr.Status = Manager.ManagerStatus.Ok;
                    if (!mgr.EndTime.HasValue)
                    {
                        mgr.EndTime = message.Date;
                    }
                }
            }
        }

        /// <summary>
        /// Updates the validation tests in the validation report.
        /// </summary>
        public void UpdateValidationReport()
        {
            if(IsUpdatingValidations || Conversion.ActiveExecution?.ValidationReport is null)
            {
                return;
            }
            IsUpdatingValidations = true;
            List<ValidationTest> newData = DU.GetAfstemninger(Conversion.ActiveExecution.ValidationReport.LastModified);
            Conversion.ActiveExecution.ValidationReport.LastModified = DateTime.Now;
            if (newData.Count > 0)
            {
                Conversion.ActiveExecution.ValidationReport.ValidationTests = newData;
                foreach (var vm in ValidationReportViewModels)
                {
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        vm.UpdateData(Conversion.ActiveExecution.ValidationReport);
                    });
                }
            }
            IsUpdatingValidations = false;
        }

        /// <summary>
        /// Updates the readings in the health report.
        /// </summary>
        public void UpdateHealthReport()
        {
            if (IsUpdatingHealthReport || Conversion.HealthReport is null)
            {
                return;
            }
            IsUpdatingHealthReport = true;
            if (Conversion.HealthReport.IsInitialized)
            {
                DU.AddHealthReportReadings(Conversion.HealthReport, Conversion.HealthReport.LastModified);
                foreach (var vm in HealthReportViewModels)
                {
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        vm.SystemLoadChart.UpdateData(Conversion.HealthReport.Ram, Conversion.HealthReport.Cpu);
                        vm.NetworkChart.UpdateData(Conversion.HealthReport.Network);
                        vm.NetworkDeltaChart.UpdateData(Conversion.HealthReport.Network);
                        vm.NetworkSpeedChart.UpdateData(Conversion.HealthReport.Network);
                    });
                }
            }
            else
            {
                DU.BuildHealthReport(Conversion.HealthReport);
            }
            IsUpdatingHealthReport = false;
        }

        /// <summary>
        /// Updates the list of managers in the current Conversion and adds them to their associated executions.
        /// </summary>
        public void UpdateManagerOverview()
        {
            if (IsUpdatingManagers || Conversion.ActiveExecution is null)
            {
                return;
            }
            IsUpdatingManagers = true;
            
            DU.GetAndUpdateManagers(Conversion.LastManagerUpdate, Conversion.AllManagers);
            Conversion.LastManagerUpdate = DateTime.Now;

            // Check for any manager values that can be updated
            if (!IsUpdatingLog)
            {
                Conversion.AllManagers.ForEach(m =>
                {
                    if (!m.Runtime.HasValue && m.StartTime.HasValue && m.EndTime.HasValue)
                    {
                        m.Runtime = m.EndTime.Value.Subtract(m.StartTime.Value);
                    }
                });
            }

            // Check for CPU or RAM readings that can be associated with the manager
            foreach (Manager manager in Conversion.AllManagers)
            {
                List<CpuLoad> cpuReadings = Conversion.HealthReport.Cpu.Readings.Where(r => r.Date >= manager.StartTime && r.Date <= manager.EndTime).ToList();
                List<RamLoad> ramReadings = Conversion.HealthReport.Ram.Readings.Where(r => r.Date >= manager.StartTime && r.Date <= manager.EndTime).ToList();
            }

            foreach (var vm in ManagerViewModels)
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    vm.UpdateData(Conversion.ActiveExecution.Managers);
                });
            }
            IsUpdatingManagers = false;
        }

        /// <summary>
        /// Ensures that there is an active profile with database credentials, and then starts the monitoring process.
        /// If the process is already running, it is ended.
        /// </summary>
        public void OnStartPressed()
        {
            if (UserSettings.ActiveProfile is null)
            {
                MessageBox.Show("Please select a profile.");
            }
            else if (!_vm.IsRunning)
            {
                if (!UserSettings.ActiveProfile.HasReceivedCredentials)
                {
                    GetCredentials();
                }
                if (UserSettings.ActiveProfile.HasReceivedCredentials)
                {
                    DU.DatabaseHandler = new SqlDatabase(UserSettings.ActiveProfile.ConnectionString);
                    StartMonitoring();
                }
            }
            else
            {
                StopMonitoring();
            }
        }

        /// <summary>
        /// Gets database credentials for the current profile and builds its connection string.
        /// </summary>
        private void GetCredentials()
        {
            ConnectDBDialog dialogPopup = new(UserSettings);
            dialogPopup.ShowDialog();
        }

        private void UpdateExecutions()
        {
            if (IsUpdatingExecutions)
            {
                return;
            }
            IsUpdatingExecutions = true;
            List<Execution> result = DU.GetExecutions(Conversion.LastExecutionUpdate);
            Conversion.LastExecutionUpdate = DateTime.Now;
            if (result.Count > 0)
            {
                foreach(Execution exec in result)
                {
                    if (!Conversion.Executions.Any(e => e.Id == exec.Id))
                    {
                        Conversion.Executions.Add(exec);
                    }
                    else
                    {

                    }
                }
            }
            IsUpdatingExecutions = false;
        }

        /// <summary>
        /// Sets up the necessary query timers that gather and update information in the system.
        /// </summary>
        private void StartMonitoring()
        {
            _timers.Add(new Timer(x => { UpdateExecutions(); }, null, 0, 5000));
            _timers.Add(new Timer(x => { UpdateHealthReport(); }, null, 500, 5000));

            if (UserSettings.SynchronizeAllQueries)
            {
                _timers.Add(new Timer(x =>
                {
                    Task.Run(() => UpdateHealthReport());
                    Task.Run(() => UpdateLog());
                    Task.Run(() => UpdateValidationReport());
                    Task.Run(() => UpdateManagerOverview());
                }, null, 1000, UserSettings.AllQueryInterval * 1000));
            }
            else
            {
                _timers.Add(new Timer(x => UpdateHealthReport(), null, 1000, UserSettings.HealthReportQueryInterval * 1000));
                _timers.Add(new Timer(x => UpdateValidationReport(), null, 1000, UserSettings.ValidationQueryInterval * 1000));
                _timers.Add(new Timer(x => UpdateManagerOverview(), null, 1000, UserSettings.ManagerQueryInterval * 1000));
                _timers.Add(new Timer(x => UpdateLog(), null, 5000, UserSettings.LoggingQueryInterval * 1000));
            }
            _vm.IsRunning = true;
        }

        /// <summary>
        /// Stops the periodic monitoring functions.
        /// </summary>
        private void StopMonitoring()
        {
            foreach (Timer timer in _timers)
            {
                timer.Dispose();
            }
            _timers.Clear();
            _vm.IsRunning = false;
        }

        /// <summary>
        /// Attempts to load user settings, catching potential exceptions if loading fails and displaying an error.
        /// </summary>
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
                DisplayGeneralError("An unexpected problem occurred while loading user settings", ex);
            }
        }
        private static void DisplayGeneralError(string message, Exception ex)
        {
            MessageBox.Show($"{message}\n\nDetails\n{ex.Message}");
        }
    }
}