using System;
using System.Collections.Generic;

namespace Entelect.BattleCity.Challenge
{
    class AiAgent
    {
        private int? _lastX;
        private int? _lastY;
        private ChallengeService.action _lastAction;
        private bool _hasShotFromLastPosition;

        private int _tankId;

        private int? _targetX;

        private bool _checkForOpenPathToMiddle;
        private bool _headingToMiddle;

        public AiAgent(int tankId, bool checkForOpenPathToMiddle)
        {
            _tankId = tankId;
            _checkForOpenPathToMiddle = checkForOpenPathToMiddle;
            _lastAction = ChallengeService.action.NONE;
            _hasShotFromLastPosition = false;
            _headingToMiddle = false;
        }

        public Move GetBestMove(ChallengeService.game game, BoardCell[][] board, ChallengeService.player me, ChallengeService.player enemy)
        {
            Move move = null;
            ChallengeService.unit tank = findTankInPlayer(_tankId, me);

            if (tank == null)
            {
                return null;
            }

            if (tank.x != _lastX || tank.y != _lastY)
            {
                _hasShotFromLastPosition = false;
            }

            var bulletInAir = checkIsBulletInAir(board, me, tank);
            var stuckLastTurn = checkStuckLastTurn(tank);

            var enemyBase = enemy.@base;
            
            var pastMidpoint = (Math.Abs(enemyBase.y-tank.y) < (board[0].Length / 2));

            if (stuckLastTurn && (tank.direction == ChallengeService.direction.UP || tank.direction == ChallengeService.direction.DOWN) && enemyBase.x != tank.x)
            {
                _targetX = tank.x + (pastMidpoint!=(tank.x > enemyBase.x) ? +1 : -1);
            }

            if (_checkForOpenPathToMiddle && !_headingToMiddle && tank.x != enemyBase.x)
            {
                _headingToMiddle = testPathToMiddleIsOpen(board, tank, enemyBase);
            }
            else if (_checkForOpenPathToMiddle && _headingToMiddle && tank.x == enemyBase.x)
            {
                _headingToMiddle = false;
            }


            ChallengeService.direction chosenDirection = 
                _headingToMiddle ?
                (
                    tank.x > enemyBase.x ?
                    ChallengeService.direction.LEFT :
                    ChallengeService.direction.RIGHT
                ) :
                (
                    tank.y != enemyBase.y ?
                    (
                        _targetX.HasValue && _targetX != tank.x ?
                        (
                            tank.x > _targetX ?
                            ChallengeService.direction.LEFT :
                            ChallengeService.direction.RIGHT
                        ) :
                        (
                            tank.y > enemyBase.y ?
                            ChallengeService.direction.UP :
                            ChallengeService.direction.DOWN
                        )
                    ) :
                    (
                        tank.x > enemyBase.x ?
                        ChallengeService.direction.LEFT :
                        ChallengeService.direction.RIGHT
                    )
                );

            if (chosenDirection != tank.direction || bulletInAir || _headingToMiddle)
            {
                move = MoveInDirection(tank.id, chosenDirection);
            }
            else
            {
                move = new Move(tank.id, ChallengeService.action.FIRE);
                _hasShotFromLastPosition = true;
            }

            _lastX = tank.x;
            _lastY = tank.y;
            _lastAction = move.Action;

            return move;
        }

        private bool testPathToMiddleIsOpen(BoardCell[][] board, ChallengeService.unit tank, ChallengeService.@base enemyBase)
        {
            var minY = tank.y - 2;
            var maxY = tank.y + 2;
            var minX = Math.Min(tank.x, enemyBase.x)-2;
            var maxX = Math.Max(tank.x, enemyBase.x)+2;

            bool insideRange = board.Length > maxX && board[maxX].Length > maxY && 0 <= minX && 0 <= minY;
            if (!insideRange)
            {
                return false;
            }

            for (int x = minX; x <= maxX; ++x)
            {
                for (int y = minY; y <= maxY; ++y)
                {
                    if (board[x][y] != BoardCell.EMPTY)
                    {
                        return false;
                    }
                    
                }
            }

            return true;
        }

        private ChallengeService.unit findTankInPlayer(int tankId, ChallengeService.player me)
        {
            if (me != null && me.units != null)
            {
                foreach (var unit in me.units)
                {
                    if (unit.id == _tankId)
                    {
                        return unit;
                    }
                }
            }
            return null;
        }

        public Move MoveInDirection(int tankId, ChallengeService.direction direction)
        {
            switch (direction)
            {
                case ChallengeService.direction.UP:
                    return new Move(tankId, ChallengeService.action.UP);
                case ChallengeService.direction.DOWN:
                    return new Move(tankId, ChallengeService.action.DOWN);
                case ChallengeService.direction.LEFT:
                    return new Move(tankId, ChallengeService.action.LEFT);
                case ChallengeService.direction.RIGHT:
                    return new Move(tankId, ChallengeService.action.RIGHT);
                default:
                    return new Move(tankId, ChallengeService.action.NONE);
            }
        }

        private bool checkIsBulletInAir(BoardCell[][] board, ChallengeService.player me, ChallengeService.unit tank)
        {
            var bulletInAir = false;
            if (me.bullets != null)
            {
                foreach (var bullet in me.bullets)
                {
                    if (Math.Abs(bullet.x - tank.x) < board.Length / 6)
                    {
                        bulletInAir = true;
                    }
                }
            }

            return bulletInAir;
        }

        private bool checkStuckLastTurn(ChallengeService.unit tank)
        {
            return !(_lastAction == ChallengeService.action.FIRE || _lastAction == ChallengeService.action.NONE)
                && tank.x == _lastX && tank.y == _lastY
                && _hasShotFromLastPosition;
        }
    }
}
