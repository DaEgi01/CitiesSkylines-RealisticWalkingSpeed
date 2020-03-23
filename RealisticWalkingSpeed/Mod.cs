using Harmony;
using ICities;
using RealisticWalkingSpeed.Patches;

namespace RealisticWalkingSpeed
{
    public class Mod : LoadingExtensionBase, IUserMod
    {
        readonly string _harmonyId = "egi.citiesskylinesmods.realisticwalkingspeed";
        HarmonyInstance _harmony;

        public string SystemName = "RealisticWalkingSpeed";
        public string Name => "Realistic Walking Speed";
        public string Description => "Adjusts pedestrian walking speeds to realistic values.";
        public string Version => "1.2.1";

        public void OnEnabled()
        {
            _harmony = HarmonyInstance.Create(_harmonyId);

            new CitizenAnimationSpeedHarmonyPatch(_harmony).Apply();
            new CitizenCyclingSpeedHarmonyPatch(_harmony).Apply();
        }

        public void OnDisabled()
        {
            _harmony.UnpatchAll(_harmonyId);
            _harmony = null;
        }

        public override void OnLevelLoaded(LoadMode mode)
        {
            if (!(mode == LoadMode.LoadGame || mode == LoadMode.NewGame || mode == LoadMode.NewGameFromScenario))
            {
                return;
            }

            new CitizenWalkingSpeedInGamePatch(new SpeedData()).Apply();
        }
    }
}
