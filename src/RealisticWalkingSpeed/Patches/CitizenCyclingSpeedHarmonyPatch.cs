using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;

namespace RealisticWalkingSpeed.Patches
{
    public class CitizenCyclingSpeedHarmonyPatch : IHarmonyPatch
    {
        private readonly Harmony _harmony;

        public CitizenCyclingSpeedHarmonyPatch(Harmony harmony)
        {
            _harmony = harmony ?? throw new ArgumentNullException(nameof(harmony));
        }

        public void Apply()
        {
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

        static IEnumerable<CodeInstruction> SimulationStepTranspiler(IEnumerable<CodeInstruction> codeInstructions)
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
                if (firstCodeOperand?.LocalIndex != 22)
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
