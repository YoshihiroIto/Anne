using System.Linq;
using Anne.Foundation.Mvvm;

namespace Anne.Model.Git
{
    public class FilePatch : ModelBase
    {
        private string _path;
        public string Path
        {
            get { return _path; }
            set { SetProperty(ref _path, value); } 
        }

        private string _patch;
        public string Patch
        {
            get { return _patch; }
            set
            {
                if (SetProperty(ref _patch, value))
                    // ReSharper disable once ExplicitCallerInfoArgument
                    RaisePropertyChanged(nameof(Diff));
            } 
        }

        public string Diff => string.Join("\n", Patch.Split('\n').Skip(4));
    }
}

#if false
diff --git a/README.md b/README.md
index 1f518ed..c870ebe 100644
--- a/README.md
+++ b/README.md
@@ -5,8 +5,8 @@ Wox   [![Build status](https://ci.appveyor.com/api/projects/status/bfktntbivg32e
 Demo
 =========
 
-[More demo]("https://github.com/Wox-launcher/Wox/wiki/Screenshot")
-![demo]("http://i.imgur.com/DtxNBJi.gifv")
+[More demo](https://github.com/Wox-launcher/Wox/wiki/Screenshot)
+![demo](http://i.imgur.com/DtxNBJi.gif)
 
 Features
 =========
#endif
