using HarmonyLib;
using RealisticWalkingSpeed.Patches;

namespace RealisticWalkingSpeed
{
    public static class Patcher
    {
        private const string _harmonyId = "egi.citiesskylinesmods.realisticwalkingspeed";
        private static bool _patched = false;

        public static void PatchAll()
        {
            if (_patched)
                return;

            var harmony = new Harmony(_harmonyId);

            new CitizenAnimationSpeedHarmonyPatch(harmony).Apply();
            new CitizenCyclingSpeedHarmonyPatch(harmony).Apply();

            _patched = true;
        }

        public static void UnpatchAll()
        {
            if (!_patched)
                return;

            var harmony = new Harmony(_harmonyId);
            harmony.UnpatchAll(_harmonyId);

            _patched = false;
        }
    }
}
