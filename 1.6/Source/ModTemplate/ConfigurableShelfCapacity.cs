﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using UnityEngine;
using Verse;
using HarmonyLib;

namespace LupineWitch.ConfigurableShelfCapacity
{
    [StaticConstructorOnStartup]
    public static class ConfigurableShelfCapacity
    {
        static ConfigurableShelfCapacity()
        {
            ConfigurableShelfCapacitySettings.InitDefCollection(DefDatabase<ThingDef>.AllDefs.Where(def => def.thingClass == typeof(Building_Storage)));
            ConfigurableShelfCapacitySettings.ApplySettings();

            //Harmony.DEBUG = true;
            var harmony = new Harmony("patch.shelfutils.lupinewitch.mods");
            harmony.PatchAll();
        }
    }

    public class ConfigurableShelfCapacityMod : Mod
    {
        public ConfigurableShelfCapacitySettings Settings;
        private Dictionary<string, string> defNameCapacityBuffer = new Dictionary<string, string>();

        public ConfigurableShelfCapacityMod(ModContentPack content) : base(content)
        {
            Settings = GetSettings<ConfigurableShelfCapacitySettings>();
            foreach (var setting in ConfigurableShelfCapacitySettings.SettingsDictionary)
                defNameCapacityBuffer.Add(setting.Key, setting.Value.ToString());
        }

        public override void DoSettingsWindowContents(Rect inRect)
        {
            Listing_Standard listingStandard = new Listing_Standard();
            listingStandard.Begin(inRect);

            listingStandard.Label(string.Format("Items should tower up to {0} stacks per cell (minimum 2)", ConfigurableShelfCapacitySettings.SplitVisualStackCount));
            string towerThresholdBuffer = ConfigurableShelfCapacitySettings.SplitVisualStackCount.ToString();
            listingStandard.IntEntry(ref ConfigurableShelfCapacitySettings.SplitVisualStackCount, ref towerThresholdBuffer);

            listingStandard.Label(string.Format("Minimum capacity is {0}, maximum capacity is {1}", ConfigurableShelfCapacitySettings.MIN_SHELF_CAPACITY, ConfigurableShelfCapacitySettings.MAX_SHELF_CAPACITY));

            foreach (var entry in ConfigurableShelfCapacitySettings.StorageBuildings)
            {
                if (!defNameCapacityBuffer.ContainsKey(entry.defName))
                {
                    defNameCapacityBuffer[entry.defName] = ConfigurableShelfCapacitySettings.SettingsDictionary.ContainsKey(entry.defName)
                        ? ConfigurableShelfCapacitySettings.SettingsDictionary[entry.defName].ToString()
                        : ConfigurableShelfCapacitySettings.MIN_SHELF_CAPACITY.ToString();
                }
                if (!ConfigurableShelfCapacitySettings.SettingsDictionary.ContainsKey(entry.defName))
                {
                    ConfigurableShelfCapacitySettings.SettingsDictionary[entry.defName] = ConfigurableShelfCapacitySettings.MIN_SHELF_CAPACITY;
                }
                string shelfBuffer = defNameCapacityBuffer[entry.defName];
                int settingReference = ConfigurableShelfCapacitySettings.SettingsDictionary[entry.defName];
                listingStandard.Label(entry.label);
                listingStandard.IntEntry(ref settingReference, ref shelfBuffer);
                ConfigurableShelfCapacitySettings.SettingsDictionary[entry.defName] = settingReference;
                defNameCapacityBuffer[entry.defName] = shelfBuffer;
            }

            listingStandard.End();

            ConfigurableShelfCapacitySettings.SplitVisualStackCount = ConfigurableShelfCapacitySettings.SplitVisualStackCount.Clamp(2, int.MaxValue);
            foreach (var valueBuffer in defNameCapacityBuffer)
            {
                int clampedValue = Mathf.Clamp(int.Parse(valueBuffer.Value), ConfigurableShelfCapacitySettings.MIN_SHELF_CAPACITY, ConfigurableShelfCapacitySettings.MAX_SHELF_CAPACITY);
                ConfigurableShelfCapacitySettings.SettingsDictionary[valueBuffer.Key] = clampedValue;
            }

            base.DoSettingsWindowContents(inRect);
        }

        public override string SettingsCategory()
        {
            return "Configurable Shelf Capacity";
        }

        public override void WriteSettings()
        {
            ConfigurableShelfCapacitySettings.ApplySettings();
            base.WriteSettings();
        }
    }
}
