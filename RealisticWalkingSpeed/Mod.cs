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
        private HarmonyInstance harmony;
        private MethodInfo setRenderParametersMethodInfo;
        private MethodInfo setRenderParametersPostfixMethodInfo;
        private MethodInfo simulationStepMethodInfo;
        private MethodInfo simulationStepTranspilerMethodInfo;

        private ConfigurationService configurationService;
        private static ConfigurationDto configurationDto;

        public string SystemName = "RealisticWalkingSpeed";
        public string Name => "Realistic Walking Speed";
        public string Description => "Adjusts pedestrian walking speeds to realistic values.";
        public string Version => "1.2.0";

        public void OnEnabled()
        {
            this.harmony = HarmonyInstance.Create("egi.citiesskylinesmods.realisticwalkingspeed");

            this.setRenderParametersMethodInfo = typeof(CitizenInfo).GetMethod("SetRenderParameters", BindingFlags.Instance | BindingFlags.Public);
            this.setRenderParametersPostfixMethodInfo = this.GetType().GetMethod(nameof(SetRenderParametersPostfix), BindingFlags.Static | BindingFlags.NonPublic);
            this.harmony.Patch(this.setRenderParametersMethodInfo, null, new HarmonyMethod(this.setRenderParametersPostfixMethodInfo));

            this.simulationStepMethodInfo = typeof(HumanAI).GetMethod(
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
            this.simulationStepTranspilerMethodInfo = this.GetType().GetMethod(nameof(SimulationStepTranspiler), BindingFlags.Static | BindingFlags.NonPublic);
            this.harmony.Patch(this.simulationStepMethodInfo, null, null, new HarmonyMethod(this.simulationStepTranspilerMethodInfo));

            var configurationFileFullName = Path.Combine(DataLocation.localApplicationData, SystemName + ".xml");
            this.configurationService = new ConfigurationService(configurationFileFullName);
            if (File.Exists(configurationFileFullName))
            {
                configurationDto = configurationService.Load();
            }
            else
            {
                configurationDto = new ConfigurationDto();
            }
        }

        public void OnDisabled()
        {
            this.configurationService = null;

            this.harmony.Unpatch(this.setRenderParametersMethodInfo, this.setRenderParametersPostfixMethodInfo);
            this.setRenderParametersPostfixMethodInfo = null;
            this.setRenderParametersMethodInfo = null;

            this.harmony.Unpatch(this.simulationStepMethodInfo, this.simulationStepTranspilerMethodInfo);
            this.simulationStepTranspilerMethodInfo = null;
            this.simulationStepMethodInfo = null;

            this.harmony = null;
        }

        public void OnSettingsUI(UIHelperBase helper)
        {
            var modFullTitle = new ModFullTitle(this.Name, this.Version);

            var mainGroupUiHelper = helper.AddGroup(modFullTitle);
            var mainGroupContentPanel = (mainGroupUiHelper as UIHelper).self as UIPanel;
            mainGroupContentPanel.backgroundSprite = string.Empty;

            mainGroupUiHelper.AddSliderWithLabel("Animation speed factor", 0f, 10f, 0.1f, configurationDto.AnimationSpeedFactor, value => 
            {
                configurationDto.AnimationSpeedFactor = value;
                this.configurationService.Save(configurationDto);
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
            ___m_animator.speed = ___m_animator.speed * configurationDto.AnimationSpeedFactor;
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
