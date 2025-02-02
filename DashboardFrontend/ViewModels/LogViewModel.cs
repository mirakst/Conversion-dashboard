﻿using System;
using System.Collections.Generic;
using Model;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Data;
using static Model.LogMessage;
using ListView = System.Windows.Controls.ListView;

namespace DashboardFrontend.ViewModels
{
    public class LogViewModel : BaseViewModel
    {
        public LogViewModel()
        {

        }

        public LogViewModel(ListView logListView)
        {
            LogListView = logListView;
        }

        public DateTime LastUpdated { get; set; }
        public bool DoAutoScroll { get; set; } = true;
        public ListView LogListView { get; set; }

        private ObservableCollection<ExecutionObservable> _executions = new();
        public ObservableCollection<ExecutionObservable> Executions
        {
            get => _executions;
            set
            {
                _executions = value;
                OnPropertyChanged(nameof(Executions));
            }
        }
        private ExecutionObservable? _selectedExecution;
        public ExecutionObservable? SelectedExecution
        {
            get => _selectedExecution;
            set
            {
                _selectedExecution = value;
                OnPropertyChanged(nameof(SelectedExecution));
                SetExecution(value);
            }
        }
        private CollectionView _messageView;
        public CollectionView MessageView
        {
            get => _messageView;
            set
            {
                _messageView = value;
                OnPropertyChanged(nameof(MessageView));
            }
        }
        private int _infoCount;
        public int InfoCount
        {
            get => _infoCount;
            set
            {
                _infoCount = value;
                OnPropertyChanged(nameof(InfoCount));
            }
        }
        private int _warnCount;
        public int WarnCount
        {
            get => _warnCount;
            set
            {
                _warnCount = value;
                OnPropertyChanged(nameof(WarnCount));
            }
        }
        private int _errorCount;
        public int ErrorCount
        {
            get => _errorCount;
            set
            {
                _errorCount = value;
                OnPropertyChanged(nameof(ErrorCount));
            }
        }
        private int _fatalCount;
        public int FatalCount
        {
            get => _fatalCount;
            set
            {
                _fatalCount = value;
                OnPropertyChanged(nameof(FatalCount));
            }
        }
        private int _reconciliationCount;
        public int ReconciliationCount
        {
            get => _reconciliationCount;
            set
            {
                _reconciliationCount = value;
                OnPropertyChanged(nameof(ReconciliationCount));
            }
        }
        private bool _showInfo = true;
        public bool ShowInfo
        {
            get => _showInfo;
            set
            {
                _showInfo = value;
                OnPropertyChanged(nameof(ShowInfo));
                MessageView?.Refresh();
                ScrollToLast();
            }
        }
        private bool _showWarn = true;
        public bool ShowWarn
        {
            get => _showWarn;
            set
            {
                _showWarn = value;
                OnPropertyChanged(nameof(ShowWarn));
                MessageView?.Refresh();
                ScrollToLast();
            }
        }
        private bool _showError = true;
        public bool ShowError
        {
            get => _showError;
            set
            {
                _showError = value;
                OnPropertyChanged(nameof(ShowError));
                MessageView?.Refresh();
                ScrollToLast();
            }
        }
        private bool _showFatal = true;
        public bool ShowFatal
        {
            get => _showFatal;
            set
            {
                _showFatal = value;
                OnPropertyChanged(nameof(ShowFatal));
                MessageView?.Refresh();
                ScrollToLast();
            }
        }
        private bool _showReconciliations = true;
        public bool ShowReconciliations
        {
            get => _showReconciliations;
            set
            {
                _showReconciliations = value;
                OnPropertyChanged(nameof(ShowReconciliations));
                MessageView?.Refresh();
                ScrollToLast();
            }
        }

        /// <summary>
        /// Updates the data for each execution in the log, and filters the messages.
        /// </summary>
        /// <param name="executions">A list of executions to add to the log.</param>
        public void UpdateData(List<Execution> executions)
        {
            Executions = new();
            for(int i = 0; i < executions.Count; i++)
            {
                Executions.Add(new ExecutionObservable(executions[i]));
            }

            if (SelectedExecution is null)
            {
                SelectedExecution = Executions.Last();
            }
            UpdateCounters(SelectedExecution);
            MessageView.Filter = OnMessagesFilter;
            ScrollToLast();
        }

        /// <summary>
        /// Used as a filter for the MessageView CollectionView.
        /// </summary>
        /// <param name="item">A LogMessage object.</param>
        /// <returns>True if the object should be shown in the CollectionView, and false otherwise.</returns>
        private bool OnMessagesFilter(object item)
        {
            LogMessageType type = ((LogMessage)item).Type;
            int ContextId = ((LogMessage) item).ContextId;
            if (ContextId > 0 && !SelectedExecution.Managers.Where(m => m.IsChecked).Any(m => m.ContextId == ContextId))
            {
                return false;
            }
            return ((ShowInfo && type.HasFlag(LogMessageType.Info))
                 || (ShowWarn && type.HasFlag(LogMessageType.Warning))
                 || (ShowError && type.HasFlag(LogMessageType.Error))
                 || (ShowFatal && type.HasFlag(LogMessageType.Fatal))
                 || (ShowReconciliations && type.HasFlag(LogMessageType.Reconciliation)));
        }

        /// <summary>
        /// Updates the number of log messages with the different possible types.
        /// </summary>
        /// <param name="exec">The execution to fetch updated counts from.</param>
        private void UpdateCounters(ExecutionObservable exec)
        {
            InfoCount = exec.InfoCount;
            WarnCount = exec.WarnCount;
            ErrorCount = exec.ErrorCount;
            FatalCount = exec.FatalCount;
            ReconciliationCount = exec.ReconciliationCount;
        }

        /// <summary>
        /// Calls the bottom scrolling event handler.
        /// </summary>
        public void ScrollToLast()
        {
            if (DoAutoScroll && LogListView is not null && !LogListView.Items.IsEmpty)
            {
                ScrollToLast(this, new RoutedEventArgs());
            }
        }

        /// <summary>
        /// Scrolls to the bottom of the log.
        /// </summary>
        public void ScrollToLast(object sender, RoutedEventArgs e)
        {
            int itemCount = LogListView.Items.Count;
            if (itemCount > 0)
            {
                LogListView.ScrollIntoView(LogListView.Items[^1]);
            }
        }

        /// <summary>
        /// Sets the execution for the log.
        /// </summary>
        /// <param name="exec">The execution to display data for.</param>
        private void SetExecution(ExecutionObservable exec)
        {
            if (exec is not null)
            {
                MessageView = (CollectionView)CollectionViewSource.GetDefaultView(exec.LogMessages);
                UpdateCounters(exec);
                MessageView.Filter = OnMessagesFilter;
                ScrollToLast();
            }
        }
    }
}
