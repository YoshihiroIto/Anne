using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text;
using LZ4;

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
        private int _orgSize;

        private string Decompress(byte[] data)
        {
            var buf = LZ4Codec.Decode(data, 0, data.Length, _orgSize);

            return Encoding.Unicode.GetString(buf, 0, buf.Length);
        }

        private byte[] Compress(string text)
        {
            var source = Encoding.Unicode.GetBytes(text);
            _orgSize = source.Length;

            return LZ4Codec.Encode(source, 0, source.Length);
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}