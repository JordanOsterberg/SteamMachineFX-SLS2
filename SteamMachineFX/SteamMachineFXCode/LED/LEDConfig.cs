namespace SteamMachineFX.SteamMachineFXCode
{
    public sealed class LEDConfig
    {
        public bool Enabled { get; set; }
        public Dictionary<string, LEDSettings> Leds { get; set; } = new();

        public override string ToString()
        {
            return $"Enabled={Enabled}\nLeds={Leds.Count}";
        }
    }
    
    public sealed class LEDSettings
    {
        public int Brightness { get; set; }
        public int[] Color { get; set; } = [255, 0, 0];
        public string Effect { get; set; } = "manual";

        public override string ToString()
        {
            return $"Brightness={Brightness}\nColor={Color}\nEffect={Effect}";
        }
    }
}