using Polytopia.Data;
using Il2CppSystem.Runtime.CompilerServices;

namespace PolyMod
{
    internal class ReplayResumer
    {
        public void ResumeAsHotseatGame()
        {
            // Check that we are in a replay
            if(!GameManager.Client.IsReplay)
            {
                return;
            }
            // Get the current gamestate and remove the upcoming commands
            GameState gameState = GameManager.Client.GameState;
            gameState.CommandStack = gameState.CommandStack.GetRange(0, gameState.LastProcessedCommand);
            // Proceed to create a new hotseat game
            GameManager.instance.SetLoadingGame(true);
            Log.Info("{0} Starting new Hotseat {1} Game...", new Il2CppSystem.Object[]
            {
                "<color=#FFFFFF>[GameManager]</color>",
                GameManager.instance.settings.BaseGameMode.ToString()
            });
            GameManager.instance.SetHotseatClient();
            TaskAwaiter<CreateSessionResult> taskAwaiter = GameManager.instance.client.CreateSession(GameManager.instance.settings, null).GetAwaiter();
            if (taskAwaiter.GetResult() == CreateSessionResult.Success)
            {
                GameManager.instance.LoadLevel();
            }
            else
            {
                Log.Warning("{0} Failed to create Hotseat game", new Il2CppInterop.Runtime.InteropTypes.Arrays.Il2CppReferenceArray<Il2CppSystem.Object>(new Il2CppSystem.Object[] { "<color=#FFFFFF>[GameManager]</color>" }));
                GameManager.instance.SetLoadingGame(false);
            }
        }
    }
}