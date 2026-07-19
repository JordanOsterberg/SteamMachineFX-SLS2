using HarmonyLib;
using MegaCrit.Sts2.Core.Context;
using MegaCrit.Sts2.Core.Multiplayer.Game.Lobby;
using MegaCrit.Sts2.Core.Runs;
using MegaCrit.Sts2.Core.Saves;
using SteamMachineFX.SteamMachineFXCode.LED;

namespace SteamMachineFX.SteamMachineFXCode.Patches;

internal class RunManagerHelper
{
    private static RunManagerHelper? _instance;
    public static RunManagerHelper Instance => _instance ??= new RunManagerHelper();

    private RunState? _currentRunState;

    public void CleanUp()
    {
        RunManager.Instance.RoomEntered -= OnRoomEnter;
        RunManager.Instance.RunStarted -= OnRunStarted;

        _currentRunState = null;
        
        LEDManager.RestoreInitialLEDState();
        SteamMachineFX.Logger.Info("Cleaned up in RunManagerHelper!");
    }

    public void OnRunStarted(RunState state)
    {
        SteamMachineFX.Logger.Info("Received run start event, configuring RunManagerHelper...");
        
        RunManager.Instance.RoomEntered += OnRoomEnter;
        RunManager.Instance.RunStarted += OnRunStarted;
        
        _currentRunState = state;
        
        Update();
    }

    private void OnRoomEnter()
    {
        Update();
    }
    
    private void Update()
    {
        if (_currentRunState == null)
        {
            SteamMachineFX.Logger.Warn("Received RunManagerHelper Update without having a copy of the current run state stored.");
            return;
        }
        var state = _currentRunState!;
        
        var localNetId = LocalContext.NetId;
        if (localNetId == null)
        {
            SteamMachineFX.Logger.Warn("No NetId from LocalContext.");
            return;
        }

        var player = state.GetPlayer(localNetId.Value);
        if (player == null)
        {
            SteamMachineFX.Logger.Error("Could not find Player from RunManager State.");
            return;
        }
        
        SteamMachineFX.Logger.Info("Writing Player health to LEDs via RunManagerHelper Update...");
        LEDManager.WriteHealthToLEDs(player.Creature);
    }
}

[HarmonyPatch(typeof(RunManager), nameof(RunManager.CleanUp))]
class RunManagerCleanUpPatch
{
    [HarmonyPostfix]
    public static void Postfix()
    {
        RunManagerHelper.Instance.CleanUp();
    }
}

[HarmonyPatch(typeof(RunManager), nameof(RunManager.SetUpSavedSingleplayer))]
class RunManagerSetUpSingleplayerPatch
{
    [HarmonyPostfix]
    public static void Postfix(RunState state, SerializableRun save)
    {
        SteamMachineFX.Logger.Info("RunManagerSetUpSingleplayerPatch called");
        RunManagerHelper.Instance.OnRunStarted(state);
    }
}

[HarmonyPatch(typeof(RunManager), nameof(RunManager.SetUpSavedMultiplayer))]
class RunManagerSetUpMultiplayerPatch
{
    [HarmonyPostfix]
    public static void Postfix(RunState state, LoadRunLobby lobby)
    {
        SteamMachineFX.Logger.Info("SetUpMultiplayerPatch called");
        RunManagerHelper.Instance.OnRunStarted(state);
    }
}