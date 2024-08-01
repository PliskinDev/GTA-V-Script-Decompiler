using System.Collections.Generic;

namespace Decompiler.Patches
{
    internal class ReplaceConditionalJumpPatch : Patch
    {
        public ReplaceConditionalJumpPatch(Function function) : base(function)
        {
        }

        public override string GetName(int start, int end) => $"Replace Conditional Jump With DROP";

        public override byte[] GetPatch(int start, int end)
        {
            List<byte> bytes = new();

            for (var i = start; i < end; i++)
            {
                bytes.Add(Instruction.UnmapOpcode(Opcode.DROP));

                foreach (var _ in Function.Instructions[i].Operands)
                {
                    bytes.Add(Instruction.UnmapOpcode(Opcode.NOP));
                }
            }

            return bytes.ToArray();
        }

        public override bool ShouldEnablePatch(int start, int end) => true;

        public override bool ShouldShowPatch(int start, int end) => end - start == 1 && Function.Instructions[start].IsConditionJump;
    }
}
