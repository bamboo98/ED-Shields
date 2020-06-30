using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;
using Verse.Sound;
using RimWorld;
using UnityEngine;
using System.Reflection;

namespace Jaxxa.EnhancedDevelopment.Shields.Shields
{

    [StaticConstructorOnStartup]
    public class Comp_ShieldGenerator : ThingComp
    {

        //Material currentMatrialColour;

        public CompProperties_ShieldGenerator Properties;

        #region Variables

        //UI elements - Unsaved
        private static Texture2D UI_DIRECT_ON;
        private static Texture2D UI_DIRECT_OFF;

        private static Texture2D UI_INDIRECT_ON;
        private static Texture2D UI_INDIRECT_OFF;

        private static Texture2D UI_INTERCEPT_DROPPOD_ON;
        private static Texture2D UI_INTERCEPT_DROPPOD_OFF;

        private static Texture2D UI_SHOW_ON;
        private static Texture2D UI_SHOW_OFF;

        private static Texture2D UI_LAUNCH_REPORT;

        //Visual Settings
        private bool m_ShowVisually_Active = true;
        //private float m_ColourRed;
        //private float m_ColourGreen;
        //private float m_ColourBlue;

        //Field Settings
        public int m_FieldIntegrity_Max;
        private int m_FieldIntegrity_Initial;

        private float m_RechargeSpeed;
        private float m_ExtraDamageRateOfEMP;
        private float m_ExtraDamageRateOfFlame;
        private float m_DamageRate;
        private float m_FieldDamageRateOfEMP;
        private float PowerUsage_Increase2 = 1;


        //Recovery Settings
        private int m_RechargeTickDelayInterval;
        private int m_RecoverWarmupDelayTicks;
        private int m_WarmupTicksRemaining;

        private List<Building> m_AppliedUpgrades = new List<Building>();

        private static readonly MaterialPropertyBlock PropBlock = new MaterialPropertyBlock();
        private int lastIntercepted = -69;
        private float lastInterceptAngle;
        private static readonly Material ConeMaterial = MaterialPool.MatFrom("Other/ForceFieldCone", ShaderDatabase.MoteGlow);
        private static readonly Material ShieldMaterial = MaterialPool.MatFrom("Things/ShieldBubbleSOS", ShaderDatabase.MoteGlow);
        #endregion Variables

        #region Settings

        // Power Usage --------------------------------------------------------------

        //Comp, found each time.
        CompPowerTrader m_Power;

        private int m_PowerRequired;


        // Range --------------------------------------------------------------------

        public int m_FieldRadius_Avalable;
        public int m_FieldRadius_Requested = 999;

        public int FieldRadius_Active()
        {
            return Math.Min(this.m_FieldRadius_Requested, this.m_FieldRadius_Avalable);
        }

        // Block Direct -------------------------------------------------------------


        private bool m_BlockDirect_Avalable;

        private bool m_BlockDirect_Requested = true;

        public bool BlockDirect_Active()
        {
            return this.m_BlockDirect_Avalable && this.m_BlockDirect_Requested;
        }

        // Block Indirect -----------------------------------------------------------


        private bool m_BlockIndirect_Avalable;

        private bool m_BlockIndirect_Requested = true;

        public bool BlockIndirect_Active()
        {
            return this.m_BlockIndirect_Avalable && this.m_BlockIndirect_Requested;
        }

        //Block Droppods ------------------------------------------------------------

        private bool m_InterceptDropPod_Avalable;

        private bool m_InterceptDropPod_Requested = true;

        public bool IntercepDropPod_Active()
        {
            return m_InterceptDropPod_Avalable && m_InterceptDropPod_Requested;
        }
        
        public bool IsInterceptDropPod_Avalable()
        {
            return this.m_InterceptDropPod_Avalable;
        }

        // Identify Friend Foe ------------------------------------------------------

        private bool m_IdentifyFriendFoe_Avalable = false;

        private bool m_IdentifyFriendFoe_Requested = true;

        public bool IdentifyFriendFoe_Active()
        {
            return this.m_IdentifyFriendFoe_Avalable && this.m_IdentifyFriendFoe_Requested;
        }

        // Slow Discharge -----------------------------------------------------------

        public bool SlowDischarge_Active;

        #endregion

        #region Initilisation

        //Static Construtor
        static Comp_ShieldGenerator()
        {
            //Setup UI
            UI_DIRECT_OFF = ContentFinder<Texture2D>.Get("UI/DirectOff", true);
            UI_DIRECT_ON = ContentFinder<Texture2D>.Get("UI/DirectOn", true);
            UI_INDIRECT_OFF = ContentFinder<Texture2D>.Get("UI/IndirectOff", true);
            UI_INDIRECT_ON = ContentFinder<Texture2D>.Get("UI/IndirectOn", true);
            UI_INTERCEPT_DROPPOD_OFF = ContentFinder<Texture2D>.Get("UI/FireOff", true);
            UI_INTERCEPT_DROPPOD_ON = ContentFinder<Texture2D>.Get("UI/FireOn", true);

            UI_SHOW_ON = ContentFinder<Texture2D>.Get("UI/ShieldShowOn", true);
            UI_SHOW_OFF = ContentFinder<Texture2D>.Get("UI/ShieldShowOff", true);
            UI_LAUNCH_REPORT = ContentFinder<Texture2D>.Get("UI/Commands/LaunchReport");
        }

        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            base.PostSpawnSetup(respawningAfterLoad);

            this.Properties = ((CompProperties_ShieldGenerator)this.props);
            this.m_Power = this.parent.GetComp<CompPowerTrader>();

            this.RecalculateStatistics();
        }

        public void RecalculateStatistics()
        {
            //Log.Message("RecalculateStatistics");

            //Visual Settings
            //this.m_ColourRed = 0.5f;
            //this.m_ColourGreen = 0.0f;
            //this.m_ColourBlue = 0.5f;

            PowerUsage_Increase2 = 1;

            //EXtra
            m_RechargeSpeed = Properties.m_RechargeSpeed;
            m_ExtraDamageRateOfEMP = Properties.m_ExtraDamageRateOfEMP;
            m_ExtraDamageRateOfFlame = Properties.m_ExtraDamageRateOfFlame;
            m_DamageRate = Properties.m_DamageRate;
            m_FieldDamageRateOfEMP = Properties.m_FieldDamageRateOfEMP;



            //Field Settings
            this.m_FieldIntegrity_Max = this.Properties.m_FieldIntegrity_Max_Base;
            this.m_FieldIntegrity_Initial = this.Properties.m_FieldIntegrity_Initial;
            this.m_FieldRadius_Avalable = this.Properties.m_Field_Radius_Base;

            //Mode Settings - Avalable
            this.m_BlockIndirect_Avalable = this.Properties.m_BlockIndirect_Avalable;
            this.m_BlockDirect_Avalable = this.Properties.m_BlockDirect_Avalable;
            this.m_InterceptDropPod_Avalable = this.Properties.m_InterceptDropPod_Avalable;

            //Power Settings
            this.m_PowerRequired = this.Properties.m_PowerRequired_Charging;

            //Recovery Settings
            this.m_RecoverWarmupDelayTicks = this.Properties.m_RecoverWarmupDelayTicks_Base;

            //Power converter
            this.SlowDischarge_Active = false;

            //IFF
            this.m_IdentifyFriendFoe_Avalable = false;

            //Store the List of Building in initilisation????
         
            this.m_AppliedUpgrades.ForEach(b =>
            {
                Building _Building = b as Building;
                Comp_ShieldUpgrade _Comp = _Building.GetComp<Comp_ShieldUpgrade>();

                Patch.Patcher.LogNULL(_Building, "_Building");
                Patch.Patcher.LogNULL(_Comp, "_Comp");

                this.AddStatsFromUpgrade(_Comp);


            });
            this.m_PowerRequired = (int)Mathf.CeilToInt((float)m_PowerRequired * PowerUsage_Increase2);

            this.m_Power.powerOutputInt = -this.m_PowerRequired;

        }

        private void AddStatsFromUpgrade(Comp_ShieldUpgrade comp)
        {

            CompProperties_ShieldUpgrade _Properties = ((CompProperties_ShieldUpgrade)comp.props);
            Patch.Patcher.LogNULL(_Properties, "_Properties");

            this.m_FieldIntegrity_Max += _Properties.FieldIntegrity_Increase;
            this.m_FieldRadius_Avalable += _Properties.Range_Increase;
            this.m_RechargeSpeed += _Properties.Field_Recharge_Speed;
            //Power
            m_PowerRequired += _Properties.PowerUsage_Increase;
            PowerUsage_Increase2 *= (100f + (float)_Properties.PowerUsage_Increase2) / 100f;


            if (_Properties.DropPodIntercept)
            {
                this.m_InterceptDropPod_Avalable = true;
                //this.m_PowerRequired = Mathf.CeilToInt(m_PowerRequired * 1.25f);
            }

            if (_Properties.IdentifyFriendFoe)
            {
                //Log.Message("Setting IFF");
                this.m_IdentifyFriendFoe_Avalable = true;
                //this.m_PowerRequired = Mathf.CeilToInt(m_PowerRequired * 1.25f);
            }

            if (_Properties.SlowDischarge)
            {
                this.SlowDischarge_Active = true;
            }

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
                        this.m_WarmupTicksRemaining = this.m_RecoverWarmupDelayTicks;
                    }
                    break;

                case (EnumShieldStatus.Initilising):
                    if (_PowerAvalable)
                    {
                        if (this.m_WarmupTicksRemaining > 0)
                        {
                            this.m_WarmupTicksRemaining--;
                        }
                        else
                        {
                            this.CurrentStatus = EnumShieldStatus.ActiveCharging;
                            this.FieldIntegrity_Current = (float)this.m_FieldIntegrity_Initial;
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
                        if (!this.SlowDischarge_Active)
                        {
                            this.m_FieldIntegrity_Current = 0;
                        }

                        if (this.FieldIntegrity_Current <= 0)
                        {
                            this.CurrentStatus = EnumShieldStatus.Offline;

                        }
                    }
                    break;

                case (EnumShieldStatus.ActiveCharging):
                    if (this.FieldIntegrity_Current < 0)
                    {
                        this.CurrentStatus = EnumShieldStatus.Offline;
                    }
                    else
                    {
                        if (!_PowerAvalable)
                        {
                            this.CurrentStatus = EnumShieldStatus.ActiveDischarging;
                        }
                        else if (this.FieldIntegrity_Current >= this.m_FieldIntegrity_Max)
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
                        if (this.FieldIntegrity_Current < this.m_FieldIntegrity_Max)
                        {
                            this.CurrentStatus = EnumShieldStatus.ActiveCharging;
                        }
                    }
                    break;
            }
        }

        public void HitShield(Projectile proj)
        {

            this.lastInterceptAngle = Vector3Utility.AngleToFlat(proj.DrawPos, GenThing.TrueCenter(this.parent));
            this.lastIntercepted = Find.TickManager.TicksGame;
            float dmg = proj.DamageAmount;
            if (proj.def.projectile.damageDef == DamageDefOf.EMP)
            {
                dmg *= m_ExtraDamageRateOfEMP;
                dmg += m_FieldIntegrity_Current / 100 * m_FieldDamageRateOfEMP;
            }
            else if (proj.def.projectile.damageDef == DamageDefOf.Flame)
                dmg *= m_ExtraDamageRateOfFlame;

            dmg = Mathf.Max(0, dmg*m_DamageRate/100f);

            //判定为爆炸弹丸
            if (proj.def.projectile.explosionRadius != 0)
            {
                //受到伤害取决于弹丸威力*伤害半径^2
                dmg *= Mathf.Max(1f, proj.def.projectile.explosionRadius * proj.def.projectile.explosionRadius);

                MoteMaker.ThrowMicroSparks(this.parent.DrawPos, this.parent.Map);
                GenExplosion.DoExplosion(proj.Position, this.parent.Map, Mathf.Min(proj.def.projectile.explosionRadius * 0.66f, 3f),
                    DefDatabase<DamageDef>.GetNamed("ED_ShieldExplosion", true),
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
            this.FieldIntegrity_Current -= Mathf.CeilToInt(dmg);

            proj.Destroy();
        }

        public bool IsActive()
        {
            //return true;
            return (this.CurrentStatus == EnumShieldStatus.ActiveCharging ||
                 this.CurrentStatus == EnumShieldStatus.ActiveDischarging ||
                 this.CurrentStatus == EnumShieldStatus.ActiveSustaining);
        }

        public bool CheckPowerOn()
        {
            if (this.m_Power != null)
            {
                if (this.m_Power.PowerOn)
                {
                    return true;
                }
            }
            return false;
        }

        public void TickRecharge()
        {
                if (this.CurrentStatus == EnumShieldStatus.ActiveCharging)
                {
                this.FieldIntegrity_Current += Mathf.Min((float)this.m_FieldIntegrity_Max, 1000) / 6000f * m_RechargeSpeed;
                }
                else if (this.CurrentStatus == EnumShieldStatus.ActiveDischarging)
                {
                    this.FieldIntegrity_Current-= (float)this.m_FieldIntegrity_Max / 6000f * 3f ;
                }
        }

        public bool WillInterceptDropPod(DropPodIncoming dropPodToCheck)
        {
            //Check if can and wants to intercept
            if (!this.IntercepDropPod_Active())
            {
                return false;
            }

            //Check if online
            if (this.CurrentStatus == EnumShieldStatus.Offline || this.CurrentStatus == EnumShieldStatus.Initilising)
            {
                return false;
            }


            //Check IFF
            if (this.IdentifyFriendFoe_Active())
            {
                bool _Hostile = dropPodToCheck.Contents.innerContainer.Any(x => x.Faction.HostileTo(Faction.OfPlayer));

                if (!_Hostile)
                {
                    return false;
                }
            }

            //Check Distance
            float _Distance = Vector3.Distance(dropPodToCheck.Position.ToVector3(), this.parent.Position.ToVector3());
            float _Radius = this.FieldRadius_Active();
            if (_Distance > _Radius)
            {
                return false;
            }

            //All Tests passed so intercept the pod
            return true;

        }

        public bool WillProjectileBeBlocked(Verse.Projectile projectile)
        {

            //Check if online
            if (this.CurrentStatus == EnumShieldStatus.Offline || this.CurrentStatus == EnumShieldStatus.Initilising)
            {
                return false;
            }

            //Check if can and wants to intercept
            if (projectile.def.projectile.flyOverhead)
            {
                if (!this.BlockIndirect_Active()) { return false; }
            }
            else
            {
                if (!this.BlockDirect_Active()) { return false; }
            }

            //Check Distance
            float _Distance = Vector3.Distance(projectile.Position.ToVector3(), this.parent.Position.ToVector3());
            if (_Distance > this.FieldRadius_Active())
            {
                return false;
            }

            //Check Angle
            if (!Comp_ShieldGenerator.CorrectAngleToIntercept(projectile, this.parent))
            {
                return false;
            }

            //Check IFF
            if (this.IdentifyFriendFoe_Active())
            {
                FieldInfo _LauncherFieldInfo = typeof(Projectile).GetField("launcher", BindingFlags.NonPublic | BindingFlags.Instance);
                Patch.Patcher.LogNULL(_LauncherFieldInfo, "_LauncherFieldInfo");
                Thing _Launcher = (Thing)_LauncherFieldInfo.GetValue(projectile);
                Patch.Patcher.LogNULL(_Launcher, "_Launcher");

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

        public EnumShieldStatus CurrentStatus
        {
            get
            {
                return this.m_CurrentStatus;
            }
            set
            {
                this.m_CurrentStatus = value;

                //if (this.m_CurrentStatus == EnumShieldStatus.ActiveSustaining)
                //{
                //    this.m_Power.powerOutputInt = -this.m_PowerRequired_Standby;
                //}
                //else
                //{
                //    this.m_Power.powerOutputInt = -this.m_PowerRequired_Charging;
                //}
            }
        }
        private EnumShieldStatus m_CurrentStatus = EnumShieldStatus.Offline;

        public float FieldIntegrity_Current
        {
            get
            {
                return this.m_FieldIntegrity_Current;
            }
            set
            {
                if (value < 0)
                {
                    this.CurrentStatus = EnumShieldStatus.Offline;
                    this.m_FieldIntegrity_Current = 0;
                }
                else if (value > this.m_FieldIntegrity_Max)
                {
                    this.m_FieldIntegrity_Current = this.m_FieldIntegrity_Max;
                }
                else
                {
                    this.m_FieldIntegrity_Current = value;
                }
            }
        }
        private float m_FieldIntegrity_Current;

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

            if (!this.IsActive() || !this.m_ShowVisually_Active)
            {
                return;
            }
            Vector3 pos = this.parent.Position.ToVector3Shifted();
            pos.y = Altitudes.AltitudeFor(AltitudeLayer.MoteOverhead);
            float alpha = this.Alpha();
            int radius = this.FieldRadius_Active();
            if (alpha > 0f)
            {
                Color value = Color.white;
                value.a *= alpha;
                Comp_ShieldGenerator.PropBlock.SetColor(ShaderPropertyIDs.Color, value);
                Matrix4x4 matrix = default(Matrix4x4);
                matrix.SetTRS(pos, Quaternion.identity, new Vector3(radius * 2.1f, 1f, radius * 2.1f));
                Graphics.DrawMesh(MeshPool.plane10, matrix, Comp_ShieldGenerator.ShieldMaterial, 0, null, 0, Comp_ShieldGenerator.PropBlock);
            }
            float coneAlpha = this.HitConeAlpha();
            if (coneAlpha > 0f)
            {
                Color color = Color.white;
                color.a *= coneAlpha;
                Comp_ShieldGenerator.PropBlock.SetColor(ShaderPropertyIDs.Color, color);
                Matrix4x4 matrix2 = default(Matrix4x4);
                matrix2.SetTRS(pos, Quaternion.Euler(0f, this.lastInterceptAngle - 90f, 0f), new Vector3(radius * 2.3f, 1f, radius * 2.3f));
                Graphics.DrawMesh(MeshPool.plane10, matrix2, Comp_ShieldGenerator.ConeMaterial, 0, null, 0, Comp_ShieldGenerator.PropBlock);
            }
            //this.DrawShields();


        }

        /// <summary>
        /// Draw the shield Field
        /// </summary>
        //public void DrawShields()
        //{
        //    if (!this.IsActive() || !this.m_ShowVisually_Active)
        //    {
        //        return;
        //    }

        //    //Draw field
        //    this.DrawField(Jaxxa.EnhancedDevelopment.Shields.Shields.Utilities.VectorsUtils.IntVecToVec(this.parent.Position));

        //}

        //public override void DrawExtraSelectionOverlays()
        //{
        //    //    GenDraw.DrawRadiusRing(base.Position, shieldField.shieldShieldRadius);
        //}

        //public void DrawSubField(IntVec3 center, float radius)
        //{
        //    this.DrawSubField(Jaxxa.EnhancedDevelopment.Shields.Shields.Utilities.VectorsUtils.IntVecToVec(center), radius);
        //}

        ////Draw the field on map
        //public void DrawField(Vector3 center)
        //{
        //    DrawSubField(center, this.FieldRadius_Active());
        //}

        //public void DrawSubField(Vector3 position, float shieldShieldRadius)
        //{
        //    position = position + (new Vector3(0.5f, 0f, 0.5f));

        //    Vector3 s = new Vector3(shieldShieldRadius, 1f, shieldShieldRadius);
        //    Matrix4x4 matrix = default(Matrix4x4);
        //    matrix.SetTRS(position, Quaternion.identity, s);

        //    if (currentMatrialColour == null)
        //    {
        //        //Log.Message("Creating currentMatrialColour");
        //        currentMatrialColour = SolidColorMaterials.NewSolidColorMaterial(new Color(m_ColourRed, m_ColourGreen, m_ColourBlue, 0.15f), ShaderDatabase.MetaOverlay);
        //        //currentMatrialColour = SolidColorMaterials.NewSolidColorMaterial(new Color(0.5f, 0.0f, 0.0f, 0.15f), ShaderDatabase.MetaOverlay);
        //    }

        //    UnityEngine.Graphics.DrawMesh(Jaxxa.EnhancedDevelopment.Shields.Shields.Utilities.Graphics.CircleMesh, matrix, currentMatrialColour, 0);

        //}

        #endregion Drawing

        #region UI

        public override string CompInspectStringExtra()
        {
            StringBuilder _StringBuilder = new StringBuilder();
            //return base.CompInspectStringExtra();
            _StringBuilder.Append(base.CompInspectStringExtra());

            if (this.IsActive())
            {
                _StringBuilder.AppendLine("充能: " + (int)this.FieldIntegrity_Current + "/" + this.m_FieldIntegrity_Max);
            }
            else if (this.CurrentStatus == EnumShieldStatus.Initilising)
            {
                //stringBuilder.AppendLine("Initiating shield: " + ((warmupTicks * 100) / recoverWarmup) + "%");
                _StringBuilder.AppendLine(Math.Round(GenTicks.TicksToSeconds(m_WarmupTicksRemaining)) + "秒后启动");
                //stringBuilder.AppendLine("Ready in " + m_warmupTicksCurrent + " seconds.");
            }
            else
            {
                _StringBuilder.AppendLine("护盾关闭,无电源");
            }

            if (m_Power != null)
            {
                string text = m_Power.CompInspectStringExtra();
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
                _StringBuilder.Append("Error, No Power Comp.");
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

            if (m_BlockDirect_Avalable)
            {
                if (this.BlockDirect_Active())
                {

                    Command_Action act = new Command_Action();
                    //act.action = () => Designator_Deconstruct.DesignateDeconstruct(this);
                    act.action = () => this.SwitchDirect();
                    act.icon = UI_DIRECT_ON;
                    act.defaultLabel = "阻挡子弹";
                    act.defaultDesc = "开启";
                    act.activateSound = SoundDef.Named("Click");
                    //act.hotKey = KeyBindingDefOf.DesignatorDeconstruct;
                    //act.groupKey = 689736;
                    yield return act;
                }
                else
                {

                    Command_Action act = new Command_Action();
                    //act.action = () => Designator_Deconstruct.DesignateDeconstruct(this);
                    act.action = () => this.SwitchDirect();
                    act.icon = UI_DIRECT_OFF;
                    act.defaultLabel = "阻挡子弹";
                    act.defaultDesc = "关闭";
                    act.activateSound = SoundDef.Named("Click");
                    //act.hotKey = KeyBindingDefOf.DesignatorDeconstruct;
                    //act.groupKey = 689736;
                    yield return act;
                }
            }

            if (this.m_BlockIndirect_Avalable)
            {
                if (this.BlockIndirect_Active())
                {

                    Command_Action act = new Command_Action();
                    //act.action = () => Designator_Deconstruct.DesignateDeconstruct(this);
                    act.action = () => this.SwitchIndirect();
                    act.icon = UI_INDIRECT_ON;
                    act.defaultLabel = "阻挡迫击炮弹";
                    act.defaultDesc = "开启";
                    act.activateSound = SoundDef.Named("Click");
                    //act.hotKey = KeyBindingDefOf.DesignatorDeconstruct;
                    //act.groupKey = 689736;
                    yield return act;
                }
                else
                {

                    Command_Action act = new Command_Action();
                    //act.action = () => Designator_Deconstruct.DesignateDeconstruct(this);
                    act.action = () => this.SwitchIndirect();
                    act.icon = UI_INDIRECT_OFF;
                    act.defaultLabel = "阻挡迫击炮弹";
                    act.defaultDesc = "关闭";
                    act.activateSound = SoundDef.Named("Click");
                    //act.hotKey = KeyBindingDefOf.DesignatorDeconstruct;
                    //act.groupKey = 689736;
                    yield return act;
                }
            }

            if (m_InterceptDropPod_Avalable)
            {
                if (this.IntercepDropPod_Active())
                {

                    Command_Action act = new Command_Action();
                    //act.action = () => Designator_Deconstruct.DesignateDeconstruct(this);
                    act.action = () => this.SwitchInterceptDropPod();
                    act.icon = UI_INTERCEPT_DROPPOD_ON;
                    act.defaultLabel = "阻挡空投仓";
                    act.defaultDesc = "开启";
                    act.activateSound = SoundDef.Named("Click");
                    //act.hotKey = KeyBindingDefOf.DesignatorDeconstruct;
                    //act.groupKey = 689736;
                    yield return act;
                }
                else
                {

                    Command_Action act = new Command_Action();
                    //act.action = () => Designator_Deconstruct.DesignateDeconstruct(this);
                    act.action = () => this.SwitchInterceptDropPod();
                    act.icon = UI_INTERCEPT_DROPPOD_OFF;
                    act.defaultLabel = "阻挡空投仓";
                    act.defaultDesc = "关闭";
                    act.activateSound = SoundDef.Named("Click");
                    //act.hotKey = KeyBindingDefOf.DesignatorDeconstruct;
                    //act.groupKey = 689736;
                    yield return act;
                }
            }


            if (true)
            {
                if (m_ShowVisually_Active)
                {

                    Command_Action act = new Command_Action();
                    //act.action = () => Designator_Deconstruct.DesignateDeconstruct(this);
                    act.action = () => this.SwitchVisual();
                    act.icon = UI_SHOW_ON;
                    act.defaultLabel = "显示护盾区域";
                    act.defaultDesc = "显示护盾区域";
                    act.activateSound = SoundDef.Named("Click");
                    //act.hotKey = KeyBindingDefOf.DesignatorDeconstruct;
                    //act.groupKey = 689736;
                    yield return act;
                }
                else
                {

                    Command_Action act = new Command_Action();
                    //act.action = () => Designator_Deconstruct.DesignateDeconstruct(this);
                    act.action = () => this.SwitchVisual();
                    act.icon = UI_SHOW_OFF;
                    act.defaultLabel = "隐藏护盾区域";
                    act.defaultDesc = "隐藏护盾区域";
                    act.activateSound = SoundDef.Named("Click");
                    //act.hotKey = KeyBindingDefOf.DesignatorDeconstruct;
                    //act.groupKey = 689736;
                    yield return act;
                }
            }

            if (true)
            {
                Command_Action act = new Command_Action();
                //act.action = () => Designator_Deconstruct.DesignateDeconstruct(this);
                act.action = () => this.ApplyUpgrades();
                act.icon = UI_LAUNCH_REPORT;
                act.defaultLabel = "安装插件";
                act.defaultDesc = "安装插件(放置在四周再点击此按钮)";
                act.activateSound = SoundDef.Named("Click");
                //act.hotKey = KeyBindingDefOf.DesignatorDeconstruct;
                //act.groupKey = 689736;
                yield return act;
            }
                                                         
        } //CompGetGizmosExtra()

        public void ApplyUpgrades()
        {
            var _PotentialUpgradeBuildings = this.parent
                                                .Map
                                                .listerBuildings
                                                .allBuildingsColonist
                                                //Add adjacent including diagonally.
                                                .Where(x => x.Position.InHorDistOf(this.parent.Position, 1.6f))
                                                .Where(x => x.TryGetComp<Comp_ShieldUpgrade>() != null);



            var _BuildingToAdd = _PotentialUpgradeBuildings.FirstOrDefault(x => this.IsAvalableUpgrade(x));
            if (_BuildingToAdd != null)
            { 
                this.m_AppliedUpgrades.Add(_BuildingToAdd);
                _BuildingToAdd.DeSpawn();
                Messages.Message("Applying Shield Upgrade: " + _BuildingToAdd.def.label, this.parent, MessageTypeDefOf.PositiveEvent);
            }
            else
            {

                var _InvalidBuildings = _PotentialUpgradeBuildings.Where(x => !this.IsAvalableUpgrade(x, true));
                if (_InvalidBuildings.Any())
                {
                    Messages.Message("No Valid Shield Upgrades Found.", this.parent, MessageTypeDefOf.RejectInput);
                }
                else
                {
                    Messages.Message("No Shield Upgrades Found.", this.parent, MessageTypeDefOf.RejectInput);
                }
            }
        }

        private bool IsAvalableUpgrade(Building buildingToCheck, bool ResultMessages = false)
        {
            Comp_ShieldUpgrade _Comp = buildingToCheck.TryGetComp<Comp_ShieldUpgrade>();
            
            if (_Comp == null) 
            {
                if (ResultMessages)
                {
                    Messages.Message("Upgrade Comp Not Found, How did you even get here?.",
                        buildingToCheck,
                        MessageTypeDefOf.RejectInput);
                }
                return false; 
            }

            if (this.m_IdentifyFriendFoe_Avalable && _Comp.Properties.IdentifyFriendFoe)
            {
                if (ResultMessages)
                {
                    Messages.Message("Upgrade Contains IFF while shield already has it.",
                        buildingToCheck,
                        MessageTypeDefOf.RejectInput);
                }
                return false;
            }

            if (this.SlowDischarge_Active && _Comp.Properties.SlowDischarge)
            {

                if (ResultMessages)
                {
                    Messages.Message("Upgrade for slow discharge while shield already has it.",
                        buildingToCheck,
                        MessageTypeDefOf.RejectInput);
                }
                return false;
            }

            if (this.m_InterceptDropPod_Avalable && _Comp.Properties.DropPodIntercept)
            {

                if (ResultMessages)
                {
                    Messages.Message("Upgrade for drop pod intercept while shield already has it.",
                        buildingToCheck,
                        MessageTypeDefOf.RejectInput);
                }
                return false;
            }

            return true;
        }

        public void SwitchDirect()
        {
            this.m_BlockDirect_Requested = !this.m_BlockDirect_Requested;
        }

        public void SwitchIndirect()
        {
            this.m_BlockIndirect_Requested = !this.m_BlockIndirect_Requested;
        }

        public void SwitchInterceptDropPod()
        {
            this.m_InterceptDropPod_Requested = !this.m_InterceptDropPod_Requested;
        }

        private void SwitchVisual()
        {
            this.m_ShowVisually_Active = !this.m_ShowVisually_Active;
        }

        #endregion UI

        #region DataAcess

        public override void PostExposeData()
        {
            base.PostExposeData();


            Scribe_Values.Look(ref m_RechargeSpeed, "m_RechargeSpeed");
            Scribe_Values.Look(ref m_DamageRate, "m_DamageRate");
            Scribe_Values.Look(ref m_ExtraDamageRateOfEMP, "m_ExtraDamageRateOfEMP");
            Scribe_Values.Look(ref m_ExtraDamageRateOfFlame, "m_ExtraDamageRateOfFlame");


            Scribe_Values.Look(ref m_FieldRadius_Requested, "m_FieldRadius_Requested");
            Scribe_Values.Look(ref m_BlockDirect_Requested, "m_BlockDirect_Requested");
            Scribe_Values.Look(ref m_BlockIndirect_Requested, "m_BlockIndirect_Requested");
            Scribe_Values.Look(ref m_InterceptDropPod_Requested, "m_InterceptDropPod_Requested");
            Scribe_Values.Look(ref m_IdentifyFriendFoe_Requested, "m_IdentifyFriendFoe_Requested");

            Scribe_Values.Look(ref m_RechargeTickDelayInterval, "m_shieldRechargeTickDelay");
            Scribe_Values.Look(ref m_RecoverWarmupDelayTicks, "m_shieldRecoverWarmup");

            //Scribe_Values.Look(ref m_ColourRed, "m_colourRed");
            //Scribe_Values.Look(ref m_ColourGreen, "m_colourGreen");
            //Scribe_Values.Look(ref m_ColourBlue, "m_colourBlue");

            Scribe_Values.Look(ref m_WarmupTicksRemaining, "m_WarmupTicksRemaining");

            Scribe_Values.Look(ref m_CurrentStatus, "m_CurrentStatus");
            Scribe_Values.Look(ref m_FieldIntegrity_Current, "m_FieldIntegrity_Current");

            Scribe_Collections.Look<Building>(ref m_AppliedUpgrades, "m_AppliedUpgrades", LookMode.Deep);

        }

        #endregion DataAcess

    }
}
