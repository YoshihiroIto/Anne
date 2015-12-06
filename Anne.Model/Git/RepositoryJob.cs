using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace Anne.Model.Git
{
    public class RepositoryJob : IDisposable
    {
        private readonly ConcurrentQueue<Action> _jobs = new ConcurrentQueue<Action>();

        private readonly Task _task;
        private readonly SemaphoreSlim _sema = new SemaphoreSlim(1);
        private readonly CancellationTokenSource _cancellationToken = new CancellationTokenSource();

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

        public RepositoryJob()
        {
            _task = Task.Run(() =>
            {
                while (true)
                {
                    _cancellationToken.Token.ThrowIfCancellationRequested();

                    _sema.Wait();

                    Action job;

                    while (_jobs.TryDequeue(out job))
                    {
                        _cancellationToken.Token.ThrowIfCancellationRequested();

                        Debug.WriteLine("---------------------------------");
                        Debug.WriteLine($"job executed. : {_jobs.Count}");

                        job();
                    }
                }

                // ReSharper disable once FunctionNeverReturns
            }, _cancellationToken.Token);
        }

        public void AddJob(Action job)
        {
            _jobs.Enqueue(job);
            _sema.Release();
        }
    }
}