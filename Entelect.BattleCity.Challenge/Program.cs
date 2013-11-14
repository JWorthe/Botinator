using System;
using System.ServiceModel;

namespace Entelect.BattleCity.Challenge
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                var endpointConfigurationName = "ChallengePort";
                var address = new EndpointAddress(args[0]);
                var service = new ChallengeService.ChallengeClient(endpointConfigurationName, address);
                var board = service.login();

                var game = new GameInProgress(service, board);
                game.run();
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine("Uncaught exception thrown. Exiting.");
                Console.Error.WriteLine(ex.StackTrace.ToString());
            }
        }
    }
}
