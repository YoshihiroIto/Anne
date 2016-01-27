using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Text;
using LZ4;

namespace Anne.Foundation
{
    public class SavingMemoryString : INotifyPropertyChanged
    {
        public string Value
        {
            get
            {
                if (_plane != null)
                    return _plane;

                return _data == null ? null : Decompress(_data);
            }

            set
            {
                if (value == null)
                {
                    _data = null;
                    _plane = null;
                }
                else
                {
                    _data = Compress(value);
                    if (_data.Length >= value.Length*2)
                    {
                        _data = null;
                        _plane = value;
                    }
                }

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

        private string _plane;

        private string Decompress(byte[] data)
        {
            Debug.Assert(data != null);

            var buf = LZ4Codec.Decode(data, 0, data.Length, _orgSize);

            return Encoding.Unicode.GetString(buf, 0, buf.Length);
        }

        private byte[] Compress(string text)
        {
            Debug.Assert(text != null);

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