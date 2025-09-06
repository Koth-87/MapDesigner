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
                        Log.Message(cat);
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
                //List<TileMutatorDef> catMuts = allMutators.Where(m => m.categories.Any(c => c== cat)).ToList();

                //if(outerListing.ButtonTextLabeled(cat, GetSelectedMutatorByCategory(cat).label))
                if (listing.ButtonTextLabeled(cat, GetSelectedMutatorByCategory(cat).label))
                {
                    Log.Message("Clicked button for " + cat);
                }
                //{
                //TODO: Dropdown options, and display list
                //List<FloatMenuOption> mutList = new List<FloatMenuOption>();
                //foreach(TileMutatorDef mut in catMuts)
                //{
                //    mutList.Add(new FloatMenuOption(mut.label, delegate
                //    {
                //        AddSelectedMutator(mut);
                //        HelperMethods.InvokeOnSettingsChanged();
                //    }));
                //}

                    //Find.WindowStack.Add(new FloatMenu(mutList));
                    //}
            }

            listing.End();
            Widgets.EndScrollView();
            outerListing.End();
            HelperMethods.EndChangeCheck();
        }


        public static TileMutatorDef GetSelectedMutatorByCategory(string cat)
        {
            // gets the currently selected mutator for a given category, if any
            Log.Message("Getting mutator for category " + cat);
            if (!settings.selectedMutators.NullOrEmpty())
            {
                if (settings.selectedMutators.Any(m => m.categories.Contains(cat)))
                {
                    return settings.selectedMutators.Where(m => m.categories.Contains(cat)).FirstOrDefault();
                }
            }
           
            Log.Message("No mutator found, returning DEFAULT");
            return ZmdDefOf.ZMD_NoMutator;
        }

        public static void AddSelectedMutator(TileMutatorDef newMut)
        {
            //remove any conflicting mutators, then add the new one
            List<TileMutatorDef> muts = settings.selectedMutators;

            //remove all mutators with the same category
            muts.RemoveAll(m => m.categories.Intersect(newMut.categories).Any());

            //remove everything that conflicts with overrideCategories
            muts.RemoveAll(m => m.overrideCategories.Intersect(newMut.categories).Any());

            muts.Add(newMut);

            settings.selectedMutators = muts;
        }


        public static void DisableAllMutators()
        {
            settings.selectedMutators.Clear();
            HelperMethods.InvokeOnSettingsChanged();
        }



    }
}
