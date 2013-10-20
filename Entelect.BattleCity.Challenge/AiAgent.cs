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

            /*if (game.events != null && game.events.unitEvents != null)
            {
                foreach (var unitEvent in game.events.unitEvents)
                {
                    if (unitEvent.unit != null && unitEvent.unit.id == _tankId && unitEvent.bullet != null)
                    {
                        Console.WriteLine("Tank was shot");
                        return null;
                    }
                }
                Console.WriteLine("No relevant events in unit events list", _tankId);
            }
            else
            {
                Console.WriteLine("No events in unit events list", _tankId);
            }*/

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
                tank.y != enemyBase.y ?
                (
                    tank.y > enemyBase.y ?
                    ChallengeService.direction.UP :
                    ChallengeService.direction.DOWN
                ) :
                (
                    tank.x > enemyBase.x ?
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
