using System;
using System.Windows;
using System.Collections.Generic;
using System.Threading;
using System.Diagnostics;

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

        private Stopwatch _stepTimer;

        public GameInProgress(ChallengeService.ChallengeClient service, ChallengeService.state?[][] board)
        {
            _service = service;
            _board = board;

            updateGameStatus(true);

            _tank1Ai = new AiAgent(_me.units[0].id);
            _tank2Ai = new AiAgent(_me.units[1].id);
        }

        public void run()
        {
            while (true)
            {
                if (_currentState.millisecondsToNextTick-_stepTimer.ElapsedMilliseconds < 0)
                {
                    Console.Error.WriteLine("Waiting for next tick. Milliseconds to next tick on last update: {0}. Elapsed milliseconds since then: {1}.", _currentState.millisecondsToNextTick, _stepTimer.ElapsedMilliseconds);
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
            var sleepTime = TimeSpan.FromMilliseconds(_currentState.millisecondsToNextTick-_stepTimer.ElapsedMilliseconds+500);
            if (sleepTime.Ticks < 0L)
            {
                Console.Error.WriteLine("ERROR: Gone passed the next tick time");
            }
            else
            {
                Console.WriteLine("Sleeping for {0}ms", sleepTime.Milliseconds);
                try
                {
                    Thread.Sleep(sleepTime);
                }
                catch (Exception ex)
                {
                    Console.Error.WriteLine("Exception thrown while waiting for next tick");
                    Console.Error.WriteLine("Exception message: "+ ex.Message);
                }

                Console.WriteLine("Time since last update after sleep: {0}ms", _stepTimer.ElapsedMilliseconds);
            }
        }

        private void updateGameStatus(bool firstTime = false)
        {
            if (firstTime)
            {
                _currentState = _service.getStatus();
            }
            else
            {
                var previousTick = _currentState.currentTick;
                Console.WriteLine("Updating game status. Current tick is {0}", previousTick);
                while (previousTick == _currentState.currentTick)
                {
                    _currentState = _service.getStatus();

                    if (previousTick == _currentState.currentTick)
                    {
                        Console.WriteLine("Tried to retrieve new status before next tick. Current tick is {0}. Last tick is {1}.", _currentState.currentTick, previousTick);
                    }
                }
            }
            _stepTimer = Stopwatch.StartNew();

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
