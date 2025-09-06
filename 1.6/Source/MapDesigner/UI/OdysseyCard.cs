using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;
using Verse.Noise;

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

            GUI.color = new Color(255, 180, 0);
            outerListing.Label("ZMD_betaWarning".Translate());
            GUI.color = Color.white;

            if (outerListing.ButtonText("ZMD_disableMutators".Translate()))
            {
                DisableAllMutators();
            }

            // get the list of mutators
            List<TileMutatorDef> allMutators = DefDatabase<TileMutatorDef>.AllDefsListForReading;

            //get the list of categories
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

            allCategories.SortBy(c => c);

            //make the scroll window
            Rect windowRect = outerListing.GetRect(rect2.height - outerListing.CurHeight).ContractedBy(4f);

            Rect viewRect = new Rect(0f, 0f, 400f, 100f + 29f * allCategories.Count());

            Widgets.BeginScrollView(windowRect, ref scrollPosition, viewRect, true);

            Listing_Standard listing = new Listing_Standard();
            listing.Begin(viewRect);

            //make the dropdown menus
            foreach (string cat in allCategories)
            {
                List<TileMutatorDef> catMuts = allMutators.Where(m => m.categories.Any(c => c == cat)).ToList();

                if (listing.ButtonTextLabeled(cat, GetSelectedMutatorByCategory(cat).label))
                {
                    List<FloatMenuOption> mutOptionList = new List<FloatMenuOption>();
                    foreach (TileMutatorDef mut in catMuts)
                    {
                        mutOptionList.Add(new FloatMenuOption(mut.label, delegate
                        {
                            //AddSelectedMutator(mut);
                            settings.selectedMutators[cat] = mut;
                            HelperMethods.InvokeOnSettingsChanged();
                            SyncSelectedMutators(mut);
                        }));
                    }
                    Find.WindowStack.Add(new FloatMenu(mutOptionList));
                }
                
            }

            listing.End();
            Widgets.EndScrollView();
            outerListing.End();
            HelperMethods.EndChangeCheck();
        }


        public static void SyncSelectedMutators(TileMutatorDef mut)
        {
            //// get the list of mutators
            //List<TileMutatorDef> allMutators = DefDatabase<TileMutatorDef>.AllDefsListForReading;

            ////get the list of categories
            //List<string> allCategories = new List<string>();
            //foreach (TileMutatorDef item in allMutators)
            //{
            //    foreach (string cat in item.categories)
            //    {
            //        if (!allCategories.Contains(cat))
            //        {
            //            allCategories.Add(cat);
            //        }
            //    }
            //}

            //// disable mutators with conflicting categories
            //foreach (string cat in allCategories)
            //{
            //    if (settings.selectedMutators[cat].categories.Any(e => mut.categories.Contains(e)))
            //    {
            //        settings.selectedMutators[cat] = ZmdDefOf.ZMD_NoMutator;

            //    }

            //}

            // everything syncs to the newly selected mutator
            foreach (string c in mut.categories)
            {
                settings.selectedMutators[c] = mut;
            }

            // remove mutatores that conflict with override categories
            foreach (string c in mut.overrideCategories)
            {
                settings.selectedMutators[c] = ZmdDefOf.ZMD_NoMutator;
            }
        }


        public static TileMutatorDef GetSelectedMutatorByCategory(string cat)
        {
            // gets the currently selected mutator for a given category, if any
            if (!settings.selectedMutators.NullOrEmpty())
            {
                if (settings.selectedMutators.ContainsKey(cat))
                {
                    return settings.selectedMutators[cat];
                }
            }
           
            return ZmdDefOf.ZMD_NoMutator;
        }


        public static void DisableAllMutators()
        {
            settings.selectedMutators.Clear();
            HelperMethods.InvokeOnSettingsChanged();
        }


    }
}
