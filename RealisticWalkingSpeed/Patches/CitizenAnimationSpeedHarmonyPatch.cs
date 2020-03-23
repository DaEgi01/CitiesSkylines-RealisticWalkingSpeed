using Harmony;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using UnityEngine;

namespace RealisticWalkingSpeed.Patches
{
    class CitizenAnimationSpeedHarmonyPatch : IHarmonyPatch
    {
        readonly HarmonyInstance _harmony;

        public CitizenAnimationSpeedHarmonyPatch(HarmonyInstance harmony)
        {
            _harmony = harmony ?? throw new ArgumentNullException(nameof(harmony));
        }

        public void Apply()
        {
            var _setRenderParametersMethodInfo = typeof(CitizenInfo).GetMethod("SetRenderParameters", BindingFlags.Instance | BindingFlags.Public);
            var _setRenderParametersTranspilerMethodInfo = GetType().GetMethod(nameof(SetRenderParametersTranspiler), BindingFlags.Static | BindingFlags.NonPublic);
            _harmony.Patch(_setRenderParametersMethodInfo, null, null, new HarmonyMethod(_setRenderParametersTranspilerMethodInfo));
        }

        static IEnumerable<CodeInstruction> SetRenderParametersTranspiler(IEnumerable<CodeInstruction> codeInstructions)
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
    }
}
