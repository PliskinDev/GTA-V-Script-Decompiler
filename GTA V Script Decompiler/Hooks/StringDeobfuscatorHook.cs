using Decompiler.Ast;
using System.Collections.Generic;
using String = Decompiler.Ast.String;

namespace Decompiler.Hooks
{
// Deobfuscator broke on b3407. TODO: update and implement constant time evaluation
#if false
    internal class StringDeobfuscator_GetStringWithRotate_Hook : FunctionHook
    {
        public override uint[] Hashes => new uint[] { 0x3270A688, 0xFA2B0B15 };

        public override bool Hook(Function function, List<AstToken> args, Stack stack)
        {
            var iParam0 = (int)(args[0] as Ast.ConstantInt).GetValue();
            iParam0 = 32 - iParam0;

            var text = iParam0 switch
            {
                0 => "port",
                1 => "al",
                2 => "ry",
                3 => "n",
                4 => "WAR",
                5 => "bar",
                6 => "m",
                7 => "dset",
                8 => "lie",
                9 => "s",
                10 => "01",
                11 => "n_DEA",
                12 => "w",
                13 => "_1_tele",
                14 => "_aln",
                15 => "_R",
                16 => "_01_soun",
                17 => "IE",
                18 => "pa",
                19 => "t",
                20 => "scr",
                21 => "rc",
                22 => "th",
                23 => "a",
                24 => "p_in",
                25 => "_scene",
                26 => "_GR_",
                27 => "dlc_gr",
                28 => "_CS2_",
                29 => "oneshot",
                30 => "General",
                31 => "sounds",
                _ => ""
            };

            stack.Push(new String(function, text));
            return true;
        }
    }

    internal class StringDeobfuscator_ReorderString_4_Hook : FunctionHook
    {
        public const uint REORDER_STRING_4_64_HASH = 0xD877850F;
        public const uint REORDER_STRING_4_32_HASH = 0x1826CAD6;
        public const uint REORDER_STRING_4_24_HASH = 0x9264C97B;

        public override uint[] Hashes => new uint[] { REORDER_STRING_4_64_HASH, REORDER_STRING_4_32_HASH, REORDER_STRING_4_24_HASH };

        public override bool Hook(Function function, List<AstToken> args, Stack stack)
        {
            var pushSize = function.Hash switch
            {
                REORDER_STRING_4_24_HASH => 6,
                _ => 8
            };

            stack.Push(new String(function,
                (args[1] as String).GetString() +
                (args[0] as String).GetString() +
                (args[3] as String).GetString() +
                (args[2] as String).GetString()
            ));

            for (var i = 0; i < (pushSize - 1); i++)
                stack.Push(new String(function, ""));

            return true;
        }

        internal class StringDeobfuscator_ReorderString_8_Hook : FunctionHook
        {
            public override uint[] Hashes => new uint[] { 0x8859FAB6 };

            public override bool Hook(Function function, List<AstToken> args, Stack stack)
            {
                stack.Push(new String(function,
                    (args[4] as String).GetString() +
                    (args[1] as String).GetString() +
                    (args[7] as String).GetString() +
                    (args[5] as String).GetString() +
                    (args[6] as String).GetString() +
                    (args[2] as String).GetString() +
                    (args[0] as String).GetString() +
                    (args[3] as String).GetString()
                ));

                for (var i = 0; i < 15; i++)
                    stack.Push(new String(function, ""));

                return true;
            }
        }
    }
#endif
}
