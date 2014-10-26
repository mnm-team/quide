/**
    This file is part of QuIDE.

    QuIDE - The Quantum IDE
    Copyright (C) 2014  Joanna Patrzyk, Bartłomiej Patrzyk

    This program is free software: you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.

    This program is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with this program.  If not, see <http://www.gnu.org/licenses/>.
 */

using Microsoft.CSharp;
using QuantumModel;
using QuantumParser.Operations;
using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace QuantumParser
{
    public class KeywordNotAllowedException : Exception
    {
        public KeywordNotAllowedException(string keyword)
            : base("Code cannot contain some special keywords. Please remove them. \nForbidden keyword found: " + keyword)
        {
        }
    }

    public class Parser
    {
        public Parser()
        {
        }    

        public Assembly CompileForRun(string code)
        {
            // Check for safety
            Validate(code);

            // Compiler and CompilerParameters
            CSharpCodeProvider codeProvider = new CSharpCodeProvider();
            CompilerParameters compParameters = new CompilerParameters();
            // Add an assembly reference.
            compParameters.ReferencedAssemblies.Add("System.dll");
            compParameters.ReferencedAssemblies.Add("System.Core.dll");
            compParameters.ReferencedAssemblies.Add("System.Numerics.dll");
            compParameters.ReferencedAssemblies.Add("Quantum.dll");

            compParameters.GenerateExecutable = true;
            compParameters.GenerateInMemory = true;

            // Compile the code
            CompilerResults compResults = codeProvider.CompileAssemblyFromSource(compParameters, code);

            if (compResults.Errors.Count > 0)
            {
                StringBuilder builder = new StringBuilder("There are errors occured during code compilation:\n");
                foreach (CompilerError ce in compResults.Errors)
                {
                    builder.AppendLine(ce.ToString()).AppendLine();
                }
                throw new Exception(builder.ToString());
            }
            return compResults.CompiledAssembly;
        }

        public Assembly CompileForBuild(string code)
        {
            code = Preprocess(code);

            // Check for safety
            Validate(code);

            // Compiler and CompilerParameters
            CSharpCodeProvider codeProvider = new CSharpCodeProvider();
            CompilerParameters compParameters = new CompilerParameters();
            // Add an assembly reference.
            compParameters.ReferencedAssemblies.Add("System.dll");
            compParameters.ReferencedAssemblies.Add("System.Core.dll");
            compParameters.ReferencedAssemblies.Add("System.Numerics.dll");
            compParameters.ReferencedAssemblies.Add("QuantumParser.dll");

            compParameters.GenerateExecutable = true;
            compParameters.GenerateInMemory = true;

            // Compile the code
            CompilerResults compResults = codeProvider.CompileAssemblyFromSource(compParameters, code);

            if (compResults.Errors.Count > 0)
            {
                StringBuilder builder = new StringBuilder("Compiler errors and warnings:\n\n");
                foreach (CompilerError ce in compResults.Errors)
                {
                    builder.AppendLine(ce.ToString()).AppendLine();
                }
                throw new Exception(builder.ToString());
            }
            return compResults.CompiledAssembly;
        }

        public ComputerModel BuildModel(Assembly asm)
        {
            // Reset previous registers
            QuantumParser.QuantumComputer localComp = QuantumParser.QuantumComputer.GetInstance();
            ComputerModel emptyModel = ComputerModel.CreateModelForParser();
            localComp.Reset(emptyModel);

            // Get the method Main
            MethodInfo method = asm.EntryPoint;

            try
            {
                // Call the static method Main to generate circuit
                method.Invoke(null, null);
            }
            catch (Exception ex)
            {
                StringBuilder builder = new StringBuilder("Circuit builder errors:\n\n");
                builder.Append(ex.InnerException.GetType().ToString()).Append(": ").AppendLine(ex.InnerException.Message);
                throw new Exception(builder.ToString());
            }

            ComputerModel generatedModel = localComp.GetModel();
            generatedModel.AddStepAfter(generatedModel.Steps.Count - 1);

            return generatedModel;
        }

        public void Execute(Assembly asm, TextWriter consoleOutput)
        {
            TextWriter oldOut = Console.Out;
            TextWriter oldErr = Console.Error;
            Console.SetOut(consoleOutput);
            Console.SetError(consoleOutput);

            MethodInfo method = asm.EntryPoint;

            // Call the static method 'Generate'
            method.Invoke(null, null);

            Console.SetOut(oldOut);
            Console.SetError(oldErr);
        }

        public Dictionary<string, List<MethodCode>> GetMethodsCodes(string code)
        {
            Dictionary<string, List<MethodCode>> bodies = new Dictionary<string, List<MethodCode>>();

            string noComments = RemoveSingleLineComments(code);
            noComments = RemoveMultiLineComments(noComments);

            List<string> extDefs = GetExtensionDefs(noComments);

            char[] braces = new char[] { '{', '}' };

            foreach (string def in extDefs)
            {
                int defInd = noComments.IndexOf(def);
                int ind = noComments.IndexOf('{', defInd + def.Length);
                
                int next = noComments.IndexOfAny(braces, ind + 1);
                int prev = ind;
                int opened = 1;
                while (opened > 0 && next > -1)
                {
                    if (noComments[next] == '{')
                    {
                        opened++;
                    }
                    else
                    {
                        opened--;
                    }
                    prev = next;
                    next = noComments.IndexOfAny(braces, next + 1);
                }
                if (opened == 0)
                {
                    string body = noComments.Substring(defInd, prev - defInd + 1);
                    string name = GetName(def);

                    string paramDef = GetParamDef(def);
                    paramDef = RemoveSqares(paramDef);
                    paramDef = RemoveAngles(paramDef);
                    paramDef = RemoveRounds(paramDef);
                    paramDef = RemoveBraces(paramDef);
                    string[] pars = GetParams(paramDef);
                    string[] types = pars.Select(x => GetKeywordTypeName(x).Item2).ToArray();

                    MethodCode meth = new MethodCode() { Name = name, Code = body, ParametersTypes = types };

                    List<MethodCode> existed;
                    if (bodies.TryGetValue(name, out existed))
                    {
                        existed.Add(meth);
                    }
                    else
                    {
                        existed = new List<MethodCode>();
                        existed.Add(meth);
                        bodies[name] = existed;
                    }
                }
            }
            return bodies;
        }

        private string Preprocess(string code)
        {
            string pattern = @"using\s+Quantum\s*;";
            string replacement = "";
            //string replacement = "using QuantumParser;";
            Regex rgx = new Regex(pattern);
            string result = rgx.Replace(code, replacement);

            pattern = @"using\s+Quantum\s*\.\s*Operations\s*;";
            replacement = "using QuantumParser.Operations;";
            rgx = new Regex(pattern);
            result = rgx.Replace(result, replacement);

            pattern = @"namespace\s+[a-zA-Z_][a-zA-Z0-9_]*\s*{";
            replacement = "namespace QuantumParser.Test\n{";
            rgx = new Regex(pattern);
            result = rgx.Replace(result, replacement);

            string noComments = RemoveSingleLineComments(result);
            noComments = RemoveMultiLineComments(noComments);

            List<string> extDefs = GetExtensionDefs(noComments);

            foreach (string def in extDefs)
            {
                StringBuilder sb = new StringBuilder("\n");

                //if (comp.Group)
                //{
                //    object[] parameters = new object[] { comp, register };
                //    comp.AddParametricGate("Walsh", parameters);
                //    return;
                //}
                //else
                //{
                //    comp.Group = true;
                //}

                string methodName = GetName(def);

                string paramDef = GetParamDef(def);
                paramDef = RemoveSqares(paramDef);
                paramDef = RemoveAngles(paramDef);
                paramDef = RemoveRounds(paramDef);
                paramDef = RemoveBraces(paramDef);
                string[] pars = GetParams(paramDef);

                string compName = GetKeywordTypeName(pars[0]).Item3;

                sb.Append("if (").Append(compName).AppendLine(".Group)");
                sb.AppendLine("{");
                sb.Append("object[] parameters = new object[] { ").Append(compName);

                for (int i = 1; i < pars.Length; i++)
                {
                    Tuple<string, string, string> keyTypeName = GetKeywordTypeName(pars[i]);
                    if (!string.IsNullOrWhiteSpace(keyTypeName.Item3))
                    {
                        sb.Append(", ").Append(keyTypeName.Item3);
                    }
                }
                sb.AppendLine(" };");
                sb.Append(compName).Append(".AddParametricGate(\"");
                sb.Append(methodName).AppendLine("\", parameters);");
                sb.AppendLine("return;");
                sb.AppendLine("}");
                sb.AppendLine("else");
                sb.AppendLine("{");
                sb.Append(compName).AppendLine(".Group = true;");
                sb.AppendLine("}");

                int defInd = noComments.IndexOf(def);
                int leftBraceInd = noComments.IndexOf("{", defInd + def.Length);
                noComments = noComments.Insert(leftBraceInd + 1, sb.ToString());
            }
            return noComments;
        }

        private void Validate(string code)
        {
            // Check for safety
            // Reject all System.* except System.Math and System.Numerics
            string regexPattern = @"System\s*\.\s*([\w]*)";
            Regex regex = new Regex(regexPattern);
            Match match = regex.Match(code);

            while (match.Success)
            {
                if (!match.Groups[1].Value.StartsWith("Math") && 
                    !match.Groups[1].Value.StartsWith("Numerics") && 
                    !match.Groups[1].Value.StartsWith("Collections") &&
                    !match.Groups[1].Value.StartsWith("Diagnostics") && // TODO for testing, probably to remove
                    !match.Groups[1].Value.StartsWith("Linq"))
                {
                    throw new KeywordNotAllowedException(match.Groups[0].Value);
                }
                match = match.NextMatch();
            }

            if (code.Contains("Microsoft"))
            {
                throw new KeywordNotAllowedException("Microsoft");
            }
        }

        private string RemoveSingleLineComments(string code)
        {
            char[] sep = new char[] { '\n', '\r' };
            string[] lines = code.Split(sep, StringSplitOptions.RemoveEmptyEntries);
            for (int i = 0; i < lines.Length; i++)
            {
                string line = lines[i];
                int ind = line.IndexOf("//");
                while (ind > -1)
                {
                    List<int> quotes = new List<int>();
                    int q = line.IndexOf("\"");
                    while (q > -1)
                    {
                        quotes.Add(q);
                        int newQ = line.IndexOf("\"", q + 1);
                        while (newQ > -1 && line[newQ - 1] == '\\')
                        {
                            newQ = line.IndexOf("\"", newQ + 1);
                        }
                        q = newQ;
                    }
                    int quotesBefore = 0;
                    foreach (int q1 in quotes)
                    {
                        if (ind > q1)
                        {
                            quotesBefore++;
                        }
                        else
                        {
                            break;
                        }
                    }
                    if (quotesBefore % 2 == 0)
                    {
                        lines[i] = line.Remove(ind);
                        ind = -1;
                    }
                    else
                    {
                        ind = line.IndexOf("//", ind + 1);
                    }
                }
            }
            StringBuilder sb = new StringBuilder();
            foreach (string line in lines)
            {
                if (!string.IsNullOrWhiteSpace(line))
                {
                    sb.AppendLine(line);
                }
            }
            return sb.ToString();
        }

        private string RemoveMultiLineComments(string code)
        {
            string pattern = @"/\*(.|[\r\n])*?\*/";

            int ind = code.IndexOf("/*");
            while (ind > -1)
            {
                List<int> quotes = new List<int>();
                int q = code.IndexOf("\"");
                while (q > -1)
                {
                    quotes.Add(q);
                    int newQ = code.IndexOf("\"", q + 1);
                    while (newQ > -1 && code[newQ - 1] == '\\')
                    {
                        newQ = code.IndexOf("\"", newQ + 1);
                    }
                    q = newQ;
                }

                int quotesBefore = 0;
                foreach (int q1 in quotes)
                {
                    if (ind > q1)
                    {
                        quotesBefore++;
                    }
                    else
                    {
                        break;
                    }
                }
                if (quotesBefore % 2 == 0)
                {
                    int indEnd = code.IndexOf("*/", ind + 1);
                    if (indEnd > -1)
                    {
                        code = code.Remove(ind, indEnd - ind + 2);
                        ind = code.IndexOf("/*");
                    }
                    else
                    {
                        ind = -1;
                    }
                }
                else
                {
                    ind = code.IndexOf("/*", ind + 1);
                }
            }
            return code;
        }

        private List<string> GetExtensionDefs(string code)
        {
            List<string> toReturn = new List<string>();

            List<int> bras = new List<int>();
            int braInd = code.IndexOf("(");
            while (braInd > -1)
            {
                bras.Add(braInd);
                braInd = code.IndexOf("(", braInd + 1);
            }

            List<int> kets = new List<int>();
            int ketInd = code.IndexOf(")");
            while (ketInd > -1)
            {
                kets.Add(ketInd);
                ketInd = code.IndexOf(")", ketInd + 1);
            }

            bras.AddRange(kets);
            bras.Sort();

            string pattern = @"public\s+static\s+void\s+[a-zA-Z_][a-zA-Z0-9_]*" +
                @"\s*\(\s*this\s+QuantumComputer";
            Regex regex = new Regex(pattern);
            Match match = regex.Match(code);
            while (match.Success)
            {
                int bra = match.Value.IndexOf("(") + match.Index;
                int ind = bras.IndexOf(bra);
                int opened = 1;
                while (opened > 0 && ind < bras.Count - 1)
                {
                    ind++;
                    if (code[bras[ind]] == '(')
                    {
                        opened++;
                    }
                    else
                    {
                        opened--;
                    }
                }
                if (opened == 0)
                {
                    int endKet = bras[ind];
                    string def = code.Substring(match.Index, endKet - match.Index + 1);
                    toReturn.Add(def);
                }
                match = match.NextMatch();
            }

            return toReturn;
        }

        private string GetName(string methodDef)
        {
            string pattern = @"(public\s+static\s+void\s+)([a-zA-Z_][a-zA-Z0-9_]*)" +
                @"(\s*\(\s*this\s+QuantumComputer)";

            Regex regex = new Regex(pattern);
            Match match = regex.Match(methodDef);
            if (match.Success)
            {
                return match.Groups[2].Value;
            }
            return null;
        }

        private string GetParamDef(string methodDef)
        {
            int braInd = methodDef.IndexOf("(");
            if (braInd > -1)
            {
                return methodDef.Substring(braInd);
            }
            return null;
        }

        private string[] GetParams(string paramDef)
        {
            string[] toReturn = paramDef.Split(',');
            for (int i = 0; i < toReturn.Length; i++)
            {
                string param = toReturn[i];
                int eqInd = param.IndexOf("=");
                if (eqInd > -1)
                {
                    toReturn[i] = param.Remove(eqInd);
                }
            }
            return toReturn;
        }

        private Tuple<string, string, string> GetKeywordTypeName(string param)
        {
            string[] words = param.Split(new char[0], StringSplitOptions.RemoveEmptyEntries);

            if(words.Length == 3) {
                return new Tuple<string, string, string>(words[0], words[1], words[2]);
            }
            else if (words.Length == 2)
            {
                return new Tuple<string, string, string>(null, words[0], words[1]);
            }
            else
            {
                return new Tuple<string, string, string>(null, null, words.Last());
            }
        }

        private string RemoveSqares(string paramDef)
        {
            char[] squares = new char[]{'[', ']'};
            return RemoveBrackets(paramDef, squares[0], squares[1]);
        }

        private string RemoveAngles(string paramDef)
        {
            char[] brackets = new char[] { '<', '>' };
            return RemoveBrackets(paramDef, brackets[0], brackets[1]);
        }

        private string RemoveRounds(string paramDef)
        {
            char[] brackets = new char[] { '(', ')' };
            if (paramDef.IndexOf('(') == 0)
            {
                paramDef = paramDef.Substring(1, paramDef.Length - 2);
            }
            return RemoveBrackets(paramDef, brackets[0], brackets[1]);
        }

        private string RemoveBraces(string paramDef)
        {
            char[] brackets = new char[] { '{', '}' };
            return RemoveBrackets(paramDef, brackets[0], brackets[1]);
        }

        private string RemoveBrackets(string text, char left, char right)
        {
            char[] squares = new char[] { left, right };
            int ind = text.IndexOf(left);
            while (ind > -1)
            {
                int next = text.IndexOfAny(squares, ind + 1);
                int prev = ind;
                int opened = 1;
                while (opened > 0 && next > -1)
                {
                    if (text[next] == left)
                    {
                        opened++;
                    }
                    else
                    {
                        opened--;
                    }
                    prev = next;
                    next = text.IndexOfAny(squares, next + 1);
                }
                if (opened == 0)
                {
                    text = text.Remove(ind, prev - ind + 1);
                    ind = text.IndexOf(left);
                }
                else
                {
                    ind = -1;
                }
            }
            return text;
        }

        //private string ChangeExtensions(string code)
        //{
        //    string pattern = @"(public\s+static\s+void\s+)([a-zA-Z_][a-zA-Z0-9_]*)" +
        //        @"(\s*\(\s*this\s+QuantumComputer)";
        //    string replacement = @"$1$2_Dynamic$3";

        //    Regex regex = new Regex(pattern);
        //    string result = regex.Replace(code, replacement);

        //    return result;
        //}

        internal static IEnumerable<MethodInfo> GetExtensionMethods(Assembly assembly, Type extendedType, string methodName = null)
        {
            var query = from type in assembly.GetTypes()
                        where type != typeof(QuantumParser.Operations.GatesExtension)
                        where type.IsSealed && !type.IsGenericType && !type.IsNested
                        from method in type.GetMethods(BindingFlags.Static
                            | BindingFlags.Public | BindingFlags.NonPublic)
                        where method.IsDefined(typeof(ExtensionAttribute), false)
                        where method.GetParameters()[0].ParameterType == extendedType
                        where (string.IsNullOrWhiteSpace(methodName) || method.Name.Equals(methodName))
                        select method;
            return query;
        }
    }
}
