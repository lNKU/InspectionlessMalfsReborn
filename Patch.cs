using System;
using System.Reflection;
using SPT.Reflection.Patching;
using HarmonyLib;
using EFT.InventoryLogic;
using InspectionlessMalfs;

namespace InspectionlessMalfsReborn
{
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
            // If the toggle in BepInEx menu is disabled, skip modifying the result
            if (!Plugin.ModEnabled.Value)
            {
                return;
            }

            __result = true;
        }
    }
}