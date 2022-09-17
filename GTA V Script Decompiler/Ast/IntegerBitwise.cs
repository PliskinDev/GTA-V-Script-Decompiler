﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Decompiler.Ast
{
    internal class IntegerAnd : AstToken
    {
        AstToken Lhs;
        AstToken Rhs;
        public IntegerAnd(Function func, AstToken lhs, AstToken rhs) : base(func)
        {
            Lhs = lhs;
            Rhs = rhs;
        }

        bool IsLogicalOperation()
        {
            return Lhs is not ConstantInt && Rhs is not ConstantInt;
        }

        public override Stack.DataType GetType()
        {
            return IsLogicalOperation() ? Stack.DataType.Bool : Stack.DataType.Int;
        }

        public override string ToString()
        {
            if (IsLogicalOperation())
                return Lhs + " && " + Rhs;
            else
                return Lhs + " & " + Rhs;
        }
    }

    internal class IntegerOr : AstToken
    {
        AstToken Lhs;
        AstToken Rhs;
        public IntegerOr(Function func, AstToken lhs, AstToken rhs) : base(func)
        {
            Lhs = lhs;
            Rhs = rhs;
        }

        bool IsLogicalOperation()
        {
            return Lhs is not ConstantInt && Rhs is not ConstantInt;
        }

        public override Stack.DataType GetType()
        {
            return IsLogicalOperation() ? Stack.DataType.Bool : Stack.DataType.Int;
        }

        public override string ToString()
        {
            if (IsLogicalOperation())
                return Lhs + " || " + Rhs;
            else
                return Lhs + " | " + Rhs;
        }
    }

    internal class IntegerXor : AstToken
    {
        AstToken Lhs;
        AstToken Rhs;
        public IntegerXor(Function func, AstToken lhs, AstToken rhs) : base(func)
        {
            Lhs = lhs;
            Rhs = rhs;
        }

        public override Stack.DataType GetType()
        {
            return Stack.DataType.Int;
        }

        public override string ToString()
        {
            return Lhs + " ^ " + Rhs;
        }
    }
}