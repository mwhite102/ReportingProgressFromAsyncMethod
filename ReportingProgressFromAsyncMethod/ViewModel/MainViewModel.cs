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
        /// Initializes a new instance of the MainViewModel class.
        /// </summary>
        public MainViewModel()
        {
        }

        private bool _IsBusy;

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

        private int _ProgressPercent;

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
            //
        }

        private async void DoStart()
        {
            var progress = new Progress<Progress>(ReportProgress);
            await DoLongRunningProcess(progress);
        }

        private async Task DoLongRunningProcess(IProgress<Progress> progress)
        {
            List<int> values = Enumerable.Range(1, 100).ToList();

            int total = values.Count;

            await Task.Run(() => {
                if (progress != null)
                {
                    int cnt = 0;
                    foreach(var v in values)
                    {
                        cnt++;
                        progress.Report(new Progress()
                        {
                            ProgressPercent = ((cnt * 100 / total)),
                            ProgressText = $"{cnt} of {total}"
                        });
                        Thread.Sleep(100);
                    }
                }
            });
        }

        private void ReportProgress(Progress progress)
        {
            this.ProgressPercent = progress.ProgressPercent;
            this.ProgressText = progress.ProgressText;
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