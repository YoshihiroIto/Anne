using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;

namespace Anne.Foundation
{
    public static class DisposableChecker
    {
        private static readonly HashSet<IDisposable> Disposables = new HashSet<IDisposable>();

        public static void Start()
        {
        }

        public static void End()
        {
            if (Disposables.Any())
                MessageBox.Show("Found undispose object.");
        }

        public static void Add(IDisposable disposable)
        {
            Disposables.Add(disposable);
        }

        public static void Remove(IDisposable disposable)
        {
            if (Disposables.Contains(disposable) == false)
                MessageBox.Show("Found multiple diposing.");
              
            Disposables.Remove(disposable);
        }
    }
}