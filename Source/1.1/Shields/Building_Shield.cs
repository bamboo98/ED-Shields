using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEngine;

using RimWorld;
using Verse;
using Verse.Sound;

using zhuzi.AdvancedEnergy.Shields.Patch;

//using EnhancedDevelopment.Shields.ShieldUtils;

namespace zhuzi.AdvancedEnergy.Shields.Shields
{
    [StaticConstructorOnStartup]
    public class Building_Shield : Building
    {

        private CompPowerTrader powerComp;


        #region Methods


        public bool PowerOn
        {
            get { return powerComp == null || powerComp.PowerOn; }
        }

        public override void SpawnSetup(Map map, bool respawningAfterLoad)
        {
            base.SpawnSetup(map, respawningAfterLoad);
            powerComp = GetComp<CompPowerTrader>();
            //Log.Message("电源组件获取" + (powerComp == null ? "失败" : PowerOn.ToString()));
        }

        public override string GetInspectString()
        {
            return this.GetComp<Comp_ShieldGenerator>().CompInspectStringExtra();
        }

        public bool WillInterceptDropPod(DropPodIncoming dropPodToCheck)
        {
            return this.GetComp<Comp_ShieldGenerator>().WillInterceptDropPod(dropPodToCheck);
        }

        public bool WillProjectileBeBlocked(Projectile projectileToCheck)
        {
            return this.GetComp<Comp_ShieldGenerator>().WillProjectileBeBlocked(projectileToCheck);
        }

        //public void TakeDamageFromProjectile(Projectile projectile)
        //{
        //    this.GetComp<Comp_ShieldGenerator>().HitShield(projectile);

        //}

        public void RecalculateStatistics()
        {
            //Log.Message("Calculate");
            this.GetComp<Comp_ShieldGenerator>().RecalculateStatistics();
        }               
        
        #endregion //Methods

    }
}