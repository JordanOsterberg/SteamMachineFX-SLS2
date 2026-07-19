using HarmonyLib;
using MegaCrit.Sts2.Core.Context;
using MegaCrit.Sts2.Core.Runs;
using SteamMachineFX.SteamMachineFXCode.LED;

namespace SteamMachineFX.SteamMachineFXCode.Patches;

internal class RunManagerHelper
{
    private static RunManagerHelper? _instance;
    public static RunManagerHelper Instance => _instance ??= new RunManagerHelper();

    private RunState? _currentRunState;
    
    public void Configure()
    {
        RunManager.Instance.RoomEntered += OnRoomEnter;
        RunManager.Instance.RunStarted += OnRunStarted;
    }

    public void CleanUp()
    {
        RunManager.Instance.RoomEntered -= OnRoomEnter;
        RunManager.Instance.RunStarted -= OnRunStarted;

        _currentRunState = null;
        
        LEDManager.RestoreInitialLEDState();
        SteamMachineFX.Logger.Info("Cleaned up in RunManagerHelper!");
    }

    private void OnRunStarted(RunState state)
    {
        _currentRunState = state;
        Update();
    }

    private void OnRoomEnter()
    {
        Update();
    }
    
    private void Update()
    {
        if (_currentRunState == null) return;
        var state = _currentRunState!;
        
        var localNetId = LocalContext.NetId;
        if (localNetId == null)
        {
            SteamMachineFX.Logger.Error("No NetId from LocalContext.");
            return;
        }

        var player = state.GetPlayer(localNetId.Value);
        if (player == null)
        {
            SteamMachineFX.Logger.Error("Could not find Player from RunManager State.");
            return;
        }
            
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