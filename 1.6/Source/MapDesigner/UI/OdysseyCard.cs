using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace MapDesigner.UI
{

    public static class OdysseyCard
    {
        public static MapDesignerSettings settings = MapDesignerMod.mod.settings;
        private static float viewHeight;
        private static Vector2 scrollPosition = Vector2.zero;

        public static void DrawOdysseyCard(Rect rect)
        {
            
            HelperMethods.BeginChangeCheck();

            Rect rect2 = rect.ContractedBy(4f);

            // to correctly initialize scrollview height
            float height = 500f;
            //if (settings.flagHillRadial)
            //{
            //    height += 100f;
            //}
            Listing_Standard outerListing = new Listing_Standard();
            outerListing.Begin(rect);

            outerListing.Label("ZMD_odysseyTabInfo".Translate());

            if (outerListing.ButtonText("ZMD_disableMutators".Translate()))
            {
                DisableAllMutators();
            }

            // get the list of mutators
            List<TileMutatorDef> allMutators = DefDatabase<TileMutatorDef>.AllDefsListForReading;
            Log.Message("All Mutators gathered. Count: " + allMutators.Count);
            //get the list of categories
            List<string> allCategories = new List<string>();
            Log.Message("CATEGORIES:");
            foreach (TileMutatorDef item in allMutators)
            {
                foreach (string cat in item.categories)
                {
                    if (!allCategories.Contains(cat))
                    {
                        Log.Message(cat);
                        allCategories.Add(cat);
                    }
                }
            }
            allCategories.SortBy(c => c);

            //make the dropdown menus
            foreach(string cat in allCategories)
            {
                Log.Message("Making dropdown for " + cat);

                List<TileMutatorDef> catMuts = allMutators.Where(m => m.categories.Any(c => c== cat)).ToList();

                if(outerListing.ButtonTextLabeled(cat, GetSelectedMutatorByCategory(cat)))
                {
                    //TODO: Dropdown options, and display list
                    List<FloatMenuOption> mutList = new List<FloatMenuOption>();
                    foreach(TileMutatorDef mut in catMuts)
                    {
                        mutList.Add(new FloatMenuOption(mut.label, delegate
                        {
                            AddSelectedMutator(mut);
                            HelperMethods.InvokeOnSettingsChanged();
                        }));
                    }

                    Find.WindowStack.Add(new FloatMenu(mutList));
                }
            }



            outerListing.End();
            HelperMethods.EndChangeCheck();
        }


        public static string GetSelectedMutatorByCategory(string cat)
        {
            // gets the currently selected mutator for a given category, if any
            foreach(string mut in settings.selectedMutators)
            {
                if (DefDatabase<TileMutatorDef>.GetNamedSilentFail(mut).categories.Any(c => c == cat))
                {
                    return mut;
                }
            }
            return "ZMD_odyNoFeature".Translate();
        }

        public static void AddSelectedMutator(TileMutatorDef newMut)
        {
            //remove any conflicting mutators, then add the new one
            List<TileMutatorDef> muts = GetSelectedMutators();

            //remove all mutators with the same category
            muts.RemoveAll(m => m.categories.Intersect(newMut.categories).Any());

            //remove everything that conflicts with overrideCategories
            muts.RemoveAll(m => m.overrideCategories.Intersect(newMut.categories).Any());

            muts.Add(newMut);

            List<string> mutDefNames = muts.Select(m => m.defName).ToList();
            settings.selectedMutators = mutDefNames;
        }

        public static List<TileMutatorDef> GetSelectedMutators()
        {
            List<TileMutatorDef> list = new List<TileMutatorDef>();

            foreach(string mut in settings.selectedMutators)
            {
                list.Add(DefDatabase<TileMutatorDef>.GetNamedSilentFail(mut));
            }

            return list;
        }

        public static void DisableAllMutators()
        {
            settings.selectedMutators.Clear();
            HelperMethods.InvokeOnSettingsChanged();
        }



    }
}
