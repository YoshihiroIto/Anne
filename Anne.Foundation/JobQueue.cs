using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Reactive.Bindings;
using StatefulModel;

namespace Anne.Foundation
{
    public class JobQueue : IDisposable
    {
        public ReadOnlyReactiveCollection<string> JobSummries { get; }
        public ReactiveProperty<string> WorkingJob { get; } = new ReactiveProperty<string>();

        private readonly ObservableSynchronizedCollection<Job> _jobs = new ObservableSynchronizedCollection<Job>();

        private readonly Task _task;
        private readonly SemaphoreSlim _sema = new SemaphoreSlim(1);
        private readonly CancellationTokenSource _cancellationToken = new CancellationTokenSource();

        private class Job
        {
            public string Summry { get; set; }
            public Action Action { get; set; }
        }

        public void Dispose()
        {
            try
            {
                _sema.Release();
                _cancellationToken.Cancel();
                _task.Wait();
            }
            catch (AggregateException)
            {
                Debug.WriteLine("Task is cancelled.");
            }

            _cancellationToken.Dispose();
            _sema.Dispose();

            _task.Dispose();

            JobSummries.Dispose();
        }

        public JobQueue()
        {
            JobSummries = _jobs
                .ToReadOnlyReactiveCollection(_jobs.ToCollectionChanged<Job>(), x => x.Summry);

            _task = Task.Run(() =>
            {
                while (true)
                {
                    _cancellationToken.Token.ThrowIfCancellationRequested();
                    _sema.Wait();

                    while (_jobs.Count > 0)
                    {
                        _cancellationToken.Token.ThrowIfCancellationRequested();

                        var job = _jobs[0];
                        _jobs.RemoveAt(0);

                        using (new AnonymousDisposable(() => WorkingJob.Value = string.Empty))
                        {
                            WorkingJob.Value = job.Summry;
                            job.Action();
                        }
                    }
                }

                // ReSharper disable once FunctionNeverReturns
            }, _cancellationToken.Token);
        }

        public void AddJob(string summary, Action action)
        {
            _jobs.Add(new Job
            {
                Summry = summary,
                Action = action
            });

            _sema.Release();
        }
    }
}