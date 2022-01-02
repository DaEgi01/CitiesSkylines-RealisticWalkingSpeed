using System;

namespace RealisticWalkingSpeed.Patches
{
    class CitizenWalkingSpeedInGamePatch : IInGamePatch
    {
        readonly SpeedData _speedData;

        public CitizenWalkingSpeedInGamePatch(SpeedData speedData)
        {
            _speedData = speedData ?? throw new ArgumentNullException(nameof(speedData));
        }

        public void Apply()
        {
            for (uint i = 0; i < PrefabCollection<CitizenInfo>.LoadedCount(); i++)
            {
                var citizenPrefab = PrefabCollection<CitizenInfo>.GetLoaded(i);
                if (citizenPrefab == null)
                {
                    continue;
                }

                citizenPrefab.m_walkSpeed = _speedData.GetAverageSpeed(citizenPrefab.m_agePhase, citizenPrefab.m_gender);
            }
        }
    }
}
