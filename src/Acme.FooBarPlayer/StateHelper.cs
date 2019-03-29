using Newtonsoft.Json;
using System.IO;

namespace Acme.FooBarPlayer
{
    public class StateHelper
    {
        private string _fileName;

        public StateHelper(string fileName)
        {
            _fileName = fileName;
        }
        public void Save<T>(T obj)
        {
            Save(_fileName, obj);
        }

        public T Load<T>()
        {
            return Load<T>(_fileName);
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
