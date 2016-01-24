﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Concurrency;
using System.Threading;
using System.Threading.Tasks;
using Reactive.Bindings;
using StatefulModel;

namespace Anne.Foundation
{
    public class JobQueue : IDisposable
    {
        public event EventHandler<ExceptionEventArgs> JobExecutingException;

        //public ReadOnlyReactiveCollection<string> JobSummaries { get; }
        public ReactiveCollection<string> JobSummaries { get; } = new ReactiveCollection<string>();
        public ReactiveProperty<string> WorkingJob { get; } =
            new ReactiveProperty<string>(Scheduler.Immediate);

        private class Job
        {
            public string Summary { get; set; }
            public Action Action { get; set; }
        }

        private readonly Queue<Job> _jobs = new Queue<Job>();
        private volatile bool _isActive;

        private readonly object _syncObj = new object();

        public void AddJob(string summary, Action action)
        {
            lock (_syncObj)
            {
                _jobs.Enqueue(new Job {Summary = summary, Action = action});

                if (_isActive)
                    return;

                _isActive = true;
                RunJob();
            }
        }

        private void RunJob()
        {
            Job job;

            lock(_syncObj)
            {
                if (_jobs.Any() == false || _disposeResetEvent != null)
                {
                    _isActive = false;
                    _disposeResetEvent?.Set();
                    return;
                }

                job = _jobs.Dequeue();
            }

            Task.Run(() =>
            {
                using (new AnonymousDisposable(() => WorkingJob.Value = string.Empty))
                {
                    WorkingJob.Value = job.Summary;

                    try
                    {
                        job.Action();
                    }
                    catch (Exception e)
                    {
                        var args = new ExceptionEventArgs {Exception = e, Summary = job.Summary};

                        JobExecutingException?.Invoke(this, args);

                        // 例外が起きたので以降のジョブは実行しない
                        lock (_syncObj)
                        {
                            _jobs.Clear();
                        }
                    }
                }
            }).ContinueWith(_ => RunJob());
        }

        private ManualResetEventSlim _disposeResetEvent;

        public void Dispose()
        {
            lock (_syncObj)
            {
                if (_isActive)
                    _disposeResetEvent = new ManualResetEventSlim();
            }

            _disposeResetEvent?.Wait();
            _disposeResetEvent?.Dispose();

            WorkingJob.Dispose();
            JobSummaries.Dispose();
        }
    }
}