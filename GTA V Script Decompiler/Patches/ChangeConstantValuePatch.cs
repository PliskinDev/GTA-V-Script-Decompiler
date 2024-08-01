using System;
using System.Collections.Generic;
using System.Linq;
#if OS_WINDOWS
using System.Windows.Forms;
#endif // OS_WINDOWS

namespace Decompiler.Patches
{
    internal class ChangeConstantValuePatch : Patch
    {
        private enum ConstantType : uint
        {
            SHORTHAND = 7,
            U8 = 255,
            U16 = 65535,
            U24 = 16777215,
            U32 = 4294967295
        }

        private ConstantType constantType;
        private uint Value;

        public ChangeConstantValuePatch(Function function) : base(function)
        {
        }

        public override string GetName(int start, int end) => "Change Constant Push Value";

        public override byte[] GetPatch(int start, int end)
        {
            List<byte> bytes = new();

            if (constantType == ConstantType.SHORTHAND)
                bytes.Add(Instruction.UnmapOpcode((Opcode)(((byte)Opcode.PUSH_CONST_0) + Value)));
            else if (constantType == ConstantType.U8)
            {
                bytes.Add(Instruction.UnmapOpcode(Opcode.PUSH_CONST_U8));
                bytes.Add((byte)Value);
            }
            else if (constantType == ConstantType.U16)
            {
                bytes.Add(Instruction.UnmapOpcode(Opcode.PUSH_CONST_S16));
                bytes.AddRange(BitConverter.GetBytes((short)Value));
            }
            else if (constantType == ConstantType.U24)
            {
                bytes.Add(Instruction.UnmapOpcode(Opcode.PUSH_CONST_U24));
                bytes.AddRange(BitConverter.GetBytes(Value).Skip(1)); // todo does this work?
            }
            else if (constantType == ConstantType.U32)
            {
                bytes.Add(Instruction.UnmapOpcode(Opcode.PUSH_CONST_U32));
                bytes.AddRange(BitConverter.GetBytes(Value));
            }

            return bytes.ToArray();
        }

        public override bool ShouldEnablePatch(int start, int end) => true;

        public override bool ShouldShowPatch(int start, int end)
        {
            var op = Function.Instructions[start].Opcode;
            return (end - start == 1) && (op == Opcode.PUSH_CONST_U8 || op == Opcode.PUSH_CONST_S16 || op == Opcode.PUSH_CONST_U32 || op == Opcode.PUSH_CONST_U24 || (op >= Opcode.PUSH_CONST_0 && op <= Opcode.PUSH_CONST_7));
        }

        public override bool GetData(int start, int end)
        {
            var op = Function.Instructions[start].Opcode;

            if (op is >= Opcode.PUSH_CONST_0 and <= Opcode.PUSH_CONST_7)
                constantType = ConstantType.SHORTHAND;
            else if (op == Opcode.PUSH_CONST_U8)
                constantType = ConstantType.U8;
            else if (op == Opcode.PUSH_CONST_S16)
                constantType = ConstantType.U16;
            else if (op == Opcode.PUSH_CONST_U24)
                constantType = ConstantType.U24;
            else if (op == Opcode.PUSH_CONST_U32)
                constantType = ConstantType.U32;

#if OS_WINDOWS
            InputBox box = new();
            box.Show("Enter value", $"Enter new value (range 0 - {(uint)constantType})\nPrefix with $ to hash string");

            uint value;

            if (box.Value.StartsWith('$'))
            {
                value = Utils.Joaat(box.Value.Remove(0, 1));
            }
            else
            {
                if (!uint.TryParse(box.Value, out value))
                {
                    MessageBox.Show("Integer is invalid", "", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return false;
                }
            }

            if (value > (uint)constantType)
            {
                MessageBox.Show("Value out of range", "", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }

            Value = value;
#endif // OS_WINDOWS

            return true;
        }
    }
}
