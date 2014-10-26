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

using ICSharpCode.AvalonEdit;
using ICSharpCode.AvalonEdit.Highlighting;
using Microsoft.Win32;
using QuIDE.Helpers;
using QuantumModel;
using QuantumParser;
using System;
using System.Linq;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using Xceed.Wpf.AvalonDock.Layout;
using System.Reflection;
using QuIDE.Controls;
using Technewlogic.WpfDialogManagement.Contracts;
using Technewlogic.WpfDialogManagement;
using System.Threading;

namespace QuIDE.ViewModels
{
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

    public class HomeVM : ViewModelBase
    {
        #region Events

        public event RoutedEventHandler CodeChanged;
        private void OnCodeChanged()
        {
            if (CodeChanged != null)
            {
                CodeChanged(this, new RoutedEventArgs());
            }
        }

        #endregion // Events


        #region Fields

        private MainWindow _window;

        private ComputerModel _model;
        private Output _outputModel;

        private CodeGenerator _codeGenerator;

        private CircuitGridVM _circuitGridVM;
        private OutputGridVM _outputGridVM;
        private PropertiesVM _propertiesVM;

        private List<string> _toolsVM;

        private ConsoleWriter _consoleWriter;

        private static string _exampleCode = "using Quantum;\n" +
            "using Quantum.Operations;\n" +
            "using System;\n" +
            "using System.Numerics;\n" +
            "using System.Collections.Generic;\n\n" +
                          "namespace QuantumConsole\n" +
                          "{\n" +
                          "\tpublic class QuantumTest\n" +
                          "\t{\n" +
                          "\t\tpublic static void Main()\n" +
                          "\t\t{\n" +
                          "\t\t\tQuantumComputer comp = QuantumComputer.GetInstance();\n\n" +
                          "\t\t\t// create new register with initial value = 0, and width = 3 \n" +
                          "\t\t\tRegister x = comp.NewRegister(0, 3);\n\n" +
                          "\t\t\t// example: apply Hadamard Gate on qubit number 0 (least significant) \n" +
                          "\t\t\t//x.Hadamard(0);\n" +
                          "\t\t}\n" +
                          "\t}\n" +
                          "}\n";

        private Dictionary<int, DocumentInfo> _documents = new Dictionary<int, DocumentInfo>();
        private Dictionary<string, int> _openFiles = new Dictionary<string, int>();

        private string _newFilename = "Class";
        private string _newFileNameExt = ".cs";
        private int _newFilenameCount = 1;

        private DelegateCommand _selectAction;

        private DelegateCommand _group;

        private DelegateCommand _clearCircuit;

        private DelegateCommand _cutGates;
        private DelegateCommand _copyGates;
        private DelegateCommand _pasteGates;
        
        private DelegateCommand _generateCode;

        private DelegateCommand _generateFromCode;
        private DelegateCommand _runInConsole;

        private DelegateCommand _restart;
        private DelegateCommand _prevStep;
        private DelegateCommand _nextStep;
        private DelegateCommand _run;

        private DelegateCommand _new;
        private DelegateCommand _open;
        private DelegateCommand _save;
        private DelegateCommand _saveAs;

        private static RoutedCommand _calculatorCommand = new RoutedCommand();
        private static RoutedCommand _aboutCommand = new RoutedCommand();
        private CalcWindow _calcWindow;

        #endregion // Fields


        #region Constructor

        public HomeVM()
        {
            InitFromModel(ComputerModel.CreateModelForGUI());
            
            _codeGenerator = new CodeGenerator();
            _consoleWriter = new ConsoleWriter();
            _consoleWriter.TextChanged += _consoleWriter_TextChanged;
            _window = Application.Current.MainWindow as MainWindow;
        }

        #endregion // Constructor


        #region Presentation Properties

        public CircuitGridVM CircuitGrid
        {
            get
            {
                if (_circuitGridVM == null)
                {
                    _circuitGridVM = new CircuitGridVM(_model);
                }
                return _circuitGridVM;
            }
            private set
            {
                if (value == _circuitGridVM)
                    return;

                _circuitGridVM = value;
                if (_propertiesVM != null)
                {
                    _propertiesVM.AddSelectionTracing(_circuitGridVM);
                }
                base.OnPropertyChanged("CircuitGrid");
            }
        }

        public OutputGridVM OutputGrid
        {
            get
            {
                if (_outputGridVM == null)
                {
                    _outputGridVM = new OutputGridVM();
                    _outputGridVM.LoadModel(_model, _outputModel);
                }
                return _outputGridVM;
            }
            private set
            {
                if (value == _outputGridVM)
                    return;

                _outputGridVM = value;
                if (_propertiesVM != null)
                {
                    _propertiesVM.AddSelectionTracing(_outputGridVM);
                }
                base.OnPropertyChanged("OutputGrid");
            }
        }

        public PropertiesVM PropertiesPane
        {
            get
            {
                if (_propertiesVM == null)
                {
                    _propertiesVM = new PropertiesVM(CircuitGrid, OutputGrid);
                }
                return _propertiesVM;
            }
            private set
            {
                if (value == _propertiesVM)
                    return;

                _propertiesVM = value;

                base.OnPropertyChanged("PropertiesPane");
            }
        }

        public List<string> CompositeTools
        {
            get
            {
                if (_toolsVM == null)
                {
                    CircuitEvaluator eval = CircuitEvaluator.GetInstance();
                    Dictionary<string, List<MethodInfo>> dict = eval.GetExtensionGates();
                    _toolsVM = dict.Keys.ToList();
                }
                return _toolsVM;
            }
            private set
            {
                _toolsVM = value;
                OnPropertyChanged("CompositeTools");
            }
        }

        public object SelectedObject
        {
            get
            {
                return _window.outputGrid.statesList.SelectedValue;
            }
        }

        public static ActionName SelectedAction
        {
            get;
            private set;
        }

        public static string SelectedComposite
        {
            get;
            set;
        }

        public static ICommand CalculatorCommand
        {
            get { return _calculatorCommand; }
        }

        public static ICommand AboutCommand
        {
            get { return _aboutCommand; }
        }

        public ICommand SelectActionCommand
        {
            get
            {
                if (_selectAction == null)
                {
                    _selectAction = new DelegateCommand(SelectAction, x => true);
                }
                return _selectAction;
            }
        }

        public ICommand GroupCommand
        {
            get
            {
                if (_group == null)
                {
                    _group = new DelegateCommand(MakeComposite, x => true);
                }
                return _group;
            }
        }

        public LayoutDocument ActiveTab
        {
            get
            {
                return _window.DocumentPane.SelectedContent as LayoutDocument;
            }
        }

        public string Code
        {
            get
            {
                if (ActiveTab != null)
                {
                    TextEditor editor = ActiveTab.Content as TextEditor;
                    return editor.Text;
                }
                return null;
            }
        }

        public string ConsoleOutput
        {
            get
            {
                return _consoleWriter.Text;
            }
        }

        public ICommand ClearCircuitCommand
        {
            get
            {
                if (_clearCircuit == null)
                {
                    _clearCircuit = new DelegateCommand(ClearCircuit, x => true);
                }
                return _clearCircuit;
            }
        }

        public ICommand CutGatesCommand
        {
            get
            {
                if (_cutGates == null)
                {
                    _cutGates = new DelegateCommand(CutGates, x => true);
                }
                return _cutGates;
            }
        }

        public ICommand CopyGatesCommand
        {
            get
            {
                if (_copyGates == null)
                {
                    _copyGates = new DelegateCommand(CopyGates, x => true);
                }
                return _copyGates;
            }
        }

        public ICommand PasteGatesCommand
        {
            get
            {
                if (_pasteGates == null)
                {
                    _pasteGates = new DelegateCommand(PasteGates, x => true);
                }
                return _pasteGates;
            }
        }

        public ICommand GenerateCodeCommand
        {
            get
            {
                if (_generateCode == null)
                {
                    _generateCode = new DelegateCommand(GenerateCode, x => true);
                }
                return _generateCode;
            }
        }

        public ICommand GenerateFromCodeCommand
        {
            get
            {
                if (_generateFromCode == null)
                {
                    _generateFromCode = new DelegateCommand(GenerateFromCode, x => true);
                }
                return _generateFromCode;
            }
        }

        public ICommand RunInConsoleCommand
        {
            get
            {
                if (_runInConsole == null)
                {
                    _runInConsole = new DelegateCommand(RunInConsole, x => true);
                }
                return _runInConsole;
            }
        }

        public ICommand RestartCommand
        {
            get
            {
                if (_restart == null)
                {
                    _restart = new DelegateCommand(Restart, x => true);
                }
                return _restart;
            }
        }

        public ICommand PrevStepCommand
        {
            get
            {
                if (_prevStep == null)
                {
                    _prevStep = new DelegateCommand(PrevStep, x => true);
                }
                return _prevStep;
            }
        }

        public ICommand NextStepCommand
        {
            get
            {
                if (_nextStep == null)
                {
                    _nextStep = new DelegateCommand(NextStep, x => true);
                }
                return _nextStep;
            }
        }

        public ICommand RunCommand
        {
            get
            {
                if (_run == null)
                {
                    _run = new DelegateCommand(RunToEnd, x => true);
                }
                return _run;
            }
        }

        public ICommand NewCommand
        {
            get
            {
                if (_new == null)
                {
                    _new = new DelegateCommand(New, x => true);
                }
                return _new;
            }
        }

        public ICommand OpenCommand
        {
            get
            {
                if (_open == null)
                {
                    _open = new DelegateCommand(Open, x => true);
                }
                return _open;
            }
        }

        public ICommand SaveCommand
        {
            get
            {
                if (_save == null)
                {
                    _save = new DelegateCommand(Save, x => true);
                }
                return _save;
            }
        }

        public ICommand SaveAsCommand
        {
            get
            {
                if (_saveAs == null)
                {
                    _saveAs = new DelegateCommand(SaveAs, x => true);
                }
                return _saveAs;
            }
        }

        #endregion // Presentation Properties


        #region Public Methods

        public void SelectAction(object parameter)
        {
            switch (parameter as string)
            {
                case "Empty":
                    SelectedAction = ActionName.Empty;
                    break;
                case "Hadamard":
                    SelectedAction = ActionName.Hadamard;
                    break;
                case "SigmaX":
                    SelectedAction = ActionName.SigmaX;
                    break;
                case "SigmaY":
                    SelectedAction = ActionName.SigmaY;
                    break;
                case "SigmaZ":
                    SelectedAction = ActionName.SigmaZ;
                    break;
                case "SqrtX":
                    SelectedAction = ActionName.SqrtX;
                    break;
                case "PhaseKick":
                    SelectedAction = ActionName.PhaseKick;
                    break;
                case "PhaseScale":
                    SelectedAction = ActionName.PhaseScale;
                    break;
                case "CPhaseShift":
                    SelectedAction = ActionName.CPhaseShift;
                    break;
                case "InvCPhaseShift":
                    SelectedAction = ActionName.InvCPhaseShift;
                    break;
                case "RotateX":
                    SelectedAction = ActionName.RotateX;
                    break;
                case "RotateY":
                    SelectedAction = ActionName.RotateY;
                    break;
                case "RotateZ":
                    SelectedAction = ActionName.RotateZ;
                    break;
                case "Unitary":
                    SelectedAction = ActionName.Unitary;
                    break;
                case "Control":
                    SelectedAction = ActionName.Control;
                    break;
                case "Measure":
                    SelectedAction = ActionName.Measure;
                    break;
                case "Ungroup":
                    SelectedAction = ActionName.Ungroup;
                    break;
                case "Composite":
                    SelectedAction = ActionName.Composite;
                    break;
                case "Pointer":
                    SelectedAction = ActionName.Pointer;
                    break;
                case "Selection":
                default:
                    SelectedAction = ActionName.Selection;
                    break;
            }
        }

        public void MakeComposite(object parameter)
        {
            try
            {
                List<Gate> toGroup = _model.GetSelectedGates();
                CompositeInput ci = new CompositeInput();
                ICustomContentDialog dialog1 = _window.DialogManager
                    .CreateCustomContentDialog(ci, DialogMode.OkCancel);
                dialog1.Ok = () =>
                {
                    try
                    {
                        string name = ci.nameBox.Text;

                        string pattern = @"^\s*[a-zA-Z_][a-zA-Z0-9_]*\s*$";
                        Regex regex = new Regex(pattern);
                        Match match = regex.Match(name);
                        if (!match.Success)
                        {
                            throw new Exception("Entered name is invalid.");
                        }

                        CircuitEvaluator eval = CircuitEvaluator.GetInstance();
                        Dictionary<string, List<MethodInfo>> dict = eval.GetExtensionGates();
                        if (dict.ContainsKey(name))
                        {
                            MessageBox.Show(
                                        "Another composite gate with the same name already exist. Please choose other name.",
                                        "New composite gate",
                                        MessageBoxButton.OK,
                                        MessageBoxImage.Error);
                        }
                        else
                        {
                            List<Gate> alreadyDefined = _model.FindComposite(name);
                            if (alreadyDefined != null)
                            {
                                MessageBox.Show(
                                        "Another composite gate with the same name already exist. Please choose other name.",
                                        "New composite gate",
                                        MessageBoxButton.OK,
                                        MessageBoxImage.Error);
                            }
                            else
                            {
                                _model.MakeComposite(name, toGroup);

                                if (!_toolsVM.Contains(name))
                                {
                                    List<string> newTools = _toolsVM;
                                    newTools.Add(name);
                                    CompositeTools = newTools;
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Unable to create composite gate from selection:\n" + ex.Message,
                            "Unable to create composite gate",
                            MessageBoxButton.OK,
                            MessageBoxImage.Error);
                    }
                };
                dialog1.Show();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Unable to create composite gate from selection:\n" + ex.Message,
                    "Unable to create composite gate",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        public void ClearCircuit(object parameter)
        {
            InitFromModel(ComputerModel.CreateModelForGUI());
        }

        public void CutGates(object parameter)
        {
            _model.Cut();
        }

        public void CopyGates(object parameter)
        {
            _model.Copy();
        }

        public void PasteGates(object parameter)
        {
            _model.Paste();
        }

        public void DeleteGates(object parameter)
        {
            _model.Delete();
        }

        public void GenerateCode(object parameter)
        {
            string code = _codeGenerator.GenerateCode();
            
            string filename = GetNewFilename();
            DocumentInfo info = CreateTab(null, filename);
            info.Editor.Text = code;
        }

        public void GenerateFromCode(object parameter)
        {
            _window.CircuitTab.IsSelected = true;

            string code = Code;
            if (string.IsNullOrWhiteSpace(code))
            {
                return;
            }

            Parser parser = new Parser();

            try
            {
                Assembly asmToBuild = parser.CompileForBuild(code);
                CircuitEvaluator eval = CircuitEvaluator.GetInstance();

                Dictionary<string, List<MethodCode>> methods = parser.GetMethodsCodes(code);
                if (methods.Count > 0)
                {
                    Assembly asmToRun = parser.CompileForRun(code);
                    eval.LoadLibMethods(asmToRun);
                    eval.LoadParserMethods(asmToBuild);
                    eval.LoadMethodsCodes(methods);
                }
                Dictionary<string, List<MethodInfo>> dict = eval.GetExtensionGates();
                CompositeTools = dict.Keys.ToList();
                PropertiesPane.LoadParametrics(dict);

                ComputerModel generatedModel = parser.BuildModel(asmToBuild);
                InitFromModel(generatedModel);
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
            }
        }

        //private void ThreadRunInConsole(object data)
        //{
        //    try
        //    {
        //        Tuple<Parser, Assembly> myData = data as Tuple<Parser, Assembly>;
        //        myData.Item1.Execute(myData.Item2, _consoleWriter);
        //    }
        //    catch
        //    {
        //    }
        //}

        public void RunInConsole(object parameter)
        {
            _window.ConsoleTab.IsSelected = true;
            _consoleWriter.Reset();
            Parser parser = new Parser();
            try
            {
                Assembly asm = parser.CompileForRun(Code);
                parser.Execute(asm, _consoleWriter);

                //Tuple<Parser, Assembly> myData = new Tuple<Parser, Assembly>(parser, asm);
                //Thread newThread = new Thread(this.ThreadRunInConsole);
                //newThread.IsBackground = true;
                //newThread.Start(myData);
            }
            catch (Exception e)
            {
                PrintException(e);
            }
        }

        public void Restart(object parameter)
        {
            try
            {
                _model.CurrentStep = 0;
                CircuitEvaluator eval = CircuitEvaluator.GetInstance();

                _outputModel = eval.InitFromModel(_model);
                OutputGrid.LoadModel(_model, _outputModel);
            }
            catch (Exception e)
            {
                PrintException(e);
            }
        }

        public void PrevStep(object parameter)
        {
            try
            {
                int currentStep = _model.CurrentStep;
                if (currentStep == 0)
                {
                    Restart(parameter);
                }
                else
                {
                    if (_model.CanStepBack(currentStep - 1))
                    {
                        CircuitEvaluator eval = CircuitEvaluator.GetInstance();
                        StepEvaluator se = eval.GetStepEvaluator();
                        bool outputChanged = se.RunStep(_model.Steps[currentStep - 1].Gates, true);
                        _model.CurrentStep = currentStep - 1;
                        if (outputChanged)
                        {
                            _outputModel.Update(eval.RootRegister);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                PrintException(e);
            }
        }

        public void NextStep(object parameter)
        {
            try
            {
                CircuitEvaluator eval = CircuitEvaluator.GetInstance();

                int currentStep = _model.CurrentStep;
                if (currentStep == 0)
                {
                    eval.InitFromModel(_model);
                }
                if (currentStep < _model.Steps.Count)
                {
                    StepEvaluator se = eval.GetStepEvaluator();
                    bool outputChanged = se.RunStep(_model.Steps[currentStep].Gates);
                    _model.CurrentStep = currentStep + 1;
                    if (outputChanged)
                    {
                        _outputModel.Update(eval.RootRegister);
                    }
                }
            }
            catch (Exception e)
            {
                PrintException(e);
            }
        }

        public void RunToEnd(object parameter)
        {
            try
            {
                CircuitEvaluator eval = CircuitEvaluator.GetInstance();

                int currentStep = _model.CurrentStep;
                if (currentStep == 0)
                {
                    eval.InitFromModel(_model);
                }
                StepEvaluator se = eval.GetStepEvaluator();
                bool outputChanged = se.RunToEnd(_model.Steps, currentStep);
                _model.CurrentStep = _model.Steps.Count;
                if (outputChanged)
                {
                    _outputModel.Update(eval.RootRegister);
                }
            }
            catch (Exception e)
            {
                PrintException(e);
            }
        }

        public CalcWindow ShowCalculator()
        {
            if (_calcWindow == null)
            {
                _calcWindow = new CalcWindow();
                _calcWindow.Owner = _window;
                _calcWindow.Show();
            }
            _calcWindow.Visibility = Visibility.Visible;
            return _calcWindow;
        }

        public void ShowAbout()
        {
            ICustomContentDialog dialog = _window.DialogManager
                .CreateCustomContentDialog(new AboutWindow(), DialogMode.Ok);
            dialog.Show();
        }

        public void New(object parameter)
        {
            string filename = GetNewFilename();
            DocumentInfo info = CreateTab(null, filename);
            info.Editor.Text = _exampleCode;
            info.Editor.IsModified = false;
        }

        public void Open(object parameter)
        {
            OpenFileDialog dialog = new OpenFileDialog();

            // Set filter options and filter index.
            dialog.Filter = "C# Files (*.cs)|*.cs|Text Files (.txt)|*.txt|All Files (*.*)|*.*";
            dialog.FilterIndex = 1;

            // Call the ShowDialog method to show the dialog box.
            bool? userClickedOK = dialog.ShowDialog();

            // Process input if the user clicked OK.
            if (userClickedOK == true)
            {
                string filename = dialog.SafeFileName;
                string fullPath = dialog.FileName;

                int openedId;
                if (_openFiles.TryGetValue(fullPath, out openedId))
                {
                    DocumentInfo info = _documents[openedId];
                    info.Tab.IsSelected = true;
                }
                else
                {
                    DocumentInfo info = CreateTab(fullPath, filename);
                    _openFiles[fullPath] = info.ID;
                }
            }
        }

        public void Save(object parameter)
        {
            LayoutDocument activeTab = ActiveTab;
            if (activeTab != null)
            {
                int id = int.Parse(activeTab.ContentId);
                DocumentInfo info = _documents[id];
                Save(info);
            }
        }

        public void SaveAs(object parameter)
        {
            LayoutDocument activeTab = ActiveTab;
            if (activeTab != null)
            {
                int id = int.Parse(activeTab.ContentId);
                DocumentInfo info = _documents[id];
                SaveAs(info);
            }
        }

        public bool Window_Closing()
        {
            IEnumerable<DocumentInfo> notSaved = _documents.Values.Where<DocumentInfo>(x => x.IsModified);

            if (notSaved.Count<DocumentInfo>() > 0)
            {
                MessageBoxResult result = MessageBox.Show("There are unsaved changes. Do you want to save?", "Quantum Simulator",
                   MessageBoxButton.YesNoCancel, MessageBoxImage.Question, MessageBoxResult.Cancel);
                if (result == MessageBoxResult.Yes)
                {
                    foreach (DocumentInfo info in notSaved)
                    {
                        Save(info);
                        if (info.IsModified)
                        {
                            return false;
                        }
                        else
                        {
                            info.Tab.Close();
                        }
                    }
                    return true;
                }
                else if (result == MessageBoxResult.Cancel)
                {
                    return false;
                }
            }
            return true;
        }

        #endregion // Public Methods


        #region Private Helpers

        private void InitFromModel(ComputerModel model)
        {
            if (_model != null)
            {
                Dictionary<string, List<Gate>> oldComposites = _model.CompositeGates;
                Dictionary<string, List<Gate>> newComposites = model.CompositeGates;

                foreach (var pair in oldComposites)
                {
                    if (!newComposites.ContainsKey(pair.Key))
                    {
                        newComposites[pair.Key] = pair.Value;
                    }
                }
            }
            _model = model;

            CircuitGrid = new CircuitGridVM(_model);

            CircuitEvaluator eval = CircuitEvaluator.GetInstance();
            _outputModel = eval.InitFromModel(_model);
            OutputGrid.LoadModel(_model, _outputModel);     
        }

        private void _consoleWriter_TextChanged(object sender, RoutedEventArgs e)
        {
            OnPropertyChanged("ConsoleOutput");
        }

        private void Save(DocumentInfo info)
        {
            if (string.IsNullOrWhiteSpace(info.FullPath))
            {
                SaveAs(info);
            }
            else
            {
                info.Editor.Save(info.FullPath);
            }
        }

        private void SaveAs(DocumentInfo info)
        {
            SaveFileDialog dialog = new SaveFileDialog();
            dialog.Filter = "C# Files (*.cs)|*.cs|Text Files (*.txt)|*.txt|All Files (*.*)|*.*";
            dialog.AddExtension = true;
            dialog.DefaultExt = ".cs";
            dialog.FileName = info.Title;           
            bool? result = dialog.ShowDialog();

            // If the file name is not an empty string open it for saving.
            if (result == true && !string.IsNullOrWhiteSpace(dialog.FileName))
            {
                TextEditor editor = info.Editor;
                editor.Save(dialog.FileName);

                if (info.FullPath != null)
                {
                    _openFiles.Remove(info.FullPath);
                }
                _openFiles[dialog.FileName] = info.ID;

                info.FullPath = dialog.FileName;             
                info.Title = dialog.SafeFileName;
                info.Tab.Title = dialog.SafeFileName;
            }
        }

        private DocumentInfo CreateTab(string fullPath, string title)
        {
            TextEditor editor = new TextEditor();
            editor.FontFamily = new FontFamily("Consolas");
            editor.SyntaxHighlighting = HighlightingManager.Instance.GetDefinition("C#");
            editor.ShowLineNumbers = true;
            if (!string.IsNullOrWhiteSpace(fullPath))
            {
                editor.Load(fullPath);
            }

            LayoutDocument document = new LayoutDocument();
            document.Title = title;
            document.Content = editor;
            document.Closing += document_Closing;

            _window.DocumentPane.InsertChildAt(_window.DocumentPane.ChildrenCount, document);
            document.IsSelected = true;

            DocumentInfo info = new DocumentInfo(fullPath, title, document, editor);
            _documents[info.ID] = info;

            document.ContentId = info.ID.ToString();

            return info;
        }

        private void document_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            LayoutDocument document = sender as LayoutDocument;
            int id = int.Parse(document.ContentId);
            DocumentInfo info = _documents[id];

            if (info.IsModified)
            {
                MessageBoxResult result = MessageBox.Show("There are unsaved changes. Do you want to save?", "Quantum Simulator",
                   MessageBoxButton.YesNoCancel, MessageBoxImage.Question, MessageBoxResult.Cancel);
                if (result == MessageBoxResult.Yes)
                {
                    Save(info);
                    if (info.IsModified)
                    {
                        e.Cancel = true;
                    }
                }
                else if (result == MessageBoxResult.Cancel)
                {
                    e.Cancel = true;
                }
            }
            if (e.Cancel == false)
            {
                if (info.FullPath != null)
                {
                    _openFiles.Remove(info.FullPath);
                    _documents.Remove(info.ID);
                }
            }
        }

        private string GetNewFilename()
        {
            string name = _newFilename + _newFilenameCount + _newFileNameExt;
            _newFilenameCount++;
            return name;
        }

        private void PrintException(Exception e)
        {
            string message = e.Message;
            if (e.InnerException != null)
            {
                message = message + ":\n" + e.InnerException.Message;
            }
            MessageBox.Show(message);
        }

        #endregion // Private Helpers


        #region Nested Classes

        private class DocumentInfo
        {
            public int ID
            {
                get;
                private set;
            }

            public string FullPath;

            public string Title;

            public LayoutDocument Tab
            {
                get;
                private set;
            }

            public TextEditor Editor
            {
                get;
                private set;
            }

            public bool IsModified
            {
                get
                {
                    return Editor.IsModified;
                }
            }

            public DocumentInfo(string fullPath, string title, LayoutDocument tab, TextEditor editor)
            {
                ID = GenerateNextID();
                FullPath = fullPath;
                Title = title;
                Tab = tab;
                Editor = editor;
            }

            private static int _nextID = -1;
            private static int GenerateNextID()
            {
                _nextID++;
                return _nextID;
            }
        }

        #endregion // Nested Classes
    }
}
