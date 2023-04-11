using System.Reactive;
using ReactiveUI;

namespace AvaloniaGUI.ViewModels;

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
    
}