using System.Text.Json;
using System.Text.RegularExpressions;

namespace SteamMachineFX.SteamMachineFXCode;

public sealed class LEDState
{
    public string Number { get; init; } = "";
    public string Path { get; init; } = "";
    public string Brightness { get; init; } = "";
    public string MultiIntensity { get; init; } = "";
    public string Effect { get; init; } = "";
        
    // MARK: - State Reading
        
    public static List<LEDState> SaveState()
    {
        return Directory.GetDirectories("/sys/class/leds", "valve-leds*")
            .Select(StateFromPath)
            .Where(state => state != null)
            .Select(state => state!)
            .OrderBy(state => int.Parse(state.Number))
            .Reverse()
            .ToList();
    }
        
    private static readonly Regex LEDNumberingRegex =
        new Regex(@"valve-leds\[(\d+)\]$");

    private static LEDState? StateFromPath(string path)
    {
        var name = System.IO.Path.GetFileName(path);
        var match = LEDNumberingRegex.Match(name);

        if (!match.Success)
        {
            SteamMachineFX.Logger.Warn($"Could not parse LED number from {name}");
            return null;
        }

        var number = int.Parse(match.Groups[1].Value);
                    
        return new LEDState
        {
            Number = number.ToString(),
            Path = path,
            Brightness = File.ReadAllText(System.IO.Path.Combine(path, "brightness")),
            MultiIntensity = File.ReadAllText(System.IO.Path.Combine(path, "multi_intensity")),
            Effect = File.ReadAllText(System.IO.Path.Combine(path, "effect"))
        };
    }
        
    // MARK: - State Writing
        
    public static void WriteStates(IEnumerable<LEDState> states)
    {
        var settings = new Dictionary<string, LEDSettings>();
            
        foreach (var led in states)
        {
            settings.Add(led.Number, new LEDSettings
            {
                Brightness = int.Parse(led.Brightness),
                Color = led.MultiIntensity.Split(" ").Select(int.Parse).ToArray(),
                Effect = led.Effect,
            });
        }
            
        WriteConfigToFile(new LEDConfig
        {
            Enabled = true,
            Leds = settings
        });
    }

    public static void DisableLEDMonitor()
    {
        WriteConfigToFile(new LEDConfig
        {
            Enabled = false,
            Leds = new Dictionary<string, LEDSettings>()
        });
    }

    private static void WriteConfigToFile(LEDConfig config)
    {
        try
        {
            const string filePath = "/home/deck/steam-machine-fx-broker/leds.json";

            var options = new JsonSerializerOptions
            {
                WriteIndented = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };

            var json = JsonSerializer.Serialize(config, options);
            File.WriteAllText(filePath, json);
        }
        catch (Exception e)
        {
            SteamMachineFX.Logger.Error("Failed to write updated leds.json with exception");
            SteamMachineFX.Logger.Error(e.ToString());
        }
    }
}