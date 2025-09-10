﻿using BiomesCore;
using BiomesCore.DefModExtensions;
using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace MapDesigner
{
    public static class HelperMethods
    {
        public static float GetHillSize()
        {
            return MapDesignerMod.mod.settings.hillSize;
        }

        public static float GetHillSmoothness()
        {
            return MapDesignerMod.mod.settings.hillSmoothness;
        }


        public static void InitBiomeDefaults()
        {
            Log.Message("[Map Designer] Finding biomes...");
            MapDesignerSettings settings = MapDesignerMod.mod.settings;

            // Biomes
            Dictionary<string, BiomeDefault>  biomeDefaults = new Dictionary<string, BiomeDefault>();
            try
            {
                foreach (BiomeDef biome in DefDatabase<BiomeDef>.AllDefs)
                {
                    BiomeDefault biodef = new BiomeDefault();
                    biodef.animalDensity = biome.animalDensity;
                    biodef.plantDensity = biome.plantDensity;
                    biodef.wildPlantRegrowDays = biome.wildPlantRegrowDays;
                    biodef.terrain = new TerrainDefault()
                    {
                        terrainsByFertility = new List<TerrainThreshold>(biome.terrainsByFertility),
                        terrainPatchMakers = new List<TerrainPatchMaker>(biome.terrainPatchMakers)
                    };
                    biomeDefaults.Add(biome.defName, biodef);
                }
                settings.biomeDefaults = biomeDefaults;
            }
            catch
            {
                Log.Message("[Map Designer] Could not initialize biome defaults");
            }


            // Densities
            Dictionary<string, FloatRange> densityDefaults = new Dictionary<string, FloatRange>();
            try
            {
                GenStepDef step = DefDatabase<GenStepDef>.GetNamed("ScatterRuinsSimple");
                densityDefaults.Add(step.defName, (step.genStep as GenStep_Scatterer).countPer10kCellsRange);

                step = DefDatabase<GenStepDef>.GetNamed("ScatterShrines");
                densityDefaults.Add(step.defName, (step.genStep as GenStep_Scatterer).countPer10kCellsRange);

                step = DefDatabase<GenStepDef>.GetNamed("SteamGeysers");
                densityDefaults.Add(step.defName, (step.genStep as GenStep_Scatterer).countPer10kCellsRange);

                settings.densityDefaults = densityDefaults;

                if (ModsConfig.IdeologyActive)
                {
                    try
                    {
                        step = DefDatabase<GenStepDef>.GetNamed("MechanoidRemains");
                        densityDefaults.Add(step.defName, (step.genStep as GenStep_ScatterGroup).countPer10kCellsRange);

                        step = DefDatabase<GenStepDef>.GetNamed("AncientPipelineSection");
                        densityDefaults.Add(step.defName, (step.genStep as GenStep_ScatterThings).countPer10kCellsRange);

                        step = DefDatabase<GenStepDef>.GetNamed("AncientJunkClusters");
                        densityDefaults.Add(step.defName, (step.genStep as GenStep_ScatterGroup).countPer10kCellsRange);
                    }
                    catch
                    {
                        Log.Message("[Map Designer] Couldn't initialize Ideology Things. Ideology settings may not work.");
                    }
                }

                if (ModsConfig.BiotechActive)
                {
                    try
                    {
                        step = DefDatabase<GenStepDef>.GetNamed("AncientPollutionJunk");
                        densityDefaults.Add(step.defName, (step.genStep as GenStep_ScatterGroup).countPer10kCellsRange);

                        //step = DefDatabase<GenStepDef>.GetNamed("AncientExostriderRemains");
                        //int count = (step.genStep as GenStep_ScatterLayout).count;
                        //densityDefaults.Add(step.defName, new FloatRange(count, count));
                    }
                    catch
                    {
                        Log.Message("[Map Designer] Couldn't initialize Biotech Things. Biotech settings may not work.");
                    }
                }

                if (ModsConfig.IsActive("DankPyon.Medieval.Overhaul") || ModsConfig.IsActive("DankPyon.Medieval.Overhaul_steam"))
                {
                    try
                    {
                        step = DefDatabase<GenStepDef>.GetNamed("DankPyon_BattlefieldRemains");
                        densityDefaults.Add(step.defName, (step.genStep as GenStep_ScatterGroup).countPer10kCellsRange);

                        step = DefDatabase<GenStepDef>.GetNamed("DankPyon_TarPits");
                        densityDefaults.Add(step.defName, (step.genStep as GenStep_ScatterThings).countPer10kCellsRange);

                        step = DefDatabase<GenStepDef>.GetNamed("DankPyon_BeeHives");
                        densityDefaults.Add(step.defName, (step.genStep as GenStep_ScatterThings).countPer10kCellsRange);

                        step = DefDatabase<GenStepDef>.GetNamed("DankPyon_HornetNest");
                        densityDefaults.Add(step.defName, (step.genStep as GenStep_ScatterThings).countPer10kCellsRange);
                    }
                    catch
                    {
                        Log.Message("[Map Designer] Couldn't initialize Medieval Overhaul Things. Medieval Overhaul settings may not work.");
                    }
                }

            }
            catch
            {
                Log.Message("[Map Designer] Could not initialize density defaults");
            }

            // Rocks
            try
            {
                if (settings.allowedRocks.EnumerableNullOrEmpty())
                {
                    settings.allowedRocks = new Dictionary<string, bool>();
                }
                List<ThingDef> list = GetRockList();
                foreach (ThingDef rock in list)
                {
                    if (!settings.allowedRocks.ContainsKey(rock.defName))
                    {
                        settings.allowedRocks.Add(rock.defName, true);
                    }
                }
                settings.rockTypeRange.max = Math.Min(list.Count, settings.rockTypeRange.max);
                settings.rockTypeRange.min = Math.Min(list.Count, settings.rockTypeRange.min);
            }
            catch
            {
                Log.Message("[Map Designer] Could not initialize rock types");
                settings.rockTypeRange.max = 3;
                settings.rockTypeRange.min = 2;
            }

            // Ore

            try
            {
                Dictionary<string, float> oreDefaults = new Dictionary<string, float>();
                if (settings.oreCommonality.EnumerableNullOrEmpty())
                {
                    settings.oreCommonality = new Dictionary<string, float>();
                }
                List<ThingDef> list = GetMineableList();
                foreach (ThingDef ore in list)
                {
                    oreDefaults.Add(ore.defName, ore.building.mineableScatterCommonality);

                    if (!settings.oreCommonality.ContainsKey(ore.defName))
                    {
                        settings.oreCommonality.Add(ore.defName, 1f);
                    }
                }

                settings.oreDefaults = oreDefaults;
            }
            catch
            {
                Log.Message("[Map Designer] Could not initialize ore types");
            }

            // Rivers
            Dictionary<string, float> rivers = new Dictionary<string, float>();
            foreach (RiverDef river in DefDatabase<RiverDef>.AllDefs)
            {
                rivers.Add(river.defName, river.widthOnMap);
            }
            settings.riverDefaults = rivers;


            // Odyssey
            if (ModsConfig.OdysseyActive)
            {
                try
                {
                    InitSelectedMutators();
                }
                catch
                {
                    Log.Message("[Map Designer] Could not initialize Odyssey mutators");
                }
            }
        }


        public static void InitSelectedMutators()
        {
            Dictionary<string, string> selMutDict = new Dictionary<string, string>();

            List<TileMutatorDef> allMutators = DefDatabase<TileMutatorDef>.AllDefsListForReading;

            List<string> allCategories = new List<string>();
            foreach (TileMutatorDef item in allMutators)
            {
                foreach (string cat in item.categories)
                {
                    if (!allCategories.Contains(cat))
                    {
                        allCategories.Add(cat);
                    }
                }
            }

            foreach (string cat in allCategories)
            {
                if (!selMutDict.ContainsKey(cat))
                {
                    selMutDict.Add(cat, ZmdDefOf.ZMD_NoMutator.defName);
                }
            }

            MapDesignerMod.mod.settings.selectedMutators = selMutDict;
        }

        public static void ApplyBiomeSettings()
        {
            Log.Message("[Map Designer] Applying settings");
            MapDesignerSettings settings = MapDesignerMod.mod.settings;

            // densities
            Dictionary<string, BiomeDefault> biomeDefaults = settings.biomeDefaults;

            float densityPlant = settings.densityPlant;
            float densityAnimal = settings.densityAnimal;
            foreach (BiomeDef biome in DefDatabase<BiomeDef>.AllDefs)
            {
                try
                {
                    biome.animalDensity = biomeDefaults[biome.defName].animalDensity * densityAnimal;
                    biome.plantDensity = biomeDefaults[biome.defName].plantDensity * densityPlant;

                    if (biome.plantDensity > 1f)
                    {
                        biome.wildPlantRegrowDays = biomeDefaults[biome.defName].wildPlantRegrowDays / biome.plantDensity;
                        biome.plantDensity = 1f;
                    }
                }
                catch
                {
                    Log.Message("[Map Designer] ERROR applying plant and animal settings to " + biome.defName);
                }
            }

            Dictionary<string, FloatRange> densityDefaults = settings.densityDefaults;

            // ruins
            try
            {
                float densityRuins = settings.densityRuins;
                if (densityRuins > 1)
                {
                    densityRuins = (float)Math.Pow(densityRuins, 3);
                }
                (DefDatabase<GenStepDef>.GetNamed("ScatterRuinsSimple").genStep as GenStep_Scatterer).countPer10kCellsRange.min = densityDefaults["ScatterRuinsSimple"].min * densityRuins;
                (DefDatabase<GenStepDef>.GetNamed("ScatterRuinsSimple").genStep as GenStep_Scatterer).countPer10kCellsRange.max = densityDefaults["ScatterRuinsSimple"].max * densityRuins;
            }
            catch
            {
                Log.Message("[Map Designer] ERROR with settings: ruins");
            }

            // ancient dangers
            try
            {
                float densityDanger = settings.densityDanger;
                if (densityDanger > 1)
                {
                    densityDanger = (float)Math.Pow(densityDanger, 4);
                }
                (DefDatabase<GenStepDef>.GetNamed("ScatterShrines").genStep as GenStep_Scatterer).countPer10kCellsRange.min = densityDefaults["ScatterShrines"].min * densityDanger;
                (DefDatabase<GenStepDef>.GetNamed("ScatterShrines").genStep as GenStep_Scatterer).countPer10kCellsRange.max = densityDefaults["ScatterShrines"].max * densityDanger;
            }
            catch
            {
                Log.Message("[Map Designer] ERROR with settings: ancient dangers");
            }

            // geysers
            try
            {
                float densityGeyser = settings.densityGeyser;
                if (densityGeyser > 1)
                {
                    densityGeyser = (float)Math.Pow(densityGeyser, 2);
                }
                (DefDatabase<GenStepDef>.GetNamed("SteamGeysers").genStep as GenStep_Scatterer).countPer10kCellsRange.min = densityDefaults["SteamGeysers"].min * densityGeyser;
                (DefDatabase<GenStepDef>.GetNamed("SteamGeysers").genStep as GenStep_Scatterer).countPer10kCellsRange.max = densityDefaults["SteamGeysers"].max * densityGeyser;
            }
            catch
            {
                Log.Message("[Map Designer] ERROR with settings: geysers");
            }

            // Ideology
            if (ModsConfig.IdeologyActive)
            {
                try
                {
                    int countMechs = settings.countMechanoidRemains;
                    (DefDatabase<GenStepDef>.GetNamed("MechanoidRemains").genStep as GenStep_ScatterGroup).count = countMechs;


                    float densityPipeline = settings.densityAncientPipelineSection;
                    if (densityPipeline > 1)
                    {
                        densityPipeline = (float)Math.Pow(densityPipeline, 2);
                    }
                    (DefDatabase<GenStepDef>.GetNamed("AncientPipelineSection").genStep as GenStep_ScatterThings).countPer10kCellsRange.min = densityDefaults["AncientPipelineSection"].min * densityPipeline;
                    (DefDatabase<GenStepDef>.GetNamed("AncientPipelineSection").genStep as GenStep_ScatterThings).countPer10kCellsRange.max = densityDefaults["AncientPipelineSection"].max * densityPipeline;


                    float densityJunk = settings.densityAncientJunkClusters;
                    if (densityJunk > 1)
                    {
                        densityJunk = (float)Math.Pow(densityJunk, 2);
                    }
                    (DefDatabase<GenStepDef>.GetNamed("AncientJunkClusters").genStep as GenStep_ScatterGroup).countPer10kCellsRange.min = densityDefaults["AncientJunkClusters"].min * densityJunk;
                    (DefDatabase<GenStepDef>.GetNamed("AncientJunkClusters").genStep as GenStep_ScatterGroup).countPer10kCellsRange.max = densityDefaults["AncientJunkClusters"].max * densityJunk;
                }
                catch
                {
                    Log.Message("[Map Designer] Couldn't apply settings to Ideology Things. Ideology settings may not work.");
                }

            }


            // Biotech
            if (ModsConfig.BiotechActive)
            {
                try
                {
                    float densityPollutionJunk = settings.densityAncientPollutionJunk;
                    if (densityPollutionJunk > 1)
                    {
                        densityPollutionJunk = (float)Math.Pow(densityPollutionJunk, 3);
                    }
                    (DefDatabase<GenStepDef>.GetNamed("AncientPollutionJunk").genStep as GenStep_ScatterGroup).countPer10kCellsRange.min = densityDefaults["AncientPollutionJunk"].min * densityPollutionJunk;
                    (DefDatabase<GenStepDef>.GetNamed("AncientPollutionJunk").genStep as GenStep_ScatterGroup).countPer10kCellsRange.max = densityDefaults["AncientPollutionJunk"].max * densityPollutionJunk;

                    int countExostrider = settings.countAncientExostriderRemains;
                    (DefDatabase<GenStepDef>.GetNamed("AncientExostriderRemains").genStep as GenStep_ScatterLayout).count = countExostrider;
                }
                catch
                {
                    Log.Message("[Map Designer] Couldn't apply settings to Biotech Things. Biotech settings may not work.");
                }
            }

            // Medieval Overhaul
            if (ModsConfig.IsActive("DankPyon.Medieval.Overhaul") || ModsConfig.IsActive("DankPyon.Medieval.Overhaul_steam"))
            {
                try
                {
                    float densityMO = settings.densityMOBattlefield;
                    if (densityMO > 1)
                    {
                        densityMO = (float)Math.Pow(densityMO, 3); (DefDatabase<GenStepDef>.GetNamed("DankPyon_BeeHives").genStep as GenStep_ScatterThings).minSpacing = 25;
                    }
                    else
                    {
                        (DefDatabase<GenStepDef>.GetNamed("DankPyon_BattlefieldRemains").genStep as GenStep_ScatterGroup).minSpacing = 85;
                    }
                    (DefDatabase<GenStepDef>.GetNamed("DankPyon_BattlefieldRemains").genStep as GenStep_ScatterGroup).countPer10kCellsRange.min = densityDefaults["DankPyon_BattlefieldRemains"].min * densityMO;
                    (DefDatabase<GenStepDef>.GetNamed("DankPyon_BattlefieldRemains").genStep as GenStep_ScatterGroup).countPer10kCellsRange.max = densityDefaults["DankPyon_BattlefieldRemains"].max * densityMO;

                    densityMO = settings.densityMOTarPit;
                    if (densityMO > 1)
                    {
                        densityMO = (float)Math.Pow(densityMO, 3);
                    }
                    (DefDatabase<GenStepDef>.GetNamed("DankPyon_TarPits").genStep as GenStep_ScatterThings).countPer10kCellsRange.min = densityDefaults["DankPyon_TarPits"].min * densityMO;
                    (DefDatabase<GenStepDef>.GetNamed("DankPyon_TarPits").genStep as GenStep_ScatterThings).countPer10kCellsRange.max = densityDefaults["DankPyon_TarPits"].max * densityMO;
                    
                    densityMO = settings.densityMOBeeHive;
                    if (densityMO > 1)
                    {
                        densityMO = (float)Math.Pow(densityMO, 3);
                        (DefDatabase<GenStepDef>.GetNamed("DankPyon_BeeHives").genStep as GenStep_ScatterThings).minSpacing = 50;
                    }
                    else
                    {
                        (DefDatabase<GenStepDef>.GetNamed("DankPyon_BeeHives").genStep as GenStep_ScatterThings).minSpacing = 200;
                    }
                    (DefDatabase<GenStepDef>.GetNamed("DankPyon_BeeHives").genStep as GenStep_ScatterThings).countPer10kCellsRange.min = densityDefaults["DankPyon_BeeHives"].min * densityMO;
                    (DefDatabase<GenStepDef>.GetNamed("DankPyon_BeeHives").genStep as GenStep_ScatterThings).countPer10kCellsRange.max = densityDefaults["DankPyon_BeeHives"].max * densityMO;
                    
                    
                    densityMO = settings.densityMOHornet;
                    if (densityMO > 1)
                    {
                        densityMO = (float)Math.Pow(densityMO, 3);
                        (DefDatabase<GenStepDef>.GetNamed("DankPyon_HornetNest").genStep as GenStep_ScatterThings).minSpacing = 50;
                    }
                    else
                    {
                        (DefDatabase<GenStepDef>.GetNamed("DankPyon_HornetNest").genStep as GenStep_ScatterThings).minSpacing = 200;
                    }
                    (DefDatabase<GenStepDef>.GetNamed("DankPyon_HornetNest").genStep as GenStep_ScatterThings).countPer10kCellsRange.min = densityDefaults["DankPyon_HornetNest"].min * densityMO;
                    (DefDatabase<GenStepDef>.GetNamed("DankPyon_HornetNest").genStep as GenStep_ScatterThings).countPer10kCellsRange.max = densityDefaults["DankPyon_HornetNest"].max * densityMO;

                }
                catch
                {
                    Log.Message("[Map Designer] Couldn't initialize Medieval Overhaul Things. Medieval Overhaul settings may not work.");
                }
            }

            // VPE
            //if (GenTypes.GetTypeInAnyAssembly("VanillaPowerExpanded.SpecialPowerSpawnsDef") != null)
            //{
            //    ApplyVPEPatches();
            //}

            // rivers
            foreach (RiverDef river in DefDatabase<RiverDef>.AllDefs)
            {
                try
                {
                    river.widthOnMap = settings.riverDefaults[river.defName] * settings.sizeRiver;

                    //river.widthOnMap *= settings.sizeRiver;
                    river.widthOnMap = Math.Min(175, river.widthOnMap);
                }
                catch
                {
                    Log.Message("[Map Designer] ERROR with settings: river width : " + river.defName);
                }
            }

            // terrain
            if (Math.Abs(settings.terrainFert - 1f) > 0.05 || Math.Abs(settings.terrainWater - 1f) > 0.05)
            {
                Log.Message(String.Format("[Map Designer] Terrain settings: fertility: {0} | Water {1}", Math.Round(100 * settings.terrainFert), Math.Round(100 * settings.terrainWater)));
                foreach (BiomeDef biome in DefDatabase<BiomeDef>.AllDefs)
                {
                    if (!biome.terrainsByFertility.NullOrEmpty())
                    {
                        //Log.Message("Doing adjustments for " + biome.defName);
                        try
                        {
                       
                            TerrainDefault newTerrain;
                            float minFertBound = -0.2f;
                            float maxFertBound = 1.2f;

                            if (ModsConfig.IsActive("BiomesTeam.BiomesCore"))
                            {
                                maxFertBound = HelperMethods.GetMaxFertByBiome(biome);
                            }
                            newTerrain = TerrainUtility.StretchTerrainFertility(biomeDefaults[biome.defName].terrain, minFertBound, maxFertBound);


                            biome.terrainsByFertility = newTerrain.terrainsByFertility;
                            biome.terrainPatchMakers = newTerrain.terrainPatchMakers;
                        }

                        catch (Exception e)
                        {
                            Log.Message("[Map Designer] ERROR with settings: terrain : " + biome.defName);
                            Log.Message(e.Message);

                            TerrainDefault dictEntry = settings.biomeDefaults[biome.defName].terrain;
                            Log.Message("--terrainsByFertility");
                            foreach (TerrainThreshold t in dictEntry.terrainsByFertility)
                            {
                                Log.Message(String.Format("- {0} .... {1} | {2}", t.terrain.defName, Math.Round(t.min, 2), Math.Round(t.max, 2)));
                            }
                            Log.Message("--terrainPatchMakers");
                            for (int i = 0; i < dictEntry.terrainPatchMakers.Count(); i++)
                            {
                                TerrainPatchMaker p = dictEntry.terrainPatchMakers[i];
                                Log.Message(String.Format("Patchmaker #{0} | min {1} | max {2}", i, p.minFertility, p.maxFertility));
                                foreach (TerrainThreshold t in p.thresholds)
                                {
                                    Log.Message(String.Format("--- {0} | {1} | {2}", t.terrain.defName, Math.Round(t.min, 2), Math.Round(t.max, 2)));
                                }
                            }
                        }
                    }
                }
            }
            else
            {
                Log.Message("[Map Designer] Terrain settings: Default");
                foreach (BiomeDef biome in DefDatabase<BiomeDef>.AllDefs)
                {
                    biome.terrainsByFertility = biomeDefaults[biome.defName].terrain.terrainsByFertility;
                    biome.terrainPatchMakers = biomeDefaults[biome.defName].terrain.terrainPatchMakers;
                }
            }

            // ore
            foreach (var ore in settings.oreDefaults)
            {
                float commonality = settings.oreCommonality[ore.Key];
                if (commonality > 1) { commonality *= commonality; }
                if (ThingDef.Named(ore.Key) != null)
                {
                    ThingDef.Named(ore.Key).building.mineableScatterCommonality = ore.Value * settings.oreCommonality[ore.Key];
                }
            }

        }

        public static float GetRiverDirection(float angle)
        {
            if (MapDesignerMod.mod.settings.flagRiverDir)
            {
                angle = MapDesignerMod.mod.settings.riverDir;
            }
            return angle;
        }


        public static float DistanceBetweenPoints(IntVec3 point1, IntVec3 point2)
        {
            float dist = 0;
            double xDist = Math.Pow(point1.x - point2.x, 2);
            double zDist = Math.Pow(point1.z - point2.z, 2);
            dist = (float)Math.Sqrt(xDist + zDist);

            return dist;
        }

        public static List<IntVec3> GenCircle(Map map, IntVec3 center, float radius)
        {
            List<IntVec3> circle = new List<IntVec3>();

            foreach (IntVec3 current in map.AllCells)
            {
                if (DistanceBetweenPoints(center, current) < radius)
                {
                    circle.Add(current);
                }
            }

            return circle;
        }


        public static string GetDisplayValue(float input, float power = 1)
        {
            if (input > 1)
            {
                input = (float)Math.Pow(input, power);
            }

            string output = "";
            if (input < 2)
            {
                output = String.Format("{0:0.0}", input);
            }
            else
            {
                output = String.Format("{0:0}", input);
            }
            return output;

        }


        public static List<ThingDef> GetRockList()
        {
            List<ThingDef> rockList = (from d in DefDatabase<ThingDef>.AllDefs
                                       where d.IsNonResourceNaturalRock
                                       select d).ToList<ThingDef>();
            return rockList;
        }

        public static List<ThingDef> GetMineableList()
        {

            List<ThingDef> oreList = (from d in DefDatabase<ThingDef>.AllDefs
                                      where d.building?.isResourceRock == true && d.building?.mineableThing != null
                                      select d).ToList<ThingDef>();

            return oreList;
        }



        // Compatibility 
        //public static void ApplyMapRerollPatches()
        //{
        //    MethodInfo targetmethod = AccessTools.Method(typeof(MapReroll.MapPreviewGenerator), "TerrainFrom");
        //    HarmonyMethod prefixmethod = new HarmonyMethod(typeof(Patches.MapReroll_TerrainFrom), "Prefix");
        //    new Harmony("zylle.MapDesigner_RerollCompat").Patch(targetmethod, prefixmethod);
        //}

        //public static void ApplyVPEPatches()
        //{
        //    try
        //    {
        //        DefDatabase<VanillaPowerExpanded.SpecialPowerSpawnsDef>.GetNamed("VPE_SpawnChemfuelPonds").numberToSpawn = MapDesignerMod.mod.settings.vpe_ChemfuelPonds;
        //        DefDatabase<VanillaPowerExpanded.SpecialPowerSpawnsDef>.GetNamed("VPE_SpawnHelixienVents").numberToSpawn = MapDesignerMod.mod.settings.vpe_HelixienVents;
        //    }
        //    catch
        //    {
        //        Log.Message("[Map Designer] Could not apply VFE settings");
        //    }
        //}

        public static event Action OnSettingsChanged;

        internal static void InvokeOnSettingsChanged()
        {
            OnSettingsChanged?.Invoke();
        }

        private static bool wasGuiChanged;

        internal static void BeginChangeCheck()
        {
            wasGuiChanged = GUI.changed;
            GUI.changed = false;
        }
        
        internal static void EndChangeCheck()
        {
            if (GUI.changed) InvokeOnSettingsChanged();
            GUI.changed = GUI.changed || wasGuiChanged;
        }

        public static float GetMaxFertByBiome(BiomeDef biome)
        {
            if (!biome.HasModExtension<BiomesCore.DefModExtensions.BiomesMap>())
            {
                return 1.2f;
            }
            if (biome.GetModExtension<BiomesMap>().isOasis)
            {
                return 17f;
            }
            if (biome.GetModExtension<BiomesMap>().isIsland)
            {
                return 17f;
            }

            return 1.2f;
        }


        public static string DictToString(Dictionary<string,float> dict)
        {
            string output = "";

            foreach(var entry in dict)
            {
                output += entry.Key + "-" + entry.Value + ",";
            }
            output += ";";
            return output;
        }

        public static string DictToString(this Dictionary<string, bool> dict)
        {
            string output = "";

            foreach (var entry in dict)
            {
                output += entry.Key + "-" + entry.Value + ",";
            }
            output += ";";

            return output;
        }

        public static string DictToString(this Dictionary<string, string> dict)
        {
            string output = "";

            foreach (var entry in dict)
            {
                output += entry.Key + "-" + entry.Value + ",";
            }
            output += ";";

            return output;
        }

    }
}
