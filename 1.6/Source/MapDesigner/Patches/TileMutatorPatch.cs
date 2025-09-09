using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using Verse;
using static MapDesigner.MapDesignerSettings;
using Verse.Noise;
using RimWorld.Planet;

namespace MapDesigner.Patches
{
    /// <summary>
    /// Coast direction
    /// </summary>


    [HarmonyPatch(typeof(Verse.MapGenerator), "GenerateMap")]
    static class TileMutatorPatch
    {
        static void Postfix(MapParent parent)
        {
            //// MUTATORS
            //if (ModsConfig.OdysseyActive)
            //{
            //    if (parent.Map.IsPlayerHome && MapDesignerMod.mod.settings.flagOdyBeta)
            //    {
            //        Log.Message("Try Add Mutators");
            //        //map.Tile.Tile.Mutators.Clear();

            //        List<TileMutatorDef> existingMutators = parent.Map.Tile.Tile.Mutators.ToList();
            //        foreach (TileMutatorDef mutDef in existingMutators)
            //        {
            //            parent.Map.TileInfo.RemoveMutator(mutDef);
            //        }

            //        Dictionary<string, string> selectedMuts = MapDesignerMod.mod.settings.selectedMutators;
            //        foreach (KeyValuePair<string, string> cat in selectedMuts)
            //        {
            //            TileMutatorDef selMut = DefDatabase<TileMutatorDef>.GetNamedSilentFail(cat.Value);
            //            if (selMut != ZmdDefOf.ZMD_NoMutator)
            //            {
            //                parent.Map.TileInfo.AddMutator(selMut);
            //            }
            //        }

            //        foreach (TileMutatorDef mutator in parent.Map.TileInfo.Mutators)
            //        {
            //            mutator.Worker?.Init(parent.Map);
            //        }
            //    }
            //}
        }
    }

}
