using HarmonyLib;
using MegaCrit.Sts2.Core.Context;
using MegaCrit.Sts2.Core.Entities.Creatures;
using SteamMachineFX.SteamMachineFXCode.LED;

namespace SteamMachineFX.SteamMachineFXCode.Patches;

internal class CreatureHelper
{
    public static void Run(Creature creature)
    {
        var player = creature.Player;
        if (!creature.IsPlayer || player == null || !LocalContext.IsMe(player)) return;
        LEDManager.WriteHealthToLEDs(player.Creature);
    }
}

[HarmonyPatch(typeof(Creature), nameof(Creature.LoseHpInternal))]
public class CreatureLoseHpPatch
{
    [HarmonyPostfix]
    public static void PatchAfter(Creature __instance, ref DamageResult __result)
    {
        CreatureHelper.Run(__instance);
    }
}

[HarmonyPatch(typeof(Creature), nameof(Creature.HealInternal))]
public class CreatureHealPatch
{
    [HarmonyPostfix]
    public static void PatchAfter(Creature __instance)
    {
        CreatureHelper.Run(__instance);
    }
}

[HarmonyPatch(typeof(Creature), nameof(Creature.SetMaxHpInternal))]
public class CreatureMaxHpPatch
{
    [HarmonyPostfix]
    public static void PatchAfter(Creature __instance)
    {
        CreatureHelper.Run(__instance);
    }
}

[HarmonyPatch(typeof(Creature), nameof(Creature.SetCurrentHpInternal))]
public class CreatureCurrentHpPatch
{
    [HarmonyPostfix]
    public static void PatchAfter(Creature __instance)
    {
        CreatureHelper.Run(__instance);
    }
}