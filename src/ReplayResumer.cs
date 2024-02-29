using PolytopiaBackendBase.Game;
using Il2CppSystem.Runtime.CompilerServices;
using Il2CppInterop.Runtime;

namespace PolyMod
{
    internal static class ReplayResumer
    {
        public static void BackToMove()
        {
            if (Plugin.replayClient == null)
            {
                Log.Warning("We don't have a replay to resume");
                return;
            }
            GameManager.instance.client = Plugin.replayClient;
            Plugin.replayClient = null;
            GameManager.instance.LoadLevel();
        }

        public static void Resume()
        {
            ClientBase? replayClient = GameManager.Client;
            if(replayClient == null || replayClient.clientType != ClientBase.ClientType.Replay)
            {
                Log.Warning("{0} Command used outside of replay game, client is {1}", new Il2CppSystem.Object[] { "<color=#FFFFFF>[GameManager]</color>", GameManager.Client.GetType().ToString()});
                GameManager.instance.SetLoadingGame(false);
                return;
            }

            GameManager.instance.SetLoadingGame(true);
            Log.Info("{0} Loading new Hotseat {1} Game from replay", new Il2CppSystem.Object[]
            {
                "<color=#FFFFFF>[GameManager]</color>",
                GameManager.instance.settings.BaseGameMode.ToString()
            });
            
            HotseatClient hotseatClient = SetHotseatClient();
            if(hotseatClient == null)
            {
                Log.Warning("{0} Failed to create Hotseat game", new Il2CppSystem.Object[] { "<color=#FFFFFF>[GameManager]</color>" });
                GameManager.instance.SetLoadingGame(false);
                return;
            }
            Log.Info("{0} Created new Hotseat game", new Il2CppSystem.Object[] { "<color=#FFFFFF>[GameManager]</color>" });
            TaskAwaiter<bool> taskAwaiter = TransformClient(replayClient, hotseatClient).GetAwaiter();
            if (taskAwaiter.GetResult())
            {
                Plugin.replayClient = replayClient;
                GameManager.instance.LoadLevel();
			}
        }

        public static HotseatClient SetHotseatClient()
        {
            GameManager.instance.settings.GameType = GameType.PassAndPlay;
            Log.Info("{0} Setting up hotseat client...", new Il2CppSystem.Object[] { "<color=#FFFFFF>[GameManager]</color>"});
            HotseatClient hotseatClient = new()
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

        public static Il2CppSystem.Threading.Tasks.Task<bool> TransformClient(ClientBase replayClient, HotseatClient hotseatClient)
        {
            GameState replayInitialGameStateCopy;
            byte[] array = SerializationHelpers.ToByteArray(replayClient.initialGameState, replayClient.initialGameState.Version);
            SerializationHelpers.FromByteArray(array, out replayInitialGameStateCopy);
            // Create a new blank PassAndPlay game
            GameSettings gameSettings = new GameSettings{
                BaseGameMode = replayInitialGameStateCopy.Settings.BaseGameMode,
                GameType = GameType.PassAndPlay,
                Difficulty = replayInitialGameStateCopy.Settings.Difficulty,
                OpponentCount = replayInitialGameStateCopy.Settings.OpponentCount,
            };
            for (int i = 0; i < replayInitialGameStateCopy.PlayerStates.Count; i++)
            {
                if (replayInitialGameStateCopy.PlayerStates[i].AutoPlay)
                {
                    PlayerData playerData = new PlayerData
                    {
                        defaultName = replayInitialGameStateCopy.PlayerStates[i].UserName,
                        type = PlayerData.Type.Bot,
                        tribe = replayInitialGameStateCopy.PlayerStates[i].tribe,
                        knownTribe = true
                    };
                    playerData.profile.id = new Il2CppSystem.Guid();
                    gameSettings.AddPlayer(playerData);
                }
                else
                {
                    PlayerData playerData2 = new PlayerData
                    {
                        defaultName = replayInitialGameStateCopy.PlayerStates[i].UserName,
                        type = PlayerData.Type.Local,
                        tribe = replayInitialGameStateCopy.PlayerStates[i].tribe,
                        knownTribe = true,
                        profile = GameManager.GetHotseatProfilesState().players[i]
                    };
                    gameSettings.AddPlayer(playerData2);
                };
            }
            Il2CppSystem.Collections.Generic.List<PlayerState> playerStates = new Il2CppSystem.Collections.Generic.List<PlayerState>();
            for (int i = 0; i < replayInitialGameStateCopy.PlayerStates.Count; i++)
            {
                PlayerData playerData3 = gameSettings.Players[i];
					if (playerData3.type != PlayerData.Type.Bot)
                    {
                        PlayerState playerState4 = new PlayerState
                        {
                            Id = (byte)(i + 1),
                            AccountId = new Il2CppSystem.Nullable<Il2CppSystem.Guid>(playerData3.profile.id),
                            AutoPlay = playerData3.type == PlayerData.Type.Bot,
                            UserName = playerData3.GetNameInternal(),
                            tribe = playerData3.tribe,
                            tribeMix = playerData3.tribeMix,
                            hasChosenTribe = true,
                            skinType = playerData3.skinType
                        };
                        playerStates.Add(playerState4);
                    }
					else
					{
						PlayerState playerState5 = new PlayerState
                        {
                            Id = (byte)(i + 1),
                            AutoPlay = true,
                            UserName = playerData3.GetNameInternal(),
                            tribe = playerData3.tribe,
                            tribeMix = playerData3.tribeMix,
                            hasChosenTribe = true,
                            skinType = playerData3.skinType
                        };
                        playerStates.Add(playerState5);
					}
            }
            GameState initialGameState = new GameState{
                Version = replayInitialGameStateCopy.Version,
                Settings = gameSettings,
                PlayerStates = playerStates,
            };
            GameState lastTurnGameState;
            GameState currentGameState;
            GameState otherCurrentGameState;
            initialGameState.Map = replayInitialGameStateCopy.Map;
            initialGameState.Settings.GameName = initialGameState.Settings.GameName + " from move " + replayClient.GetLastSeenCommand().ToString();
            array = SerializationHelpers.ToByteArray(replayClient.initialGameState, replayClient.initialGameState.Version);
            SerializationHelpers.FromByteArray(array, out currentGameState);
            SerializationHelpers.FromByteArray(array, out lastTurnGameState);
            SerializationHelpers.FromByteArray(array, out otherCurrentGameState);
            for (int i = 0; i < replayClient.GetLastSeenCommand(); i++)
            {
                otherCurrentGameState.CommandStack.Add(replayClient.currentGameState.CommandStack[i]);
            }
            Il2CppSystem.Collections.Generic.List<CommandBase> executedCommands = new Il2CppSystem.Collections.Generic.List<CommandBase>();
            Il2CppSystem.Collections.Generic.List<CommandResultEvent> events = new Il2CppSystem.Collections.Generic.List<CommandResultEvent>();
            string? error;
            ExecuteCommands(currentGameState, otherCurrentGameState.CommandStack, out executedCommands, out events, out error);
            if (error != null)
            {
                Log.Error("{0} Failed to execute commands: {1}", new Il2CppSystem.Object[] { "<color=#FFFFFF>[GameManager]</color>", error });
                return Il2CppSystem.Threading.Tasks.Task.FromResult<bool>(false);
            }
            Log.Info("{0} Transforming replay session...", new Il2CppSystem.Object[]
                {HotseatClient.LOG_PREFIX,
            });
            hotseatClient.Reset();
            hotseatClient.gameId = Il2CppSystem.Guid.NewGuid();
            hotseatClient.SetSavedSeenCommands(new ushort[replayClient.currentGameState.PlayerStates.Count]);
            hotseatClient.initialGameState = initialGameState;
            hotseatClient.currentGameState = currentGameState;
            hotseatClient.lastTurnGameState = initialGameState;
            hotseatClient.lastSeenCommands = new ushort[replayClient.currentGameState.PlayerStates.Count];
            for (int j = 0; j < replayClient.currentGameState.PlayerStates.Count; j++)
            {
                hotseatClient.lastSeenCommands[j] = replayClient.GetLastSeenCommand();
            }
            hotseatClient.currentLocalPlayerIndex = hotseatClient.currentGameState.CurrentPlayerIndex;
            hotseatClient.hasInitializedSaveData = true;
            hotseatClient.UpdateGameStateImmediate(hotseatClient.currentGameState, StateUpdateReason.GameJoined);
            hotseatClient.SaveSession(hotseatClient.gameId.ToString(), false);
            hotseatClient.PrepareSession();
            return Il2CppSystem.Threading.Tasks.Task.FromResult(true);
        }

    	private static bool ExecuteCommands(GameState gameState, Il2CppSystem.Collections.Generic.List<CommandBase> commands, out Il2CppSystem.Collections.Generic.List<CommandBase> executedCommands, out Il2CppSystem.Collections.Generic.List<CommandResultEvent> events, out string? error)
        {
            executedCommands = new Il2CppSystem.Collections.Generic.List<CommandBase>();
            events = new Il2CppSystem.Collections.Generic.List<CommandResultEvent>();
            error = null;
            byte currentPlayer = gameState.CurrentPlayer;
            try
            {
                ActionManager actionManager = new ActionManager(gameState);
                foreach (CommandBase commandBase in commands)
                {
                    GameState.State currentState = gameState.CurrentState;
                    uint currentTurn = gameState.CurrentTurn;
                    if (!actionManager.ExecuteCommand(commandBase, out error))
                    {
                        return false;
                    }
                    executedCommands.Add(commandBase);
                    CommandResultEvent commandResultEvent;
                    if (GameStateUtils.RegisterCommandResultEvent(gameState, currentState, currentTurn, commandBase, out commandResultEvent, false))
                    {
                        events.Add(commandResultEvent);
                    }
                }
                PlayerState playerState;
                while (gameState.TryGetPlayer(gameState.CurrentPlayer, out playerState) && playerState.AutoPlay && gameState.CurrentState != GameState.State.Ended)
                {
                    CommandBase move;
                    if (!CommandTriggerUtils.TryGetTriggerCommand(gameState, out move))
                    {
                        move = AI.GetMove(gameState, playerState, CommandType.None);
                    }
                    GameState.State currentState2 = gameState.CurrentState;
                    uint currentTurn2 = gameState.CurrentTurn;
                    if (!actionManager.ExecuteCommand(move, out error))
                    {
                        throw new System.Exception(string.Format("AI Failed to perform command: {0} with error {1})", move.ToString(), error));
                    }
                    executedCommands.Add(move);
                    CommandResultEvent commandResultEvent2;
                    if (GameStateUtils.RegisterCommandResultEvent(gameState, currentState2, currentTurn2, move, out commandResultEvent2, playerState.Id == currentPlayer))
                    {
                        events.Add(commandResultEvent2);
                    }
                }
            }
            catch (Exception ex)
            {
                error = ex.ToString();
                Console.WriteLine(ex);
                executedCommands = new Il2CppSystem.Collections.Generic.List<CommandBase>();
                return false;
            }
            return true;
        }
    }
}