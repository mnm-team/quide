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

#region

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.Loader;
using System.Text;
using System.Text.RegularExpressions;
using QuIDE.ViewModels.MainModels.QuantumModel;
using QuIDE.ViewModels.MainModels.QuantumParser.Operations;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using System.Reflection.Metadata;

#endregion

namespace QuIDE.ViewModels.MainModels.QuantumParser;

public class KeywordNotAllowedException(string keyword) : Exception(
    "Code cannot contain some special keywords. Please remove them. \nForbidden keyword found: " +
    keyword);

public partial class Parser
{
    private static long _assemblyLoads;

    public Assembly CompileForRun(string code)
    {
        var compResults = CompilerResults(code);

        return compResults;
    }

    public Assembly CompileForBuild(string code)
    {
        var compResults = CompilerResults(code);

        return compResults;
    }

    private static MetadataReference GetMetadataReference(Assembly a)
    {
        // to avoid Assembly.Location, which does not work in single-file-app
        // see https://github.com/dotnet/runtime/issues/36590#issuecomment-689883856
        unsafe
        {
            a.TryGetRawMetadata(out byte* blob, out int length);
            var moduleMetadata = ModuleMetadata.CreateFromMetadata((IntPtr)blob, length);
            var assemblyMetadata = AssemblyMetadata.Create(moduleMetadata);
            return assemblyMetadata.GetReference();
        }
    }

    private Assembly CompilerResults(string code)
    {
        code = Preprocess(code);
        
        // Check for safety
        Validate(code);

        // TODO: needs to be separated out from view in future! and assembly needs to be loaded into separate context to be manually unloadable!
        // memory usage doesnt appear to increase per call, so maybe it is unloaded automatically by garbage collection
        // in any case we still need to assign a unique name to each assembly
        var assemblies = new HashSet<Assembly>
        {
            typeof(object).Assembly,
            GetType().Assembly,
            Assembly.Load(new AssemblyName("System.Console")), // useful for debugging
            Assembly.Load(new AssemblyName("System.Runtime")),
            Assembly.Load(new AssemblyName("System.Runtime.Numerics"))
        };

        var syntaxTree = CSharpSyntaxTree.ParseText(code);
        var compilation = CSharpCompilation.Create(
            $"QuantumComputer{_assemblyLoads++}",
            new[] { syntaxTree },
            assemblies.Select(GetMetadataReference),
            new CSharpCompilationOptions(OutputKind.ConsoleApplication)); // needs an entry point apparently
        var ms = new MemoryStream();
        var result = compilation.Emit(ms);

        if (!result.Success)
        {
            var builder = new StringBuilder("Compiler errors and warnings:\n\n");
            foreach (var d in result.Diagnostics) builder.AppendLine(d.ToString()).AppendLine();

            ms.Dispose();

            //builder.Append(Environment.NewLine + code);
            throw new Exception(builder.ToString());
        }

        ms.Seek(0, SeekOrigin.Begin);
        var assembly =
            AssemblyLoadContext.Default.LoadFromStream(ms);

        ms.Dispose();

        return assembly;
    }

    public static ComputerModel BuildModel(Assembly asm)
    {
        // Reset previous registers
        var localComp = QuantumComputer.GetInstance();
        var emptyModel = ComputerModel.CreateModelForParser();
        localComp.Reset(emptyModel);

        // Get the method Main
        var method = asm.EntryPoint;

        try
        {
            // Call the static method Main to modify existing QuantumComputer instance from Parser!
            method?.Invoke(null, null);
        }
        catch (Exception ex)
        {
            var builder = new StringBuilder("Circuit builder errors:\n\n");
            builder.Append(ex.InnerException?.GetType()).Append(": ")
                .AppendLine(ex.InnerException?.Message);
            throw new Exception(builder.ToString());
        }

        //TODO: need to unload assembly manually to avoid needing new name for each new assembly
        // AssemblyLoadContext.Default.Unload();

        // get model out new QuantumComputer instance
        var generatedModel = localComp.GetModel();
        generatedModel.AddStepAfter(generatedModel.Steps.Count - 1);

        return generatedModel;
    }

    public static void Execute(Assembly asm, TextWriter consoleOutput)
    {
        var oldOut = Console.Out;
        var oldErr = Console.Error;
        Console.SetOut(consoleOutput);
        Console.SetError(consoleOutput);

        PrintRegisterValues(asm);

        Console.SetOut(oldOut);
        Console.SetError(oldErr);
    }

    private static void PrintRegisterValues(Assembly asm)
    {
        // Reset previous registers
        var localComp = QuantumComputer.GetInstance();
        var emptyModel = ComputerModel.CreateModelForParser();
        localComp.Reset(emptyModel);

        var method = asm.EntryPoint;
        try
        {
            // Call the static method Main to modify existing QuantumComputer instance from Parser!
            method?.Invoke(null, null);
        }
        catch (Exception ex)
        {
            var builder = new StringBuilder("Circuit builder errors:\n\n");
            builder.Append(ex.InnerException?.GetType()).Append(": ")
                .AppendLine(ex.InnerException?.Message);
            throw new Exception(builder.ToString());
        }

        var model = localComp.GetModel();
        var currentStep = model.CurrentStep;

        var eval = CircuitEvaluator.GetInstance();
        eval.InitFromModel(model);

        var se = eval.GetStepEvaluator();
        se.RunToEnd(model.Steps, currentStep);
        model.CurrentStep = model.Steps.Count;

        Console.WriteLine();
        Console.WriteLine(@"Register values after execution:");
        Console.WriteLine();
        Console.WriteLine(eval.RootRegister);
    }

    public static Dictionary<string, List<MethodCode>> GetMethodsCodes(string code)
    {
        var bodies = new Dictionary<string, List<MethodCode>>();

        var noComments = RemoveSingleLineComments(code);
        noComments = RemoveMultiLineComments(noComments);

        var extDefs = GetExtensionDefs(noComments);

        char[] braces = { '{', '}' };

        foreach (var def in extDefs)
        {
            var defInd = noComments.IndexOf(def);
            var ind = noComments.IndexOf('{', defInd + def.Length);

            var next = noComments.IndexOfAny(braces, ind + 1);
            var prev = ind;
            var opened = 1;
            while (opened > 0 && next > -1)
            {
                if (noComments[next] == '{')
                    opened++;
                else
                    opened--;

                prev = next;
                next = noComments.IndexOfAny(braces, next + 1);
            }

            if (opened != 0) continue;

            var body = noComments.Substring(defInd, prev - defInd + 1);
            var name = GetName(def);

            var paramDef = GetParamDef(def);
            paramDef = RemoveSqares(paramDef);
            paramDef = RemoveAngles(paramDef);
            paramDef = RemoveRounds(paramDef);
            paramDef = RemoveBraces(paramDef);
            var pars = GetParams(paramDef);
            var types = pars.Select(x => GetKeywordTypeName(x).Item2).ToArray();

            var meth = new MethodCode { Name = name, Code = body, ParametersTypes = types };

            if (bodies.TryGetValue(name, out var existed))
            {
                existed.Add(meth);
            }
            else
            {
                existed = new List<MethodCode> { meth };
                bodies[name] = existed;
            }
        }

        return bodies;
    }

    private static string Preprocess(string code)
    {
        const string parserLocation = "using QuIDE.ViewModels.MainModels.QuantumParser";

        // Delete "using Quantum{.Operations};" with QuantumParser equivalents
        var pattern = @"using\s+Quantum\s*;";
        var replacement = parserLocation + ";";
        var rgx = new Regex(pattern);
        var result = rgx.Replace(code, replacement);
        pattern = @"using\s+Quantum\s*\.\s*Operations\s*;";
        replacement = parserLocation + ".Operations;";
        rgx = new Regex(pattern);
        result = rgx.Replace(result, replacement);

        // Replace Namespace declaration with "namespace QuantumParser.Test{"
        pattern = @"namespace\s+[a-zA-Z_][a-zA-Z0-9_]*\s*{";
        replacement = "namespace QuantumParser.Test\n{";
        rgx = new Regex(pattern);
        return rgx.Replace(result, replacement);
    }

    private static void Validate(string code)
    {
        // Check for safety
        // Reject all System.* except System.Math and System.Numerics
        // @"System\s*\.\s*([\w]*)";
        var regex = SystemImports();
        var match = regex.Match(code);

        while (match.Success)
        {
            if (!match.Groups[1].Value.StartsWith("Math") &&
                !match.Groups[1].Value.StartsWith("Numerics") &&
                !match.Groups[1].Value.StartsWith("Collections") &&
                !match.Groups[1].Value.StartsWith("Diagnostics") && // TODO for testing, probably to remove
                !match.Groups[1].Value.StartsWith("Linq"))
                throw new KeywordNotAllowedException(match.Groups[0].Value);

            match = match.NextMatch();
        }

        if (code.Contains("Microsoft")) throw new KeywordNotAllowedException("Microsoft");
    }

    private static string RemoveSingleLineComments(string code)
    {
        char[] sep = { '\n', '\r' };
        var lines = code.Split(sep, StringSplitOptions.RemoveEmptyEntries);
        for (var i = 0; i < lines.Length; i++)
        {
            var line = lines[i];
            var ind = line.IndexOf("//");
            while (ind > -1)
            {
                var quotes = new List<int>();
                var q = line.IndexOf("\"");
                while (q > -1)
                {
                    quotes.Add(q);
                    var newQ = line.IndexOf("\"", q + 1);
                    while (newQ > -1 && line[newQ - 1] == '\\') newQ = line.IndexOf("\"", newQ + 1);

                    q = newQ;
                }

                var quotesBefore = 0;
                foreach (var q1 in quotes)
                    if (ind > q1)
                        quotesBefore++;
                    else
                        break;

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

        var sb = new StringBuilder();
        foreach (var line in lines)
            if (!string.IsNullOrWhiteSpace(line))
                sb.AppendLine(line);

        return sb.ToString();
    }

    private static string RemoveMultiLineComments(string code)
    {
        var ind = code.IndexOf("/*");
        while (ind > -1)
        {
            var quotes = new List<int>();
            var q = code.IndexOf("\"");
            while (q > -1)
            {
                quotes.Add(q);
                var newQ = code.IndexOf("\"", q + 1);
                while (newQ > -1 && code[newQ - 1] == '\\') newQ = code.IndexOf("\"", newQ + 1);

                q = newQ;
            }

            var quotesBefore = 0;
            foreach (var q1 in quotes)
                if (ind > q1)
                    quotesBefore++;
                else
                    break;

            if (quotesBefore % 2 == 0)
            {
                var indEnd = code.IndexOf("*/", ind + 1);
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

    private static List<string> GetExtensionDefs(string code)
    {
        var toReturn = new List<string>();

        var bras = new List<int>();
        var braInd = code.IndexOf("(");
        while (braInd > -1)
        {
            bras.Add(braInd);
            braInd = code.IndexOf("(", braInd + 1);
        }

        var kets = new List<int>();
        var ketInd = code.IndexOf(")");
        while (ketInd > -1)
        {
            kets.Add(ketInd);
            ketInd = code.IndexOf(")", ketInd + 1);
        }

        bras.AddRange(kets);
        bras.Sort();

        // @"public\s+static\s+void\s+[a-zA-Z_][a-zA-Z0-9_]*" +
        //                       @"\s*\(\s*this\s+QuantumComputer";
        var regex = MainFinderWithRegexGroups();
        var match = regex.Match(code);
        while (match.Success)
        {
            var bra = match.Value.IndexOf("(") + match.Index;
            var ind = bras.IndexOf(bra);
            var opened = 1;
            while (opened > 0 && ind < bras.Count - 1)
            {
                ind++;
                if (code[bras[ind]] == '(')
                    opened++;
                else
                    opened--;
            }

            if (opened == 0)
            {
                var endKet = bras[ind];
                var def = code.Substring(match.Index, endKet - match.Index + 1);
                toReturn.Add(def);
            }

            match = match.NextMatch();
        }

        return toReturn;
    }

    private static string GetName(string methodDef)
    {
        // @"(public\s+static\s+void\s+)([a-zA-Z_][a-zA-Z0-9_]*)" +
        //                       @"(\s*\(\s*this\s+QuantumComputer)";

        var regex = MainFinder();
        var match = regex.Match(methodDef);
        return match.Success ? match.Groups[2].Value : null;
    }

    private static string GetParamDef(string methodDef)
    {
        var braInd = methodDef.IndexOf("(");
        return braInd > -1 ? methodDef.Substring(braInd) : null;
    }

    private static string[] GetParams(string paramDef)
    {
        var toReturn = paramDef.Split(',');
        for (var i = 0; i < toReturn.Length; i++)
        {
            var param = toReturn[i];
            var eqInd = param.IndexOf("=");
            if (eqInd > -1) toReturn[i] = param.Remove(eqInd);
        }

        return toReturn;
    }

    private static Tuple<string, string, string> GetKeywordTypeName(string param)
    {
        var words = param.Split(Array.Empty<char>(), StringSplitOptions.RemoveEmptyEntries);

        return words.Length switch
        {
            3 => new Tuple<string, string, string>(words[0], words[1], words[2]),
            2 => new Tuple<string, string, string>(null, words[0], words[1]),
            _ => new Tuple<string, string, string>(null, null, words.Last())
        };
    }

    private static string RemoveSqares(string paramDef)
    {
        char[] squares = { '[', ']' };
        return RemoveBrackets(paramDef, squares[0], squares[1]);
    }

    private static string RemoveAngles(string paramDef)
    {
        char[] brackets = { '<', '>' };
        return RemoveBrackets(paramDef, brackets[0], brackets[1]);
    }

    private static string RemoveRounds(string paramDef)
    {
        char[] brackets = { '(', ')' };
        if (paramDef.IndexOf('(') == 0) paramDef = paramDef.Substring(1, paramDef.Length - 2);

        return RemoveBrackets(paramDef, brackets[0], brackets[1]);
    }

    private static string RemoveBraces(string paramDef)
    {
        char[] brackets = { '{', '}' };
        return RemoveBrackets(paramDef, brackets[0], brackets[1]);
    }

    private static string RemoveBrackets(string text, char left, char right)
    {
        char[] squares = { left, right };
        var ind = text.IndexOf(left);
        while (ind > -1)
        {
            var next = text.IndexOfAny(squares, ind + 1);
            var prev = ind;
            var opened = 1;
            while (opened > 0 && next > -1)
            {
                if (text[next] == left)
                    opened++;
                else
                    opened--;

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

    internal static IEnumerable<MethodInfo> GetExtensionMethods(Assembly assembly, Type extendedType,
        string methodName = null)
    {
        var query = from type in assembly.GetTypes()
            where type != typeof(GatesExtension)
            where type.IsSealed && !type.IsGenericType && !type.IsNested
            from method in type.GetMethods(BindingFlags.Static
                                           | BindingFlags.Public | BindingFlags.NonPublic)
            where method.IsDefined(typeof(ExtensionAttribute), false)
            where method.GetParameters()[0].ParameterType == extendedType
            where string.IsNullOrWhiteSpace(methodName) || method.Name.Equals(methodName)
            select method;
        return query;
    }

    [GeneratedRegex("System\\s*\\.\\s*(\\w*)")]
    private static partial Regex SystemImports();

    [GeneratedRegex("public\\s+static\\s+void\\s+[a-zA-Z_][a-zA-Z0-9_]*\\s*\\(\\s*this\\s+QuantumComputer")]
    private static partial Regex MainFinderWithRegexGroups();

    [GeneratedRegex("(public\\s+static\\s+void\\s+)([a-zA-Z_][a-zA-Z0-9_]*)(\\s*\\(\\s*this\\s+QuantumComputer)")]
    private static partial Regex MainFinder();
}