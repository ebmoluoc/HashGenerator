using CsharpHelpers.Helpers;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Security.Cryptography;

namespace HashGenerator.Services
{

    public class WorkerProgressChangedEventArgs : EventArgs
    {
        public WorkerProgressChangedEventArgs(int progressPercentage)
        {
            ProgressPercentage = progressPercentage;
        }
        public int ProgressPercentage { get; }
    }


    public class WorkerRunCompletedEventArgs : EventArgs
    {
        public WorkerRunCompletedEventArgs(string hashValue)
        {
            HashValue = hashValue;
        }
        public string HashValue { get; }
    }


    public delegate void WorkerProgressChangedEventHandler(object sender, WorkerProgressChangedEventArgs e);
    public delegate void WorkerRunCompletedEventHandler(object sender, WorkerRunCompletedEventArgs e);


    public interface IHasherService
    {
        IEnumerable<string> Hashers { get; }
        string DefaultHasher { get; }
        bool HasherExists(string name);
        void RunWorker(string hashName, string filePath, int progressMax);
        void CancelWorker();
        event WorkerProgressChangedEventHandler WorkerProgressChanged;
        event WorkerRunCompletedEventHandler WorkerRunCompleted;
    }


    public class HasherService : IHasherService, IDisposable
    {

        private readonly Dictionary<string, HashAlgorithm> _hashers;
        private readonly BackgroundWorker _worker;
        private bool _disposed;


        public HasherService()
        {
            _hashers = new Dictionary<string, HashAlgorithm>
            {
                { "SHA-1", new SHA1CryptoServiceProvider() },
                { "SHA-256", new SHA256CryptoServiceProvider() },
                { "SHA-384", new SHA384CryptoServiceProvider() },
                { "SHA-512", new SHA512CryptoServiceProvider() },
                { "MD5", new MD5CryptoServiceProvider() }
            };

            Hashers = _hashers.Keys;
            DefaultHasher = CollectionHelper.GetFirstItem(Hashers);

            _worker = new BackgroundWorker { WorkerReportsProgress = true, WorkerSupportsCancellation = true };
            _worker.DoWork += WorkerDoWork;
            _worker.ProgressChanged += ProgressChanged;
            _worker.RunWorkerCompleted += RunWorkerCompleted;
        }


        ~HasherService()
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
                    _worker.Dispose();

                _disposed = true;
            }
        }


        public IEnumerable<string> Hashers { get; }


        public string DefaultHasher { get; }


        public bool HasherExists(string name)
        {
            return _hashers.ContainsKey(name ?? "");
        }


        public void RunWorker(string hashName, string filePath, int progressMax)
        {
            if (_worker.IsBusy)
                throw new InvalidOperationException("A request to run the worker thread has been made on an already running worker thread.");

            if (!HasherExists(hashName))
                throw new ArgumentException("This hasher does not exist.", nameof(hashName));

            if (!PathHelper.FileExists(filePath))
                throw new ArgumentException("This file does not exist.", nameof(filePath));

            if (progressMax < 1)
                throw new ArgumentException("The progress maximum must be higher than 0.", nameof(progressMax));

            _worker.RunWorkerAsync(new WorkerArgument(_hashers[hashName], filePath, progressMax));
        }


        public void CancelWorker()
        {
            if (_worker.IsBusy)
                _worker.CancelAsync();
        }


        private void WorkerDoWork(object sender, DoWorkEventArgs e)
        {
            const int BufferSize = 16 * 4096;

            var args = (WorkerArgument)e.Argument;

            using (var stream = new FileStream(args.FilePath, FileMode.Open, FileAccess.Read, FileShare.Read, BufferSize))
            {
                var worker = (BackgroundWorker)sender;
                var progress = new WorkerProgress(stream.Length, args.ProgressMax);
                var buffer = new byte[BufferSize];
                var hasher = args.HashAlgo;
                int bytesRead;

                hasher.Initialize();

                while ((bytesRead = stream.Read(buffer, 0, BufferSize)) != 0)
                {
                    hasher.TransformBlock(buffer, 0, bytesRead, null, 0);

                    if (progress.SetValue(bytesRead))
                        worker.ReportProgress(progress.Value);

                    if (worker.CancellationPending)
                    {
                        e.Cancel = true;
                        return;
                    }
                }

                hasher.TransformFinalBlock(buffer, 0, bytesRead);

                e.Result = StringHelper.HexadecimalFromBytes(hasher.Hash, "");
            }
        }


        public event WorkerProgressChangedEventHandler WorkerProgressChanged;
        public event WorkerRunCompletedEventHandler WorkerRunCompleted;


        private void ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            WorkerProgressChanged?.Invoke(this, new WorkerProgressChangedEventArgs(e.ProgressPercentage));
        }


        private void RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (e.Error != null)
                throw new Exception("An exception occurred in the worker thread.", e.Error);

            WorkerRunCompleted?.Invoke(this, new WorkerRunCompletedEventArgs(!e.Cancelled ? (string)e.Result : ""));
        }


        private class WorkerArgument
        {
            public WorkerArgument(HashAlgorithm hashAlgo, string filePath, int progressMax)
            {
                HashAlgo = hashAlgo;
                FilePath = filePath;
                ProgressMax = progressMax;
            }
            public HashAlgorithm HashAlgo { get; }
            public string FilePath { get; }
            public int ProgressMax { get; }
        }


        private class WorkerProgress
        {
            private readonly double _bytesTotal;
            private readonly int _progressMax;
            private long _bytesCumul;

            public WorkerProgress(long bytesTotal, int progressMax)
            {
                _bytesTotal = bytesTotal;

                // +1 because no visible ProgressMax update can be done to show that the computation
                // has reach the end correctly. This way ProgressMax is reached one step faster.
                _progressMax = progressMax + 1;
            }

            public int Value { get; private set; }

            public bool SetValue(int bytesRead)
            {
                _bytesCumul += bytesRead;

                if ((int)(_bytesCumul / _bytesTotal * _progressMax) <= Value)
                    return false;

                // Skip displaying ProgressMax + 1 (see comment above about ProgressMax).
                return ++Value < _progressMax;
            }
        }

    }

}
