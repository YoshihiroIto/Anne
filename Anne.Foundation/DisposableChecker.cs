using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Diagnostics;

namespace Anne.Foundation
{
    public static class DisposableChecker
    {
        private static readonly HashSet<IDisposable> Disposables = new HashSet<IDisposable>();

        [Conditional("DEBUG")]
        public static void Start()
        {
        }

        [Conditional("DEBUG")]
        public static void End()
        {
            if (Disposables.Any())
            {
                MessageBox.Show("Found undispose object.");
            }
        }

        [Conditional("DEBUG")]
        public static void Add(IDisposable disposable)
        {
            Disposables.Add(disposable);
        }

        [Conditional("DEBUG")]
        public static void Remove(IDisposable disposable)
        {
#if false
            if (Disposables.Contains(disposable) == false)
            {
                MessageBox.Show("Found multiple diposing.");
            }
#endif
              
            Disposables.Remove(disposable);
        }
    }
}