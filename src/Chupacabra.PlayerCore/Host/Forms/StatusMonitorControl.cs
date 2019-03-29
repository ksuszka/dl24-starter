// using System;
// using System.Collections.Concurrent;
// using System.Collections.Generic;
// using System.ComponentModel;
// using System.Drawing;
// using System.Data;
// using System.Linq;
// using System.Text;
// using System.Threading;
// using System.Threading.Tasks;
// using System.Windows.Forms;

// namespace Chupacabra.PlayerCore.Host.Forms
// {
//     public partial class StatusMonitorControl : UserControl, IStatusMonitor
//     {
//         private ConcurrentQueue<Tuple<string, string>> _valueQueue = new ConcurrentQueue<Tuple<string, string>>();
//         private SynchronizationContext _synchronizationContext;

//         public StatusMonitorControl()
//         {
//             _synchronizationContext = SynchronizationContext.Current;
//             InitializeComponent();
//         }

//         /// <summary>
//         /// Schedule updating node in tree.
//         /// Possibly called on different thread!
//         /// </summary>
//         /// <param name="key"></param>
//         /// <param name="value"></param>
//         public void Set(string key, object value)
//         {
//             // Normalize key
//             var normalizedKey = NormalizeKey(key);
//             var valueText = (value != null) ? value.ToString() : null;
//             _valueQueue.Enqueue(Tuple.Create(normalizedKey, valueText));
//             _synchronizationContext.Post(_ => UpdateTree(), null);
//         }

//         public void Delete(string key)
//         {
//             var normalizedKey = NormalizeKey(key);
//             _valueQueue.Enqueue(Tuple.Create(normalizedKey, (string)null));
//             _synchronizationContext.Post(_ => UpdateTree(), null);
//         }

//         public void DeleteChildren(string key)
//         {
//             var normalizedKey = NormalizeKey(key) + "/";
//             _valueQueue.Enqueue(Tuple.Create(normalizedKey, (string)null));
//             _synchronizationContext.Post(_ => UpdateTree(), null);
//         }

//         private void UpdateTree()
//         {
//             Tuple<string, string> data;
//             while (_valueQueue.TryDequeue(out data))
//             {
//                 SetValueInTree(data.Item1, data.Item2);
//             }
//         }

//         private void SetValueInTree(string rawPath, string value)
//         {
//             var removal = value == null;
//             var path = rawPath.Split("/".ToCharArray()).Skip(1);
//             var nodes = tvMain.Nodes;
//             while (path.Any())
//             {
//                 var key = path.First();
//                 path = path.Skip(1);
//                 TreeNode node = null;
//                 if (nodes.ContainsKey(key))
//                 {
//                     node = nodes[key];
//                 }
//                 else if (!removal)
//                 {
//                     // TODO: binary search
//                     int i = 0;
//                     foreach (TreeNode childNode in nodes)
//                     {
//                         if (string.Compare(childNode.Name, key, true) > 0) break;
//                         ++i;
//                     }
//                     node = nodes.Insert(i, key, key);
//                     node.Expand();
//                 }
//                 else if (string.IsNullOrEmpty(key))
//                 {
//                     nodes.OfType<TreeNode>().ToList().ForEach(n => n.Remove());
//                 }
//                 if (node == null)
//                 {
//                     break;
//                 }
//                 nodes = node.Nodes;
//                 if (!path.Any())
//                 {
//                     if (value == null)
//                     {
//                         node.Remove();
//                     }
//                     else
//                     {
//                         node.Text = string.Format("{0}: {1}", key, value);
//                     }
//                 }
//             }
//         }

//         public void ConfirmTurn()
//         {
//         }

//         private static string NormalizeKey(string key)
//         {
//             return "/" +
//                    string.Join("/",
//                        (key ?? "").Split(@"/\".ToCharArray())
//                            .Where(k => !string.IsNullOrWhiteSpace(k))
//                            .Select(k => k.Trim()));
//         }
//     }
// }
