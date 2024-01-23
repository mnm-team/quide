#region

using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using Avalonia.Controls;
using AvaloniaGUI.CodeHelpers;
using AvaloniaGUI.ViewModels.Controls;
using AvaloniaGUI.ViewModels.Dialog;
using AvaloniaGUI.ViewModels.MainModels.QuantumModel;
using AvaloniaGUI.ViewModels.MainModels.QuantumParser;
using AvaloniaGUI.Views;
using AvaloniaGUI.Views.Dialog;
using CommunityToolkit.Mvvm.Input;

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

public partial class MainWindowViewModel : ViewModelBase
{
    // TODO: bad style for Properties: should be initialized once and then assumed as non-null
    // because they are interconnected preferably in a single place
    public CircuitGridViewModel CircuitGrid
    {
        get
        {
            if (_circuitGridVM != null) return _circuitGridVM;

            _circuitGridVM = new CircuitGridViewModel(_model, _dialogManager);

            if (_propertiesVM != null) _propertiesVM.AddSelectionAndQubitsTracing(_circuitGridVM);

            if (_outputGridVM != null) _outputGridVM.AddQubitsTracing(_circuitGridVM);

            return _circuitGridVM;
        }
        private set
        {
            if (value == _circuitGridVM)
                return;

            _circuitGridVM = value;

            if (_propertiesVM != null) _propertiesVM.AddSelectionAndQubitsTracing(_circuitGridVM);

            if (_outputGridVM != null) _outputGridVM.AddQubitsTracing(_circuitGridVM);

            OnPropertyChanged(nameof(CircuitGrid));
        }
    }

    public OutputGridViewModel OutputGrid
    {
        get
        {
            if (_outputGridVM == null)
            {
                _outputGridVM = new OutputGridViewModel();
                _outputGridVM.LoadModel(_model, _outputModel);
                if (_circuitGridVM != null) _outputGridVM.AddQubitsTracing(_circuitGridVM);

                if (_propertiesVM != null) _propertiesVM.AddSelectionTracing(_outputGridVM);
            }

            return _outputGridVM;
        }
        private set
        {
            if (value == _outputGridVM)
                return;

            _outputGridVM = value;

            if (_circuitGridVM != null) _outputGridVM.AddQubitsTracing(_circuitGridVM);

            if (_propertiesVM != null) _propertiesVM.AddSelectionTracing(_outputGridVM);

            OnPropertyChanged(nameof(OutputGrid));
        }
    }

    public PropertiesViewModel PropertiesPane
    {
        get
        {
            if (_propertiesVM != null) return _propertiesVM;

            _propertiesVM = new PropertiesViewModel(CircuitGrid, OutputGrid);
            if (_outputGridVM != null) _propertiesVM.AddSelectionTracing(_outputGridVM);

            if (_circuitGridVM != null) _propertiesVM.AddSelectionAndQubitsTracing(_circuitGridVM);

            return _propertiesVM;
        }
        private set
        {
            if (value == _propertiesVM)
                return;

            _propertiesVM = value;

            if (_outputGridVM != null) _propertiesVM.AddSelectionTracing(_outputGridVM);

            if (_circuitGridVM != null) _propertiesVM.AddSelectionAndQubitsTracing(_circuitGridVM);

            OnPropertyChanged(nameof(PropertiesPane));
        }
    }

    public EditorViewModel EditorPane
    {
        get => _editorVM;
        private set
        {
            if (value == _editorVM)
                return;

            _editorVM = value;
            OnPropertyChanged(nameof(EditorPane));
        }
    }

    public ObservableCollection<string> CompositeTools
    {
        get
        {
            if (_toolsVM != null) return _toolsVM;

            var eval = CircuitEvaluator.GetInstance();
            var dict = eval.GetExtensionGates();
            _toolsVM = new ObservableCollection<string>(dict.Keys);
            return _toolsVM;
        }
        private set
        {
            _toolsVM = value;
            OnPropertyChanged(nameof(CompositeTools));
        }
    }

    public static ActionName SelectedAction { get; private set; }

    public string SelectedComposite
    {
        get => _selectedComposite;
        set
        {
            _selectedComposite = value;
            SelectedCompositeStatic = value;
            OnPropertyChanged(nameof(SelectedComposite));
        }
    }

    public static string SelectedCompositeStatic { get; private set; } = string.Empty;

    public string ConsoleOutput => _consoleWriter.Text;


    #region Constructor

    public void InitializeWindow(MainWindow window)
    {
        _window = window;
        _dialogManager = new DialogManager(_window);

        // they need dialogManager
        InitFromModel(ComputerModel.CreateModelForGUI());

        // inject dialogManager and notify handler
        EditorPane = new EditorViewModel(_dialogManager, NotifyEditorDependentCommands);
        _window.Closing += WindowClosing;

        _consoleWriter = new ConsoleWriter();
        _consoleWriter.TextChanged += _consoleWriter_TextChanged;
    }

    private async void WindowClosing(object sender, WindowClosingEventArgs args)
    {
        args.Cancel = true;

        var canClose = await EditorPane.EditorCanClose();
        if (!canClose) return;

        // Detach self and close with default handler
        _window.Closing -= WindowClosing;
        _window.Close();
    }

    #endregion // Constructor


    #region Fields

    private MainWindow _window;
    private DialogManager _dialogManager;

    private ComputerModel _model;
    private OutputViewModel _outputModel;

    private CircuitGridViewModel _circuitGridVM;

    private OutputGridViewModel _outputGridVM;
    private PropertiesViewModel _propertiesVM;

    private EditorViewModel _editorVM;

    private ObservableCollection<string> _toolsVM;
    private string _selectedComposite;

    private ConsoleWriter _consoleWriter;

    private DelegateCommand _selectAction;

    private DelegateCommand _group;

    private DelegateCommand _clearCircuit;

    private DelegateCommand _cutGates;
    private DelegateCommand _copyGates;
    private DelegateCommand _pasteGates;

    private DelegateCommand _restart;
    private DelegateCommand _prevStep;
    private DelegateCommand _nextStep;
    private DelegateCommand _run;

    private static DelegateCommand _calculatorCommand;

    private DelegateCommand _aboutCommand;

    #endregion // Fields


    #region Commands

    public ICommand CalculatorCommand
    {
        get
        {
            if (_calculatorCommand == null) _calculatorCommand = new DelegateCommand(null, _ => false);

            return _calculatorCommand;
        }
    }

    public ICommand AboutCommand
    {
        get
        {
            if (_aboutCommand == null) _aboutCommand = new DelegateCommand(ShowAbout, _ => true);

            return _aboutCommand;
        }
    }

    public ICommand SelectActionCommand
    {
        get
        {
            if (_selectAction == null) _selectAction = new DelegateCommand(SelectAction, x => true);

            return _selectAction;
        }
    }

    public ICommand GroupCommand
    {
        get
        {
            if (_group == null) _group = new DelegateCommand(MakeComposite, x => true);

            return _group;
        }
    }

    public ICommand ClearCircuitCommand
    {
        get
        {
            if (_clearCircuit == null) _clearCircuit = new DelegateCommand(ClearCircuit, x => true);

            return _clearCircuit;
        }
    }

    public ICommand CutGatesCommand
    {
        get
        {
            if (_cutGates == null) _cutGates = new DelegateCommand(CutGates, x => true);

            return _cutGates;
        }
    }

    public ICommand CopyGatesCommand
    {
        get
        {
            if (_copyGates == null) _copyGates = new DelegateCommand(CopyGates, x => true);

            return _copyGates;
        }
    }

    public ICommand PasteGatesCommand
    {
        get
        {
            if (_pasteGates == null) _pasteGates = new DelegateCommand(PasteGates, x => true);

            return _pasteGates;
        }
    }

    public ICommand DeleteGatesCommand
    {
        get
        {
            if (_pasteGates == null) _pasteGates = new DelegateCommand(DeleteGates, x => true);

            return _pasteGates;
        }
    }

    public ICommand RestartCommand
    {
        get
        {
            if (_restart == null) _restart = new DelegateCommand(Restart, x => true);

            return _restart;
        }
    }

    public ICommand PrevStepCommand
    {
        get
        {
            if (_prevStep == null) _prevStep = new DelegateCommand(PrevStep, x => true);

            return _prevStep;
        }
    }

    public ICommand NextStepCommand
    {
        get
        {
            if (_nextStep == null) _nextStep = new DelegateCommand(NextStep, x => true);

            return _nextStep;
        }
    }

    public ICommand RunCommand
    {
        get
        {
            if (_run == null) _run = new DelegateCommand(RunToEnd, x => true);

            return _run;
        }
    }

    #endregion // Commands


    #region Public Methods

    public static void CompositeSelected()
    {
        SelectedAction = ActionName.Composite;
    }

    private static void SelectAction(object parameter)
    {
        SelectedAction = (parameter as string) switch
        {
            "Empty" => ActionName.Empty,
            "Hadamard" => ActionName.Hadamard,
            "SigmaX" => ActionName.SigmaX,
            "SigmaY" => ActionName.SigmaY,
            "SigmaZ" => ActionName.SigmaZ,
            "SqrtX" => ActionName.SqrtX,
            "PhaseKick" => ActionName.PhaseKick,
            "PhaseScale" => ActionName.PhaseScale,
            "CPhaseShift" => ActionName.CPhaseShift,
            "InvCPhaseShift" => ActionName.InvCPhaseShift,
            "RotateX" => ActionName.RotateX,
            "RotateY" => ActionName.RotateY,
            "RotateZ" => ActionName.RotateZ,
            "Unitary" => ActionName.Unitary,
            "Control" => ActionName.Control,
            "Measure" => ActionName.Measure,
            "Ungroup" => ActionName.Ungroup,
            "Composite" => ActionName.Composite,
            "Pointer" => ActionName.Pointer,
            _ => ActionName.Selection
        };
    }

    private async void MakeComposite(object parameter)
    {
        try
        {
            // throws if no selection
            var toGroup = _model.GetSelectedGates();

            var dict = CircuitEvaluator.GetInstance().GetExtensionGates();
            var compositeVM = new CompositeInputViewModel(dict, _model);

            await _dialogManager.ShowDialogAsync(new CompositeInput(compositeVM), () =>
            {
                var name = compositeVM.Name;

                _model.MakeComposite(name, toGroup);

                if (_toolsVM.Contains(name)) return;

                var newTools = _toolsVM;
                newTools.Add(name);
                CompositeTools = newTools;
                SelectedComposite = name;
            });
        }
        catch (Exception ex)
        {
            SimpleDialogHandler.ShowSimpleMessage("Unable to create composite gate from selection:\n" + ex.Message,
                "Unable to create composite gate");
        }
    }

    private void ClearCircuit(object parameter)
    {
        InitFromModel(ComputerModel.CreateModelForGUI());
    }

    private void CutGates(object parameter)
    {
        _model.Cut();
    }

    private void CopyGates(object parameter)
    {
        _model.Copy();
    }

    private void PasteGates(object parameter)
    {
        _model.Paste();
    }

    private void DeleteGates(object parameter)
    {
        _model.Delete();
    }

    [RelayCommand(CanExecute = nameof(EditorDocumentSelected))]
    private void GenerateFromCode()
    {
        var parser = new Parser();

        try
        {
            var code = EditorPane.SelectedDocument?.Editor.Document.Text;
            if (string.IsNullOrWhiteSpace(code)) throw new NullReferenceException("Code is empty or not existing");

            var asmToBuild = parser.CompileForBuild(code);
            var eval = CircuitEvaluator.GetInstance();

            var methods = Parser.GetMethodsCodes(code);
            if (methods.Count > 0)
            {
                var asmToRun = parser.CompileForRun(code);
                eval.LoadLibMethods(asmToRun);
                eval.LoadParserMethods(asmToBuild);
                eval.LoadMethodsCodes(methods);
            }

            var dict = eval.GetExtensionGates();
            CompositeTools = new ObservableCollection<string>(dict.Keys);
            PropertiesPane.LoadParametrics(dict);

            var generatedModel = Parser.BuildModel(asmToBuild);
            InitFromModel(generatedModel);

            _window.CircuitTab.IsSelected = true;
        }
        catch (Exception e)
        {
            PrintException(e);
        }
    }

    [RelayCommand(CanExecute = nameof(EditorDocumentSelected))]
    private void RunInConsole()
    {
        _consoleWriter.Reset();
        var parser = new Parser();
        try
        {
            var code = EditorPane.SelectedDocument?.Editor.Document.Text;
            if (string.IsNullOrWhiteSpace(code)) throw new NullReferenceException("Code is empty or not existing");

            var asm = parser.CompileForRun(code);
            Parser.Execute(asm, _consoleWriter);

            _window.ConsoleTab.IsSelected = true;
        }
        catch (Exception e)
        {
            PrintException(e);
        }
    }

    private bool EditorDocumentSelected()
    {
        return EditorPane.SelectedDocument != null;
    }

    private void NotifyEditorDependentCommands()
    {
        RunInConsoleCommand.NotifyCanExecuteChanged();
        GenerateFromCodeCommand.NotifyCanExecuteChanged();
    }

    private void Restart(object parameter)
    {
        try
        {
            _model.CurrentStep = 0;
            var eval = CircuitEvaluator.GetInstance();

            _outputModel = eval.InitFromModel(_model);
            OutputGrid.LoadModel(_model, _outputModel);
        }
        catch (Exception e)
        {
            PrintException(e);
        }
    }

    private void PrevStep(object parameter)
    {
        try
        {
            var currentStep = _model.CurrentStep;
            if (currentStep == 0)
            {
                Restart(parameter);
            }
            else
            {
                if (!_model.CanStepBack(currentStep - 1)) return;

                var eval = CircuitEvaluator.GetInstance();
                var se = eval.GetStepEvaluator();
                var outputChanged = se.RunStep(_model.Steps[currentStep - 1].Gates, true);
                _model.CurrentStep = currentStep - 1;
                if (outputChanged) _outputModel.Update(eval.RootRegister);
            }
        }
        catch (Exception e)
        {
            PrintException(e);
        }
    }

    private void NextStep(object parameter)
    {
        try
        {
            var eval = CircuitEvaluator.GetInstance();

            var currentStep = _model.CurrentStep;
            if (currentStep == 0) eval.InitFromModel(_model);

            if (currentStep >= _model.Steps.Count) return;

            var se = eval.GetStepEvaluator();
            var outputChanged = se.RunStep(_model.Steps[currentStep].Gates);
            _model.CurrentStep = currentStep + 1;
            if (outputChanged) _outputModel.Update(eval.RootRegister);
        }
        catch (Exception e)
        {
            PrintException(e);
        }
    }

    private void RunToEnd(object parameter)
    {
        try
        {
            var eval = CircuitEvaluator.GetInstance();

            var currentStep = _model.CurrentStep;
            if (currentStep == 0) eval.InitFromModel(_model);

            var se = eval.GetStepEvaluator();
            var outputChanged = se.RunToEnd(_model.Steps, currentStep);
            _model.CurrentStep = _model.Steps.Count;
            if (outputChanged) _outputModel.Update(eval.RootRegister);
        }
        catch (Exception e)
        {
            PrintException(e);
        }
    }

    //TODO: Calculator
    // public CalcWindow ShowCalculator()
    // {
    //     if (_calcWindow == null)
    //     {
    //         _calcWindow = new CalcWindow();
    //         _calcWindow.Owner = _window;
    //         _calcWindow.Show();
    //     }
    //
    //     _calcWindow.Visibility = Visibility.Visible;
    //     return _calcWindow;
    // }

    private async void ShowAbout(object o)
    {
        await new AboutWindow().ShowDialog(_window);
    }

    #endregion // Public Methods


    #region Private Helpers

    private void InitFromModel(ComputerModel model)
    {
        if (_model != null)
        {
            var oldComposites = _model.CompositeGates;
            var newComposites = model.CompositeGates;

            foreach (var pair in oldComposites.Where(pair => !newComposites.ContainsKey(pair.Key)))
                newComposites[pair.Key] = pair.Value;
        }

        _model = model;

        CircuitGrid = new CircuitGridViewModel(_model, _dialogManager);

        var eval = CircuitEvaluator.GetInstance();
        _outputModel = eval.InitFromModel(_model);
        OutputGrid.LoadModel(_model, _outputModel);
    }

    private void _consoleWriter_TextChanged(object sender, EventArgs eventArgs)
    {
        OnPropertyChanged(nameof(ConsoleOutput));
    }

    private static void PrintException(Exception e)
    {
        var message = e.Message;
        if (e.InnerException != null) message = message + ":\n" + e.InnerException.Message;
        message = message + "\n" + e.StackTrace;

        SimpleDialogHandler.ShowSimpleMessage(message);
    }

    #endregion // Private Helpers
}