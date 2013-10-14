using System;
using System.Collections.Generic;

namespace Entelect.BattleCity.Challenge
{
    class AiAgent
    {
        private int _tankId;

        public AiAgent()
        {

        }

        public AiAgent(int tankId)
        {
            _tankId = tankId;
        }

        public Move GetBestMove(ChallengeService.state?[][] state, ChallengeService.game game, int tankIndex)
        {
            ChallengeService.player me = null;
            ChallengeService.player enemy = null;
            ChallengeService.unit tank = null;
            bool bulletInAir = false;

            string playerName = game.playerName;
            foreach (ChallengeService.player player in game.players)
            {
                if (player.name.Equals(playerName))
                {
                    me = player;
                }
                else
                {
                    enemy = player;
                }
            }
            if (me.units.Length <= tankIndex)
            {
                return null;
            }
            tank = me.units[tankIndex];

            if (me.bullets != null)
            {
                foreach (var bullet in me.bullets)
                {
                    if (Math.Abs(bullet.x - tank.x) < state.Length / 4)
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
