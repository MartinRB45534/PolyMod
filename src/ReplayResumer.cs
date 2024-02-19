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
            ClientBase replayClient = GameManager.Client;
            if(!replayClient.IsReplay)
            {
                Log.Warning("{0} Command used outside of replay game, client is {1}", new Il2CppSystem.Object[] { "<color=#FFFFFF>[GameManager]</color>", GameManager.Client.GetType().ToString()});
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
            
            // Get the new hotseat client
            HotseatClient hotseatClient = SetHotseatClient();
            if(hotseatClient == null)
            {
                Log.Warning("{0} Failed to create Hotseat game", new Il2CppSystem.Object[] { "<color=#FFFFFF>[GameManager]</color>" });
                GameManager.instance.SetLoadingGame(false);
                return;
            }
            TaskAwaiter<bool> taskAwaiter = TransformClient(replayClient, hotseatClient).GetAwaiter();
            if (taskAwaiter.GetResult())
            {
                GameManager.instance.LoadLevel();
                MakeResumePopup();
            }
        }

        public HotseatClient SetHotseatClient()
        {
            GameManager.instance.settings.GameType = GameType.PassAndPlay;
            Log.Verbose("{0} Setting up hotseat client...", new Il2CppSystem.Object[] { "<color=#FFFFFF>[GameManager]</color>"});
            HotseatClient hotseatClient = new HotseatClient
            {
                OnConnected = new Action(GameManager.instance.OnLocalClientConnected),
                OnDisconnected = new Action(GameManager.instance.OnClientDisconnected),
                OnSessionOpened = new Action(GameManager.instance.OnClientSessionOpened),
                OnStateUpdated = new Action<StateUpdateReason>(GameManager.instance.OnStateUpdated),
                OnStartedProcessingActions = new Action(GameManager.instance.OnStartedProcessingActions),
                OnFinishedProcessingActions = new Action(GameManager.instance.OnFinishedProcessingActions)
            };
            GameManager.instance.client = hotseatClient;
            return hotseatClient;
        }

        public Il2CppSystem.Threading.Tasks.Task<bool> TransformClient(ClientBase replayClient, HotseatClient hotseatClient)
        {
            // Modify the gameState as needed
            replayClient.initialGameState.Settings.GameType = GameType.PassAndPlay;
            replayClient.currentGameState.Settings.GameType = GameType.PassAndPlay;
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
            return Il2CppSystem.Threading.Tasks.Task.FromResult<bool>(true);            
        }

        public void MakeResumePopup()
        {
            // Make a popup to tell the user that the game is resuming from replay
            BasicPopup popup = PopupManager.GetBasicPopup(new PopupManager.BasicPopupData{
                header = "Resuming from Replay",
                description = "The game has been turned from a replay into a hotseat game. You can now continue playing.",
                buttonData = new PopupBase.PopupButtonData[]
                {
                    new PopupBase.PopupButtonData("buttons.ok", PopupBase.PopupButtonData.States.Selected, null, -1, true, null)
                }
            });
            popup.Show();
        }
    }
}