using System.ServiceModel;

namespace Entelect.BattleCity.Challenge
{
    class Program
    {
        static void Main(string[] args)
        {
            var endpointConfigurationName = "ChallengePort";
            var address = new EndpointAddress(args[0]);
            var service = new ChallengeService.ChallengeClient(endpointConfigurationName, address);
            var board = service.login();

            var game = new GameInProgress(service, board);
            game.run();
        }
    }
}
