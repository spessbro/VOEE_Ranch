using System;
using System.Collections.Generic;
using System.Linq;
using Outposts;
using AnimalBehaviours;
using RimWorld;
using RimWorld.Planet;
using UnityEngine;
using Verse;

namespace VOEE;

public class Outpost_Ranching : Outpost
{
[PostToSetings("Outposts.Settings.BodySize", PostToSetingsAttribute.DrawMode.Percentage, 1f, 0.01f, 2f, null, null)]
public float BodySize = 1f;
[PostToSetings("Outposts.Settings.HungerRate", PostToSetingsAttribute.DrawMode.Percentage, 1f, 0.01f, 2f, null, null)]
public float HungerRate = 1f;
[PostToSetings("Outposts.Settings.Wildness", PostToSetingsAttribute.DrawMode.Percentage, 1f, 0.01f, 2f, null, null)]
public float Wildness = 1f;

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

[PostToSetings("Outposts.Settings.OtherProduct", PostToSetingsAttribute.DrawMode.Percentage, 0.5f, 0.01f, 2f, null, null)]
public float Other = 0.5f;

[PostToSetings("Outposts.Settings.Production", PostToSetingsAttribute.DrawMode.Percentage, 0.5f, 0.01f, 5f, null, null)]
public float ProductionMultiplier = 0.5f;
[PostToSetings("Outposts.Settings.Count", PostToSetingsAttribute.DrawMode.Percentage, 1f, 0.01f, 5f, null, null)]
public float CountMultiplier = 1f;

private ThingDef animalRaised;
private float CurrentAnimals;
private int MaxAnimals;

	private int TimeFromConceptionTilAdult(ThingDef race){
		return (int)((race.race?.gestationPeriodDays==null ?
					race.GetCompProperties<CompProperties_EggLayer>() == null ?
						 0 :
/*egg time*/			 	race.GetCompProperties<CompProperties_EggLayer>().eggFertilizedDef.GetCompProperties<CompProperties_Hatcher>().hatcherDaystoHatch :
						 	race.race.gestationPeriodDays)
				+race.race.lifeStageAges.Last().minAge*60);
	}
	private int AverageOffspringCount(ThingDef race){
		return (int)( race.HasComp(typeof(CompEggLayer)) ?
			 	race.GetCompProperties<CompProperties_EggLayer> ().eggCountRange.Average :
			 	race.race.litterSizeCurve == null ?
			 		1 :
			 		Rand.ByCurveAverage(race.race.litterSizeCurve));
	}

public override List<ResultOption> ResultOptions
	{

		get
		{
			List <ResultOption> outy = new List<ResultOption>();
			if(CurrentAnimals > MaxAnimals){
				if(StatExtension.GetStatValueAbstract(animalRaised, StatDefOf.LeatherAmount, null) > 0){
					outy.Add(new ResultOption{
							Thing = (animalRaised.race.leatherDef ?? ThingDefOf.Leather_Plain),
							BaseAmount = (int)(ProductionMultiplier * Leather * StatExtension.GetStatValueAbstract(animalRaised, StatDefOf.LeatherAmount, (ThingDef)null) * (CurrentAnimals-MaxAnimals))
							});
						}
				if(StatExtension.GetStatValueAbstract(animalRaised, StatDefOf.MeatAmount, null) > 0){
					outy.Add(new ResultOption{
							Thing = (animalRaised.race.leatherDef ?? ThingDefOf.Leather_Plain),
							BaseAmount = (int)(ProductionMultiplier * Meat * StatExtension.GetStatValueAbstract(animalRaised, StatDefOf.MeatAmount, (ThingDef)null) * (CurrentAnimals-MaxAnimals))
							});
						}
				}
			CompProperties_Milkable milkies = animalRaised.GetCompProperties<CompProperties_Milkable>();
			if(milkies != null){
				outy.Add(
					new ResultOption{
						Thing = milkies.milkDef,
						BaseAmount = (int)(ProductionMultiplier * Milk * CurrentAnimals * (milkies.milkFemaleOnly ? 0.5 : 1) * milkies.milkAmount / milkies.milkIntervalDays * 15)
					}


				);
			}
			CompProperties_Shearable shearies = animalRaised.GetCompProperties<CompProperties_Shearable>();
			if(shearies != null){
				outy.Add(
					new ResultOption{
						Thing = shearies.woolDef,
						BaseAmount = (int)(ProductionMultiplier * Wool * CurrentAnimals * shearies.woolAmount / shearies.shearIntervalDays * 15)

					}


				);
			}
			CompProperties_EggLayer eggies = animalRaised.GetCompProperties<CompProperties_EggLayer>();
			if(eggies != null && eggies.eggProgressUnfertilizedMax == 1){
				outy.Add(
					new ResultOption{
						Thing = eggies.eggUnfertilizedDef,
						BaseAmount = (int)(ProductionMultiplier * Egg * CurrentAnimals * (eggies.eggLayFemaleOnly ? 0.5 : 1)* eggies.eggCountRange.Average / eggies.eggLayIntervalDays * 15)

					}


				);
			}
			AnimalBehaviours.CompProperties_AnimalProduct otheries = animalRaised.GetCompProperties<AnimalBehaviours.CompProperties_AnimalProduct>();
			if(otheries?.resourceDef != null){
				outy.Add(
					new ResultOption{
						Thing = otheries.resourceDef,
						BaseAmount = (int)(ProductionMultiplier * Other * CurrentAnimals * otheries.resourceAmount / otheries.gatheringIntervalDays * 15)

					}


				);
			}
			return outy;
		}
	}

	public override void Produce(){
		MaxAnimals = (int) Math.Ceiling(TotalSkill(SkillDefOf.Animals)*CountMultiplier/(HungerRate*animalRaised.race.baseHungerRate+BodySize*animalRaised.race.baseBodySize));
		CurrentAnimals+= AverageOffspringCount(animalRaised)*15/TimeFromConceptionTilAdult(animalRaised);
		base.Produce();
		CurrentAnimals = CurrentAnimals > MaxAnimals ? MaxAnimals : CurrentAnimals;
	}



	public string CanSpawnOnWith(int tile, List<Pawn> pawns) {
		List<Caravan> C = (List<Caravan>)Find.WorldObjects.Caravans.Where(c => c.IsPlayerControlled && c.Tile == tile);
		List<Pawn> creatorAnimals =(List<Pawn>) ((from c in C where c.ContainsPawn(pawns.FirstOrDefault()) select c).FirstOrDefault().PawnsListForReading.Where(p => p.RaceProps.Animal));
		List<ThingDef> AnimalTypes = new List<ThingDef>();
		foreach(Pawn a in creatorAnimals){
			if(!AnimalTypes.Contains(a.def) && a.RaceProps.hasGenders){
				AnimalTypes.Add(a.def);
			}
		}
		bool match = false;
		foreach(ThingDef d in AnimalTypes){
			if(creatorAnimals.Any(a => a.gender == Gender.Female && a.def == d) && creatorAnimals.Any(a => a.gender == Gender.Male && a.def == d)){
				match = true;
				break;
			}
		}

		return match ? "VOEE.Ranch.Moses".Translate() : null;

	}

	public override void PostMake(){
		base.PostMake();
		List<ThingDef> AnimalTypes = new List<ThingDef>();
		foreach(Pawn a in AllPawns.Where(p => p.RaceProps.Animal)){
			if(!AnimalTypes.Contains(a.def) && a.RaceProps.hasGenders){
				AnimalTypes.Add(a.def);
			}
		}
		foreach(ThingDef d in AnimalTypes){
			if(((List<Pawn>)AllPawns).Any(a => a.gender == Gender.Female && a.def == d) && ((List<Pawn>)AllPawns).Any(a => a.gender == Gender.Male && a.def == d)){
				animalRaised = d;
				break;
			}
		}
		CurrentAnimals = ((List<Pawn>)AllPawns).Where(o => o.def == animalRaised).Count();
		IEnumerable<Pawn> lordHelpMe = ((List<Pawn>)AllPawns).Where(o => o.def != animalRaised);
		((List<Pawn>)AllPawns).Clear();
		((List<Pawn>)AllPawns).InsertRange(0,lordHelpMe);
	}

	public override void Tick(){
	if(AllPawns.Any(o => o.def ==animalRaised)){
			CurrentAnimals = AllPawns.Where(o => o.def == animalRaised).Count();
			((List<Pawn>)AllPawns).RemoveAll(o => o.def == animalRaised);
		}
	}
}



