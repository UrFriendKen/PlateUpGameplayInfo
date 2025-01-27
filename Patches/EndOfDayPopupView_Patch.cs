using HarmonyLib;
using Kitchen;
using KitchenData;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;

namespace KitchenGameplayInfo.Patches
{
    [HarmonyPatch]
    static class EndOfDayPopupView_Patch
    {
        static readonly Type TARGET_TYPE = typeof(EndOfDayPopupView);
        const bool IS_ORIGINAL_LAMBDA_BODY = false;
        const int LAMBDA_BODY_INDEX = 0;
        const string TARGET_METHOD_NAME = "FirstUpdate";
        const string DESCRIPTION = "Modify dish served text to include count"; // Logging purpose of patch

        const int EXPECTED_MATCH_COUNT = 1;

        static string GetDishItemText(Dish dish)
        {
            if (Main.PrefManager.Get<bool>(Main.SHOW_END_OF_DAY_DISH_DETAILS_ID))
            {
                int minReward = 0;
                int maxReward = 0;
                if (dish.UnlockItemOverride)
                {
                    maxReward = dish.UnlockItemOverride.Reward;
                }
                else if (dish.UnlocksMenuItems.Count > 0)
                {
                    IEnumerable<int> rewards = dish.UnlocksMenuItems
                        .Where(x => x.Item != default)
                        .Select(x => x.Item.Reward);
                    minReward = rewards.Min();
                    maxReward = rewards.Max();
                }
                
                if (minReward == maxReward)
                    return $"{dish.Name} ({maxReward}<sprite name=\"coin\">)";
                return $"{dish.Name} ({minReward}-{maxReward}<sprite name=\"coin\">)";
            }
            return dish.Name;
        }

        static readonly List<OpCode> OPCODES_TO_MATCH = new List<OpCode>()
        {
            OpCodes.Ldloc_S,
            OpCodes.Callvirt
        };

        // null is ignore
        static readonly List<object> OPERANDS_TO_MATCH = new List<object>()
        {
            null,
            typeof(Unlock).GetProperty("Name").GetGetMethod()
        };

        static readonly List<OpCode> MODIFIED_OPCODES = new List<OpCode>()
        {
            OpCodes.Ldloc_S,
            OpCodes.Call
        };

        // null is ignore
        static readonly List<object> MODIFIED_OPERANDS = new List<object>()
        {
            null,
            typeof(EndOfDayPopupView_Patch).GetMethod("GetDishItemText", BindingFlags.NonPublic | BindingFlags.Static)
        };

        public static MethodBase TargetMethod()
        {
            Type type = IS_ORIGINAL_LAMBDA_BODY ? AccessTools.FirstInner(TARGET_TYPE, t => t.Name.Contains($"c__DisplayClass_OnUpdate_LambdaJob{LAMBDA_BODY_INDEX}")) : TARGET_TYPE;
            return AccessTools.FirstMethod(type, method => method.Name.Contains(IS_ORIGINAL_LAMBDA_BODY ? "OriginalLambdaBody" : TARGET_METHOD_NAME));
        }

        [HarmonyTranspiler]
        static IEnumerable<CodeInstruction> OriginalLambdaBody_Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            Main.LogInfo($"{TARGET_TYPE.Name} Transpiler");
            if (!(DESCRIPTION == null || DESCRIPTION == string.Empty))
                Main.LogInfo(DESCRIPTION);
            List<CodeInstruction> list = instructions.ToList();

            int matches = 0;
            int windowSize = OPCODES_TO_MATCH.Count;
            for (int i = 0; i < list.Count - windowSize; i++)
            {
                for (int j = 0; j < windowSize; j++)
                {
                    if (OPCODES_TO_MATCH[j] == null)
                    {
                        Main.LogError("OPCODES_TO_MATCH cannot contain null!");
                        return instructions;
                    }

                    string logLine = $"{j}:\t{OPCODES_TO_MATCH[j]}";

                    int index = i + j;
                    OpCode opCode = list[index].opcode;
                    if (j < OPCODES_TO_MATCH.Count && opCode != OPCODES_TO_MATCH[j])
                    {
                        if (j > 0)
                        {
                            logLine += $" != {opCode}";
                            Main.LogInfo($"{logLine}\tFAIL");
                        }
                        break;
                    }
                    logLine += $" == {opCode}";

                    if (j == 0)
                        Debug.Log("-------------------------");

                    if (j < OPERANDS_TO_MATCH.Count && OPERANDS_TO_MATCH[j] != null)
                    {
                        logLine += $"\t{OPERANDS_TO_MATCH[j]}";
                        object operand = list[index].operand;
                        if (OPERANDS_TO_MATCH[j] != operand)
                        {
                            logLine += $" != {operand} ({operand.GetType()})";
                            Main.LogInfo($"{logLine}\tFAIL");
                            break;
                        }
                        logLine += $" == {operand} ({operand.GetType()})";
                    }
                    Main.LogInfo($"{logLine}\tPASS");

                    if (j == OPCODES_TO_MATCH.Count - 1)
                    {
                        Main.LogInfo($"Found match {++matches}");
                        if (matches > EXPECTED_MATCH_COUNT)
                        {
                            Main.LogError("Number of matches found exceeded EXPECTED_MATCH_COUNT! Returning original IL.");
                            return instructions;
                        }

                        // Perform replacements
                        for (int k = 0; k < MODIFIED_OPCODES.Count; k++)
                        {
                            int replacementIndex = i + k;
                            if (MODIFIED_OPCODES[k] == null || list[replacementIndex].opcode == MODIFIED_OPCODES[k])
                            {
                                continue;
                            }
                            OpCode beforeChange = list[replacementIndex].opcode;
                            list[replacementIndex].opcode = MODIFIED_OPCODES[k];
                            Main.LogInfo($"Line {replacementIndex}: Replaced Opcode ({beforeChange} ==> {MODIFIED_OPCODES[k]})");
                        }

                        for (int k = 0; k < MODIFIED_OPERANDS.Count; k++)
                        {
                            if (MODIFIED_OPERANDS[k] != null)
                            {
                                int replacementIndex = i + k;
                                object beforeChange = list[replacementIndex].operand;
                                list[replacementIndex].operand = MODIFIED_OPERANDS[k];
                                Main.LogInfo($"Line {replacementIndex}: Replaced operand ({beforeChange ?? "null"} ==> {MODIFIED_OPERANDS[k] ?? "null"})");
                            }
                        }
                    }
                }
            }

            Main.LogWarning($"{(matches > 0 ? (matches == EXPECTED_MATCH_COUNT ? "Transpiler Patch succeeded with no errors" : $"Completed with {matches}/{EXPECTED_MATCH_COUNT} found.") : "Failed to find match")}");
            return list.AsEnumerable();
        }
    }
}
