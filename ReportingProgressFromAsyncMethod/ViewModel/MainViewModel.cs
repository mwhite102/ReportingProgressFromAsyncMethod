using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.CommandWpf;
using System.Windows.Input;
using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace ReportingProgressFromAsyncMethod.ViewModel
{
    public class MainViewModel : ViewModelBase
    {
        /// <summary>
        /// The CancellationTokenSource used to create the cancellation token
        /// </summary>
        CancellationTokenSource _Cts;

        /// <summary>
        /// Initializes a new instance of the MainViewModel class.
        /// </summary>
        public MainViewModel()
        {
        }

        private bool _IsBusy;

        /// <summary>
        /// Gets or sets IsBusy
        /// </summary>
        public bool IsBusy
        {
            get { return _IsBusy; }
            set
            {
                if (_IsBusy != value)
                {
                    _IsBusy = value;
                    RaisePropertyChanged();
                }
            }
        }

        private string _OutputText;

        /// <summary>
        /// Gets or sets the OutputText
        /// </summary>
        public string OutputText
        {
            get { return _OutputText; }
            set
            {
                if (_OutputText != value)
                {
                    _OutputText = value;
                    RaisePropertyChanged();
                }
            }
        }

        private int _ProgressPercent;

        /// <summary>
        /// Gets or sets the ProgressPercent
        /// </summary>
        public int ProgressPercent
        {
            get { return _ProgressPercent; }
            set
            {
                if (_ProgressPercent != value)
                {
                    _ProgressPercent = value;
                    RaisePropertyChanged();
                }
            }
        }

        private string _ProgressText;

        /// <summary>
        /// Gets or sets the ProgressText
        /// </summary>
        public string ProgressText
        {
            get { return _ProgressText; }
            set
            {
                if (_ProgressText != value)
                {
                    _ProgressText = value;
                    RaisePropertyChanged();
                }
            }
        }

        #region Private Methods

        private void DoCancel()
        {
            _Cts.Cancel();
        }

        private async void DoStart()
        {
            // Create the ProgresStatus object
            var progressStatus = new Progress<ProgressStatus>(ReportProgress);
            // Crete a new CancellationTokenSource
            _Cts = new CancellationTokenSource();

            try
            {
                IsBusy = true;
                OutputText = "Working...";
                await DoLongRunningProcess(progressStatus, _Cts.Token);
                OutputText = "Process completed";
            }
            catch (OperationCanceledException ex)
            {
                OutputText = "Process canceled by user";
            }
            finally
            {
                // Reset everything
                IsBusy = false;
                ProgressPercent = 0;
                ProgressText = string.Empty;
                // The buttons don't change state when done unless you do this
                CommandManager.InvalidateRequerySuggested();
            }
        }

        private async Task DoLongRunningProcess(IProgress<ProgressStatus> progress, CancellationToken ct)
        {
            List<int> values = Enumerable.Range(1, 100).ToList();

            int total = values.Count;

            await Task.Run(() =>
            {
                if (progress != null)
                {
                    int count = 0;
                    foreach (var v in values)
                    {
                        count++;

                        // Report the current progress
                        progress.Report(new ProgressStatus()
                        {
                            ProgressPercent = ((count * 100 / total)),
                            ProgressText = $"{count} of {total} ({ProgressPercent} %)"
                        });

                        // Simulate long running process
                        Thread.Sleep(100);
                        // Has cancel been requested?
                        ct.ThrowIfCancellationRequested();
                    }
                }
            });
        }

        private void ReportProgress(ProgressStatus progressStatus)
        {
            this.ProgressPercent = progressStatus.ProgressPercent;
            this.ProgressText = progressStatus.ProgressText;
        }

        #endregion

        #region Commands

        private ICommand _CancelCommand;

        public ICommand CancelCommand
        {
            get
            {
                if (_CancelCommand == null)
                {
                    _CancelCommand = new RelayCommand(CancelCommandExecute, CancelCommandCanExecute);
                }
                return _CancelCommand;
            }
        }

        private bool CancelCommandCanExecute()
        {
            return _IsBusy;
        }

        private void CancelCommandExecute()
        {
            DoCancel();
        }

        private ICommand _ExitCommand;

        public ICommand ExitCommand
        {
            get
            {
                if (_ExitCommand == null)
                {
                    _ExitCommand = new RelayCommand(ExitCommandExecute, ExitCommandCanExecute);
                }
                return _ExitCommand;
            }
        }

        private bool ExitCommandCanExecute()
        {
            return !_IsBusy;
        }

        private void ExitCommandExecute()
        {
            App.Current.Shutdown();
        }

        private ICommand _StartCommand;

        public ICommand StartCommand
        {
            get
            {
                if (_StartCommand == null)
                {
                    _StartCommand = new RelayCommand(StartCommandExecute, StartCommandCanExecute);
                }
                return _StartCommand;
            }
        }

        private bool StartCommandCanExecute()
        {
            return !_IsBusy;
        }

        private void StartCommandExecute()
        {
            DoStart();
        }

        #endregion
    }
}