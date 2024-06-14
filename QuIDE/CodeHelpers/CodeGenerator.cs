
using QuIDE.QuantumModel;
using QuIDE.QuantumModel.Gates;
using QuIDE.QuantumParser;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;

namespace QuIDE.CodeHelpers;

public class CodeGenerator
{
    private bool _rootNeeded;

    private string GenerateCompositeExtensions(ComputerModel model, Dictionary<string, List<MethodCode>> methods)
    {
        if (methods is not { Count: > 0 } &&
            model.CompositeGates.Count <= 0) return null;

        var builder = new StringBuilder();

        builder.AppendLine("public static class CompositeExtension");
        builder.AppendLine("{");

        if (methods != null)
            foreach (var list in methods.Values)
                foreach (var code in list)
                    builder.Append("\t\t").AppendLine(code.Code);

        foreach (var pair in model.CompositeGates)
        {
            if (methods != null && methods.ContainsKey(pair.Key)) continue;

            const string regName = "regA";

            builder.Append("\tpublic static void ").Append(pair.Key);
            builder.Append("(this QuantumComputer comp, Register ");
            builder.Append(regName).AppendLine(")");
            builder.AppendLine("\t{");

            const string indent = "\t\t";

            var column = 0;
            foreach (var gate in pair.Value)
            {
                var gateString = GenerateGateCode(model, gate, column, indent, regName);
                if (gateString.Length > 0) builder.Append(indent).AppendLine(gateString);

                column++;
            }

            builder.AppendLine("\t}");
            builder.AppendLine();
        }

        builder.AppendLine("}");
        return builder.ToString();
    }

    public string GenerateCode()
    {
        var eval = CircuitEvaluator.GetInstance();
        var model = eval.Model;
        var methodsCodes = eval.GetMethodsCodes();

        var builder = new StringBuilder();
        builder.AppendLine("using Quantum;");
        builder.AppendLine("using Quantum.Operations;");
        builder.AppendLine("using System;");
        builder.AppendLine("using System.Numerics;");
        builder.AppendLine("using System.Collections.Generic;");
        builder.AppendLine();
        builder.AppendLine("namespace QuantumConsole;");
        builder.AppendLine();

        var extensions = GenerateCompositeExtensions(model, methodsCodes);
        if (!string.IsNullOrWhiteSpace(extensions)) builder.AppendLine(extensions);

        builder.AppendLine("public class QuantumTest");
        builder.AppendLine("{");
        builder.AppendLine("\tpublic static void Main()");
        builder.AppendLine("\t{");
        builder.AppendLine("\t\tQuantumComputer comp = QuantumComputer.GetInstance();");

        const string indent = "\t\t";

        foreach (var reg in model.Registers) builder.AppendLine(GenerateRegisterCode(reg, indent));

        var rootDefPlace = builder.Length;
        _rootNeeded = false;

        for (var i = 0; i < model.Steps.Count; i++)
        {
            var stepCode = GenerateStepCode(model, model.Steps[i], i, indent);
            builder.Append(stepCode);
        }

        if (_rootNeeded)
        {
            var b = new StringBuilder();
            b.Append(indent).Append("Register root = comp.GetRootRegister(");
            b.Append(model.Registers[0].Name);
            for (var i = 1; i < model.Registers.Count; i++)
            {
                var reg = model.Registers[i];
                b.Append(" ,").Append(reg.Name);
            }

            b.AppendLine(");");

            builder.Insert(rootDefPlace, b.ToString());
        }

        builder.AppendLine("\t}");
        builder.AppendLine("}");

        return builder.ToString();
    }

    private static string GenerateRegisterCode(RegisterModel reg, string indent)
    {
        IDictionary<ulong, Complex> initStates = reg.InitStates;
        if (initStates.Count > 1)
        {
            var initStatesName = reg.Name + "_initStates";
            var initStatesDef = GenerateStatesDefinition(initStatesName, initStates, indent);
            return
                $"{initStatesDef}{indent}Register {reg.Name} = comp.NewRegister({initStatesName}, {reg.Qubits.Count});";
        }

        if (initStates is { Count: > 0 })
        {
            var initState = initStates.Keys.First();
            return $"{indent}Register {reg.Name} = comp.NewRegister({initState}, {reg.Qubits.Count});";
        }

        return $"{indent}Register {reg.Name} = comp.NewRegister(0, {reg.Qubits.Count});";
    }

    private static string GenerateStatesDefinition(string varName, IDictionary<ulong, Complex> states, string indent)
    {
        var builder = new StringBuilder();
        builder.Append(indent).Append("var ").Append(varName);
        builder.Append(" = new Dictionary<ulong, Complex>() {\n");
        var i = 0;
        foreach (var pair in states)
        {
            builder.Append(indent).Append($"\t{{{pair.Key}, new Complex{pair.Value}}}");
            i++;
            if (i < states.Count) builder.Append(',');

            builder.AppendLine();
        }

        builder.Append(indent).AppendLine("};");

        return builder.ToString();
    }

    /// <summary>
    ///     Executed once for every step.
    /// </summary>
    /// <param name="model"></param>
    /// <param name="step">gates in step</param>
    /// <param name="column">step idx</param>
    /// <param name="indent"></param>
    /// <returns></returns>
    private string GenerateStepCode(ComputerModel model, StepModel step, int column, string indent)
    {
        var builder = new StringBuilder();
        Gate lastGate = null;
        foreach (var gate in step.Gates)
        {
            if (gate != lastGate)
            {
                var gateString = GenerateGateCode(model, gate, column, indent);
                if (gateString.Length > 0) builder.Append(indent).AppendLine(gateString);
            }

            lastGate = gate;
        }

        return builder.ToString();
    }
    private string GenerateGateCode(ComputerModel model, Gate gate, int column, string indent,
        string defaultRegName = null)
    {
        switch (gate.Name)
        {
            case GateName.Hadamard:
                return AppendControlTarget("Hadamard(", gate);
            case GateName.SigmaX:
                return AppendControlTarget("SigmaX(", gate);
            case GateName.SigmaY:
                return AppendControlTarget("SigmaY(", gate);
            case GateName.SigmaZ:
                return AppendControlTarget("SigmaZ(", gate);
            case GateName.SqrtX:
                return AppendControlTarget("SqrtX(", gate);
            case GateName.RotateX:
                var rx = gate as RotateXGate;
                return AppendControlTarget($"RotateX({rx.Gamma}, ", gate);
            case GateName.RotateY:
                var ry = gate as RotateYGate;
                return AppendControlTarget($"RotateY({ry.Gamma}, ", gate);
            case GateName.RotateZ:
                var rz = gate as RotateZGate;
                return AppendControlTarget($"RotateZ({rz.Gamma}, ", gate);
            case GateName.PhaseKick:
                var pk = gate as PhaseKickGate;
                if (pk.Controls.Length > 0)
                    return AppendMoreControls(
                        $"PhaseKick({pk.Gamma}, ",
                        "PhaseKick(<gamma_value>, ",
                        gate,
                        pk.Controls);
                return AppendControlTarget($"PhaseKick({pk.Gamma}, ", gate);
            case GateName.PhaseScale:
                var ps = gate as PhaseScaleGate;
                return AppendControlTarget($"PhaseScale({ps.Gamma}, ", gate);
            case GateName.CNot:
                return AppendControlTarget("CNot(", gate);
            case GateName.CPhaseShift:
                var cps = gate as CPhaseShiftGate;
                if (cps.Controls.Length > 0)
                    return AppendMoreControls(
                        $"CPhaseShift({cps.Dist}, ",
                        "CPhaseShift(<phase_distance_value>, ",
                        gate,
                        cps.Controls);
                return AppendControlTarget($"CPhaseShift({cps.Dist}, ", gate);
            case GateName.InvCPhaseShift:
                var icps = gate as InvCPhaseShiftGate;
                if (icps.Controls.Length > 0)
                    return AppendMoreControls(
                        $"InverseCPhaseShift({icps.Dist}, ",
                        "InverseCPhaseShift(<phase_distance_value>, ",
                        gate,
                        icps.Controls);
                return AppendControlTarget($"InverseCPhaseShift({icps.Dist}, ", gate);
            case GateName.Toffoli:
                var t = gate as ToffoliGate;
                return AppendMoreControls(
                    "Toffoli(",
                    "Toffoli(",
                    gate,
                    t.Controls);
            case GateName.Measure:
                var m = gate as MeasureGate;
                if (m.Begin == m.End)
                    return $"{GetRegName(m.Target.Register, defaultRegName)}.Measure({m.Target.Offset});";

                if (m.BeginRow.Register == m.EndRow.Register)
                {
                    var regName = GetRegName(m.BeginRow.Register, defaultRegName);
                    if (m.BeginRow.Register != null)
                    {
                        if (m.End - m.Begin + 1 == m.BeginRow.Register.Qubits.Count) return $"{regName}.Measure();";

                        var builder = new StringBuilder();
                        for (var i = m.BeginRow.Offset; i <= m.EndRow.Offset; i++)
                            builder.Append($"{regName}.Measure({i});\n");

                        return builder.ToString();
                    }

                    if (m.End - m.Begin + 1 == model.TotalWidth) return $"{regName}.Measure();";

                    {
                        var builder = new StringBuilder();
                        for (var i = m.BeginRow.Offset; i <= m.EndRow.Offset; i++)
                            builder.Append($"{regName}.Measure({i});\n");

                        return builder.ToString();
                    }
                }

            {
                var builder = new StringBuilder();
                var i = m.Begin;
                while (i <= m.End)
                {
                    var regRef = model.GetRefFromOffset(i);
                    if (i + regRef.Register.Qubits.Count < m.End + 2)
                    {
                        builder.Append($"{regRef.Register.Name}.Measure();\n");
                        i += regRef.Register.Qubits.Count;
                    }
                    else
                    {
                        builder.Append($"{regRef.Register.Name}.Measure({regRef.Offset});\n");
                        i++;
                    }
                }

                return builder.ToString();
            }
            case GateName.Unitary:
                var u = gate as UnitaryGate;
                var uMatrixName = "unitary_" + column + "_" + u.Target.OffsetToRoot;
                var matrixDef = GenerateMatrixDefinition(uMatrixName, u.Matrix, indent);
                var gateDef = AppendControlTarget($"Gate1({uMatrixName}, ", gate);
                return $"{matrixDef}\n{indent}{gateDef}";
            case GateName.Parametric:
                return GenerateParametricGateCode(gate as ParametricGate, defaultRegName);
            case GateName.Composite:
                return GenerateCompositeGateCode(gate as CompositeGate, defaultRegName);
            case GateName.Empty:
            default:
                return string.Empty;
        }

        string AppendControlTarget(string begin, Gate g)
        {
            if (g.Control.HasValue)
            {
                var namedC = "control: ";
                var namedT = "target: ";
                if (g.Name == GateName.CNot)
                    return g.Control.Value.Register == g.Target.Register
                        ? $"{GetRegName(g.Target.Register, defaultRegName)}.{begin}{namedT}{g.Target.Offset}, {namedC}{g.Control.Value.Offset});"
                        : $"comp.{begin}{namedT}{GetRegName(g.Target.Register, defaultRegName)}[{g.Target.Offset}], {namedC}{GetRegName(g.Control.Value.Register, defaultRegName)}[{g.Control.Value.Offset}]);";
                namedC = "";
                namedT = "";

                return g.Control.Value.Register == g.Target.Register
                    ? $"{GetRegName(g.Target.Register, defaultRegName)}.{begin}{namedT}{g.Target.Offset}, {namedC}{g.Control.Value.Offset});"
                    : $"comp.{begin}{namedT}{GetRegName(g.Target.Register, defaultRegName)}[{g.Target.Offset}], {namedC}{GetRegName(g.Control.Value.Register, defaultRegName)}[{g.Control.Value.Offset}]);";
            }

            return $"{GetRegName(g.Target.Register, defaultRegName)}.{begin}{g.Target.Offset});";
        }

        string AppendMoreControls(string begin, string beginComment, Gate g, RegisterRefModel[] controls)
        {
            if (controls.All(x => x.Register == g.Target.Register) &&
                (g.Control == null || g.Control.Value.Register == g.Target.Register))
            {
                var format = new StringBuilder(GetRegName(g.Target.Register, defaultRegName)).Append('.')
                    .Append(begin);
                format.Append(g.Target.Offset);

                if (g.Control.HasValue) format.Append(", ").Append(g.Control.Value.Offset);

                foreach (var control in controls)
                    format.Append(", ").Append(control.Offset);

                format.Append(");");
                format.Append("\t\t// ").Append(beginComment).Append("<target_bit>, ... <control_bits> ...)");
                return format.ToString();
            }
            else
            {
                var format = new StringBuilder("comp.").Append(begin);
                format.Append(GetRegName(g.Target.Register, defaultRegName))
                    .Append('[')
                    .Append(g.Target.Offset)
                    .Append(']');

                if (g.Control.HasValue)
                    format.Append(", ")
                        .Append(GetRegName(g.Control.Value.Register, defaultRegName))
                        .Append('[')
                        .Append(g.Control.Value.Offset)
                        .Append(']');

                foreach (var rrm in controls)
                    format.Append(", ")
                        .Append(GetRegName(rrm.Register, defaultRegName))
                        .Append('[')
                        .Append(rrm.Offset)
                        .Append(']');

                format.Append(");");
                format.Append("\t\t// ").Append(beginComment).Append("<target_bit>, ... <control_bits> ...)");
                return format.ToString();
            }
        }
    }

    private static string GenerateMatrixDefinition(string varName, Complex[,] matrix, string indent)
    {
        var builder = new StringBuilder();

        builder.Append("Complex[,] ").Append(varName);
        builder.Append(" = new Complex[2, 2];\n");

        builder.Append(indent).Append(varName).Append("[0, 0] = new Complex(")
            .Append(matrix[0, 0].Real).Append(", ")
            .Append(matrix[0, 0].Imaginary).Append(");\n");
        builder.Append(indent).Append(varName).Append("[0, 1] = new Complex(")
            .Append(matrix[0, 1].Real).Append(", ")
            .Append(matrix[0, 1].Imaginary).Append(");\n");
        builder.Append(indent).Append(varName).Append("[1, 0] = new Complex(")
            .Append(matrix[1, 0].Real).Append(", ")
            .Append(matrix[1, 0].Imaginary).Append(");\n");
        builder.Append(indent).Append(varName).Append("[1, 1] = new Complex(")
            .Append(matrix[1, 1].Real).Append(", ")
            .Append(matrix[1, 1].Imaginary).Append(");\n");

        return builder.ToString();
    }

    private string GenerateParametricGateCode(ParametricGate cg, string defaultRegName = null)
    {
        var sb = new StringBuilder("comp.").Append(cg.FunctionName).Append('(');

        var infos = cg.Method.GetParameters();

        for (var i = 1; i < cg.Parameters.Length; i++)
        {
            var par = cg.Parameters[i];
            if (infos[i].GetCustomAttributes(typeof(ParamArrayAttribute), false).Length > 0)
            {
                var pars = par as IEnumerable;
                foreach (var item in pars)
                {
                    sb.Append(", ");
                    sb.Append(AppendPar(item));
                }
            }
            else
            {
                if (i > 1) sb.Append(", ");

                sb.Append(AppendPar(par));
            }
        }

        sb.Append(");");

        return sb.ToString();

        string AppendPar(object x)
        {
            var builder = new StringBuilder();

            switch (x)
            {
                case RegisterPartModel rm:
                    {
                        builder.Append(GetRegName(rm.Register, defaultRegName));
                        if (rm.Register == null || rm.Width != rm.Register.Qubits.Count)
                        {
                            builder.Append('[').Append(rm.Offset).Append(", ");
                            builder.Append(rm.Width).Append(']');
                        }

                        break;
                    }
                case RegisterRefModel rrm:
                    builder.Append(GetRegName(rrm.Register, defaultRegName)).Append('[').Append(rrm.Offset).Append(']');
                    break;
                default:
                    builder.Append(x);
                    break;
            }

            return builder.ToString();
        }
    }

    private string GenerateCompositeGateCode(CompositeGate cg, string defaultRegName = null)
    {
        var sb = new StringBuilder("comp.").Append(cg.FunctionName).Append('(');
        var rm = cg.TargetRegister;
        sb.Append(GetRegName(rm.Register, defaultRegName));
        if (rm.Register == null || rm.Width != rm.Register.Qubits.Count)
        {
            sb.Append('[').Append(rm.Offset).Append(", ");
            sb.Append(rm.Width).Append(']');
        }

        sb.Append(");");

        return sb.ToString();
    }

    private string GetRegName(RegisterModel reg, string defaultRegName)
    {
        if (reg != null) return reg.Name;

        if (defaultRegName != null) return defaultRegName;

        _rootNeeded = true;
        return "root";
    }
}