using MegaCrit.Sts2.Core.Entities.Creatures;

namespace SteamMachineFX.SteamMachineFXCode.LED;

public sealed class LEDManager
{
    private static List<LEDState> _initialLedStates = [];
    
    public static void StoreInitialLEDState()
    {
        _initialLedStates = LEDState.SaveState();
    }

    public static void RestoreInitialLEDState()
    {
        LEDState.WriteStates(_initialLedStates);

        Task.Run(async () =>
        {
            await Task.Delay(300); // Wait 300ms so we'll have waited at least long enough for the last update to have been written
            LEDState.DisableLEDMonitor();
        });
    }

    public static void WriteHealthToLEDs(Creature creature)
    {
        SteamMachineFX.Logger.Info($"Received new health: {creature.CurrentHp} / {creature.MaxHp}");

        var percentage = (float)creature.CurrentHp / creature.MaxHp;
        WritePercentageToLEDs(percentage);
    }

    private static void WritePercentageToLEDs(float percentage)
    {
        var totalLEDs = _initialLedStates.Count;
        var litLEDs = (int)MathF.Ceiling(percentage * totalLEDs);

        SteamMachineFX.Logger.Info($"Writing percentage {percentage} across {totalLEDs} LEDs...");

        var states = new List<LEDState>();
        for (var i = 0; i < totalLEDs; i++)
        {
            var led = _initialLedStates[i];

            SteamMachineFX.Logger.Info($"{led.Path} / {led.Number} will be {(i < litLEDs ? "255" : "1")}");

            var newState = new LEDState
            {
                Number = led.Number,
                Path = led.Path,
                Brightness = i < litLEDs ? "255" : "0",
                Effect = "manual",
                MultiIntensity = i < litLEDs ? "255 0 0" : "0 0 0"
            };
            states.Add(newState);
        }

        LEDState.WriteStates(states);
    }
}