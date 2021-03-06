﻿using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;

namespace zhuzi.AdvancedEnergy.Shields.Shields
{
    class ITab_ShieldGenerator : ITab
    {

        //private Comp_ShieldGenerator _CachedComp;

        //public ITab_ShieldGenerator() : base()
        //{
        //    _CachedComp = 

        //}

        private static readonly Vector2 WinSize = new Vector2(500f, 400f);

        private Comp_ShieldGenerator SelectedCompShieldGenerator
        {
            get
            {
                Thing thing = Find.Selector.SingleSelectedThing;
                MinifiedThing minifiedThing = thing as MinifiedThing;
                if (minifiedThing != null)
                {
                    thing = minifiedThing.InnerThing;
                }
                if (thing == null)
                {
                    return null;
                }
                return thing.TryGetComp<Comp_ShieldGenerator>();
            }
        }

        public override bool IsVisible
        {
            get
            {
                return this.SelectedCompShieldGenerator != null;
            }
        }

        public ITab_ShieldGenerator()
        {
            base.size = ITab_ShieldGenerator.WinSize;
            base.labelKey = "TabShield";
        }

        protected override void FillTab()
        {

            Vector2 winSize = ITab_ShieldGenerator.WinSize;
            float x = winSize.x;
            Vector2 winSize2 = ITab_ShieldGenerator.WinSize;
            Rect rect = new Rect(0f, 0f, x, winSize2.y).ContractedBy(10f);
            //Rect rect2 = rect;
            //Text.Font = GameFont.Medium;
            //Widgets.Label(rect2, "Shield Generator Label Rec2");
            //if (ITab_Art.cachedImageSource != this.SelectedCompArt || ITab_Art.cachedTaleRef != this.SelectedCompArt.TaleRef)
            //{
            //    ITab_Art.cachedImageDescription = this.SelectedCompArt.GenerateImageDescription();
            //    ITab_Art.cachedImageSource = this.SelectedCompArt;
            //    ITab_Art.cachedTaleRef = this.SelectedCompArt.TaleRef;
            //}
            //Rect rect3 = rect;
            //rect3.yMin += 35f;
            //Text.Font = GameFont.Small;
            //Widgets.Label(rect3, "ShieldGenerator Rec3");

            Listing_Standard listing_Standard = new Listing_Standard();
            listing_Standard.ColumnWidth = 250f;
            listing_Standard.Begin(rect);


            listing_Standard.GapLine(12f);
            listing_Standard.Label("Shield_Energy_Current".Translate(Mathf.FloorToInt(SelectedCompShieldGenerator.ShieldEnergyCurrent), SelectedCompShieldGenerator.ShieldEnergyMax));

            listing_Standard.Gap(12f);

            listing_Standard.Label("Shield_Radius".Translate(SelectedCompShieldGenerator.ShieldRadius_Current, SelectedCompShieldGenerator.ShieldRadiusMax));

            listing_Standard.IntAdjuster(ref SelectedCompShieldGenerator.zzShieldRadius_Current, 1, 1);
            if (SelectedCompShieldGenerator.ShieldRadius_Current > SelectedCompShieldGenerator.ShieldRadiusMax)
            {
                SelectedCompShieldGenerator.zzShieldRadius_Current = SelectedCompShieldGenerator.ShieldRadiusMax;
            }

            if (this.SelectedCompShieldGenerator.ShieldDefenceBulletActive)
            {
                if (listing_Standard.ButtonText("DefenceBullet".Translate() + ": " + "TurnOn".Translate()))
                {
                    this.SelectedCompShieldGenerator.FlickShieldDefenceBullet();
                }
            } 
            else
            {
                if (listing_Standard.ButtonText("DefenceBullet".Translate() + ": " + "TurnOff".Translate()))
                {
                    this.SelectedCompShieldGenerator.FlickShieldDefenceBullet();
                }

            }

            if (this.SelectedCompShieldGenerator.ShieldDefenceProjectileInstall)
                if (this.SelectedCompShieldGenerator.ShieldDefenceProjectileActive)
                {
                    if (listing_Standard.ButtonText("DefenceProjectile".Translate() + ": " + "TurnOn".Translate()))
                    {
                        this.SelectedCompShieldGenerator.FlickShieldDefenceProjectile();
                    }
                }
                else
                {
                    if (listing_Standard.ButtonText("DefenceProjectile".Translate() + ": " + "TurnOff".Translate()))
                    {
                        this.SelectedCompShieldGenerator.FlickShieldDefenceProjectile();
                    }

                }
            else
                listing_Standard.ButtonText("DefenceProjectile".Translate() + ": " + "Not_Installed".Translate());

            if (this.SelectedCompShieldGenerator.ShieldDefenceSkyInstall)
                if (this.SelectedCompShieldGenerator.ShieldDefenceSkyActive)
                {
                    if (listing_Standard.ButtonText("DefenceSky".Translate() + ": " + "TurnOn".Translate()))
                    {
                        this.SelectedCompShieldGenerator.FlickShieldDefenceSky();
                    }
                }
                else
                {
                    if (listing_Standard.ButtonText("DefenceSky".Translate() + ": " + "TurnOff".Translate()))
                    {
                        this.SelectedCompShieldGenerator.FlickShieldDefenceSky();
                    }

                }
            else
                listing_Standard.ButtonText("DefenceSky".Translate() + ": " + "Not_Installed".Translate());

            if (SelectedCompShieldGenerator.ShieldDefenceIFFInstall)
            {
                listing_Standard.ButtonText("DefenceIFF".Translate() + ": " + "Auto_TurnOn".Translate());

            }
            else
            {
                listing_Standard.ButtonText("DefenceIFF".Translate() + ": " + "Not_Installed".Translate());
            }

            listing_Standard.End();
        }

    } //Class

} //NameSpace

