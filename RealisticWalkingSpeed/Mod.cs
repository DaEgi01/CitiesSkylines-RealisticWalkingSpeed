using ColossalFramework.UI;
using Harmony;
using ICities;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using UnityEngine;

namespace RealisticWalkingSpeed
{
    public class Mod : LoadingExtensionBase, IUserMod
    {
        private readonly string _harmonyId = "egi.citiesskylinesmods.realisticwalkingspeed";
        private HarmonyInstance _harmony;

        public string SystemName = "RealisticWalkingSpeed";
        public string Name => "Realistic Walking Speed";
        public string Description => "Adjusts pedestrian walking speeds to realistic values.";
        public string Version => "1.2.1";

        public void OnEnabled()
        {
            _harmony = HarmonyInstance.Create(_harmonyId);

            var _setRenderParametersMethodInfo = typeof(CitizenInfo).GetMethod("SetRenderParameters", BindingFlags.Instance | BindingFlags.Public);
            var _setRenderParametersTranspilerMethodInfo = GetType().GetMethod(nameof(SetRenderParametersTranspiler), BindingFlags.Static | BindingFlags.NonPublic);
            _harmony.Patch(_setRenderParametersMethodInfo, null, null, new HarmonyMethod(_setRenderParametersTranspilerMethodInfo));

            var _simulationStepMethodInfo = typeof(HumanAI).GetMethod(
                "SimulationStep",
                BindingFlags.Instance | BindingFlags.Public,
                Type.DefaultBinder, 
                new Type[] 
                {
                    typeof(ushort),
                    typeof(CitizenInstance).MakeByRefType(),
                    typeof(CitizenInstance.Frame).MakeByRefType(),
                    typeof(bool) 
                }, 
                null
            );
            var _simulationStepTranspilerMethodInfo = GetType().GetMethod(nameof(SimulationStepTranspiler), BindingFlags.Static | BindingFlags.NonPublic);
            _harmony.Patch(_simulationStepMethodInfo, null, null, new HarmonyMethod(_simulationStepTranspilerMethodInfo));
        }

        public void OnDisabled()
        {
            _harmony.UnpatchAll(_harmonyId);
            _harmony = null;
        }

        public void OnSettingsUI(UIHelperBase helper)
        {
            var modFullTitle = new ModFullTitle(Name, Version);

            var mainGroupUiHelper = helper.AddGroup(modFullTitle);
            var mainGroupContentPanel = (mainGroupUiHelper as UIHelper).self as UIPanel;
            mainGroupContentPanel.backgroundSprite = string.Empty;
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

        private static IEnumerable<CodeInstruction> SetRenderParametersTranspiler(IEnumerable<CodeInstruction> codeInstructions)
        {
            var hookOperand = typeof(Animator).GetMethod(
                "SetFloat", 
                BindingFlags.Public | BindingFlags.Instance,
                Type.DefaultBinder,
                new Type[]
                {
                    typeof(int),
                    typeof(float)
                },
                null
            );

            var codes = new List<CodeInstruction>(codeInstructions);
            for (int i = 0; i < codes.Count; i++)
            {
                var code = codes[i];
                if (!(code.opcode == OpCodes.Callvirt && code.operand == hookOperand))
                {
                    continue;
                }

                //float magnitude = velocity.magnitude;
                //->
                //float magnitude = velocity.magnitude * 2.1f;
                codes.InsertRange(i - 6, new[] {
                    new CodeInstruction(OpCodes.Ldc_R4, 2.1f), //TODO finetune per CitizenInfo
                    new CodeInstruction(OpCodes.Mul)
                });

                break;
            }

            return codes;
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
