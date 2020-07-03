using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;
using Verse.Sound;
using RimWorld;
using UnityEngine;
using System.Reflection;

namespace zhuzi.AdvancedEnergy.Shields.Shields
{

    [StaticConstructorOnStartup]
    public class Comp_ShieldGenerator : ThingComp
    {

        //Material currentMatrialColour;

        public CompProperties_ShieldGenerator Properties;

        #region Variables


        //stat
        private float zzShieldRechargePerSec;
        public int ShieldRadiusMax { get; private set; }

        private int zzShieldWarmupDelay;
        private int zzShieldEnergyInit;

        private float zzShieldHurtRate;
        private float zzShieldHurtRate_EMP;
        private float zzShieldHurtRate_Flame;
        private float zzShieldHurtRate_AOE;
        private float zzShieldHurtRateExtra_EMP;

        private bool zzShieldDefenceBullet;
        private bool zzShieldDefenceProjectile;
        private bool zzShieldDefenceSky;
        private bool zzShieldDefenceIFF;

        public float zzPowerConsumptionCache;


        //save field
        private float zzShieldEnergy_Current;
        public int zzShieldRadius_Current = 4;//开放给UI
        private int zzShieldWarmupTicks;
        private bool zzShieldShowVisually = true;
        private bool zzShieldDefenceBulletActive = true;
        private bool zzShieldDefenceProjectileActive;
        private bool zzShieldDefenceSkyActive;
        private bool zzShieldDefenceIFFActive;


        //other
        private static readonly MaterialPropertyBlock PropBlock = new MaterialPropertyBlock();
        private int lastIntercepted = -69;
        private float lastInterceptAngle;


        public EnumShieldStatus CurrentStatus { get; set; } = EnumShieldStatus.Offline;


        #endregion Variables

        #region Settings


        //ParentComp.
        private CompStatPowerIdle CompPower;


        //public setting
        public int ShieldRadius_Current
        {
            get {
                return Math.Min(zzShieldRadius_Current, ShieldRadiusMax);
            }
        }

        public bool ShieldDefenceBulletInstall
        {
            get
            {
                return zzShieldDefenceBullet;
            }
        }

        public bool ShieldDefenceProjectileInstall
        {
            get
            {
                return zzShieldDefenceProjectile;
            }
        }

        public bool ShieldDefenceSkyInstall
        {
            get
            {
                return zzShieldDefenceSky;
            }
        }
        public bool ShieldDefenceIFFInstall
        {
            get
            {
                return zzShieldDefenceIFF;
            }
        }

        public bool ShieldDefenceBulletActive {
            get
            {
                return zzShieldDefenceBullet && zzShieldDefenceBulletActive;
            }
        }

        public bool ShieldDefenceProjectileActive
        {
            get
            {
                return zzShieldDefenceProjectile && zzShieldDefenceProjectileActive;
            }
        }

        public bool ShieldDefenceSkyActive
        {
            get
            {
                return zzShieldDefenceSky && zzShieldDefenceSkyActive;
            }
        }
        public bool ShieldDefenceIFFActive
        {
            get
            {
                return zzShieldDefenceIFF && zzShieldDefenceIFFActive;
            }
        }

        public float ShieldEnergyCurrent
        {
            get
            {
                return zzShieldEnergy_Current;
            }
            set
            {
                if (value < 0)
                {
                    CurrentStatus = EnumShieldStatus.Offline;
                    zzShieldEnergy_Current = 0;
                }
                else if (value > ShieldEnergyMax)
                {
                    zzShieldEnergy_Current = ShieldEnergyMax;
                }
                else
                {
                    zzShieldEnergy_Current = value;
                }
            }
        }

        public int ShieldEnergyMax { get; private set; }

        public bool ShieldOnline
        {
            get
            {
                return CurrentStatus != EnumShieldStatus.Offline && CurrentStatus != EnumShieldStatus.Initilising;
            }
        }

        #endregion

        #region Initilisation

        //Static Construtor
        static Comp_ShieldGenerator()
        {
        }

        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            base.PostSpawnSetup(respawningAfterLoad);

            this.Properties = ((CompProperties_ShieldGenerator)this.props);
            //关掉电源模块的自动休眠检查
            this.CompPower = this.parent.GetComp<CompStatPowerIdle>();
            CompPower._disableAutoCheck = true;
            this.RecalculateStatistics();
        }

        public override void ReceiveCompSignal(string signal)
        {
            //Log.Message("power接收到信号" + signal);
            base.ReceiveCompSignal(signal);
            //属性升级要重算一下
            if (signal == CompUpgrade.UpgradeCompleteSignal) RecalculateStatistics();
        }

        public void RecalculateStatistics()
        {
            //Log.Message("RecalculateStatistics");


            ShieldEnergyMax = Mathf.FloorToInt(parent.GetStatValue(Resources.Stat.zzShieldEnergyMax));
            zzShieldRechargePerSec = parent.GetStatValue(Resources.Stat.zzShieldRechargePerSec);
            ShieldRadiusMax = Mathf.FloorToInt(parent.GetStatValue(Resources.Stat.zzShieldRadius));

            zzShieldWarmupDelay = parent.GetStatValue(Resources.Stat.zzShieldWarmupDelay).SecondsToTicks();
            zzShieldEnergyInit = Mathf.FloorToInt(parent.GetStatValue(Resources.Stat.zzShieldEnergyInit));

            zzShieldHurtRate = parent.GetStatValue(Resources.Stat.zzShieldHurtRate);
            zzShieldHurtRate_EMP = parent.GetStatValue(Resources.Stat.zzShieldHurtRate_EMP);
            zzShieldHurtRate_Flame = parent.GetStatValue(Resources.Stat.zzShieldHurtRate_Flame);
            zzShieldHurtRate_AOE = parent.GetStatValue(Resources.Stat.zzShieldHurtRate_AOE);
            zzShieldHurtRateExtra_EMP = parent.GetStatValue(Resources.Stat.zzShieldHurtRateExtra_EMP);



            //Field Settings

            //Mode Settings - Avalable
            zzShieldDefenceBullet = Mathf.FloorToInt(parent.GetStatValue(Resources.Stat.zzShieldDefenceBullet)) != 0;
            zzShieldDefenceProjectile = Mathf.FloorToInt(parent.GetStatValue(Resources.Stat.zzShieldDefenceProjectile))!=0;
            zzShieldDefenceSky = Mathf.FloorToInt(parent.GetStatValue(Resources.Stat.zzShieldDefenceSky)) != 0;
            zzShieldDefenceIFF = Mathf.FloorToInt(parent.GetStatValue(Resources.Stat.zzShieldDefenceIFF)) != 0;


            //Recovery Settings

            //Power converter
            zzPowerConsumptionCache = parent.GetStatValue(Resources.Stat.zzPowerConsumptionCache);

        }

        #endregion Initilisation

        #region Methods

        public override void CompTick()
        {
            base.CompTick();

            //this.RecalculateStatistics();

            this.UpdateShieldStatus();

            this.TickRecharge();

        }

        public void UpdateShieldStatus()
        {
            Boolean _PowerAvalable = this.CheckPowerOn();

            switch (this.CurrentStatus)
            {

                case (EnumShieldStatus.Offline):

                    //If it is offline bit has Power start initialising
                    if (_PowerAvalable)
                    {
                        this.CurrentStatus = EnumShieldStatus.Initilising;
                        zzShieldWarmupTicks = this.zzShieldWarmupDelay;
                    }
                    break;

                case (EnumShieldStatus.Initilising):
                    if (_PowerAvalable)
                    {
                        if (zzShieldWarmupTicks > 0)
                        {
                            zzShieldWarmupTicks--;
                        }
                        else
                        {
                            this.CurrentStatus = EnumShieldStatus.ActiveCharging;
                            this.ShieldEnergyCurrent = (float)this.zzShieldEnergyInit;
                        }
                    }
                    else
                    {
                        this.CurrentStatus = EnumShieldStatus.Offline;
                    }
                    break;

                case (EnumShieldStatus.ActiveDischarging):
                    if (_PowerAvalable)
                    {
                        this.CurrentStatus = EnumShieldStatus.ActiveCharging;
                    }
                    else
                    {
                        if (this.ShieldEnergyCurrent <= 0)
                        {
                            this.CurrentStatus = EnumShieldStatus.Offline;

                        }
                    }
                    break;

                case (EnumShieldStatus.ActiveCharging):
                    if (this.ShieldEnergyCurrent < 0)
                    {
                        this.CurrentStatus = EnumShieldStatus.Offline;
                    }
                    else
                    {
                        if (!_PowerAvalable)
                        {
                            this.CurrentStatus = EnumShieldStatus.ActiveDischarging;
                        }
                        else if (this.ShieldEnergyCurrent >= this.ShieldEnergyMax)
                        {
                            this.CurrentStatus = EnumShieldStatus.ActiveSustaining;
                        }
                    }
                    break;

                case (EnumShieldStatus.ActiveSustaining):
                    if (!_PowerAvalable)
                    {
                        this.CurrentStatus = EnumShieldStatus.ActiveDischarging;
                    }
                    else
                    {
                        if (this.ShieldEnergyCurrent < this.ShieldEnergyMax)
                        {
                            this.CurrentStatus = EnumShieldStatus.ActiveCharging;
                        }
                    }
                    break;
            }
            //修正功耗,如果没有安装空闲电源则不会生效
            if (CompPower._powerConsumptionRate != 0.2f && CurrentStatus == EnumShieldStatus.ActiveSustaining)
                CompPower._powerConsumptionRate = 0.2f;
            else if (CompPower._powerConsumptionRate != 1.5f && CurrentStatus != EnumShieldStatus.ActiveSustaining)
                CompPower._powerConsumptionRate = 1.5f;
        }

        public void HitShield(Projectile proj)
        {

            this.lastInterceptAngle = Vector3Utility.AngleToFlat(proj.DrawPos, GenThing.TrueCenter(this.parent));
            this.lastIntercepted = Find.TickManager.TicksGame;
            float dmg = proj.DamageAmount;
            if (proj.def.projectile.damageDef == DamageDefOf.EMP)
            {
                dmg *= zzShieldHurtRate_EMP;
                dmg += ShieldEnergyCurrent * zzShieldHurtRateExtra_EMP;
            }
            else if (proj.def.projectile.damageDef == DamageDefOf.Flame)
                dmg *= zzShieldHurtRate_Flame;

            dmg = Mathf.Max(0, dmg*zzShieldHurtRate);

            //判定为爆炸弹丸
            if (proj.def.projectile.explosionRadius != 0)
            {
                //受到伤害取决于弹丸威力*伤害半径^2
                dmg *= Mathf.Max(1f, proj.def.projectile.explosionRadius * proj.def.projectile.explosionRadius) * zzShieldHurtRate_AOE;

                MoteMaker.ThrowMicroSparks(this.parent.DrawPos, this.parent.Map);
                GenExplosion.DoExplosion(proj.Position, this.parent.Map, Mathf.Min(proj.def.projectile.explosionRadius * 0.66f, 3f),
                    DefDatabase<DamageDef>.GetNamed("zz_ShieldExplosion", true),
                    null, -1, -1f, null, null, null, null, null, 0f, 1, false, null, 0f, 1, 0f, false, null, null);
                //Log.Message("爆炸伤害:"+ dmg * proj.def.projectile.explosionRadius+" = "+ dmg+" * "+ proj.def.projectile.explosionRadius);
            }
            //判定为普通弹丸
            else
            {

                //还用旧的伤害效果
                //On hit effects
                MoteMaker.ThrowLightningGlow(proj.ExactPosition, this.parent.Map, 0.5f);
                //On hit sound
                ShieldManagerMapComp.HitSoundDef.PlayOneShot((SoundInfo)new TargetInfo(proj.Position, proj.Map, false));
                //Log.Message("子弹伤害:" + dmg );

            }
            ShieldEnergyCurrent -= dmg;

            proj.Destroy();
        }

        public bool IsActive()
        {
            //return true;
            return (CurrentStatus == EnumShieldStatus.ActiveCharging ||
                 CurrentStatus == EnumShieldStatus.ActiveDischarging ||
                 CurrentStatus == EnumShieldStatus.ActiveSustaining);
        }

        public bool CheckPowerOn()
        {
            return !(parent is Building_Shield par) ? false : par.PowerOn;
        }

        public void TickRecharge()
        {
                if (CurrentStatus == EnumShieldStatus.ActiveCharging)
                {
                ShieldEnergyCurrent += Mathf.Min(ShieldEnergyMax, 1000f) / 60f * zzShieldRechargePerSec;
                }
                else if (CurrentStatus == EnumShieldStatus.ActiveDischarging)
                {
                    ShieldEnergyCurrent-= (float)ShieldEnergyMax / 60f * zzPowerConsumptionCache ;
                }
        }

        public bool WillInterceptDropPod(DropPodIncoming dropPodToCheck)
        {
            //Active and Online
            if (!ShieldDefenceSkyActive || !ShieldOnline)
            {
                return false;
            }

            //Check IFF
            if (ShieldDefenceIFFActive)
            {
                bool _Hostile = dropPodToCheck.Contents.innerContainer.Any(x => x.Faction==null || x.Faction.AllyOrNeutralTo(Faction.OfPlayer));

                if (!_Hostile)
                {
                    return true;
                }
            }

            //Check Distance
            float _Distance = Vector3.Distance(dropPodToCheck.Position.ToVector3(), parent.Position.ToVector3());
            float _Radius = ShieldRadius_Current;
            if (_Distance > _Radius)
            {
                return false;
            }

            return true;

        }

        public bool WillProjectileBeBlocked(Projectile projectile)
        {

            //Active and Online
            if (!ShieldOnline)
            {
                return false;
            }

            //Check if can and wants to intercept
            if (projectile.def.projectile.flyOverhead)
            {
                if (!ShieldDefenceProjectileActive) { return false; }
            }
            else
            {
                if (!ShieldDefenceBulletActive) { return false; }
            }

            //Check Distance
            float _Distance = Vector3.Distance(projectile.Position.ToVector3(), this.parent.Position.ToVector3());
            if (_Distance > ShieldRadius_Current)
            {
                return false;
            }

            //Check Angle
            if (!Comp_ShieldGenerator.CorrectAngleToIntercept(projectile, this.parent))
            {
                return false;
            }

            //Check IFF
            if (ShieldDefenceIFFActive)
            {
                FieldInfo _LauncherFieldInfo = typeof(Projectile).GetField("launcher", BindingFlags.NonPublic | BindingFlags.Instance);
                if (_LauncherFieldInfo == null) return false;

                Thing _Launcher = (Thing)_LauncherFieldInfo.GetValue(projectile);
                if (_Launcher == null || _Launcher.Faction==null) return false;

                if (_Launcher.Faction.IsPlayer)
                {
                    return false;
                }
            }

            return true;

        }

        public static Boolean CorrectAngleToIntercept(Projectile pr, Thing shieldBuilding)
        {
            //Detect proper collision using angles
            Quaternion targetAngle = pr.ExactRotation;

            Vector3 projectilePosition2D = pr.ExactPosition;
            projectilePosition2D.y = 0;

            Vector3 shieldPosition2D = shieldBuilding.Position.ToVector3();
            shieldPosition2D.y = 0;

            Quaternion shieldProjAng = Quaternion.LookRotation(projectilePosition2D - shieldPosition2D);

            if ((Quaternion.Angle(targetAngle, shieldProjAng) > 90))
            {
                return true;
            }

            return false;
        }

        #endregion Methods

        #region Properties



        #endregion Properties

        #region Drawing

        public float Alpha()
        {
            if (this.IsActive())
            {
                float baseAlpha;
                if (Find.Selector.IsSelected(this.parent))
                {
                    baseAlpha = Mathf.Lerp(0.5f, 1f, (Mathf.Sin((float)(Gen.HashCombineInt(this.parent.thingIDNumber, 42069) % 100) + Time.realtimeSinceStartup * 2f) + 1f) / 2f);
                }
                else
                {
                    baseAlpha = Mathf.Lerp(0.25f, 0.75f, (Mathf.Sin((float)(Gen.HashCombineInt(this.parent.thingIDNumber, 69420) % 100) + Time.realtimeSinceStartup * 0.7f) + 1f) / 2f);
                }
                int num = Find.TickManager.TicksGame - this.lastIntercepted;
                float interceptAlpha = Mathf.Clamp01(1f - (float)num / 120f) * 0.99f;
                return Mathf.Max(baseAlpha, interceptAlpha);
            }
            if (!Find.Selector.IsSelected(this.parent))
            {
                return 0f;
            }
            return 0.5f;
        }
        private float HitConeAlpha()
        {
            int num = Find.TickManager.TicksGame - this.lastIntercepted;
            return Mathf.Clamp01(1f - (float)num / 42f) * 0.99f;
        }

        public override void PostDraw()
        {
            //Log.Message("DrawComp");
            base.PostDraw();

            if (!IsActive() || !zzShieldShowVisually)
            {
                return;
            }
            Vector3 pos = this.parent.Position.ToVector3Shifted();
            pos.y = Altitudes.AltitudeFor(AltitudeLayer.MoteOverhead);
            float alpha = this.Alpha();
            int radius = ShieldRadius_Current;
            if (alpha > 0f)
            {
                Color value = Color.white;
                value.a *= alpha;
                Comp_ShieldGenerator.PropBlock.SetColor(ShaderPropertyIDs.Color, value);
                Matrix4x4 matrix = default(Matrix4x4);
                matrix.SetTRS(pos, Quaternion.identity, new Vector3(radius * 2.1f, 1f, radius * 2.1f));
                Graphics.DrawMesh(MeshPool.plane10, matrix, Resources.Materials.ShieldMaterial, 0, null, 0, Comp_ShieldGenerator.PropBlock);
            }
            float coneAlpha = this.HitConeAlpha();
            if (coneAlpha > 0f)
            {
                Color color = Color.white;
                color.a *= coneAlpha;
                Comp_ShieldGenerator.PropBlock.SetColor(ShaderPropertyIDs.Color, color);
                Matrix4x4 matrix2 = default(Matrix4x4);
                matrix2.SetTRS(pos, Quaternion.Euler(0f, this.lastInterceptAngle - 90f, 0f), new Vector3(radius * 2.3f, 1f, radius * 2.3f));
                Graphics.DrawMesh(MeshPool.plane10, matrix2, Resources.Materials.ConeMaterial, 0, null, 0, Comp_ShieldGenerator.PropBlock);
            }
            //this.DrawShields();


        }


        #endregion Drawing

        #region UI

        public override string CompInspectStringExtra()
        {
            StringBuilder _StringBuilder = new StringBuilder();
            //return base.CompInspectStringExtra();
            _StringBuilder.Append(base.CompInspectStringExtra());

            if (IsActive())
            {
                _StringBuilder.AppendLine("Shield_Energy_Current".Translate(Mathf.FloorToInt(ShieldEnergyCurrent), ShieldEnergyMax));
            }
            else if (this.CurrentStatus == EnumShieldStatus.Initilising)
            {
                _StringBuilder.AppendLine("Shield_Init".Translate(GenTicks.TicksToSeconds(zzShieldWarmupTicks).ToString("f1")));
            }
            else
            {
                _StringBuilder.AppendLine("Shield_Inactive".Translate());
            }

            if (CompPower != null)
            {
                string text = CompPower.CompInspectStringExtra();
                if (!text.NullOrEmpty())
                {
                    _StringBuilder.Append(text);
                }
                else
                {
                    _StringBuilder.Append("Error, No Power Comp Text.");
                }
            }
            else
            {
                _StringBuilder.Append("Error, No Power Comp?!What the f*ck have you done?");
            }

            return _StringBuilder.ToString();

        }

        public override IEnumerable<Gizmo> CompGetGizmosExtra()
        {
            //return base.CompGetGizmosExtra();

            //Add the stock Gizmoes
            foreach (var g in base.CompGetGizmosExtra())
            {
                yield return g;
            }

            if (zzShieldDefenceBullet)
            {
                if (ShieldDefenceBulletActive)
                {

                    Command_Action act = new Command_Action();
                    //act.action = () => Designator_Deconstruct.DesignateDeconstruct(this);
                    act.action = () => FlickShieldDefenceBullet();
                    act.icon = Resources.Textures.UI_DIRECT_ON;
                    act.defaultLabel = "DefenceBullet".Translate();
                    act.defaultDesc = "TurnOn".Translate();
                    act.activateSound = SoundDef.Named("Click");
                    //act.hotKey = KeyBindingDefOf.DesignatorDeconstruct;
                    //act.groupKey = 689736;
                    yield return act;
                }
                else
                {

                    Command_Action act = new Command_Action();
                    //act.action = () => Designator_Deconstruct.DesignateDeconstruct(this);
                    act.action = () => FlickShieldDefenceBullet();
                    act.icon = Resources.Textures.UI_DIRECT_OFF;
                    act.defaultLabel = "DefenceBullet".Translate();
                    act.defaultDesc = "TurnOff".Translate();
                    act.activateSound = SoundDef.Named("Click");
                    //act.hotKey = KeyBindingDefOf.DesignatorDeconstruct;
                    //act.groupKey = 689736;
                    yield return act;
                }
            }

            if (zzShieldDefenceProjectile)
            {
                if (ShieldDefenceProjectileActive)
                {

                    Command_Action act = new Command_Action();
                    //act.action = () => Designator_Deconstruct.DesignateDeconstruct(this);
                    act.action = () => FlickShieldDefenceProjectile();
                    act.icon = Resources.Textures.UI_INDIRECT_ON;
                    act.defaultLabel = "DefenceProjectile".Translate();
                    act.defaultDesc = "TurnOn".Translate();
                    act.activateSound = SoundDef.Named("Click");
                    //act.hotKey = KeyBindingDefOf.DesignatorDeconstruct;
                    //act.groupKey = 689736;
                    yield return act;
                }
                else
                {

                    Command_Action act = new Command_Action();
                    //act.action = () => Designator_Deconstruct.DesignateDeconstruct(this);
                    act.action = () => FlickShieldDefenceProjectile();
                    act.icon = Resources.Textures.UI_INDIRECT_OFF;
                    act.defaultLabel = "DefenceProjectile".Translate();
                    act.defaultDesc = "TurnOff".Translate();
                    act.activateSound = SoundDef.Named("Click");
                    //act.hotKey = KeyBindingDefOf.DesignatorDeconstruct;
                    //act.groupKey = 689736;
                    yield return act;
                }
            }

            if (zzShieldDefenceSky)
            {
                if (ShieldDefenceSkyActive)
                {

                    Command_Action act = new Command_Action();
                    //act.action = () => Designator_Deconstruct.DesignateDeconstruct(this);
                    act.action = () => FlickShieldDefenceSky();
                    act.icon = Resources.Textures.UI_INTERCEPT_DROPPOD_ON;
                    act.defaultLabel = "DefenceSky".Translate();
                    act.defaultDesc = "TurnOn".Translate();
                    act.activateSound = SoundDef.Named("Click");
                    //act.hotKey = KeyBindingDefOf.DesignatorDeconstruct;
                    //act.groupKey = 689736;
                    yield return act;
                }
                else
                {

                    Command_Action act = new Command_Action();
                    //act.action = () => Designator_Deconstruct.DesignateDeconstruct(this);
                    act.action = () => FlickShieldDefenceSky();
                    act.icon = Resources.Textures.UI_INTERCEPT_DROPPOD_OFF;
                    act.defaultLabel = "DefenceSky".Translate();
                    act.defaultDesc = "TurnOff".Translate();
                    act.activateSound = SoundDef.Named("Click");
                    //act.hotKey = KeyBindingDefOf.DesignatorDeconstruct;
                    //act.groupKey = 689736;
                    yield return act;
                }
            }


            if (true)
            {
                if (zzShieldShowVisually)
                {

                    Command_Action act = new Command_Action();
                    //act.action = () => Designator_Deconstruct.DesignateDeconstruct(this);
                    act.action = () => FlickShieldShowVisually();
                    act.icon = Resources.Textures.UI_SHOW_ON;
                    act.defaultLabel = "ShowShieldArea".Translate();
                    act.defaultDesc = "ShowShieldArea".Translate();
                    act.activateSound = SoundDef.Named("Click");
                    //act.hotKey = KeyBindingDefOf.DesignatorDeconstruct;
                    //act.groupKey = 689736;
                    yield return act;
                }
                else
                {

                    Command_Action act = new Command_Action();
                    //act.action = () => Designator_Deconstruct.DesignateDeconstruct(this);
                    act.action = () => FlickShieldShowVisually();
                    act.icon = Resources.Textures.UI_SHOW_OFF;
                    act.defaultLabel = "HideShieldArea".Translate();
                    act.defaultDesc = "HideShieldArea".Translate();
                    act.activateSound = SoundDef.Named("Click");
                    //act.hotKey = KeyBindingDefOf.DesignatorDeconstruct;
                    //act.groupKey = 689736;
                    yield return act;
                }
            }
                                                         
        }

        public void FlickShieldDefenceBullet()
        {
            zzShieldDefenceBulletActive = !zzShieldDefenceBulletActive;
        }

        public void FlickShieldDefenceProjectile()
        {
            zzShieldDefenceProjectileActive = !zzShieldDefenceProjectileActive;
        }

        public void FlickShieldDefenceSky()
        {
            zzShieldDefenceSkyActive = !zzShieldDefenceSkyActive;
        }

        private void FlickShieldShowVisually()
        {
            zzShieldShowVisually = !zzShieldShowVisually;
        }

        #endregion UI

        #region DataAcess

        public override void PostExposeData()
        {
            base.PostExposeData();



            Scribe_Values.Look(ref zzShieldEnergy_Current, "zzShieldEnergy_Current");
            Scribe_Values.Look(ref zzShieldRadius_Current, "zzShieldRadius_Current",ShieldRadiusMax);
            Scribe_Values.Look(ref zzShieldWarmupTicks, "zzShieldWarmupTicks");
            Scribe_Values.Look(ref zzShieldShowVisually, "zzShieldShowVisually",true);
            Scribe_Values.Look(ref zzShieldDefenceBulletActive, "zzShieldDefenceBulletActive");
            Scribe_Values.Look(ref zzShieldDefenceProjectileActive, "zzShieldDefenceProjectileActive");
            Scribe_Values.Look(ref zzShieldDefenceSkyActive, "zzShieldDefenceSkyActive");
            Scribe_Values.Look(ref zzShieldDefenceIFFActive, "zzShieldDefenceIFFActive");



        }

        #endregion DataAcess

    }
}
