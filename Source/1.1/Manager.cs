using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using HarmonyLib;
using HugsLib;
using HugsLib.Settings;
using HugsLib.Utils;
using RimWorld;
using UnityEngine;
using UnityEngine.SceneManagement;
using Verse;
using zhuzi.AdvancedEnergy.Shields.Patch;

namespace zhuzi.AdvancedEnergy.Shields.Shields
{
    class Manager : ModBase
    {
        public static Manager Instance { get; private set; }
        public override string ModIdentifier
        {
            get { return "zzShield"; }
        }
        public Manager()
        {
            Instance = this;
        }


        public override void DefsLoaded()
        {
            InjectUpgradeableStatParts();
        }

        /// <summary>
        /// Add StatPart_Upgradeable to all stats that are used in any CompProperties_Upgrade
        /// </summary>
        private void InjectUpgradeableStatParts()
        {
            try
            {
                var relevantStats = new HashSet<StatDef>();
                var relevantStats2 = new HashSet<StatDef>();
                var allThings = DefDatabase<ThingDef>.AllDefs.ToArray();
                for (var i = 0; i < allThings.Length; i++)
                {
                    var def = allThings[i];
                    if (def.comps.Count > 0)
                    {
                        for (int j = 0; j < def.comps.Count; j++)
                        {
                            var comp = def.comps[j];
                            if (comp is CompProperties_Upgrade upgradeProps)
                            {
                                foreach (var upgradeProp in upgradeProps.statModifiers)
                                {
                                    relevantStats.Add(upgradeProp.stat);
                                }
                                foreach (var upgradeProp in upgradeProps.statModifiersOffset)
                                {
                                    relevantStats2.Add(upgradeProp.stat);
                                }
                            }
                        }
                    }
                }
                foreach (var stat in relevantStats2)
                {
                    var parts = stat.parts ?? (stat.parts = new List<StatPart>());
                    parts.Add(new StatPart_Upgradeable2 { parentStat = stat });
                }
                foreach (var stat in relevantStats)
                {
                    var parts = stat.parts ?? (stat.parts = new List<StatPart>());
                    parts.Add(new StatPart_Upgradeable { parentStat = stat });
                }
            }
            catch (Exception e)
            {
                Logger.ReportException(e);
            }
        }
    }
}
