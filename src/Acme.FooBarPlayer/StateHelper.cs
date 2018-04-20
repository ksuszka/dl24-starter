using Newtonsoft.Json;
using System.IO;

namespace Acme.FooBarPlayer
{
    public static class StateHelper
    {
        public static void Save<T>(T obj)
        {
            Save(Properties.Settings.Default.StateFilename, obj);
        }

        public static T Load<T>()
        {
            return Load<T>(Properties.Settings.Default.StateFilename);
        }

        public static void Save<T>(string fileName, T obj)
        {
            var serialized = JsonConvert.SerializeObject(obj, Formatting.Indented);
            File.WriteAllText(fileName, serialized);
        }

        public static T Load<T>(string fileName)
        {
            if (File.Exists(fileName))
            {
                var serialized = File.ReadAllText(fileName);
                return JsonConvert.DeserializeObject<T>(serialized);
            }

            return default(T);
        }
    }
}
