using System;
using System.Windows;
using System.Collections.Generic;
using System.Threading;
using System.Diagnostics;
using System.Text;

namespace Entelect.BattleCity.Challenge
{
    public class GameInProgress
    {
        private static ChallengeService.action _nullAction = ChallengeService.action.NONE;

        private ChallengeService.ChallengeClient _service;
        private ChallengeService.game _currentState;
        private ChallengeService.player _me;
        private ChallengeService.player _enemy;

        private BoardCell[][] _board;

        private AiAgent _tank1Ai;
        private AiAgent _tank2Ai;

        private Stopwatch _stepTimer;

        public GameInProgress(ChallengeService.ChallengeClient service, ChallengeService.state?[][] board)
        {
            _service = service;
            _board = getBoardCellArrayFromServiceStates(board);

            updateGameStatus(true);
            _board[_me.@base.x][_me.@base.y] = BoardCell.BASE;
            _board[_enemy.@base.x][_enemy.@base.y] = BoardCell.BASE;

            _tank1Ai = new AiAgent(_me.units[0].id, false);
            _tank2Ai = new AiAgent(_me.units[1].id, true);
        }

        

        public void run()
        {
            while (true)
            {
                if (_currentState.millisecondsToNextTick-_stepTimer.ElapsedMilliseconds < 0)
                {
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
                _service.setActions(tank1Move.Action, tank2Move.Action);
            }
            else if (tank1Move != null)
            {
                _service.setAction(tank1Move.Tank, tank1Move.Action);
            }
            else if (tank2Move != null)
            {
                _service.setAction(tank2Move.Tank, tank2Move.Action);
            }
        }

        private void waitForNextTick()
        {
            var sleepTime = TimeSpan.FromMilliseconds(_currentState.millisecondsToNextTick-_stepTimer.ElapsedMilliseconds+500);
            if (sleepTime.Ticks > 0L)
            {
                try
                {
                    Thread.Sleep(sleepTime);
                }
                catch (Exception ex)
                {
                }
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
                while (previousTick == _currentState.currentTick)
                {
                    _currentState = _service.getStatus();
                }
            }
            _stepTimer = Stopwatch.StartNew();

            foreach (ChallengeService.player player in _currentState.players)
            {
                if (player.name.Equals(_currentState.playerName))
                {
                    _me = player;
                }
                else
                {
                    _enemy = player;
                }
            }
        }

        private BoardCell[][] getBoardCellArrayFromServiceStates(ChallengeService.state?[][] stateBoard)
        {
            BoardCell[][] newBoard = new BoardCell[stateBoard.Length][];
            for (int x = 0; x < stateBoard.Length; ++x)
            {
                newBoard[x] = new BoardCell[stateBoard[x].Length];
                for (int y = 0; y < stateBoard[x].Length; ++y)
                {
                    switch (stateBoard[x][y])
                    {
                        case null:
                        case ChallengeService.state.NONE:
                        case ChallengeService.state.EMPTY:
                            newBoard[x][y] = BoardCell.EMPTY;
                            break;
                        case ChallengeService.state.FULL:
                            newBoard[x][y] = BoardCell.WALL;
                            break;
                        case ChallengeService.state.OUT_OF_BOUNDS:
                            newBoard[x][y] = BoardCell.OUT_OF_BOUNDS;
                            break;
                        default:
                            newBoard[x][y] = BoardCell.OUT_OF_BOUNDS;
                            break;
                    }

                }
            }

            return newBoard;
        }
    }
}
