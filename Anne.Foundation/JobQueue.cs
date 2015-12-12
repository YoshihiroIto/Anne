using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace Anne.Foundation
{
    public class JobQueue : IDisposable
    {
        private readonly ConcurrentQueue<Job> _jobs = new ConcurrentQueue<Job>();

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
        }

        public JobQueue()
        {
            _task = Task.Run(() =>
            {
                while (true)
                {
#if !DEBUG
                    _cancellationToken.Token.ThrowIfCancellationRequested();
#endif

                    _sema.Wait();

                    Job job;

                    while (_jobs.TryDequeue(out job))
                    {
                        _cancellationToken.Token.ThrowIfCancellationRequested();

                        Debug.WriteLine($"job:{job.Summry}, rest:{_jobs.Count}");

                        job.Action();
                    }
                }

                // ReSharper disable once FunctionNeverReturns
            }, _cancellationToken.Token);
        }

        public void AddJob(string summary, Action action)
        {
            _jobs.Enqueue(new Job
            {
                Summry = summary,
                Action = action
            });

            _sema.Release();
        }

        public void AddJob(Action job)
        {
            AddJob(string.Empty, job);
        }
    }
}