﻿using System;
using System.Collections.Generic;
using RimWorld;
using Verse;
using HarmonyLib;

namespace zhuzi.AdvancedEnergy.Shields.Patch
{
    [StaticConstructorOnStartup]
    internal class Patcher
    {
        static Patcher()
        {
            string _LogLocation = "zhuzi.AdvancedEnergy.Shields.Patch.Patcher(): ";

            Log.Message(_LogLocation + "Starting.");

            //Create List of Patches
            List<Patch> _Patches = new List<Patch>();
            _Patches.Add(new Patches.PatchProjectile());

            //Create Harmony Instance
            Harmony _Harmony = new HarmonyLib.Harmony("zhuzi.AdvancedEnergy.Shields");

            
            //Iterate Patches
            _Patches.ForEach(p => p.ApplyPatchIfRequired(_Harmony));

            Log.Message(_LogLocation + "Complete.");
        }

        /// <summary>
        /// Debug Logging Helper
        /// </summary>
        /// <param name="objectToTest"></param>
        /// <param name="name"></param>
        /// <param name="logSucess"></param>
        public static void LogNULL(object objectToTest, String name, bool logSucess = false)
        {
            if (objectToTest == null)
            {
                Log.Error(name + " Is NULL.");
            }
            else if (logSucess)
            {
                Log.Message(name + " Is Not NULL.");
            }
        }

    }

}
