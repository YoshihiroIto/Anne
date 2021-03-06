﻿using System.Collections.Concurrent;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.NetworkInformation;
using System.Security.Cryptography;
using System.Text;
using System.Windows;
using System.Windows.Media.Imaging;
using ReactiveBingViewer.IO;

namespace Anne.Foundation
{
    public static class GravatarLoader
    {
        private static readonly ConcurrentDictionary<string, BitmapImage> Cache =
            new ConcurrentDictionary<string, BitmapImage>();

        public static BitmapImage GetFromCache(string email)
        {
            email = email.ToLower();

            BitmapImage bi;
            Cache.TryGetValue(email, out bi);
            return bi;
        }

        public static BitmapImage Get(string email)
        {
            email = email.ToLower();

            return Cache.GetOrAdd(email, LoadImage);
        }

        private static BitmapImage LoadImage(string email)
        {
            if (NetworkInterface.GetIsNetworkAvailable() == false)
                return null;

            try
            {
                BitmapImage bitmap = null;

                var url = GenerateUrlFromEmail(email);

                using (var wc = new WebClient())
                {
                    var data = wc.DownloadData(url);

                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        using (var stream = new WrappingStream(new MemoryStream(data)))
                        {
                            bitmap = new BitmapImage();
                            bitmap.BeginInit();
                            bitmap.StreamSource = stream;
                            bitmap.CacheOption = BitmapCacheOption.OnLoad;
                            bitmap.EndInit();
                            bitmap.StreamSource = null;

                            if (bitmap.CanFreeze)
                                bitmap.Freeze();
                        }
                    });
                }

                Debug.Assert(bitmap != null);

                return bitmap;
            }
            catch
            {
                return null;
            }
        }

        private static string GenerateUrlFromEmail(string email)
        {
            using (var hasher = MD5.Create())
            {
                var hash = hasher.ComputeHash(Encoding.Default.GetBytes(email.Trim().ToLower()));
                var sb = new StringBuilder();

                foreach (var h in hash)
                    sb.Append(h.ToString("x2"));

                return $"http://www.gravatar.com/avatar/{sb}?&size=256&default=retro";
            }
        }
    }
}