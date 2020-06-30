using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;

namespace Jaxxa.EnhancedDevelopment.Shields.Shields
{
    public class CompProperties_ShieldGenerator : CompProperties
    {

        public CompProperties_ShieldGenerator()
        {
            this.compClass = typeof(Comp_ShieldGenerator);
        }

        //Extra

        public float m_RechargeSpeed = 1.0f;
        public float m_DamageRate = 100f;
        public float m_ExtraDamageRateOfEMP = 5f;
        public float m_FieldDamageRateOfEMP = 7f;
        public float m_ExtraDamageRateOfFlame = 2.5f;

        //Field Settings
        public int m_FieldIntegrity_Initial = 0;
        public int m_FieldIntegrity_Max_Base = 0;
        public int m_Field_Radius_Base = 0;

        //Power Settings
        public int m_PowerRequired_Charging = 0;
        public int m_PowerRequired_Standby = 0;

        //Recovery Settings
        public int m_RechargeAmmount_Base = 1;
        public int m_RecoverWarmupDelayTicks_Base = 0;

        //Mode Selections
        public bool m_BlockDirect_Avalable = false;
        public bool m_BlockIndirect_Avalable = false;
        public bool m_InterceptDropPod_Avalable = false;
        public bool m_StructuralIntegrityMode = false;

        // public List<string> SIFBuildings = new List<string>(); // Move to Global List.
    }
}
