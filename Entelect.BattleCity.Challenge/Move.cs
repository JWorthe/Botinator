using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Entelect.BattleCity.Challenge
{
    class Move
    {
        public int Tank { get; private set; }
        public ChallengeService.action Action { get; private set; }

        public Move(int tank, ChallengeService.action action)
        {
            Tank = tank;
            Action = action;
        }
    }
}
