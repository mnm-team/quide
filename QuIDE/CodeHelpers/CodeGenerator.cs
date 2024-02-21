#region

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Reflection;
using System.Text;
using QuIDE.QuantumModel;
using QuIDE.QuantumModel.Gates;
using QuIDE.ViewModels.MainModels.QuantumParser;

#endregion

namespace QuIDE.CodeHelpers;

public class CodeGenerator
{
    private bool _rootNeeded = false;

    public string GenerateCompositeExtensions(ComputerModel model, Dictionary<string, List<MethodCode>> methods)
    {
        if (methods is { Count: > 0 } ||
            model.CompositeGates.Count > 0)
        {
            StringBuilder builder = new StringBuilder();

            builder.AppendLine("\tpublic static class CompositeExtension");
            builder.AppendLine("\t{");

            if (methods != null)
            {
                foreach (List<MethodCode> list in methods.Values)
                {
                    foreach (MethodCode code in list)
                    {
                        builder.Append("\t\t").AppendLine(code.Code);
                    }
                }
            }

            foreach (var pair in model.CompositeGates)
            {
                if (methods == null || !methods.ContainsKey(pair.Key))
                {
                    string regName = "regA";

                    builder.Append("\t\tpublic static void ").Append(pair.Key);
                    builder.Append("(this QuantumComputer comp, Register ");
                    builder.Append(regName).AppendLine(")");
                    builder.AppendLine("\t\t{");

                    string indent = "\t\t\t";

                    int column = 0;
                    foreach (Gate gate in pair.Value)
                    {
                        string gateString = GenerateGateCode(model, gate, column, indent, regName);
                        if (gateString.Length > 0)
                        {
                            builder.Append(indent).AppendLine(gateString);
                        }

                        column++;
                    }

                    builder.AppendLine("\t\t}");
                    builder.AppendLine();
                }
            }

            builder.AppendLine("\t}");
            return builder.ToString();
        }
        else
        {
            return null;
        }
    }

    public string GenerateCode()
    {
        CircuitEvaluator eval = CircuitEvaluator.GetInstance();
        ComputerModel model = eval.Model;
        Dictionary<string, List<MethodCode>> methodsCodes = eval.GetMethodsCodes();

        StringBuilder builder = new StringBuilder();
        builder.AppendLine("using Quantum;");
        builder.AppendLine("using Quantum.Operations;");
        builder.AppendLine("using System;");
        builder.AppendLine("using System.Numerics;");
        builder.AppendLine("using System.Collections.Generic;");
        builder.AppendLine();
        builder.AppendLine("namespace QuantumConsole");
        builder.AppendLine("{");

        string extensions = GenerateCompositeExtensions(model, methodsCodes);
        if (!string.IsNullOrWhiteSpace(extensions))
        {
            builder.AppendLine(extensions);
        }

        builder.AppendLine("\tpublic class QuantumTest");
        builder.AppendLine("\t{");
        builder.AppendLine("\t\tpublic static void Main()");
        builder.AppendLine("\t\t{");
        builder.AppendLine("\t\t\tQuantumComputer comp = QuantumComputer.GetInstance();");

        string indent = "\t\t\t";

        foreach (RegisterModel reg in model.Registers)
        {
            builder.AppendLine(GenerateRegisterCode(reg, indent));
        }

        int rootDefPlace = builder.Length;
        _rootNeeded = false;

        string stepCode;
        for (int i = 0; i < model.Steps.Count; i++)
        {
            stepCode = GenerateStepCode(model, model.Steps[i], i, indent);
            builder.Append(stepCode);
        }

        if (_rootNeeded)
        {
            StringBuilder b = new StringBuilder();
            b.Append(indent).Append("Register root = comp.GetRootRegister(");
            b.Append(model.Registers[0].Name);
            for (int i = 1; i < model.Registers.Count; i++)
            {
                RegisterModel reg = model.Registers[i];
                b.Append(" ,").Append(reg.Name);
            }

            b.AppendLine(");");

            builder.Insert(rootDefPlace, b.ToString());
        }

        builder.AppendLine("\t\t}");
        builder.AppendLine("\t}");
        builder.AppendLine("}");

        return builder.ToString();
    }

    private string GenerateRegisterCode(RegisterModel reg, string indent)
    {
        IDictionary<ulong, Complex> initStates = reg.InitStates;
        if (initStates.Count > 1)
        {
            string initStatesName = reg.Name + "_initStates";
            string initStatesDef = GenerateStatesDefinition(initStatesName, initStates, indent);
            return
                $"{initStatesDef}{indent}Register {reg.Name} = comp.NewRegister({initStatesName}, {reg.Qubits.Count});";
        }
        else if (initStates != null && initStates.Count > 0)
        {
            ulong initState = initStates.Keys.First();
            return $"{indent}Register {reg.Name} = comp.NewRegister({initState}, {reg.Qubits.Count});";
        }
        else
        {
            return $"{indent}Register {reg.Name} = comp.NewRegister(0, {reg.Qubits.Count});";
        }
    }

    private string GenerateStatesDefinition(string varName, IDictionary<ulong, Complex> states, string indent)
    {
        StringBuilder builder = new StringBuilder();
        builder.Append(indent).Append("var ").Append(varName);
        builder.Append(" = new Dictionary<ulong, Complex>() {\n");
        int i = 0;
        foreach (KeyValuePair<ulong, Complex> pair in states)
        {
            builder.Append(indent).Append($"\t{{{pair.Key}, new Complex{pair.Value}}}");
            i++;
            if (i < states.Count)
            {
                builder.Append(',');
            }

            builder.AppendLine();
        }

        builder.Append(indent).AppendLine("};");

        return builder.ToString();
    }

    private string GenerateStepCode(ComputerModel model, StepModel step, int column, string indent)
    {
        StringBuilder builder = new StringBuilder();
        Gate lastGate = null;
        foreach (Gate gate in step.Gates)
        {
            if (gate != lastGate)
            {
                var gateString = GenerateGateCode(model, gate, column, indent);
                if (gateString.Length > 0)
                {
                    builder.Append(indent).AppendLine(gateString);
                }
            }

            lastGate = gate;
        }

        return builder.ToString();
    }

    private string GenerateGateCode(ComputerModel model, Gate gate, int column, string indent,
        string defaultRegName = null)
    {
        string AppendControlTarget(string begin, Gate g)
        {
            if (g.Control.HasValue)
            {
                string namedC = "control: ";
                string namedT = "target: ";
                if (g.Name != GateName.CNot)
                {
                    namedC = "";
                    namedT = "";
                }

                if (g.Control.Value.Register == g.Target.Register)
                {
                    return
                        $"{GetRegName(g.Target.Register, defaultRegName)}.{begin}{namedT}{g.Target.Offset}, {namedC}{g.Control.Value.Offset});";
                }
                else
                {
                    return
                        $"comp.{begin}{namedT}{GetRegName(g.Target.Register, defaultRegName)}[{g.Target.Offset}], {namedC}{GetRegName(g.Control.Value.Register, defaultRegName)}[{g.Control.Value.Offset}]);";
                }
            }
            else
            {
                return $"{GetRegName(g.Target.Register, defaultRegName)}.{begin}{g.Target.Offset});";
            }
        }

        string AppendMoreControls(string begin, string beginComment, Gate g, RegisterRefModel[] controls)
        {
            if (controls.All(x => x.Register == g.Target.Register) &&
                (g.Control == null || g.Control.Value.Register == g.Target.Register))
            {
                StringBuilder format = new StringBuilder(GetRegName(g.Target.Register, defaultRegName)).Append('.')
                    .Append(begin);
                format.Append(g.Target.Offset);

                if (g.Control.HasValue)
                {
                    format.Append(", ").Append(g.Control.Value.Offset);
                }

                for (int i = 0; i < controls.Length; i++)
                {
                    format.Append(", ").Append(controls[i].Offset);
                }

                format.Append(");");
                format.Append("\t\t// ").Append(beginComment).Append("<target_bit>, ... <control_bits> ...)");
                return format.ToString();
            }
            else
            {
                StringBuilder format = new StringBuilder("comp.").Append(begin);
                format.Append(GetRegName(g.Target.Register, defaultRegName))
                    .Append('[')
                    .Append(g.Target.Offset)
                    .Append(']');

                if (g.Control.HasValue)
                {
                    format.Append(", ")
                        .Append(GetRegName(g.Control.Value.Register, defaultRegName))
                        .Append('[')
                        .Append(g.Control.Value.Offset)
                        .Append(']');
                }

                foreach (var rrm in controls)
                {
                    format.Append(", ")
                        .Append(GetRegName(rrm.Register, defaultRegName))
                        .Append('[')
                        .Append(rrm.Offset)
                        .Append(']');
                }

                format.Append(");");
                format.Append("\t\t// ").Append(beginComment).Append("<target_bit>, ... <control_bits> ...)");
                return format.ToString();
            }
        }

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
                RotateXGate rx = gate as RotateXGate;
                return AppendControlTarget($"RotateX({rx.Gamma}, ", gate);
            case GateName.RotateY:
                RotateYGate ry = gate as RotateYGate;
                return AppendControlTarget($"RotateY({ry.Gamma}, ", gate);
            case GateName.RotateZ:
                RotateZGate rz = gate as RotateZGate;
                return AppendControlTarget($"RotateZ({rz.Gamma}, ", gate);
            case GateName.PhaseKick:
                PhaseKickGate pk = gate as PhaseKickGate;
                if (pk.Controls.Length > 0)
                {
                    return AppendMoreControls(
                        $"PhaseKick({pk.Gamma}, ",
                        "PhaseKick(<gamma_value>, ",
                        gate,
                        pk.Controls);
                }
                else
                {
                    return AppendControlTarget($"PhaseKick({pk.Gamma}, ", gate);
                }
            case GateName.PhaseScale:
                PhaseScaleGate ps = gate as PhaseScaleGate;
                return AppendControlTarget($"PhaseScale({ps.Gamma}, ", gate);
            case GateName.CNot:
                return AppendControlTarget("CNot(", gate);
            case GateName.CPhaseShift:
                CPhaseShiftGate cps = gate as CPhaseShiftGate;
                if (cps.Controls.Length > 0)
                {
                    return AppendMoreControls(
                        $"CPhaseShift({cps.Dist}, ",
                        "CPhaseShift(<phase_distance_value>, ",
                        gate,
                        cps.Controls);
                }
                else
                {
                    return AppendControlTarget($"CPhaseShift({cps.Dist}, ", gate);
                }
            case GateName.InvCPhaseShift:
                InvCPhaseShiftGate icps = gate as InvCPhaseShiftGate;
                if (icps.Controls.Length > 0)
                {
                    return AppendMoreControls(
                        $"InverseCPhaseShift({icps.Dist}, ",
                        "InverseCPhaseShift(<phase_distance_value>, ",
                        gate,
                        icps.Controls);
                }
                else
                {
                    return AppendControlTarget($"InverseCPhaseShift({icps.Dist}, ", gate);
                }
            case GateName.Toffoli:
                ToffoliGate t = gate as ToffoliGate;
                return AppendMoreControls(
                    "Toffoli(",
                    "Toffoli(",
                    gate,
                    t.Controls);
            case GateName.Measure:
                MeasureGate m = gate as MeasureGate;
                if (m.Begin == m.End)
                {
                    return $"{GetRegName(m.Target.Register, defaultRegName)}.Measure({m.Target.Offset});";
                }
                else if (m.BeginRow.Register == m.EndRow.Register)
                {
                    string regName = GetRegName(m.BeginRow.Register, defaultRegName);
                    if (m.BeginRow.Register != null)
                    {
                        if (m.End - m.Begin + 1 == m.BeginRow.Register.Qubits.Count)
                        {
                            return $"{regName}.Measure();";
                        }
                        else
                        {
                            StringBuilder builder = new StringBuilder();
                            for (int i = m.BeginRow.Offset; i <= m.EndRow.Offset; i++)
                            {
                                builder.Append($"{regName}.Measure({i});\n");
                            }

                            return builder.ToString();
                        }
                    }
                    else
                    {
                        if (m.End - m.Begin + 1 == model.TotalWidth)
                        {
                            return $"{regName}.Measure();";
                        }
                        else
                        {
                            StringBuilder builder = new StringBuilder();
                            for (int i = m.BeginRow.Offset; i <= m.EndRow.Offset; i++)
                            {
                                builder.Append($"{regName}.Measure({i});\n");
                            }

                            return builder.ToString();
                        }
                    }
                }
                else
                {
                    StringBuilder builder = new StringBuilder();
                    int i = m.Begin;
                    while (i <= m.End)
                    {
                        RegisterRefModel regRef = model.GetRefFromOffset(i);
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
                UnitaryGate u = gate as UnitaryGate;
                string uMatrixName = "unitary_" + column + "_" + u.Target.OffsetToRoot;
                string matrixDef = GenerateMatrixDefinition(uMatrixName, u.Matrix, indent);
                string gateDef = AppendControlTarget($"Gate1({uMatrixName}, ", gate);
                return $"{matrixDef}\n{indent}{gateDef}";
            case GateName.Parametric:
                return GenerateParametricGateCode(gate as ParametricGate, defaultRegName);
            case GateName.Composite:
                return GenerateCompositeGateCode(gate as CompositeGate, defaultRegName);
            case GateName.Empty:
            default:
                return String.Empty;
        }
    }

    private string GenerateMatrixDefinition(string varName, Complex[,] matrix, string indent)
    {
        StringBuilder builder = new StringBuilder();

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
        string AppendPar(object x)
        {
            StringBuilder builder = new StringBuilder();

            if (x is RegisterPartModel rm)
            {
                builder.Append(GetRegName(rm.Register, defaultRegName));
                if (rm.Register == null || rm.Width != rm.Register.Qubits.Count)
                {
                    builder.Append('[').Append(rm.Offset).Append(", ");
                    builder.Append(rm.Width).Append(']');
                }
            }
            else if (x is RegisterRefModel rrm)
            {
                builder.Append(GetRegName(rrm.Register, defaultRegName)).Append('[').Append(rrm.Offset).Append(']');
            }
            else
            {
                builder.Append(x);
            }

            return builder.ToString();
        }

        StringBuilder sb = new StringBuilder("comp.").Append(cg.FunctionName).Append('(');

        ParameterInfo[] infos = cg.Method.GetParameters();

        for (int i = 1; i < cg.Parameters.Length; i++)
        {
            object par = cg.Parameters[i];
            if (infos[i].GetCustomAttributes(typeof(ParamArrayAttribute), false).Length > 0)
            {
                IEnumerable pars = par as IEnumerable;
                foreach (var item in pars)
                {
                    sb.Append(", ");
                    sb.Append(AppendPar(item));
                }
            }
            else
            {
                if (i > 1)
                {
                    sb.Append(", ");
                }

                sb.Append(AppendPar(par));
            }
        }

        sb.Append(");");

        return sb.ToString();
    }

    private string GenerateCompositeGateCode(CompositeGate cg, string defaultRegName = null)
    {
        StringBuilder sb = new StringBuilder("comp.").Append(cg.FunctionName).Append('(');
        RegisterPartModel rm = cg.TargetRegister;
        sb.Append(GetRegName(rm.Register, defaultRegName));
        if (rm.Register == null || (rm.Width != rm.Register.Qubits.Count))
        {
            sb.Append('[').Append(rm.Offset).Append(", ");
            sb.Append(rm.Width).Append(']');
        }

        sb.Append(");");

        return sb.ToString();
    }

    private string GetRegName(RegisterModel reg, string defaultRegName)
    {
        if (reg != null)
        {
            return reg.Name;
        }

        if (defaultRegName != null)
        {
            return defaultRegName;
        }

        _rootNeeded = true;
        return "root";
    }
}