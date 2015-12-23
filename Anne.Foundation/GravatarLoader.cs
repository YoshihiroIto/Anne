using System.Collections.Concurrent;
using System.Diagnostics;
using System.Security.Cryptography;
using System.Text;
using System.Windows.Media.Imaging;
using ReactiveBingViewer.Models;

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
            Debug.WriteLine("LoadImage: " + email);

// ネットアクセスしたくない時用
#if true
            var url = GenerateUrlFromEmail(email);
            var bi = BitmapImageHelper.DownloadImageAsync(url, Livet.DispatcherHelper.UIDispatcher).Result;
            return bi;
#else
            return null;
#endif
        }

        private static string GenerateUrlFromEmail(string email)
        {
            using (var hasher = MD5.Create())
            {
                var hash = hasher.ComputeHash(Encoding.Default.GetBytes(email));
                var sb = new StringBuilder();

                foreach (var h in hash)
                    sb.Append(h.ToString("x2"));

                return $"http://www.gravatar.com/avatar/{sb}&size=256&d=mm";
            }
        }
    }
}
