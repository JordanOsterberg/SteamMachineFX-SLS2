using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Modding;
using SteamMachineFX.SteamMachineFXCode.LED;
using SteamMachineFX.SteamMachineFXCode.Patches;

namespace SteamMachineFX.SteamMachineFXCode;

[ModInitializer(nameof(Initialize))]
public partial class SteamMachineFX : Node
{
	public const string ModId = "SteamMachineFX";

	public static MegaCrit.Sts2.Core.Logging.Logger Logger { get; } =
		new(ModId, MegaCrit.Sts2.Core.Logging.LogType.Generic);

	public static void Initialize()
	{
		if (!Directory.Exists("/sys/class/leds"))
		{
			LogWrongLEDsWarning();
			return;
		}
		
		var directories = Directory.GetDirectories("/sys/class/leds", "valve-leds*");
		if (directories.Length <= 0)
		{
			LogWrongLEDsWarning();
			return;
		}

		if (!Directory.Exists("/home/deck/steam-machine-fx-broker/"))
		{
			Logger.Error("Please install SteamMachineFX before using this mod -- https://github.com/JordanOsterberg/SteamMachineFX-Installer");
			return;
		}
		
		Logger.Info($"Found appropriate LED directories (total {directories.Length}), initializing patches & storing current LED state.");
		
		Patch();
				
		LEDManager.StoreInitialLEDState();
	}

	private static void LogWrongLEDsWarning()
	{
		Logger.Warn("Did not find appropriate LED directories, skipping initialization. If you are using a Steam Machine, please report this as a bug.");
	}
	
	private static void Patch()
	{
		Harmony harmony = new(ModId);
		harmony.PatchAll();
	}
}
