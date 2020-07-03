using HugsLib.Utils;
using RimWorld;
using UnityEngine;
using Verse;

namespace zhuzi.AdvancedEnergy.Shields.Shields
{
    /// <summary>
    /// Auto-filled repository of all external resources referenced in the code
    /// </summary>
    public static class Resources
    {
        [DefOf]
        public static class Job
        {
            public static JobDef zzInstallUpgrade;
        }


        [DefOf]
        public static class Stat
        {
            public static StatDef zzShieldEnergyMax;
            public static StatDef zzShieldRechargePerSec;
            public static StatDef zzShieldRadius;
            public static StatDef zzShieldWarmupDelay;
            public static StatDef zzShieldEnergyInit;
            public static StatDef zzShieldHurtRate;
            public static StatDef zzShieldHurtRate_EMP;
            public static StatDef zzShieldHurtRate_Flame;
            public static StatDef zzShieldHurtRate_AOE;
            public static StatDef zzShieldHurtRateExtra_EMP;
            public static StatDef zzShieldDefenceBullet;
            public static StatDef zzShieldDefenceProjectile;
            public static StatDef zzShieldDefenceSky;
            public static StatDef zzShieldDefenceIFF;
            public static StatDef zzPowerConsumption;
            public static StatDef zzPowerConsumptionRate;
            public static StatDef zzPowerConsumptionCache;
        }

        [StaticConstructorOnStartup]
        public static class Textures
        {

            public static Texture2D zzUIUpgrade;

            //UI elements - Unsaved
            public static Texture2D UI_DIRECT_ON;
            public static Texture2D UI_DIRECT_OFF;

            public static Texture2D UI_INDIRECT_ON;
            public static Texture2D UI_INDIRECT_OFF;

            public static Texture2D UI_INTERCEPT_DROPPOD_ON;
            public static Texture2D UI_INTERCEPT_DROPPOD_OFF;

            public static Texture2D UI_SHOW_ON;
            public static Texture2D UI_SHOW_OFF;

            static Textures()
            {

                foreach (var fieldInfo in typeof(Textures).GetFields(HugsLibUtility.AllBindingFlags))
                {
                    if (fieldInfo.IsInitOnly) continue;
                    fieldInfo.SetValue(null, ContentFinder<Texture2D>.Get(fieldInfo.Name));
                }
            }

        }

        [DefOf]
        public static class Designation
        {
            public static DesignationDef zzInstallUpgrade;
        }

        [DefOf]
        public static class StatCategory
        {
            public static StatCategoryDef zzShield_stat;
        }

        [StaticConstructorOnStartup]
        public static class Materials
        {
            public static readonly Material ConeMaterial = MaterialPool.MatFrom("Other/ForceFieldCone", ShaderDatabase.MoteGlow);
            public static readonly Material ShieldMaterial = MaterialPool.MatFrom("Things/ShieldBubbleSOS", ShaderDatabase.MoteGlow);
        }

    }
}
