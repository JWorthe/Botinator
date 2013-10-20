using System;
using System.Windows;
using System.Collections.Generic;
using System.Threading;

namespace Entelect.BattleCity.Challenge
{
    public class GameInProgress
    {
        private static ChallengeService.action _nullAction = ChallengeService.action.NONE;

        private ChallengeService.ChallengeClient _service;
        private ChallengeService.game _currentState;
        private ChallengeService.player _me;
        private ChallengeService.player _enemy;

        private ChallengeService.state?[][] _board;

        private AiAgent _tank1Ai;
        private AiAgent _tank2Ai;

        public GameInProgress(ChallengeService.ChallengeClient service, ChallengeService.state?[][] board)
        {
            _service = service;
            _board = board;

            updateGameStatus();

            _tank1Ai = new AiAgent(_me.units[0].id);
            _tank2Ai = new AiAgent(_me.units[1].id);
        }



        public void run()
        {
            while (true)
            {
                long nextTick = _currentState.nextTickTime.Ticks;
                long currentTick = DateTime.Now.Ticks;

                if (currentTick > nextTick)
                {
                    Console.Error.WriteLine("Waiting for next tick. Current time: {0}, next game tick at: {1}", currentTick, nextTick);
                    updateGameStatus();
                    continue;
                }
                makeNextMove();
                waitForNextTick();

                updateGameStatus();
            }
        }

        private void makeNextMove()
        {
            Move tank1Move = _tank1Ai.GetBestMove(_currentState, _board, _me, _enemy);
            Move tank2Move = _tank2Ai.GetBestMove(_currentState, _board, _me, _enemy);

            sendMovesToService(tank1Move, tank2Move);
        }

        private void sendMovesToService(Move tank1Move, Move tank2Move)
        {
            if (tank1Move != null && tank2Move != null)
            {
                Console.WriteLine("Actions chosen for two tanks");
                Console.WriteLine(tank1Move.ToString());
                Console.WriteLine(tank2Move.ToString());
                _service.setActions(tank1Move.Action, tank2Move.Action);
            }
            else if (tank1Move != null)
            {
                Console.WriteLine("Actions chosen for first tank only");
                Console.WriteLine(tank1Move.ToString());
                _service.setAction(tank1Move.Tank, tank1Move.Action);
            }
            else if (tank2Move != null)
            {
                Console.WriteLine("Actions chosen for second tank only");
                Console.WriteLine(tank2Move.ToString());
                _service.setAction(tank2Move.Tank, tank2Move.Action);
            }
            else
            {
                Console.WriteLine("No tanks to set actions for");
            }
        }

        private void waitForNextTick()
        {
            var nextTick = _currentState.nextTickTime.Ticks;
            var currentTick = DateTime.Now.Ticks;

            var sleepTime = TimeSpan.FromTicks(nextTick - currentTick)+TimeSpan.FromMilliseconds(500);
            if (sleepTime.Ticks < 0L)
            {
                Console.Error.WriteLine("ERROR: Gone passed the next tick time");
            }
            else
            {
                Console.WriteLine("Sleeping until {1} for {0} processor ticks", sleepTime.Ticks, nextTick);
                try
                {
                    Thread.Sleep(sleepTime);
                }
                catch (Exception ex)
                {
                    Console.Error.WriteLine("Exception thrown while waiting for next tick");
                    Console.Error.WriteLine("Exception message: "+ ex.Message);
                }

                Console.WriteLine("Time after sleep: {0}", DateTime.Now.Ticks);
            }
        }

        private void updateGameStatus()
        {
            _currentState = _service.getStatus();

            bool meFound = false;
            bool enemyFound = false;

            foreach (ChallengeService.player player in _currentState.players)
            {
                if (player.name.Equals(_currentState.playerName))
                {
                    _me = player;
                    meFound = true;
                }
                else
                {
                    _enemy = player;
                    enemyFound = true;
                }
            }

            if (!meFound)
            {
                Console.Error.WriteLine("Logged in player was not found");
            }
            if (!enemyFound)
            {
                Console.Error.WriteLine("Logged in opponent was not found");
            }
        }
    }
}
