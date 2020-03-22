using ColossalFramework.IO;
using ColossalFramework.UI;
using Harmony;
using ICities;
using RealisticWalkingSpeed.Configuration;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Reflection.Emit;
using UnityEngine;

namespace RealisticWalkingSpeed
{
    public class Mod : LoadingExtensionBase, IUserMod
    {
        private HarmonyInstance _harmony;
        private MethodInfo _setRenderParametersMethodInfo;
        private MethodInfo _setRenderParametersPostfixMethodInfo;
        private MethodInfo _simulationStepMethodInfo;
        private MethodInfo _simulationStepTranspilerMethodInfo;

        private ConfigurationService _configurationService;
        private static ConfigurationDto _configurationDto;

        public string SystemName = "RealisticWalkingSpeed";
        public string Name => "Realistic Walking Speed";
        public string Description => "Adjusts pedestrian walking speeds to realistic values.";
        public string Version => "1.2.0";

        public void OnEnabled()
        {
            _harmony = HarmonyInstance.Create("egi.citiesskylinesmods.realisticwalkingspeed");

            _setRenderParametersMethodInfo = typeof(CitizenInfo).GetMethod("SetRenderParameters", BindingFlags.Instance | BindingFlags.Public);
            _setRenderParametersPostfixMethodInfo = GetType().GetMethod(nameof(SetRenderParametersPostfix), BindingFlags.Static | BindingFlags.NonPublic);
            _harmony.Patch(_setRenderParametersMethodInfo, null, new HarmonyMethod(_setRenderParametersPostfixMethodInfo));

            _simulationStepMethodInfo = typeof(HumanAI).GetMethod(
                "SimulationStep"
                , BindingFlags.Instance | BindingFlags.Public
                , Type.DefaultBinder
                , new Type[] {
                    typeof(ushort),
                    typeof(CitizenInstance).MakeByRefType(),
                    typeof(CitizenInstance.Frame).MakeByRefType(),
                    typeof(bool) }
                , null
            );
            _simulationStepTranspilerMethodInfo = GetType().GetMethod(nameof(SimulationStepTranspiler), BindingFlags.Static | BindingFlags.NonPublic);
            _harmony.Patch(_simulationStepMethodInfo, null, null, new HarmonyMethod(_simulationStepTranspilerMethodInfo));

            var configurationFileFullName = Path.Combine(DataLocation.localApplicationData, SystemName + ".xml");
            _configurationService = new ConfigurationService(configurationFileFullName);
            if (File.Exists(configurationFileFullName))
            {
                _configurationDto = _configurationService.Load();
            }
            else
            {
                _configurationDto = new ConfigurationDto();
            }
        }

        public void OnDisabled()
        {
            _configurationService = null;

            _harmony.Unpatch(_setRenderParametersMethodInfo, _setRenderParametersPostfixMethodInfo);
            _setRenderParametersPostfixMethodInfo = null;
            _setRenderParametersMethodInfo = null;

            _harmony.Unpatch(_simulationStepMethodInfo, _simulationStepTranspilerMethodInfo);
            _simulationStepTranspilerMethodInfo = null;
            _simulationStepMethodInfo = null;

            _harmony = null;
        }

        public void OnSettingsUI(UIHelperBase helper)
        {
            var modFullTitle = new ModFullTitle(Name, Version);

            var mainGroupUiHelper = helper.AddGroup(modFullTitle);
            var mainGroupContentPanel = (mainGroupUiHelper as UIHelper).self as UIPanel;
            mainGroupContentPanel.backgroundSprite = string.Empty;

            mainGroupUiHelper.AddSliderWithLabel("Animation speed factor", 0f, 10f, 0.1f, _configurationDto.AnimationSpeedFactor, value => 
            {
                _configurationDto.AnimationSpeedFactor = value;
                _configurationService.Save(_configurationDto);
            });
        }

        public override void OnLevelLoaded(LoadMode mode)
        {
            if (!(mode == LoadMode.LoadGame || mode == LoadMode.NewGame || mode == LoadMode.NewGameFromScenario))
            {
                return;
            }

            var speedData = new SpeedData();
            var citizenPrefabCount = PrefabCollection<CitizenInfo>.LoadedCount();
            for (uint i = 0; i < citizenPrefabCount; i++)
            {
                var citizenPrefab = PrefabCollection<CitizenInfo>.GetLoaded(i);
                if (citizenPrefab == null)
                {
                    continue;
                }

                citizenPrefab.m_walkSpeed = speedData.GetAverageSpeed(citizenPrefab.m_agePhase, citizenPrefab.m_gender);
            }
        }

        private static void SetRenderParametersPostfix(Animator ___m_animator)
        {
            ___m_animator.speed = ___m_animator.speed * _configurationDto.AnimationSpeedFactor;
        }

        private static IEnumerable<CodeInstruction> SimulationStepTranspiler(IEnumerable<CodeInstruction> codeInstructions)
        {
            var codes = new List<CodeInstruction>(codeInstructions);
            for (int i = 0; i < codes.Count; i++)
            {
                var firstCode = codes[i];
                if (firstCode.opcode != OpCodes.Ldloc_S)
                {
                    continue;
                }

                var firstCodeOperand = firstCode.operand as LocalBuilder;
                if (firstCodeOperand.LocalIndex != 22)
                {
                    continue;
                }

                var onBikeLaneFactor = codes[i + 1];
                var onBikeLaneMultiplication = codes[i + 2];
                var notOnBikeLaneFactor = codes[i + 6];
                var notOnBikeLaneMultiplication = codes[i + 7];

                if (!(onBikeLaneFactor.opcode == OpCodes.Ldc_R4 
                    && onBikeLaneMultiplication.opcode == OpCodes.Mul 
                    && notOnBikeLaneFactor.opcode == OpCodes.Ldc_R4 
                    && notOnBikeLaneMultiplication.opcode == OpCodes.Mul))
                {
                    continue;
                }

                onBikeLaneFactor.operand = 3.5f;
                notOnBikeLaneFactor.operand = 2.5f;

                break;
            }

            return codes;
        }
    }
}
