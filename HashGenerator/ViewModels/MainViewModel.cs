using CsharpHelpers.Helpers;
using CsharpHelpers.NotifyServices;
using CsharpHelpers.WindowServices;
using HashGenerator.Errors;
using HashGenerator.Services;
using System;
using System.Collections.Generic;
using System.Windows;

namespace HashGenerator.ViewModels
{

    public class MainViewModel : NotifyModel
    {

        private const string _ValidationImageHelp = "/HashGenerator;component/Resources/help.png";
        private const string _ValidationImageCheck = "/HashGenerator;component/Resources/check.png";
        private const string _ValidationImageError = "/HashGenerator;component/Resources/error.png";
        private const string _VisibilityVisible = "Visible";
        private const string _VisibilityHidden = "Hidden";

        private readonly IHasherService _hasherService;
        private readonly IWatcherService _watcherService;
        private readonly IDialogService _dialogService;


        public MainViewModel() : this(new HasherService(), new WatcherService(), new DialogService(), new ArgumentService())
        {
        }


        public MainViewModel(IHasherService hasherService, IWatcherService watcherService, IDialogService dialogService, IArgumentService argumentService)
        {
            BrowseCommand = new DelegateCommand<Window>(_BrowseAction);
            ResetCommand = new DelegateCommand(_ResetAction);
            CancelCommand = new DelegateCommand(_CancelAction);
            ComputeCommand = new DelegateCommand(_ComputeAction);

            _hasherService = hasherService;
            _watcherService = watcherService;
            _dialogService = dialogService;

            _hasherService.WorkerProgressChanged += _WorkerProgressChanged;
            _hasherService.WorkerRunCompleted += _WorkerRunCompleted;

            _watcherService.WatcherFileDeleteEvent += _WatcherFileDeleteEvent;
            _watcherService.WatcherFileRenameEvent += _WatcherFileRenameEvent;

            HashName = argumentService.HashName ?? _hasherService.DefaultHasher;
            FilePath = argumentService.FilePath ?? "";

            if (CanCompute && argumentService.ComputeNow)
                _ComputeAction(null);
        }


        public DelegateCommand<Window> BrowseCommand { get; }
        public DelegateCommand ResetCommand { get; }
        public DelegateCommand CancelCommand { get; }
        public DelegateCommand ComputeCommand { get; }


        public static string WindowTitle
        {
            get { return AppHelper.AssemblyInfo.Title; }
        }


        public IEnumerable<string> Hashers
        {
            get { return _hasherService.Hashers; }
        }


        private string _hashName;
        public string HashName
        {
            get { return _hashName; }
            set
            {
                SetError(new HasherNotExistError(_hasherService, value));
                SetProperty(ref _hashName, value);
                HashValue = "";

                RaisePropertyChanged(nameof(CanCompute));
            }
        }


        private string _filePath;
        public string FilePath
        {
            get { return _filePath; }
            private set
            {
                SetError(new FilePathNotExistError(value));
                SetProperty(ref _filePath, value);
                HashValue = "";

                _watcherService.SetWatcherFile(value);

                RaisePropertyChanged(nameof(CanCompute));
            }
        }


        public bool CanCompute
        {
            get { return !HasErrors; }
        }


        private string _hashValue;
        public string HashValue
        {
            get { return _hashValue; }
            private set
            {
                SetProperty(ref _hashValue, value);
                _SetValidationImage();
            }
        }


        private string _validationValue = "";
        public string ValidationValue
        {
            get { return _validationValue; }
            set
            {
                ExceptionHelper.ThrowIfNull(value);

                SetProperty(ref _validationValue, value);
                _SetValidationImage();
            }
        }


        private string _validationImage;
        public string ValidationImage
        {
            get { return _validationImage; }
            private set { SetProperty(ref _validationImage, value); }
        }


        public int ProgressMaximum { get; } = 20;


        private int _progressValue;
        public int ProgressValue
        {
            get { return _progressValue; }
            private set { SetProperty(ref _progressValue, value); }
        }


        private string _progressVisibility = _VisibilityHidden;
        public string ProgressVisibility
        {
            get { return _progressVisibility; }
            private set { SetProperty(ref _progressVisibility, value); }
        }


        private string _cancelVisibility = _VisibilityHidden;
        public string CancelVisibility
        {
            get { return _cancelVisibility; }
            private set { SetProperty(ref _cancelVisibility, value); }
        }


        private string _computeVisibility = _VisibilityVisible;
        public string ComputeVisibility
        {
            get { return _computeVisibility; }
            private set { SetProperty(ref _computeVisibility, value); }
        }


        private bool _isComputeStandBy = true;
        public bool IsComputeStandBy
        {
            get { return _isComputeStandBy; }
            private set
            {
                SetProperty(ref _isComputeStandBy, value);
                if (value == false)
                {
                    HashValue = "";
                    ProgressValue = 0;
                    ProgressVisibility = _VisibilityVisible;
                    CancelVisibility = _VisibilityVisible;
                    ComputeVisibility = _VisibilityHidden;
                }
                else
                {
                    ProgressVisibility = _VisibilityHidden;
                    CancelVisibility = _VisibilityHidden;
                    ComputeVisibility = _VisibilityVisible;
                }
            }
        }


        private void _BrowseAction(Window window)
        {
            var file = _dialogService.BrowseFile(window, FilePath);
            if (file != null)
                FilePath = file;
        }


        private void _ResetAction(object parameter)
        {
            FilePath = "";
            ValidationValue = "";
            HashName = _hasherService.DefaultHasher;
        }


        private void _CancelAction(object parameter)
        {
            _hasherService.CancelWorker();
        }


        private void _ComputeAction(object parameter)
        {
            IsComputeStandBy = false;
            _hasherService.RunWorker(HashName, FilePath, ProgressMaximum);
        }


        private void _SetValidationImage()
        {
            if (HashValue.Length == 0 || ValidationValue.Length == 0)
            {
                ValidationImage = _ValidationImageHelp;
            }
            else if (string.Equals(HashValue, ValidationValue, StringComparison.OrdinalIgnoreCase))
            {
                ValidationImage = _ValidationImageCheck;
            }
            else
            {
                ValidationImage = _ValidationImageError;
            }
        }


        private void _WorkerProgressChanged(object sender, WorkerProgressChangedEventArgs e)
        {
            ProgressValue = e.ProgressPercentage;
        }


        private void _WorkerRunCompleted(object sender, WorkerRunCompletedEventArgs e)
        {
            HashValue = e.HashValue;
            IsComputeStandBy = true;
        }


        private void _WatcherFileDeleteEvent(object sender, EventArgs e)
        {
            FilePath = "";
        }


        private void _WatcherFileRenameEvent(object sender, WatcherFileEventArgs e)
        {
            _filePath = e.FilePath;
            RaisePropertyChanged(nameof(FilePath));
        }

    }

}
