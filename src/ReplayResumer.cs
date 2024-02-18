using PolytopiaBackendBase.Game;
using Il2CppSystem.Runtime.CompilerServices;
using Il2CppSystem.Threading.Tasks;

namespace PolyMod
{
    internal class ReplayResumer
    {
        public async void ResumeAsHotseatGame()
        {
            // Check that we are in a replay
            ReplayClient replayClient = GameManager.Client as ReplayClient;
            if(replayClient == null)
            {
                Log.Warning("{0} Command used outside of replay game", new Il2CppInterop.Runtime.InteropTypes.Arrays.Il2CppReferenceArray<Il2CppSystem.Object>(new Il2CppSystem.Object[] { "<color=#FFFFFF>[GameManager]</color>" }));
                GameManager.instance.SetLoadingGame(false);
                return;
            }

            // Proceed to create a new hotseat game
            GameManager.instance.SetLoadingGame(true);
            Log.Info("{0} Loading new Hotseat {1} Game from replay", new Il2CppSystem.Object[]
            {
                "<color=#FFFFFF>[GameManager]</color>",
                GameManager.instance.settings.BaseGameMode.ToString()
            });
            GameManager.instance.SetHotseatClient();
            
            // Get the client as a hotseat client
            HotseatClient hotseatClient = GameManager.Client as HotseatClient;
            if(hotseatClient == null)
            {
                Log.Warning("{0} Failed to create Hotseat game", new Il2CppInterop.Runtime.InteropTypes.Arrays.Il2CppReferenceArray<Il2CppSystem.Object>(new Il2CppSystem.Object[] { "<color=#FFFFFF>[GameManager]</color>" }));
                GameManager.instance.SetLoadingGame(false);
                return;
            }
            TaskAwaiter<bool> taskAwaiter = TransformClient(replayClient, hotseatClient).GetAwaiter();
            if (!taskAwaiter.IsCompleted)
            {
                await taskAwaiter;
                TaskAwaiter<bool> taskAwaiter2;
                taskAwaiter = taskAwaiter2;
                taskAwaiter2 = default(TaskAwaiter<bool>);
            }
            if (taskAwaiter.GetResult())
            {
                GameManager.instance.LoadLevel();
            }
        }

        public Il2CppSystem.Threading.Tasks.Task<bool> TransformClient(ReplayClient replayClient, HotseatClient hotseatClient)
        {
            // Mimic the session opening process
            Log.Verbose("{0} Transforming replay session...", new Il2CppSystem.Object[]
                {HotseatClient.LOG_PREFIX,
            });
            hotseatClient.Reset();
            hotseatClient.gameId = Il2CppSystem.Guid.NewGuid();
            hotseatClient.SetSavedSeenCommands(new ushort[replayClient.currentGameState.PlayerStates.Count]);
            hotseatClient.initialGameState = replayClient.initialGameState;
            hotseatClient.currentGameState = replayClient.currentGameState;
            hotseatClient.lastTurnGameState = replayClient.initialGameState;
            hotseatClient.lastSeenCommands = new ushort[replayClient.currentGameState.PlayerStates.Count];
            hotseatClient.currentLocalPlayerIndex = hotseatClient.currentGameState.CurrentPlayerIndex;
            hotseatClient.hasInitializedSaveData = true;
            hotseatClient.UpdateGameStateImmediate(hotseatClient.currentGameState, StateUpdateReason.GameJoined);
            hotseatClient.SaveSession(hotseatClient.gameId, false);
            hotseatClient.PrepareSession();
            return Task.FromResult<bool>(true);            
        }
    }
}