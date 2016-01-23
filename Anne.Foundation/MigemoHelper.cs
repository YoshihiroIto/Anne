using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using KaoriYa.Migemo;

namespace Anne.Foundation
{
    public class MigemoHelper
    {
        public static MigemoHelper Instance { get; } = new MigemoHelper();

        private Migemo _internal;
        private Migemo Internal
        {
            get
            {
                if (_internal != null)
                    return _internal;

                InitializeDll();

                var asmLoc = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
                Debug.Assert(asmLoc != null);

                _internal = new Migemo(Path.Combine(asmLoc, "cmigemo", "dict", "migemo-dict"));
                return _internal;
            }
        }

        [DllImport("kernel32")]
        private static extern IntPtr LoadLibrary(string lpFileName);

        private MigemoHelper()
        {
        }

        public void Initialize()
        {
        }

        public void Destory()
        {
            _internal?.Dispose();
        }

        private readonly Regex _emptyRegex = new Regex(string.Empty);

        public Regex MakeRegex(string query)
        {
            query = query.Trim();

            if (string.IsNullOrEmpty(query))
                return _emptyRegex;

            try
            {
                var pattern = Internal.Query(query);
                return new Regex(pattern, RegexOptions.IgnoreCase | RegexOptions.IgnorePatternWhitespace);  
            }
            catch
            {
                return _emptyRegex;
            }
        }

        private void InitializeDll()
        {
            var asmLoc = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            Debug.Assert(asmLoc != null);

            var dllPath = Path.Combine(asmLoc, "cmigemo", Environment.Is64BitProcess ? "Win64" : "Win32", "migemo.dll");

            if (File.Exists(dllPath) == false)
                throw new DllNotFoundException(dllPath);

            if (LoadLibrary(dllPath) == IntPtr.Zero)
                throw new DllNotFoundException(dllPath);
        }
    }
}