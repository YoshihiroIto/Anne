using System.Collections.Generic;
using System.IO;
using YamlDotNet.Serialization;

namespace Anne.Model
{
    public class AppConfig
    {
        public List<string> Repositories { get; set; } = new List<string>();

        public int MaxCommitCount { get; set; } = int.MaxValue;

        public static AppConfig LoadFromFile(string filePath)
        {
            try
            {
                using (var reader = new StreamReader(filePath))
                {
                    return new Deserializer().Deserialize<AppConfig>(reader) ?? new AppConfig();
                }
            }
            catch
            {
                return new AppConfig();
            }
        }

        public static void SaveToFile(string filePath, AppConfig config)
        {
            using (var writer = new StreamWriter(filePath))
            {
                new Serializer().Serialize(writer, config);
            }
        }
    }
}