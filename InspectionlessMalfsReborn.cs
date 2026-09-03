using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using Comfort.Common;
using EFT;
using EFT.InventoryLogic;
using UnityEngine;
using InspectionlessMalfsReborn;

namespace InspectionlessMalfs
{
    [BepInPlugin("com.inspectionlessmalfsreborn.inku", "InspectionlessMalfsReborn", "1.0.0")]
    public class Plugin : BaseUnityPlugin
    {
        public const string PLUGIN_GUID = "com.inku.inspectionlessmalfsreborn";
        public const string PLUGIN_NAME = "InspectionlessMalfsReborn";
        public const string PLUGIN_VERSION = "1.0.0";
        public static ConfigEntry<bool> ModEnabled { get; private set; }
        public static ConfigEntry<bool> DebugEnabled { get; private set; }
        public static ConfigEntry<KeyboardShortcut> ForceMalfKey { get; private set; }
        public static ManualLogSource Log { get; private set; }
        
        private void Awake()
        {
            Log = Logger;
            
            // BepInEx Menu Configuration Entries
            ModEnabled = Config.Bind(
                "1. General",
                "Mod Enabled",
                true,
                "Enable or disable inspectionless malfunction identification. ON - clear malfunctions without inspecting. OFF - vanilla EFT behavior."
            );

            DebugEnabled = Config.Bind(
                "2. Debug",
                "Debug Enabled",
                false,
                "Allow debug options to take effect, like forced weapon malfunctions."
            );

            ForceMalfKey = Config.Bind(
                "2. Debug",
                "Force Malfunction Key",
                new KeyboardShortcut(KeyCode.End),
                "Keybind to force a malfunction on currently equipped weapon."
            );
            
            new KnowMalf().Enable();
            Log.LogInfo($"{PLUGIN_NAME} is loaded!");
        }
        
        private void Update()
        {
            if (DebugEnabled.Value && ForceMalfKey.Value.IsDown())
            {
                ForceWeaponMalfunction();
            }
        }

        private void ForceWeaponMalfunction()
        {
            var gameWorld = Singleton<GameWorld>.Instance;
            if (gameWorld == null || gameWorld.MainPlayer == null)
            {
                return;
            }
            
            Weapon.EMalfunctionState[] malfunctionTypes = new[]
            {
                Weapon.EMalfunctionState.Misfire,
                Weapon.EMalfunctionState.Jam,
                Weapon.EMalfunctionState.Feed,
                Weapon.EMalfunctionState.HardSlide,
                Weapon.EMalfunctionState.SoftSlide
            };

            Player player = gameWorld.MainPlayer;
            if (player.HandsController is Player.FirearmController firearmController)
            {
                Weapon weapon = firearmController.Item;
                if (weapon == null) return;

                // force a random malfunction state on the current firearm
                Weapon.EMalfunctionState randomMalf = malfunctionTypes[UnityEngine.Random.Range(0, malfunctionTypes.Length)];
                weapon.MalfState.State = randomMalf;
                
                Log.LogInfo($"[{PLUGIN_NAME}] Successfully forced random malfunction - {randomMalf} - on active weapon ({weapon.ShortName.Localized()}).");
            }
        }
    }
}