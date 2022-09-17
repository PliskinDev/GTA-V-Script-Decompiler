﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Decompiler
{
	/// <summary>
	/// This is what i use for detecting if a variable is a int/float/bool/struct/array etc
	/// </summary>
	public class VariableStorage
    {
        ListType listType;//static/function_var/parameter
        List<Variable> Vars;
        Dictionary<int, int> VarRemapper; //not necessary, just shifts variables up if variables before are bigger than 1 DWORD
		private int count;
		private int scriptParamCount = 0;
		private int scriptParamStart { get { return Vars.Count - scriptParamCount; } }
        public VariableStorage(ListType type, int varcount)
        {
            listType = type;
            Vars = new List<Variable>();
            for (int i = 0; i < varcount; i++)
            {
                Vars.Add(new Variable(i));
            }
			count = varcount;
        }
        public VariableStorage(ListType type)
        {
            listType = type;
			Vars = new List<Variable>();
        }
        public void AddVar(int value)
        {
            Vars.Add(new Variable(Vars.Count, value));//only used for static variables that are pre assigned
        }

        public void AddVar(long value)
        {
            Vars.Add(new Variable(Vars.Count, value));
        }
        public void checkvars()
        {
            unusedcheck();
        }
		//This shouldnt be needed but in gamever 1.0.757.2
		//It seems a few of the scripts are accessing items from the
		//Stack frame that they havent reserver
		void BrokenCheck(uint index)
		{
			if (index >= Vars.Count)
			{
				for (int i = Vars.Count; i <= index; i++)
				{
					Vars.Add(new Variable(i));
				}
			}
		}
        public string GetVarName(uint index)
        {
            Variable var = Vars[(int)index];

			if (var.Name != "")
				return var.Name;

			string name = "";
            if (var.DataType == Stack.DataType.String)
            {
                name = "c";
            }
            else if (var.ImmediateSize == 1)
            {
                name = Types.GetTypeInfo(var.DataType).Prefix;
            }

            switch (listType)
            {
                case ListType.Statics: name += (index >= scriptParamStart ? "ScriptParam_" : "Local_"); break;
                case ListType.Vars: name += "Var"; break;
                case ListType.Params: name += "Param"; break;
            }

            try
            {
                if (Properties.Settings.Default.ShiftVariables) return name + VarRemapper[(int)index].ToString();
                else
                {
                    return name + (listType == ListType.Statics && index >= scriptParamStart ? index - scriptParamStart : index).ToString();
                }
            }
            catch (KeyNotFoundException)
            {
                return name + (listType == ListType.Statics && index >= scriptParamStart ? index - scriptParamStart : index).ToString();
            }
        }
		public void SetScriptParamCount(int count)
		{
			if (listType == ListType.Statics)
			{
				scriptParamCount = count;
			}
		}
		public string[] GetDeclaration()
		{
			List<string> Working = new List<string>();
			string varName = "";
			string dataType = "";

			int i = 0;
			int j = -1;
			foreach (Variable var in Vars)
			{
				varName = GetVarName((uint)i);
				j++;
				if (!var.Is_Used)
				{
					if (!Properties.Settings.Default.ShiftVariables)
						i++;
					continue;
				}

				if (listType == ListType.Vars && !var.Is_Called)
				{
					if (!Properties.Settings.Default.ShiftVariables)
						i++;
					continue;
				}

				if (var.ImmediateSize == 1)
				{
					dataType = Types.GetTypeInfo(var.DataType).VarDec;
					
				}
				else if (var.DataType == Stack.DataType.String)
				{
					dataType = "char ";
				}
				else
				{
					dataType = "struct<" + var.ImmediateSize.ToString() + "> ";
				}

				string value = "";

				if (!var.Is_Array)
				{
					if (listType == ListType.Statics)
					{
						if (var.ImmediateSize == 1)
						{
							value = " = " + Utils.Represent(Vars[j].Value, var.DataType);
						}
						else if (var.DataType == Stack.DataType.String)
						{

							List<byte> data = new List<byte>();

							for (int l = 0; l < var.ImmediateSize; l++)
							{
								data.AddRange(BitConverter.GetBytes(Vars[j + l].Value));
							}

							int len = data.IndexOf(0);
							data.RemoveRange(len, data.Count - len);
							value = " = \"" + Encoding.ASCII.GetString(data.ToArray()) + "\"";

						}
						else if (var.ImmediateSize > 1)
						{
							value += " = { " + Utils.Represent(Vars[j].Value, Stack.DataType.Int);

							for (int l = 1; l < var.ImmediateSize; l++)
							{
								value += ", " + Utils.Represent(Vars[j + l].Value, Stack.DataType.Int);
							}

							value += " } ";
						}
					}
				}
				else
				{
					if (listType == ListType.Statics)
					{
						if (var.ImmediateSize == 1)
						{
							value = " = { ";

							for (int k = 0; k < var.Value; k++)
							{
								value += Utils.Represent(Vars[j + 1 + k].Value, var.DataType) + ", ";
							}

							if (value.Length > 2)
							{
								value = value.Remove(value.Length - 2);
							}

							value += " }";
						}
						else if (var.DataType == Stack.DataType.String)
						{
							value = " = { ";

							for (int k = 0; k < var.Value; k++)
							{
								List<byte> data = new List<byte>();
								for (int l = 0; l < var.ImmediateSize; l++)
								{
									data.AddRange(BitConverter.GetBytes(Vars[j + 1 + var.ImmediateSize * k + l].Value));
								}
								value += "\"" + Encoding.ASCII.GetString(data.ToArray()) + "\", ";
							}


							if (value.Length > 2)
							{
								value = value.Remove(value.Length - 2);
							}

							value += " }";
						}
					}
				}

				string decl = dataType + varName;

				if (var.Is_Array)
				{
					decl += "[" + var.Value.ToString() + "]";
				}

				if (var.DataType == Stack.DataType.String)
				{
					decl += "[" + (var.ImmediateSize*(8)).ToString() + "]";
				}

				Working.Add(decl + value + ";");
				i++;
			}
			return Working.ToArray();
		}
		public string GetPDec()
		{
			if (listType != ListType.Params)
				throw new DecompilingException("Only params use this declaration");
			string decl = "";
			int i = 0;
			foreach (Variable var in Vars)
			{
				if (!var.Is_Used)
				{
					if (!Properties.Settings.Default.ShiftVariables)
					{
						i++;	 
					}
					continue;
				}			   
				string datatype = "";
				if (!var.Is_Array)
				{
					if (var.DataType == Stack.DataType.String)
					{
						datatype = "char[" + (var.ImmediateSize * 4).ToString() + "] c";
					}
					else if (var.ImmediateSize == 1)
						datatype = Types.GetTypeInfo(var.DataType).VarDec;
					/*else if (var.Immediatesize == 3)
					{
						datatype = "vector3 v";
					}*/
					else datatype = "struct<" + var.ImmediateSize.ToString() + "> ";
				}
				else
				{
					if (var.DataType == Stack.DataType.String)
					{
						datatype = "char[" + (var.ImmediateSize * 4).ToString() + "][] c";
					}
					else if (var.ImmediateSize == 1)
						datatype = Types.GetTypeInfo(var.DataType).ArrayDec;
					/*else if (var.Immediatesize == 3)
					{
						datatype = "vector3[] v";
					}*/
					else datatype = "struct<" + var.ImmediateSize.ToString() + ">[] ";
				}
				decl += datatype + "Param" + i.ToString() + ", ";
				i++;
			}
			if (decl.Length > 2)
				decl = decl.Remove(decl.Length - 2);
			return decl;
		}
        /// <summary>
        /// Remove unused vars from declaration, and shift var indexes down
        /// </summary>
        private void unusedcheck()
        {
            VarRemapper = new Dictionary<int, int>();
            for (int i = 0, k=0; i < Vars.Count; i++)
            {
                if (!Vars[i].Is_Used)
                    continue;
                if (listType == ListType.Vars && !Vars[i].Is_Called)
                    continue;
                if (Vars[i].Is_Array)
                {
                    for (int j = i + 1; j < i + 1 + Vars[i].Value * Vars[i].ImmediateSize; j++)
                    {
                        Vars[j].SetNotUsed();
                    }
                }
                else if (Vars[i].ImmediateSize > 1)
                {
                    for (int j = i + 1; j < i + Vars[i].ImmediateSize; j++)
                    {
	                    BrokenCheck((uint)j);
                        Vars[j].SetNotUsed();
                    }
                }
                VarRemapper.Add(i, k);
                k++;
            }
        }
        public Stack.DataType GetTypeAtIndex(uint index)
        {
            return Vars[(int)index].DataType;
        }
        public void SetTypeAtIndex(uint index, Stack.DataType type)
        {
            Vars[(int)index].DataType = type;
        }
        public Variable GetVarAtIndex(uint index)
        {
	        BrokenCheck(index);
            return Vars[(int)index];
        }

        public enum ListType
        {
            Statics,
            Params,
            Vars
        }
    }
}