using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using SPT.Reflection.Patching;
using EFT;
using EFT.InventoryLogic;

namespace InspectionlessMalfs
{
     public class KnowMalf : ModulePatch
     {
         public string ModName = "InspectionlessMalfsReborn";
        protected override MethodBase GetTargetMethod()
        {
            // Candidate target types: Weapon itself + all nested types inside Weapon
            var candidateTypes = new List<Type> { typeof(Weapon) };

            candidateTypes.AddRange(
                typeof(Weapon).GetNestedTypes(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static)
            );
            
            foreach (var type in candidateTypes)
            {
                var methods = type.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static);
                
                foreach (var method in methods)
                {
                    // Match any method named IsKnownMalfType or starting with IsKnownMalf regardless of parameters
                    if (method.Name.Equals("IsKnownMalfType", StringComparison.OrdinalIgnoreCase) ||
                        method.Name.Equals("IsKnownMalf", StringComparison.OrdinalIgnoreCase) ||
                        method.Name.StartsWith("IsKnownMalf", StringComparison.OrdinalIgnoreCase))
                    {
                        return method;
                    }
                }
            }
            
            // Diagnostic reporting if no matching method is found
            var malfMethodsOnWeapon = string.Join(", ", typeof(Weapon)
                .GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static)
                .Where(m => m.Name.IndexOf("Malf", StringComparison.OrdinalIgnoreCase) >= 0)
                .Select(m => m.Name));
            
            throw new Exception($"[{ModName}] Could not locate IsKnownMalfType target method. Methods on Weapon containing 'Malf': [{malfMethodsOnWeapon}]");
        }
        
        [PatchPostfix]
        private static void PatchPostfix(object __instance, ref bool __result)
        {
            if (Plugin.EnableInspectionlessMalfs == null || !Plugin.EnableInspectionlessMalfs.Value)
            {
                return;
            }

            try
            {
                object malfState = __instance;
                
                // If __instance is a Weapon, retrieve its MalfState property
                if (__instance is Weapon weapon)
                {
                    var malfProp = weapon.GetType().GetProperty("MalfState", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

                    if (malfProp != null)
                    {
                        malfState = malfProp.GetValue(weapon);
                    }
                }


                if (malfState != null)
                {
                    // Check if the malfunction State is active (0 == None)
                    var stateProp = malfState.GetType().GetProperty("State", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

                    if (stateProp != null)
                    {
                        int stateVal = Convert.ToInt32(stateProp.GetValue(malfState));
                        
                        // Only force known malfunction if an active malfunction exists
                        if (stateVal != 0)
                        {
                            __result = true;
                        }
                    }
                }
            }
            catch
            {
                // Fallback: retain default game behavior on exception
            }
        }
    }
}