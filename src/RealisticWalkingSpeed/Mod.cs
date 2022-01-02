using CitiesHarmony.API;
using ICities;
using RealisticWalkingSpeed.Patches;

namespace RealisticWalkingSpeed
{
    public class Mod : LoadingExtensionBase, IUserMod
    {
        public string Name => "Realistic Walking Speed";
        public string Description => "Adjusts pedestrian walking speeds to realistic values.";
        public string Version => "1.3.0";

        public void OnEnabled()
        {
            HarmonyHelper.DoOnHarmonyReady(() => Patcher.PatchAll());
        }

        public void OnDisabled()
        {
            if (!HarmonyHelper.IsHarmonyInstalled)
                return;

            Patcher.UnpatchAll();
        }

        public override void OnLevelLoaded(LoadMode mode)
        {
            if (mode == LoadMode.NewGame
                || mode == LoadMode.NewGameFromScenario
                || mode == LoadMode.LoadGame)
            {
                new CitizenWalkingSpeedInGamePatch(new SpeedData()).Apply();
            }
        }
    }
}
