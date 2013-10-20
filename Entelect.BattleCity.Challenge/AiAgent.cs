using System;
using System.Collections.Generic;

namespace Entelect.BattleCity.Challenge
{
    class AiAgent
    {
        private int _tankId;

        public AiAgent(int tankId)
        {
            _tankId = tankId;
        }

        public Move GetBestMove(ChallengeService.game game, ChallengeService.state?[][] board, ChallengeService.player me, ChallengeService.player enemy)
        {
            ChallengeService.unit tank = null;
            bool bulletInAir = false;

            foreach (var unit in me.units)
            {
                if (unit.id == _tankId)
                {
                    tank = unit;
                }
            }

            if (tank == null)
            {
                Console.WriteLine("Tank {0} does not exist", _tankId);
                return null;
            }

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

            var enemyBase = enemy.@base;

            ChallengeService.direction chosenDirection = 
                tank.y+2 != enemyBase.y ?
                (
                    tank.y+2 > enemyBase.y ?
                    ChallengeService.direction.UP :
                    ChallengeService.direction.DOWN
                ) :
                (
                    tank.x+2 > enemyBase.x ?
                    ChallengeService.direction.LEFT :
                    ChallengeService.direction.RIGHT
                );

            Console.WriteLine("Chosen direction for tank {0} is {1} and bulletInAir is {2}", _tankId, chosenDirection, bulletInAir);

            if (chosenDirection != tank.direction || bulletInAir)
            {
                return MoveInDirection(tank.id, chosenDirection);
            }
            else
            {
                return new Move(tank.id, ChallengeService.action.FIRE);
            }
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
    }
}
