﻿using System.Collections.Generic;
using RimWorld;
using Verse;
using Verse.AI;

namespace zhuzi.AdvancedEnergy.Shields.Shields
{
    public class JobDriver_InstallUpgrade : JobDriver
    {
        public override bool TryMakePreToilReservations(bool errorOnFailed)
        {
            return pawn.Reserve(job.targetA, job);
        }

        protected override IEnumerable<Toil> MakeNewToils()
        {
            var upgrade = TargetThingA.FirstUpgradeableComp();
            if (upgrade == null)
            {
                EndJobWith(JobCondition.Incompletable);
                yield break;
            }
            AddFailCondition(() => {
                var comp = TargetThingA.FirstUpgradeableComp();
                return comp == null || !comp.parent.Spawned || !comp.WantsWork;
            });
            this.FailOnDestroyedNullOrForbidden(TargetIndex.A);
            var gotoUpgradeable = Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.InteractionCell);
            var lookForIngredient = new Toil
            {
                initAction = () => {
                    var missingIngredient = upgrade.TryGetNextMissingIngredient();
                    job.count = missingIngredient.Count;
                    if (missingIngredient.Count > 0)
                    {
                        bool SearchPredicate(Thing thing) => !thing.IsForbidden(pawn) && pawn.CanReserve(thing);
                        var t = GenClosest.ClosestThingReachable(pawn.Position, pawn.Map, ThingRequest.ForDef(missingIngredient.ThingDef), PathEndMode.ClosestTouch, TraverseParms.For(pawn), 999,
                            SearchPredicate);
                        if (t == null)
                        {
                            EndJobWith(JobCondition.Incompletable);
                        }
                        else
                        {
                            job.SetTarget(TargetIndex.B, t);
                        }
                    }
                    else
                    {
                        JumpToToil(gotoUpgradeable);
                    }
                }
            };
            yield return lookForIngredient;
            yield return Toils_Reserve.Reserve(TargetIndex.B, 1, job.count);
            yield return Toils_Goto.GotoCell(TargetIndex.B, PathEndMode.Touch).FailOnDestroyedNullOrForbidden(TargetIndex.B);
            yield return Toils_Haul.StartCarryThing(TargetIndex.B);
            yield return Toils_Goto.GotoCell(TargetIndex.A, PathEndMode.InteractionCell);
            yield return new Toil
            {
                initAction = () => {
                    if (pawn.carryTracker.CarriedThing != null)
                    {
                        pawn.carryTracker.innerContainer.TryTransferToContainer(pawn.carryTracker.CarriedThing, upgrade.GetDirectlyHeldThings(), pawn.carryTracker.CarriedThing.stackCount);
                        pawn.Map.reservationManager.ReleaseAllForTarget(TargetThingB);
                        job.SetTarget(TargetIndex.B, null);
                        JumpToToil(lookForIngredient);
                    }
                }
            };
            yield return gotoUpgradeable;
            yield return new Toil
            {
                tickAction = () => {
                    upgrade.DoWork(GetActor().GetStatValue(StatDefOf.ConstructionSpeed));
                    if (upgrade.curComplete)
                    {
                        EndJobWith(JobCondition.Succeeded);
                    }
                },
                defaultCompleteMode = ToilCompleteMode.Never
            }.WithEffect(EffecterDefOf.ConstructMetal, TargetIndex.A)
                .WithProgressBar(TargetIndex.A, () => upgrade.WorkProgress);
        }
    }
}
