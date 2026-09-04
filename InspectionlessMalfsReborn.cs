using System;
using System.Reflection;
using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using Comfort.Common;
using EFT;
using EFT.InventoryLogic;
using UnityEngine;

namespace InspectionlessMalfsReborn
{
    [BepInPlugin("com.inspectionlessmalfsreborn.inku", "InspectionlessMalfsReborn", "1.0.0")]
    public class Plugin : BaseUnityPlugin
    {
        public const string PLUGIN_GUID = "com.inku.inspectionlessmalfsreborn";
        public const string PLUGIN_NAME = "InspectionlessMalfsReborn";
        public const string PLUGIN_VERSION = "1.0.0";
        public static ConfigEntry<bool> ModEnabled { get; private set; }
        public static ConfigEntry<bool> DebugEnabled { get; private set; }
        public static ConfigEntry<KeyboardShortcut> ForceOverheatKey { get; private set; }
        public static ManualLogSource Log { get; private set; }
        
        private void Awake()
        {
            Log = Logger;
            
            // BepInEx Menu Configuration Entries
            ModEnabled = Config.Bind(
                "1. General",
                "Mod Enabled",
                true,
                "Enable or disable inspectionless malfunction identification.\nON - clear malfunctions without inspecting. OFF - vanilla EFT behavior."
            );
            
            DebugEnabled = Config.Bind(
                "2. Debug",
                "Debug Enabled",
                false,
                "Enable/Disable debugging options."
            );

            ForceOverheatKey = Config.Bind(
                "2. Debug",
                "Force Overheat",
                new KeyboardShortcut(KeyCode.End),
                "Keybind to instantly overheat currently active weapon."
            );
            
            new KnowMalf().Enable();
            Log.LogInfo($"{PLUGIN_NAME} loaded!");
        }
        
        private void Update()
        {
            if (DebugEnabled.Value && ForceOverheatKey.Value.IsDown())
            {
                ForceWeaponOverheat();
            }
        }
        
        private void ForceWeaponOverheat()
        {
            var gameWorld = Singleton<GameWorld>.Instance;
            if (gameWorld == null || gameWorld.MainPlayer == null) return;

            Player player = gameWorld.MainPlayer;
            if (player.HandsController is Player.FirearmController firearmController)
            {
                Weapon weapon = firearmController.Item;
                if (weapon == null) return;

                if (SetWeaponHeat(weapon, 500f))
                {
                    Log.LogInfo($" *** DEBUG *** Successfully forced overheat on {weapon.ShortName.Localized()}.");
                }
                else
                {
                    Log.LogWarning($" *** DEBUG *** Could not apply overheat to {weapon.ShortName.Localized()}.");
                }
            }
        }
        
        private bool SetWeaponHeat(Weapon weapon, float heatValue)
        {
            if (weapon?.MalfState == null) return false;

            Type malfStateType = weapon.MalfState.GetType();
            FieldInfo overheatField = malfStateType.GetField("LastShotOverheat", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

            if (overheatField != null && overheatField.FieldType == typeof(float))
            {
                overheatField.SetValue(weapon.MalfState, heatValue);
                return true;
            }

            return false;
        }
    }
}