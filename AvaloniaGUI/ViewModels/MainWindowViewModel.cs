#region

using System.Collections.Generic;
using System.Reactive;
using ReactiveUI;

#endregion

namespace AvaloniaGUI.ViewModels;

public enum ActionName
{
    Selection,
    Pointer,
    Empty,
    Hadamard,
    SigmaX,
    SigmaY,
    SigmaZ,
    SqrtX,
    PhaseKick,
    PhaseScale,
    CPhaseShift,
    InvCPhaseShift,
    RotateX,
    RotateY,
    RotateZ,
    Unitary,
    Control,
    Measure,
    Ungroup,
    Composite
}

public class MainWindowViewModel : ViewModelBase
{
    // for key bindings
    public ReactiveCommand<Unit, Unit> New { get; }
    public ReactiveCommand<Unit, Unit> Open { get; }
    public ReactiveCommand<Unit, Unit> Save { get; }
    public ReactiveCommand<Unit, Unit> SaveAs { get; }
    public ReactiveCommand<Unit, Unit> NavFirst { get; }
    public ReactiveCommand<Unit, Unit> NavPrev { get; }
    public ReactiveCommand<Unit, Unit> NavNext { get; }
    public ReactiveCommand<Unit, Unit> NavLast { get; }
    public ReactiveCommand<Unit, Unit> Calc { get; }
    public ReactiveCommand<Unit, Unit> About { get; }


    public ReactiveCommand<Unit, Unit> GenerateCode { get; }
    public List<string> CompositeTools { get; }
}