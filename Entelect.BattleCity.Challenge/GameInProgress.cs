using System;
using System.Windows;
using System.Collections.Generic;
using System.Threading;

namespace Entelect.BattleCity.Challenge
{
    class GameInProgress
    {
        public static void run(ChallengeService.ChallengeClient service, ChallengeService.state?[][] state)
        {
            AiAgent agent = new AiAgent();

            while (true)
            {
                var game = service.getStatus();
                long currentTick = DateTime.Now.Ticks;
                long nextTick = game.nextTickTime.Ticks;
                if (currentTick > nextTick)
                {
                    continue;
                }

                // AI logic here
                Move tank1Move = agent.GetBestMove(state, game, 0);
                Move tank2Move = agent.GetBestMove(state, game, 1);

                if (tank1Move != null)
                {
                    service.setActionAsync(tank1Move.Tank, tank1Move.Action);
                }
                if (tank2Move != null)
                {
                    service.setActionAsync(tank2Move.Tank, tank2Move.Action);
                }

                currentTick = DateTime.Now.Ticks;

                long sleepTime = nextTick - currentTick;
                if (sleepTime < 0L)
                {
                    Console.Error.WriteLine("ERROR: Gone passed the next tick time");
                }
                else
                {
                    Console.WriteLine("Sleeping until {1} for {0}ms", sleepTime, game.nextTickTime.ToString());
                }

                try
                {
                    Thread.Sleep(TimeSpan.FromTicks(sleepTime));
                }
                catch (Exception)
                {
                    continue;
                }
                //while (startTick < nextTick)
                //{
                //    startTick += (DateTime.Now.Ticks - startTick);
                //}
            }
        }
    }
}
