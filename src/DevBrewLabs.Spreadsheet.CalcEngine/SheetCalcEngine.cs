using DevBrewLabs.Evalis;
using DevBrewLabs.Parserly;
using DevBrewLabs.Spreadsheet.CalcEngine.Evaluator;
using DevBrewLabs.Spreadsheet.CalcEngine.Parsers;
using DevBrewLabs.Spreadsheet.CalcEngine.Parsers.TokenParsers;
using DevBrewLabs.Spreadsheet.CalcEngine.Utils;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DevBrewLabs.Spreadsheet.CalcEngine
{
    /// <summary>
    /// Delegate for handling the event when a cell is recalculated.
    /// </summary>
    /// <param name="row">The row index of the recalculated cell.</param>
    /// <param name="column">The column index of the recalculated cell.</param>
    public delegate void CellRecalculatedEventHandler(int row, int column);
    
    /// <summary>
    /// The primary implementation of <see cref="ICalcEngine"/>, responsible for parsing, 
    /// evaluating formulas, and managing spreadsheet state.
    /// </summary>
    public class SheetCalcEngine : ICalcEngine
    {
        private readonly FormulaEngine _formulaEngine;
        private readonly CalcEngineContext _engineContext;
        private readonly IDataAdapter _provider;
        private readonly IDependencyManager _dependencyManager;

        /// <inheritdoc />
        public event CellRecalculatedEventHandler CellRecalculated;

        /// <summary>
        /// Initializes a new instance of the <see cref="SheetCalcEngine"/> class.
        /// </summary>
        /// <param name="dataProvider">The data provider for accessing cell values and metadata.</param>
        public SheetCalcEngine(IDataAdapter dataProvider) 
            : this(dataProvider, new DependencyManager(dataProvider))
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SheetCalcEngine"/> class with a custom dependency manager.
        /// </summary>
        /// <param name="dataProvider">The data provider for accessing cell values and metadata.</param>
        /// <param name="dependencyManager">The dependency manager to use.</param>
        internal SheetCalcEngine(IDataAdapter dataProvider, IDependencyManager dependencyManager)
        {
            _provider = dataProvider ?? throw new ArgumentNullException(nameof(dataProvider));
            _dependencyManager = dependencyManager ?? throw new ArgumentNullException(nameof(dependencyManager));
            _engineContext = new CalcEngineContext(dataProvider);
            _formulaEngine = new FormulaEngine(_engineContext);

            var settings = new EngineSettings
            {
                CustomTokenParsers = new List<IParser>
                {
                    new CellRangeTokenParser(),
                    new CellRefTokenParser()
                }
            };
            _formulaEngine.ApplySettings(settings);
            _provider.ValueChanged += OnCellValueChanged;
            _provider.FormulaChanged += OnFormulaChanged;
        }

        private void OnCellValueChanged(ValueChangedEventArgs args)
        {
            UpdateDependents(args.SheetName, args.Row, args.Column);
        }

        #region ICalcEngine Implementation

        /// <inheritdoc />
        public CalcValue EvaluateExpression(string expression, string sheetName = "")
        {
            if (expression == null) throw new ArgumentNullException(nameof(expression));

            _engineContext.SetCurrentSheet(sheetName);
            var result = _formulaEngine.Evaluate(GetPureFormula(expression));
            return FormulaEngineConverter.ConvertToCalcValue(result);
        }

        /// <inheritdoc />
        public IEnumerable<FormulaInfo> GetRegisteredFormulas()
        {
            return _formulaEngine.FormulaStore.GetAll();
        }

        /// <inheritdoc />
        public void RegisterCustomFormula(FormulaBase formula)
        {
            if (formula == null) throw new ArgumentNullException(nameof(formula));

            _formulaEngine.FormulaStore.Add(formula);
        }

        /// <inheritdoc />
        public object GetValue(string sheetName, int row, int column)
        {
            var formula = _provider.GetFormula(sheetName, row, column);
            if (string.IsNullOrEmpty(formula))
            {
                return null;
            }

            if (!(_provider.GetMetadata(sheetName, row, column) is CalcCellMetaInfo metaInfo))
            {
                metaInfo = new CalcCellMetaInfo { Dependents = new HashSet<CellRef>() };
                _provider.SetMetadata(sheetName, row, column, metaInfo);
            }

            // Value not calculated yet, calculate value
            if (metaInfo.CalculatedValue == null)
            {
                if (metaInfo.IsEvaluating)
                {
                    return new CalcValue { Kind = CalcValueKind.Error, Value = "#REF!" };
                }

                metaInfo.IsEvaluating = true;
                var prevSheet = _engineContext.CurrentSheetName;

                try
                {
                    _engineContext.SetCurrentSheet(sheetName);
                    var result = _formulaEngine.Evaluate(GetPureFormula(formula));
                    metaInfo.CalculatedValue = FormulaEngineConverter.ConvertToCalcValue(result);
                }
                catch (Exception)
                {
                    metaInfo.CalculatedValue = new CalcValue { Kind = CalcValueKind.Error, Value = "#VALUE!" };
                }
                finally
                {
                    metaInfo.IsEvaluating = false;
                    _engineContext.SetCurrentSheet(prevSheet);
                }
            }

            return metaInfo.CalculatedValue;
        }

        /// <summary>
        /// Recalculates the value of a specific cell and its dependents.
        /// </summary>
        /// <param name="sheetName">The name of the sheet.</param>
        /// <param name="row">The row index of the cell.</param>
        /// <param name="column">The column index of the cell.</param>
        public void RecalculateCell(string sheetName, int row, int column)
        {
            RecalculateCellImpl(sheetName, row, column);
        }

        /// <inheritdoc />
        public List<object> GetRangesInFormulaString(string formula)
        {
            if (formula == null) throw new ArgumentNullException(nameof(formula));

            var variables = _formulaEngine.ExtractVariables(GetPureFormula(formula));
            var ranges = new List<object>();
            
            foreach (var v in variables)
            {
                if (v.Contains(":"))
                {
                    ranges.Add(new CellRangeRef(v));
                }
                else
                {
                    ranges.Add(new CellRef(v));
                }
            }

            return ranges;
        }

        #endregion

        #region Private Helper Methods

        private void OnFormulaChanged(FormulaChangedEventArgs args)
        {
            var sheetName = args.SheetName;
            var row = args.Row;
            var column = args.Column;
            var formula = args.NewFormula;

            if (string.IsNullOrEmpty(formula))
            {
                ClearFormula(sheetName, row, column);
                return;
            }

            formula = formula.TrimStart('=');

            // Ensure meta info exists and get any existing dependents
            CalcCellMetaInfo metaInfo = _provider.GetMetadata(sheetName, row, column) as CalcCellMetaInfo;
            if (metaInfo == null)
            {
                metaInfo = new CalcCellMetaInfo { Dependents = new HashSet<CellRef>() };
            }

            var curCell = new CellRef(row, column, sheetName);
            _dependencyManager.ClearDependencies(curCell);

            List<string> extractedVars;
            try
            {
                extractedVars = _formulaEngine.ExtractVariables(GetPureFormula(formula)).ToList();
            }
            catch (Exception)
            {
                throw new CalcEngineException("Invalid formula");
            }

            var dependencies = new List<object>();
            foreach (var v in extractedVars)
            {
                if (v.Contains(":"))
                {
                    dependencies.Add(new CellRangeRef(v));
                }
                else
                {
                    dependencies.Add(new CellRef(v));
                }
            }
            metaInfo.Dependencies = dependencies;

            if (dependencies.Count > 0)
            {
                foreach (var dependency in dependencies)
                {
                    if (dependency is CellRef cellRef)
                    {
                        _dependencyManager.SetCellDependency(curCell, cellRef);
                    }
                    else if (dependency is CellRangeRef rangeRef)
                    {
                        _dependencyManager.SetRangeDependency(curCell, rangeRef);
                    }
                }
            }

            _provider.SetMetadata(sheetName, row, column, metaInfo);
            RecalculateCellImpl(sheetName, row, column);
        }

        private void ClearFormula(string sheetName, int row, int column)
        {
            var curCell = new CellRef(row, column, sheetName);
            _dependencyManager.ClearDependencies(curCell);

            if (_provider.GetMetadata(sheetName, row, column) is CalcCellMetaInfo metaInfo)
            {
                metaInfo.CalculatedValue = null;
                metaInfo.Dependencies.Clear();
            }

            UpdateDependents(sheetName, row, column);
        }

        private void RecalculateCellImpl(string sheetName, int row, int column)
        {
            UpdateCellValue(sheetName, row, column);
            UpdateDependents(sheetName, row, column);
        }

        private void UpdateDependents(string sheetName, int row, int column)
        {
            // Update dependent cells, first clear value, then recalc
            var dependents = _dependencyManager.GetDependentCells(sheetName, row, column);
            
            foreach (var dependent in dependents)
            {
                if (_provider.GetMetadata(dependent.SheetName, dependent.Row, dependent.Column) is CalcCellMetaInfo metaInfo)
                {
                    metaInfo.CalculatedValue = null;
                }
            }

            // Calculate values
            foreach (var dependent in dependents)
            {
                UpdateCellValue(dependent.SheetName, dependent.Row, dependent.Column);
            }

            CellRecalculated?.Invoke(row, column);
        }

        private void UpdateCellValue(string sheetName, int row, int column)
        {
            var formula = _provider.GetFormula(sheetName, row, column);
            if (string.IsNullOrEmpty(formula))
            {
                return;
            }

            if (!(_provider.GetMetadata(sheetName, row, column) is CalcCellMetaInfo metaInfo))
            {
                metaInfo = new CalcCellMetaInfo { Dependents = new HashSet<CellRef>() };
                _provider.SetMetadata(sheetName, row, column, metaInfo);
            }

            // Has formula, update value
            _engineContext.SetCurrentSheet(sheetName);
            var result = _formulaEngine.Evaluate(GetPureFormula(formula));
            var val = FormulaEngineConverter.ConvertToCalcValue(result);
            metaInfo.CalculatedValue = val;
        }

        private string GetPureFormula(string formula)
        {
            if (string.IsNullOrEmpty(formula))
                return formula;
                
            return formula.StartsWith("=") ? formula.Substring(1) : formula;
        }

        #endregion
    }
}
