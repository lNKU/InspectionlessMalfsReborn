using BepInEx;
using InspectionlessMalfsReborn;

namespace InspectionlessMalfs
{
    [BepInPlugin("com.inspectionlessmalfsreborn.inku", "InspectionlessMalfsReborn", "1.0.0")]
    public class Plugin : BaseUnityPlugin
    {
        private void Awake()
        {
            new KnowMalf().Enable();
            Logger.LogInfo("InspectionlessMalfsReborn is loaded!");
        }
    }
}