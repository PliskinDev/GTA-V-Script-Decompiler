using System;
using System.Collections.Generic;
#if OS_WINDOWS
using System.Windows.Forms;
#endif // OS_WINDOWS

namespace Decompiler.Patches
{
    internal class ReturnFunctionPatch : Patch
    {
        private uint ReturnValue = 0;

        public ReturnFunctionPatch(Function function) : base(function)
        {
        }

        public override string GetName(int start, int end) => "Place Function Return";

        public override byte[] GetPatch(int start, int end)
        {
            List<byte> bytes = new();

            if (Function.NumReturns != 0)
            {
                if (ReturnValue <= 7)
                    bytes.Add(Instruction.UnmapOpcode((Opcode)(((byte)Opcode.PUSH_CONST_0) + ReturnValue)));
                else if (ReturnValue <= 65535)
                {
                    bytes.Add(Instruction.UnmapOpcode(Opcode.PUSH_CONST_S16));
                    bytes.AddRange(BitConverter.GetBytes((short)ReturnValue));
                }
                else
                {
                    bytes.Add(Instruction.UnmapOpcode(Opcode.PUSH_CONST_U32));
                    bytes.AddRange(BitConverter.GetBytes(ReturnValue));
                }
            }

            bytes.Add(Instruction.UnmapOpcode(Opcode.LEAVE));
            bytes.Add((byte)Function.NumParams);
            bytes.Add((byte)Function.NumReturns);

            return bytes.ToArray();
        }

        public override bool ShouldEnablePatch(int start, int end) => end - start == 1 && Function.Instructions[start].OriginalOpcode != Opcode.ENTER;

        public override bool ShouldShowPatch(int start, int end) => true;

        public override bool GetData(int start, int end)
        {
            uint value = 0;

#if OS_WINDOWS
            if (Function.Instructions[start].OriginalOpcode == Opcode.ENTER)
            {
                MessageBox.Show("Cannot place function return directly on an ENTER, try placing it after the ENTER", "", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            if (Function.NumReturns > 1)
            {
                MessageBox.Show("Cannot apply patch as this function returns multiple values", "", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
            else if (Function.NumReturns > 0)
            {
                InputBox box = new();
                box.Show("Function Return", "Enter value to return", "0");

                if (!uint.TryParse(box.Value, out value))
                {
                    MessageBox.Show("Integer is invalid", "", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return false;
                }
            }
#endif // OS_WINDOWS

            ReturnValue = value;

            return true;
        }
    }
}
