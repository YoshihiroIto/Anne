using System;
using System.Diagnostics;
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

        //public ReadOnlyReactiveCollection<string> JobSummries { get; }
        public ReactiveCollection<string> JobSummries { get; } = new ReactiveCollection<string>();
        public ReactiveProperty<string> WorkingJob { get; } =
            new ReactiveProperty<string>(Scheduler.Immediate);

        private readonly ObservableSynchronizedCollection<Job> _jobs =
            new ObservableSynchronizedCollection<Job>();

        private readonly Task _task;
        private readonly SemaphoreSlim _sema = new SemaphoreSlim(0, 1);
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
                _cancellationToken.Cancel();
                _sema.Release();
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

// todo: _jobsから作成すると例外が起きてします。一旦処理しないようにする。要調査
#if false
            JobSummries = _jobs
                .ToReadOnlyReactiveCollection(
                    _jobs.ToCollectionChanged<Job>(),
                    x => x.Summry,
                    Scheduler.Immediate);
#endif

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

                            try
                            {
                                job.Action();
                            }
                            catch(Exception e)
                            {
                                var args = new ExceptionEventArgs { Exception = e };

                                JobExecutingException?.Invoke(this, args);

                                // 例外が起きたので以降のジョブは実行しない
                                _jobs.Clear();
                            }
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

            try
            {
                _sema.Release();
            }
            catch (SemaphoreFullException)
            {
            }
        }
    }
}