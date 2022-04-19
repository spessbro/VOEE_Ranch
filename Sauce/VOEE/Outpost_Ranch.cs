using System;
using System.Collections.Generic;
using System.Linq;
using Outposts;
using RimWorld;
using RimWorld.Planet;
using UnityEngine;
using Verse;

namespace VOEE;

public class Outpost_Ranching : Outpost_ChooseResult
{
[PostToSetings("Outposts.Settings.Animals", PostToSetingsAttribute.DrawMode.Percentage, 1f, 0.01f, 2f, null, null)]
public float Animals = 1f;

[PostToSetings("Outposts.Settings.Leather", PostToSetingsAttribute.DrawMode.Percentage, 0.5f, 0.01f, 2f, null, null)]
public float Leather = 0.5f;

[PostToSetings("Outposts.Settings.Meat", PostToSetingsAttribute.DrawMode.Percentage, 0.5f, 0.01f, 2f, null, null)]
public float Meat = 0.5f;

[PostToSetings("Outposts.Settings.Milk", PostToSetingsAttribute.DrawMode.Percentage, 0.5f, 0.01f, 2f, null, null)]
public float Milk = 0.5f;

[PostToSetings("Outposts.Settings.Wool", PostToSetingsAttribute.DrawMode.Percentage, 0.5f, 0.01f, 2f, null, null)]
public float Wool = 0.5f;

[PostToSetings("Outposts.Settings.Egg", PostToSetingsAttribute.DrawMode.Percentage, 0.5f, 0.01f, 2f, null, null)]
public float Egg = 0.5f;

[PostToSetings("Outposts.Settings.Production", PostToSetingsAttribute.DrawMode.Percentage, 0.5f, 0.01f, 5f, null, null)]
public float ProductionMultiplier = 0.5f;
[PostToSetings("Outposts.Settings.CountMultiplier", PostToSetingsAttribute.DrawMode.Percentage, 1f, 0.01f, 5f, null, null)]
public float CountMultiplier = 1f;
public override List<ResultOption> ResultOptions
	{

		get
		{
			Log.Message("Resulting the Options");
			ResultOption resultOption = base.ResultOptions.FirstOrDefault();
			if (resultOption?.Thing == null)
			{
				return new List<ResultOption>();
			}
			List <ResultOption> outy = new List<ResultOption>
			{
				new ResultOption
				{
					Thing = (resultOption.Thing.race.leatherDef ?? ThingDefOf.Leather_Plain),
					AmountsPerSkills = new List<AmountBySkill>
					{
						new AmountBySkill
						{
							Skill = SkillDefOf.Animals,
							Count = (int)(ProductionMultiplier * Leather * StatExtension.GetStatValueAbstract((BuildableDef)(object)resultOption.Thing, StatDefOf.LeatherAmount, (ThingDef)null) * (float)resultOption.AmountsPerSkills[0].Count * (resultOption.Thing.HasComp(typeof(CompEggLayer)) ?  resultOption.Thing.GetCompProperties<CompProperties_EggLayer> ().eggCountRange.Average : resultOption.Thing.race.litterSizeCurve == null ? 1 : Rand.ByCurveAverage(resultOption.Thing.race.litterSizeCurve)) * 0.5 * 15/ ((resultOption.Thing.race?.gestationPeriodDays==null ? resultOption.Thing.GetCompProperties<CompProperties_EggLayer>() == null ? 0 : resultOption.Thing.GetCompProperties<CompProperties_EggLayer>().eggFertilizedDef.GetCompProperties<CompProperties_Hatcher>().hatcherDaystoHatch : resultOption.Thing.race.gestationPeriodDays)+resultOption.Thing.race.lifeStageAges.Last().minAge*60))
						}
					}
				},
				new ResultOption
				{
					Thing = (resultOption.Thing.race.meatDef ?? ThingDefOf.Cow.race.meatDef ?? ThingDefOf.Meat_Human),
					AmountsPerSkills = new List<AmountBySkill>
					{
						new AmountBySkill
						{
							Skill = SkillDefOf.Animals,
							Count = (int)(ProductionMultiplier * Meat * StatExtension.GetStatValueAbstract((BuildableDef)(object)resultOption.Thing, StatDefOf.MeatAmount, (ThingDef)null) * (float)resultOption.AmountsPerSkills[0].Count * (resultOption.Thing.HasComp(typeof(CompEggLayer)) ?  resultOption.Thing.GetCompProperties<CompProperties_EggLayer> ().eggCountRange.Average : resultOption.Thing.race.litterSizeCurve == null ? 1 : Rand.ByCurveAverage(resultOption.Thing.race.litterSizeCurve)) * 0.5 * 15/ ((resultOption.Thing.race?.gestationPeriodDays==null ? resultOption.Thing.GetCompProperties<CompProperties_EggLayer>() == null ? 0 : resultOption.Thing.GetCompProperties<CompProperties_EggLayer>().eggFertilizedDef.GetCompProperties<CompProperties_Hatcher>().hatcherDaystoHatch : resultOption.Thing.race.gestationPeriodDays)+resultOption.Thing.race.lifeStageAges.Last().minAge*60))
						}
					}
				}
			};
			CompProperties_Milkable milkies = resultOption.Thing.GetCompProperties<CompProperties_Milkable>();
			if(milkies != null){
				outy.Add(
					new ResultOption{
						Thing = milkies.milkDef,
						AmountsPerSkills = new List<AmountBySkill>{
							new AmountBySkill{
								Skill = SkillDefOf.Animals,
								Count = (int)(ProductionMultiplier * Milk * resultOption.AmountsPerSkills[0].Count * (milkies.milkFemaleOnly ? 0.5 : 1) * milkies.milkAmount / milkies.milkIntervalDays * 15)

							}
						}
					}


				);
			}
			CompProperties_Shearable shearies = resultOption.Thing.GetCompProperties<CompProperties_Shearable>();
			if(shearies != null){
				outy.Add(
					new ResultOption{
						Thing = shearies.woolDef,
						AmountsPerSkills = new List<AmountBySkill>{
							new AmountBySkill{
								Skill = SkillDefOf.Animals,
								Count = (int)(ProductionMultiplier * Wool * resultOption.AmountsPerSkills[0].Count * shearies.woolAmount / shearies.shearIntervalDays * 15)

							}
						}
					}


				);
			}
			CompProperties_EggLayer eggies = resultOption.Thing.GetCompProperties<CompProperties_EggLayer>();
			if(eggies != null && eggies.eggProgressUnfertilizedMax == 1){
				outy.Add(
					new ResultOption{
						Thing = eggies.eggUnfertilizedDef,
						AmountsPerSkills = new List<AmountBySkill>{
							new AmountBySkill{
								Skill = SkillDefOf.Animals,
								Count = (int)(ProductionMultiplier * Egg * resultOption.AmountsPerSkills[0].Count * (eggies.eggLayFemaleOnly ? 0.5 : 1)* eggies.eggCountRange.Average / eggies.eggLayIntervalDays * 15)

							}
						}
					}


				);
			}
			return outy;
		}
	}

	public override IEnumerable<ResultOption> GetExtraOptions()
	{
		Log.Message("Checking animals hopefully");
		foreach(PawnKindDef pkd in DefDatabase<PawnKindDef>.AllDefs){
			if(pkd.race?.tradeTags != null && pkd.race.tradeTags.Contains("AnimalFarm")){ Log.Message(string.Format("{0:N3}", CountMultiplier/(pkd.race.race.baseHungerRate+pkd.race.race.baseBodySize)) + ':' + pkd.label);}}
		return from pkd in (from pkd in DefDatabase<PawnKindDef>.AllDefs
				where pkd.race?.tradeTags != null && pkd.race.tradeTags.Contains("AnimalFarm") || pkd.label=="boomalope"
				select pkd)
			select new ResultOption
			{
				Thing = pkd.race,
				AmountsPerSkills = new List<AmountBySkill>
				{
					new AmountBySkill
					{
						Skill = SkillDefOf.Animals,
						Count = (int)Math.Ceiling(CountMultiplier/(pkd.race.race.baseHungerRate+pkd.race.race.baseBodySize))

					}
				}
			};
	}

}



