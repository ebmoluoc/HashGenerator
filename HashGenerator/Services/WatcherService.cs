using CsharpHelpers.Helpers;
using System;
using System.IO;

namespace HashGenerator.Services
{

    public class WatcherFileEventArgs : EventArgs
    {
        public WatcherFileEventArgs(string filePath)
        {
            FilePath = filePath;
        }
        public string FilePath { get; }
    }


    public delegate void WatcherFileDeleteEventHandler(object sender, EventArgs e);
    public delegate void WatcherFileRenameEventHandler(object sender, WatcherFileEventArgs e);


    public interface IWatcherService
    {
        void SetWatcherFile(string filePath);
        event WatcherFileDeleteEventHandler WatcherFileDeleteEvent;
        event WatcherFileRenameEventHandler WatcherFileRenameEvent;
    }


    public class WatcherService : IWatcherService, IDisposable
    {

        private readonly FileSystemWatcher _watcher;
        private bool _disposed;


        public WatcherService()
        {
            _watcher = new FileSystemWatcher { NotifyFilter = NotifyFilters.FileName };
            _watcher.Deleted += _WatcherFileEvent;
            _watcher.Renamed += _WatcherFileEvent;
        }


        ~WatcherService()
        {
            Dispose(false);
        }


        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }


        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                    _watcher.Dispose();

                _disposed = true;
            }
        }


        public void SetWatcherFile(string filePath)
        {
            var isEnabled = PathHelper.FileExists(filePath);

            if (isEnabled)
            {
                _watcher.Path = Path.GetDirectoryName(filePath);
                _watcher.Filter = Path.GetFileName(filePath);
            }

            _watcher.EnableRaisingEvents = isEnabled;
        }


        public event WatcherFileDeleteEventHandler WatcherFileDeleteEvent;
        public event WatcherFileRenameEventHandler WatcherFileRenameEvent;


        private void _WatcherFileEvent(object sender, FileSystemEventArgs e)
        {
            if (e.ChangeType == WatcherChangeTypes.Deleted)
            {
                _watcher.EnableRaisingEvents = false;
                WatcherFileDeleteEvent?.Invoke(this, EventArgs.Empty);
            }
            else if (e.ChangeType == WatcherChangeTypes.Renamed)
            {
                _watcher.Filter = e.Name;
                WatcherFileRenameEvent?.Invoke(this, new WatcherFileEventArgs(e.FullPath));
            }
        }

    }

}
