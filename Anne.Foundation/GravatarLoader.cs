using System.Collections.Concurrent;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using ReactiveBingViewer.Models;

namespace Anne.Foundation
{
    public static class GravatarLoader
    {
        public static ConcurrentDictionary<string, BitmapImage> Cache { get; } =
            new ConcurrentDictionary<string, BitmapImage>();

        public static async Task<BitmapImage> LoadImage(string email)
        {
            var url = GenerateUrlFromEmail(email);

            var bi = await BitmapImageHelper.DownloadImageAsync(url, Livet.DispatcherHelper.UIDispatcher);

            Cache[email] = bi;

            return bi;
        }

        private static string GenerateUrlFromEmail(string email)
        {
            using (var hasher = MD5.Create())
            {
                var hash = hasher.ComputeHash(Encoding.Default.GetBytes(email));
                var sb = new StringBuilder();

                foreach (var h in hash)
                    sb.Append(h.ToString("x2"));

                return string.Format($"http://www.gravatar.com/avatar/{sb}&size=256&d=mm");
            }
        }
    }
}
