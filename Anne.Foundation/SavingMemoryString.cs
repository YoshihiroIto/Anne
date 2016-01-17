using System.ComponentModel;
using System.IO;
using System.IO.Compression;
using System.Runtime.CompilerServices;
using System.Text;

namespace Anne.Foundation
{
    public class SavingMemoryString : INotifyPropertyChanged
    {
        public string Value
        {
            get { return _data == null ? string.Empty : Decompress(_data); }
            set
            {
                _data = Compress(value);
                OnPropertyChanged();
            }
        }

        public SavingMemoryString()
        {
        }

        public SavingMemoryString(string text)
        {
            Value = text;
        }

        private byte[] _data;

        private static string Decompress(byte[] destination)
        {
            using (var ms = new MemoryStream(destination))
            using (var decomp = new DeflateStream(ms, CompressionMode.Decompress))
            {
                var sb = new StringBuilder();
                var buf = new byte[4096];
                int length;

                while ((length = decomp.Read(buf, 0, buf.Length)) > 0)
                {
                    sb.Append(Encoding.Unicode.GetString(buf, 0, length));
                }

                return sb.ToString();
            }
        }

        private static byte[] Compress(string text)
        {
            using (var ms = new MemoryStream())
            using (var comp = new DeflateStream(ms, CompressionMode.Compress))
            {
                var source = Encoding.Unicode.GetBytes(text);

                comp.Write(source, 0, source.Length);
                comp.Close();

                return ms.ToArray();
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}