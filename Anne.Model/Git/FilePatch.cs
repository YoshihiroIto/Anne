using System.Diagnostics;
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
            set { SetProperty(ref _patch, value); } 
        }
    }
}

// https://git-scm.com/docs/git-diff

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

#if false
diff --git a/Packages.dgml b/Packages.dgml
new file mode 100644
index 0000000..2f84352
--- /dev/null
+++ b/Packages.dgml
@@ -0,0 +1,15 @@
+﻿<?xml version="1.0" encoding="utf-8"?>
+<DirectedGraph GraphDirection="LeftToRight" xmlns="http://schemas.microsoft.com/vs/2009/dgml">
+  <Nodes />
+  <Links />
+  <Categories>
+    <Category Id="项目" />
+    <Category Id="包" />
+  </Categories>
+  <Styles>
+    <Style TargetType="Node" GroupLabel="项目" ValueLabel="True">
+      <Condition Expression="HasCategory('项目')" />
+      <Setter Property="Background" Value="Blue" />
+    </Style>
+  </Styles>
+</DirectedGraph>
\ No newline at end of file
#endif
