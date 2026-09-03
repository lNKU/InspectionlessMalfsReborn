using System;
using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using UnityEngine;
using EFT;
using EFT.InventoryLogic;
using Comfort.Common;

namespace InspectionlessMalfs
{
    [BepInPlugin("com.inku.inspectionlessmalfs_reborn", "InspectionlessMalfsReborn", "1.0.0")]
    public class Plugin : BaseUnityPlugin
    {
        public const string PLUGIN_GUID = "com.inku.inspectionlessmalfs_reborn";
        public const string PLUGIN_NAME = "InspectionlessMalfsReborn";
        public const string PLUGIN_VERSION = "1.0.0";
        
        public static ConfigEntry<bool> EnableInspectionlessMalfs;
        public static ManualLogSource Log;

        private void Awake()
        {
            Log = Logger;

            // F12 config menu items
            EnableInspectionlessMalfs = Config.Bind(
                "General",
                "Enable Inspectionless Malfunctions",
                true,
                "When enabled, weapon malfunctions will not require inspection to resolve. If disabled, will require inspecting to resolve malfunctions."
            );

            Config.Bind(
                "Debug",
                "Force Malfunction",
                false,
                new ConfigDescription(
                    "DEBUG ONLY: Force a malfunction on the currently active weapon.",
                    null,
                    new ConfigurationManagerAttributes
                    {
                        CustomDrawer = DrawForceMalfButton
                    }
                )
            );

            Logger.LogInfo($"[{PLUGIN_NAME}] Starting initialization...");

            try
            {
                new KnowMalf().Enable();
                Logger.LogInfo($"[{PLUGIN_NAME}] Successfully applied Inspectionless Malfunctions patch!");
            }
            catch (Exception ex)
            {
                Logger.LogError($"[{PLUGIN_NAME}] Failed to apply patch: {ex.Message}\n{ex.StackTrace}");
            }
        }

        private static void DrawForceMalfButton(ConfigEntryBase entry)
        {
            if (GUILayout.Button("Force Active Weapon Jam/Misfire", GUILayout.ExpandWidth(true)))
            {
                try
                {
                    Player mainPlayer = Singleton<GameWorld>.Instantiated ? Singleton<GameWorld>.Instance?.MainPlayer : null;
                    InventoryEquipment equipment = GetPlayerEquipment(mainPlayer);

                    // Dynamically get the currently held/active weapon (or slot fallback)
                    Weapon weapon = GetActiveWeapon(mainPlayer, equipment);

                    if (weapon != null && weapon.MalfState != null)
                    {
                        Weapon.EMalfunctionState[] malfunctionTypes = new[]
                        {
                            Weapon.EMalfunctionState.Misfire,
                            Weapon.EMalfunctionState.Jam,
                            Weapon.EMalfunctionState.Feed,
                            Weapon.EMalfunctionState.HardSlide,
                            Weapon.EMalfunctionState.SoftSlide
                        };

                        // Pick a random malfunction from the array
                        Weapon.EMalfunctionState randomMalf = malfunctionTypes[UnityEngine.Random.Range(0, malfunctionTypes.Length)];
                        weapon.MalfState.State = randomMalf;

                        Log.LogInfo($"[{PLUGIN_NAME}] Successfully forced random malfunction - {randomMalf} - on active weapon ({weapon.ShortName.Localized()}).");
                    }
                    else
                    {
                        Log.LogWarning($"[{PLUGIN_NAME}] Cannot force malfunction: No equipped weapon found.");
                    }
                }
                catch (Exception ex)
                {
                    Log.LogError($"[{PLUGIN_NAME}] Error forcing malfunction: {ex.Message}");
                }
            }
        }

        private static Weapon GetActiveWeapon(Player player, InventoryEquipment equipment)
        {
            // 1. In Raid / Hideout: Check the item currently held in hands
            if (player?.HandsController?.Item is Weapon heldWeapon)
            {
                return heldWeapon;
            }

            // 2. Fallback (holding melee/item, or in Stash/Menu): Check slots in priority order
            if (equipment != null)
            {
                EquipmentSlot[] weaponSlots = {
                    EquipmentSlot.FirstPrimaryWeapon,
                    EquipmentSlot.SecondPrimaryWeapon,
                    EquipmentSlot.Holster
                };

                foreach (var slot in weaponSlots)
                {
                    if (equipment.GetSlot(slot)?.ContainedItem is Weapon weapon)
                    {
                        return weapon;
                    }
                }
            }

            return null;
        }

        private static InventoryEquipment GetPlayerEquipment(Player player)
        {
            // 1. Get equipment from active MainPlayer
            if (player != null)
            {
                var eq = player.Profile?.Inventory?.Equipment ?? player.InventoryController?.Inventory?.Equipment;
                if (eq != null)
                {
                    return eq;
                }
            }

            // 2. In Stash / Main Menu: Get equipment via TarkovApplication scene object
            var tarkovApp = UnityEngine.Object.FindObjectOfType<TarkovApplication>();
            if (tarkovApp != null)
            {
                var eq = tarkovApp.Session?.Profile?.Inventory?.Equipment;
                if (eq != null)
                {
                    return eq;
                }
            }

            return null;
        }

        public class ConfigurationManagerAttributes
        {
            public Action<ConfigEntryBase> CustomDrawer;
        }
    }
}