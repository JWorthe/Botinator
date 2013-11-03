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

        public AiAgent(int tankId)
        {
            _tankId = tankId;
            _lastAction = ChallengeService.action.NONE;
            _hasShotFromLastPosition = false;
        }

        public Move GetBestMove(ChallengeService.game game, ChallengeService.state?[][] board, ChallengeService.player me, ChallengeService.player enemy)
        {
            Move move = null;
            ChallengeService.unit tank = null;

            if (me != null && me.units != null)
            {
                foreach (var unit in me.units)
                {
                    if (unit.id == _tankId)
                    {
                        Console.WriteLine("Tank found in list of tanks");
                        tank = unit;
                    }
                }
            }

            if (tank == null)
            {
                Console.WriteLine("Tank {0} does not exist", _tankId);
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

            if (stuckLastTurn && (tank.direction == ChallengeService.direction.UP || tank.direction == ChallengeService.direction.DOWN))
            {
                _targetX = tank.x + (pastMidpoint!=(tank.x > enemyBase.x) ? +1 : -1);
            }

            ChallengeService.direction chosenDirection = 
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
                );

            Console.WriteLine("Chosen direction for tank {0} is {1} and bulletInAir is {2}", _tankId, chosenDirection, bulletInAir);

            if (chosenDirection != tank.direction || bulletInAir)
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

        private bool checkIsBulletInAir(ChallengeService.state?[][] board, ChallengeService.player me, ChallengeService.unit tank)
        {
            var bulletInAir = false;
            if (me.bullets != null)
            {
                foreach (var bullet in me.bullets)
                {
                    if (Math.Abs(bullet.x - tank.x) < board.Length / 4)
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
