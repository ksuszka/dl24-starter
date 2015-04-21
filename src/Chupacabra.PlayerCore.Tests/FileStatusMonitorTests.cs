﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Moq;
using Moq.Protected;
using NUnit.Framework;
using Chupacabra.PlayerCore.Host;

namespace Chupacabra.PlayerCore.Tests
{
    public class FileStatusMonitorTests
    {
        /// <summary>
        /// Synchronous task scheduler to be used in testing method which fires new tasks.
        /// http://stackoverflow.com/a/17844870/3568443
        /// </summary>
        class SynchronousTaskScheduler : TaskScheduler
        {
            protected override void QueueTask(Task task)
            {
                this.TryExecuteTask(task);
            }

            protected override bool TryExecuteTaskInline(Task task, bool taskWasPreviouslyQueued)
            {
                return this.TryExecuteTask(task);
            }

            protected override IEnumerable<Task> GetScheduledTasks()
            {
                yield break;
            }
        }

        [Test]
        public void ConfirmingTurnShouldGenerateTextualTree()
        {
            Task.Factory.StartNew(() =>
            {
                var mock = new Mock<FileStatusMonitor>("fake");
                string result = null;
                mock.Protected().Setup("WriteData", ItExpr.IsAny<string>()).Callback((string s) => { result = s; });
                var monitor = mock.Object;

                monitor.SetValue("level1_1", "something");
                monitor.SetValue("level1_2/level2_1/level3_1", "something else");
                monitor.SetValue("level1_2/level2_1/level3_1/level4_1", "something else");
                monitor.SetValue("level1_2/level2_2", "something else");
                monitor.SetValue("level1_3/level2", "another thing");
                monitor.SetValue("level1_2/level2_1/level3_2/level4_2", "something else 2");
                monitor.SetValue("level1_2/level2_1/level3_2", "something else 2");
                monitor.SetValue("level1_2/level2_1", null);
                monitor.ConfirmTurn();
                Assert.AreEqual(
@"+- level1_1: something
+- level1_2
|  +- level2_1: null
|  |  +- level3_1: something else
|  |  |  `- level4_1: something else
|  |  `- level3_2: something else 2
|  |     `- level4_2: something else 2
|  `- level2_2: something else
`- level1_3
   `- level2: another thing
",
                               result);

            }, CancellationToken.None, TaskCreationOptions.None, new SynchronousTaskScheduler()).Wait();
        }
    }
}
