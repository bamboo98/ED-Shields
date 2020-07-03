using HarmonyLib;
using RimWorld;
using Verse;
using zhuzi.AdvancedEnergy.Shields.Shields;
namespace zhuzi.AdvancedEnergy.Shields.Patch
{
    /// <summary>
    /// Allows types extending CompPowerTrader to be recognized as power grid connectables
    /// </summary>
    [HarmonyPatch(typeof(ThingDef))]
    [HarmonyPatch("ConnectToPower", MethodType.Getter)]
    internal class ThingDef_ConnectToPower_Patch
    {
        [HarmonyPostfix]
        public static void AllowPolymorphicComps(ThingDef __instance, ref bool __result)
        {
            //Log.Message("patched!");
            if (!__instance.EverTransmitsPower)
            {
                for (var i = 0; i < __instance.comps.Count; i++)
                {
                    if (typeof(CompPowerTrader).IsAssignableFrom(__instance.comps[i]?.compClass))
                    {
                        __result = true;
                        return;
                    }
                }
            }
        }
    }

    [HarmonyPatch(typeof(StatWorker), "ShouldShowFor")]
    static class StatWorker_ShouldShowFor
    {
        static bool Prefix(StatWorker __instance, StatRequest req, ref bool __result, ref StatDef ___stat)
        {
            if (___stat.category == Resources.StatCategory.zzShield_stat && req.Thing is Building_Shield)
            {
                __result = true;
                return false;
            }
            return true;
        }
    }
}
