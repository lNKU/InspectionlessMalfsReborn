using System;
using System.Reflection;
using SPT.Reflection.Patching;
using HarmonyLib;
using EFT;
using EFT.InventoryLogic;
using UnityEngine;
using InspectionlessMalfsReborn;

namespace InspectionlessMalfsReborn
{
    // forces the game to recognize that the player knows the SPECIFIC TYPE of malfunction
    public class KnowMalf : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return AccessTools.FirstMethod(
                typeof(Weapon.MalfunctionState), 
                method => method.Name == nameof(Weapon.MalfunctionState.IsKnownMalfType)
            );
        }

        [PatchPostfix]
        private static void PatchPostfix(ref bool __result)
        {
            if (!Plugin.ModEnabled.Value)
            {
                return;
            }

            __result = true;
        }
    }
}