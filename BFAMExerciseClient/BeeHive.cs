using BFAMExercise.Server.MessageStream;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace BFAMExerciseClient
{
    public class BeeHive
    {
        public static async Task<bool> AttackAsync(int numBees, int numGunsPerBee)
        {
            var taskArr = new Task<bool>[numBees];
            int beeId = 0;
            while (beeId++ < numBees)
            {
                var task = new BeeWithGuns(beeId, numGunsPerBee).AttackAsync();
                taskArr[beeId - 1] = task;
            }
            await Task.WhenAll(taskArr);
            return taskArr.All(task => task.Result);
        }
    }
}
