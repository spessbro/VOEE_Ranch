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
[PostToSetings("Outposts.Setting.AllowNonGrazers", PostToSetingsAttribute.DrawMode.Checkbox,false)]
public static bool AllowNonGrazers = false;
[PostToSetings("Outposts.Settings.BodySize", PostToSetingsAttribute.DrawMode.Percentage, 1f, 0.01f, 2f, null, null)]
public float BodySize = 1f;
[PostToSetings("Outposts.Settings.HungerRate", PostToSetingsAttribute.DrawMode.Percentage, 1f, 0.01f, 2f, null, null)]
public float HungerRate = 1f;
//[PostToSetings("Outposts.Settings.Wildness", PostToSetingsAttribute.DrawMode.Percentage, 1f, 0.01f, 2f, null, null)]
//public float Wildness = 1f;

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

public ThingDef animalRaised;
public float CurrentAnimals;
public int MaxAnimals;
public float ToRaise;

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
			int FutureAnimals = (int)Math.Ceiling(CurrentAnimals+ToRaise);
			if(animalRaised == null){return outy;}
			if(FutureAnimals > MaxAnimals){
				if(StatExtension.GetStatValueAbstract(animalRaised, StatDefOf.LeatherAmount, null) > 0){
					outy.Add(new ResultOption{
						Thing = (animalRaised.race.leatherDef ?? ThingDefOf.Leather_Plain),
						BaseAmount = (int)(ProductionMultiplier * Leather * StatExtension.GetStatValueAbstract(animalRaised, StatDefOf.LeatherAmount, (ThingDef)null) * (FutureAnimals-MaxAnimals))
						});
					}
				if(StatExtension.GetStatValueAbstract(animalRaised, StatDefOf.MeatAmount, null) > 0){
					outy.Add(new ResultOption{
						Thing = animalRaised.race.meatDef,
						BaseAmount = (int)(ProductionMultiplier * Meat * StatExtension.GetStatValueAbstract(animalRaised, StatDefOf.MeatAmount, (ThingDef)null) * (FutureAnimals-MaxAnimals))
						});
					}
				if(animalRaised.butcherProducts!=null){
					foreach(ThingDefCountClass P in animalRaised.butcherProducts){

							outy.Add(new ResultOption{
								Thing = P.thingDef,
								BaseAmount = (int)(ProductionMultiplier * P.count * (FutureAnimals-MaxAnimals))
							});
						}
					}
				}
			CompProperties_Milkable milkies = animalRaised.GetCompProperties<CompProperties_Milkable>();
			if(milkies != null){
				outy.Add(
					new ResultOption{
						Thing = milkies.milkDef,
						BaseAmount = (int)(ProductionMultiplier * Milk * FutureAnimals * (milkies.milkFemaleOnly ? 0.5 : 1) * milkies.milkAmount / milkies.milkIntervalDays * 15)
					}


				);
			}

			CompProperties_Shearable shearies = animalRaised.GetCompProperties<CompProperties_Shearable>();
			if(shearies != null){
				outy.Add(
					new ResultOption{
						Thing = shearies.woolDef,
						BaseAmount = (int)(ProductionMultiplier * Wool * FutureAnimals * shearies.woolAmount / shearies.shearIntervalDays * 15)

					}


				);
			}
			CompProperties_EggLayer eggies = animalRaised.GetCompProperties<CompProperties_EggLayer>();
			if(eggies != null && eggies.eggProgressUnfertilizedMax == 1){
				outy.Add(
					new ResultOption{
						Thing = eggies.eggUnfertilizedDef,
						BaseAmount = (int)(ProductionMultiplier * Egg * FutureAnimals * (eggies.eggLayFemaleOnly ? 0.5 : 1)* eggies.eggCountRange.Average / eggies.eggLayIntervalDays * 15)

					}


				);
			}
			AnimalBehaviours.CompProperties_AnimalProduct otheries = animalRaised.GetCompProperties<AnimalBehaviours.CompProperties_AnimalProduct>();
			if(otheries?.resourceDef != null){
				outy.Add(
					new ResultOption{
						Thing = otheries.resourceDef,
						BaseAmount = (int)(ProductionMultiplier * Other * FutureAnimals * otheries.resourceAmount / otheries.gatheringIntervalDays * 15)

					}


				);
			}
			return outy;
		}
	}

	public override void Produce(){
		MaxAnimals = (int) Math.Ceiling(TotalSkill(SkillDefOf.Animals)*CountMultiplier/(HungerRate*animalRaised.race.baseHungerRate+BodySize*animalRaised.race.baseBodySize));
		ToRaise = (float)(CurrentAnimals*0.5*(AverageOffspringCount(animalRaised)*(float)15/TimeFromConceptionTilAdult(animalRaised)));
		//Log.Message("upped to "+string.Format("{0:N3}",(CurrentAnimals+ToRaise))+" "+animalRaised.label);
		base.Produce();
		CurrentAnimals+= (float)(ToRaise);
		CurrentAnimals = CurrentAnimals > MaxAnimals ? MaxAnimals : CurrentAnimals;
		//Log.Message("Current is now :"+CurrentAnimals.ToString());
	}


	private static bool spawnCheck(int tile, List<Pawn> pawns){
		List<Caravan> C = Find.WorldObjects.Caravans.Where(c => c.IsPlayerControlled && c.Tile == tile).ToList();
		//Log.Message("Here?");
		List<Pawn> creatorAnimals = C.Where(c => c.ContainsPawn(pawns.FirstOrDefault())).FirstOrDefault().PawnsListForReading.Where(p => p.RaceProps.Animal).ToList();
		//Log.Message("There");
		List<ThingDef> AnimalTypes = new List<ThingDef>();
		foreach(Pawn a in creatorAnimals){
			if(!AnimalTypes.Contains(a.def) && a.RaceProps.hasGenders && (a.RaceProps.Eats(FoodTypeFlags.Tree) || a.RaceProps.Eats(FoodTypeFlags.Plant) || AllowNonGrazers)){
				AnimalTypes.Add(a.def);
			}
		}
		foreach(ThingDef d in AnimalTypes){
			if(creatorAnimals.Any(a => a.gender == Gender.Female && a.def == d) && creatorAnimals.Any(a => a.gender == Gender.Male && a.def == d)){
				return true;
			}
		}
		return false;
	}


	public static string CanSpawnOnWith(int tile, List<Pawn> pawns) => !spawnCheck(tile,pawns) ? "VOEE.Ranch.Moses".Translate() : null;


	public static string RequirementString(int tile, List<Pawn> pawns) => "VOEE.Ranch.Moses".Translate().Requirement(spawnCheck(tile,pawns));


	public void Generate(){
		List<Pawn> occupants = (List<Pawn>)AllPawns;
		List<ThingDef> AnimalTypes = new List<ThingDef>();
		foreach(Pawn a in occupants.Where(p => p.RaceProps.Animal)){
			if(!AnimalTypes.Contains(a.def) && a.RaceProps.hasGenders && (a.RaceProps.Eats(FoodTypeFlags.Tree) || a.RaceProps.Eats(FoodTypeFlags.Plant) || AllowNonGrazers)){
				AnimalTypes.Add(a.def);
			}
		}
		foreach(ThingDef d in AnimalTypes){
			if((occupants).Any(a => a.gender == Gender.Female && a.def == d) && (occupants).Any(a => a.gender == Gender.Male && a.def == d)){
				animalRaised = d;
				break;
			}
		}
		//Log.Warning(occupants.Count().ToString());
		CurrentAnimals = ((List<Pawn>)AllPawns).Where(o => o.def == animalRaised).Count();
		//Log.Message("Starting with "+CurrentAnimals.ToString()+" "+animalRaised.label);
		((List<Pawn>)AllPawns).RemoveAll(p => p.def== animalRaised);
		//Log.Message(((List<Pawn>)AllPawns).Count().ToString());
		MaxAnimals = (int) Math.Ceiling(TotalSkill(SkillDefOf.Animals)*CountMultiplier/(HungerRate*animalRaised.race.baseHungerRate+BodySize*animalRaised.race.baseBodySize));
		ToRaise = (float)(CurrentAnimals*0.5*(AverageOffspringCount(animalRaised)*15.0/TimeFromConceptionTilAdult(animalRaised)));
	}

	public override void Tick(){
		if(animalRaised==null){
			Generate();
		}
		if(AllPawns.Any(o => o.def ==animalRaised)){
				CurrentAnimals = AllPawns.Where(o => o.def == animalRaised).Count();
				((List<Pawn>)AllPawns).RemoveAll(o => o.def == animalRaised);
			}
		base.Tick();
	}
	public override string ProductionString(){
		string outie = "VOEE.Ranch.HerdSize".Translate(((int)CurrentAnimals).ToString()+" "+animalRaised.label).RawText;
		outie += "\n"+"VOEE.Ranch.MaxHerdSize".Translate((int)MaxAnimals).RawText;
		if(base.ProductionString().Count()>0){
			outie += "\n"+base.ProductionString();
			}
		return outie;
	}


	public override void ExposeData()
	{
		base.ExposeData();
		Scribe_Defs.Look<ThingDef>(ref animalRaised, "animalRaised");
		Scribe_Values.Look<float>(ref CurrentAnimals, "CurrentAnimals", 0, false);
	}

	public override void PostAdd()
	{
		base.PostAdd();
		if(animalRaised != null){
			MaxAnimals = (int) Math.Ceiling(TotalSkill(SkillDefOf.Animals)*CountMultiplier/(HungerRate*animalRaised.race.baseHungerRate+BodySize*animalRaised.race.baseBodySize));
			ToRaise = (float)(CurrentAnimals*0.5*(AverageOffspringCount(animalRaised)*(float)15/TimeFromConceptionTilAdult(animalRaised)));
		}

	}
}



