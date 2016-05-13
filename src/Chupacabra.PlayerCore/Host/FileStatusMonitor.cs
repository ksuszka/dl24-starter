using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chupacabra.PlayerCore.Host
{
    public class FileStatusMonitor : IStatusMonitor
    {
        private readonly string _filePath;
        private ConcurrentDictionary<string, string> _values = new ConcurrentDictionary<string, string>();

        public FileStatusMonitor(string filePath)
        {
            _filePath = filePath;
        }

        public void Set(string key, object value)
        {
            // Normalize key
            var normalizedKey = NormalizeKey(key);
            var valueText = (value != null) ? value.ToString() : "null";
            _values.AddOrUpdate(normalizedKey, valueText, (k, v) => valueText);
        }

        public void Delete(string key)
        {
            var normalizedKey = NormalizeKey(key);
            RemoveKeys(normalizedKey);
        }

        public void DeleteChildren(string key)
        {
            var normalizedKey = NormalizeKey(key) + "/";
            RemoveKeys(normalizedKey);
        }

        public void ConfirmTurn()
        {
            var snapshot = _values.ToArray();

            // This design is intentionally bad ;)
            // In general tasks should not be fired and forgot, but in this case I don't really care.
            Task.Factory.StartNew(() =>
            {
                // Build tree
                var tree = new Dictionary<string, HashSet<string>>();
                tree.Add("/", new HashSet<string>());

                foreach (var line in snapshot.Select(kv => kv.Key))
                {
                    var tokens = line.Split(@"/".ToCharArray()).Skip(1).ToList();
                    var path = "";
                    foreach (var token in tokens)
                    {
                        var childPath = path + "/" + token;
                        var set = tree[string.IsNullOrWhiteSpace(path) ? "/" : path];
                        set.Add(childPath);
                        path = childPath;
                        if (!tree.ContainsKey(path)) tree.Add(path, new HashSet<string>());
                    }
                }

                var map = snapshot.ToDictionary(kv => kv.Key, kv => kv.Value);
                var sb = new StringBuilder();
                Action<string, string, bool> printTree = null;
                printTree = (prefix, path, isLast) =>
                {
                    if (path != "/")
                    {
                        var valueString = (map.ContainsKey(path)) ? string.Format(": {0}", map[path]) : "";
                        var name = path.Split("/".ToCharArray()).Last();
                        var currentPrefix = (isLast) ? "`- " : "+- ";
                        sb.AppendLine(string.Format("{0}{1}{2}{3}", prefix, currentPrefix, name, valueString));
                        prefix += (isLast) ? "   " : "|  ";
                    }
                    var children = tree[path].OrderBy(i => i).ToList();
                    for (int i = 0; i < children.Count; i++)
                    {
                        printTree(prefix, children[i], i == children.Count - 1);
                    }
                };

                printTree("", "/", false);
                WriteData(sb.ToString());
            });
        }

        /// <summary>
        /// Writes gathered data to file. Template method to be overriden in tests.
        /// </summary>
        /// <param name="text"></param>
        protected virtual void WriteData(string text)
        {
            File.WriteAllText(_filePath, text);
        }

        private static string NormalizeKey(string key)
        {
            return "/" +
                   string.Join("/",
                       (key ?? "").Split(@"/\".ToCharArray())
                           .Where(k => !string.IsNullOrWhiteSpace(k))
                           .Select(k => k.Trim()));
        }

        private void RemoveKeys(string key)
        {
            var keys = _values.Keys.ToList();
            foreach (var k in keys)
            {
                if (k.StartsWith(key))
                {
                    string out2;
                    _values.TryRemove(k, out out2);
                }
            }
        }
    }
}