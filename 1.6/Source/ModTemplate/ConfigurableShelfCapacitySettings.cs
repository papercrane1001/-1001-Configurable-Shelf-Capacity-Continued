﻿using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace LupineWitch.ConfigurableShelfCapacity
{
    public class ConfigurableShelfCapacitySettings : ModSettings
    {
        public const int MAX_SHELF_CAPACITY = 80;
        public const int MIN_SHELF_CAPACITY = 1;

        public static int SplitVisualStackCount = 3;

        public static IReadOnlyCollection<ThingDef> StorageBuildings => storageDefs;
        public static Dictionary<string, int> SettingsDictionary = new Dictionary<string, int>();
        
        private static List<ThingDef> storageDefs = new List<ThingDef>();

        public static void InitDefCollection(IEnumerable<ThingDef> foundDefs)
        {
            storageDefs.AddRange(foundDefs);
            foreach (var def in foundDefs)
            {
                if (!SettingsDictionary.ContainsKey(def.defName))
                    SettingsDictionary.Add(def.defName, def.building.maxItemsInCell);
            }
        }

        public override void ExposeData()
        {
            List<string> keys = SettingsDictionary.Keys.ToList();
            List<int> values = SettingsDictionary.Values.ToList();
            Scribe_Collections.Look(ref keys, "SettingsDictionary_keys", LookMode.Value);
            Scribe_Collections.Look(ref values, "SettingsDictionary_values", LookMode.Value);
            Scribe_Values.Look(ref SplitVisualStackCount, "SplitVisualStackCount", 3);
            base.ExposeData();

            if (Scribe.mode == LoadSaveMode.LoadingVars)
            {
                SettingsDictionary.Clear();
                if (keys != null && values != null && keys.Count == values.Count)
                {
                    for (int i = 0; i < keys.Count; i++)
                    {
                        SettingsDictionary[keys[i]] = values[i];
                    }
                }
            }
        }

        public static void ApplySettings()
        {
            foreach(ThingDef storage in storageDefs)
            {
                if (SettingsDictionary.ContainsKey(storage.defName))
                    storage.building.maxItemsInCell = SettingsDictionary[storage.defName];
                else
                    Log.Warning($"[{nameof(ConfigurableShelfCapacityMod)}]:No ThingDef:{storageDefs} with thingClass:{nameof(Building_Storage)}");
            }
        }
    }
}
