using BFAMExercise.Server.MessageStream;
using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace BFAMExerciseClient
{
    public class BeesWithGuns
    {
        public static void Attack(int numBees, int numGunsPerBee)
        {
            var taskArr = new Task[numBees];
            int beeId = 0;
            while (beeId++ < numBees)
            {
                var task = new BeeWithGuns(beeId, numGunsPerBee).Attack();
                taskArr[beeId - 1] = task;
            }
            Task.WhenAll(taskArr).GetAwaiter().GetResult();
        }
    }
}
