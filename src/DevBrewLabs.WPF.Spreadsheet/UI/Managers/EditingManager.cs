using DevBrewLabs.Spreadsheet;
using DevBrewLabs.Spreadsheet.CalcEngine;
using DevBrewLabs.Spreadsheet.Core;
using DevBrewLabs.Spreadsheet.Formatters;
using DevBrewLabs.WPF.Spreadsheet.CellTypes;
using DevBrewLabs.WPF.Spreadsheet.Components;
using DevBrewLabs.WPF.Spreadsheet.Enums;
using DevBrewLabs.WPF.Spreadsheet.UI.Editors;
using DevBrewLabs.WPF.Spreadsheet.UI.Interaction;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace DevBrewLabs.WPF.Spreadsheet.UI.Managers
{
    internal class EditingManager : UIManager
    {
        private SheetView _editingView;
        private ICellEditor _activeEditor;
        private IEditorContext _activeContext;
        private int _activeRow;
        private int _activeColumn;
        private bool _isSyncing;

        public EditingManager(Spread spread) : base(spread)
        {
        }

        public ICellEditor ActiveEditor => _activeEditor;
        public FrameworkElement ActiveEditorElement => _activeEditor?.Element;
        public bool IsEditing => _activeEditor != null;
        public bool IsShowingFormulaSuggestion => Spread?.FormulaSuggestionManager?.IsOpen == true;
        public int ActiveRow => _activeRow;
        public int ActiveColumn => _activeColumn;

        public void BeginEdit(SheetView sheetView, int row, int column, EditTrigger trigger = EditTrigger.Programmatic, string initialInput = null, bool focusEditor = true)
        {
            if (IsEditing || sheetView == null)
                return;

            var workSheet = (Worksheet)sheetView.WorkSheet;
            if (workSheet == null)
                return;

            var anchor = workSheet.GetSpanCellRange(row, column);
            int editRow = anchor != default ? anchor.TopRow : row;
            int editColumn = anchor != default ? anchor.LeftColumn : column;

            var sheetColumn = ((Columns)workSheet.Columns).GetItem(editColumn);
            var sheetRow = ((Rows)workSheet.Rows).GetItem(editRow);

            bool locked = workSheet.GetLocked(editRow, editColumn) ||
                (sheetRow != null && sheetRow.Locked) ||
                (sheetColumn != null && sheetColumn.Locked);

            if (locked)
                return;

            var cellType = (BaseCellType)(workSheet.GetCellType(editRow, editColumn)) ?? (BaseCellType)sheetColumn?.CellType ?? TextCellType.Default;
            if (!cellType.SupportsEditing)
                return;

            var startingArgs = new CellEditStartingEventArgs(sheetView, editRow, editColumn, trigger);
            if (Spread != null && !Spread.RaiseCellEditStarting(startingArgs))
                return;

            var viewPort = sheetView.ViewPort.As<ViewPort>();
            var cellRect = viewPort.GetCellRect(editRow, editColumn);
            cellRect.X -= viewPort.LeftColumnLocation;
            cellRect.Y -= viewPort.TopRowLocation;

            double zoom = sheetView.ZoomFactor > 0 ? sheetView.ZoomFactor : 1.0;
            var style = workSheet.GetCellStyle(editRow, editColumn, sheetRow, sheetColumn);
            var formatter = workSheet.GetCellFormatter(editRow, editColumn, sheetRow, sheetColumn);
            var value = workSheet.GetValue(editRow, editColumn);
            var formula = workSheet.GetFormula(editRow, editColumn);
            string formattedText = formatter?.Format(value) ?? value?.ToString() ?? string.Empty;

            var context = new EditorContext
            {
                SheetView = sheetView,
                Worksheet = workSheet,
                Row = editRow,
                Column = editColumn,
                CellBounds = cellRect,
                ZoomFactor = zoom,
                Value = value,
                Formula = formula,
                FormattedText = formattedText,
                Style = style,
                Formatter = formatter,
                Trigger = trigger,
                InitialInput = initialInput
            };

            var editor = cellType.CreateEditor(context);
            if (editor == null || editor.Element == null)
                return;

            _editingView = sheetView;
            _activeContext = context;
            _activeEditor = editor;
            _activeRow = editRow;
            _activeColumn = editColumn;

            editor.StartEdit(context);

            var cellsInteractionLayer = _editingView.CellsSurface.GetInteractionLayer();
            cellsInteractionLayer.Children.Add(editor.Element);
            UpdateEditorLayout();

            editor.Element.KeyDown += OnEditorKeyDown;

            if (Spread != null && Spread.ShowFormulaSuggestions && editor is IFormulaEditor formulaEditor)
            {
                Spread.FormulaSuggestionManager.Attach(formulaEditor.TextBox);
            }

            AttachSyncBridge();

            if (focusEditor)
            {
                editor.Element.Focus();
            }
        }

        public void UpdateEditorLayout()
        {
            if (!IsEditing || _editingView == null || Spread?.Sheets?.ActiveSheet == null)
                return;

            var sheetView = _editingView;
            var workSheet = sheetView.WorkSheet as Worksheet;
            if (workSheet == null)
                return;

            double zoom = sheetView.ZoomFactor > 0 ? sheetView.ZoomFactor : 1.0;
            var viewPort = sheetView.ViewPort.As<ViewPort>();

            var cellRect = viewPort.GetCellRect(_activeRow, _activeColumn);
            cellRect.X -= viewPort.LeftColumnLocation;
            cellRect.Y -= viewPort.TopRowLocation;

            var sheetColumn = ((Columns)workSheet.Columns).GetItem(_activeColumn);
            var cellType = (workSheet.GetCellType(_activeRow, _activeColumn) ?? sheetColumn?.CellType) as BaseCellType ?? TextCellType.Default;

            var scaledCellRect = new Rect(
                cellRect.X * zoom,
                cellRect.Y * zoom,
                cellRect.Width * zoom,
                cellRect.Height * zoom);

            var contentRect = cellType.GetContentRect(sheetView, _activeRow, _activeColumn, scaledCellRect, zoom);
            _activeEditor.UpdateLayout(contentRect, zoom);
        }

        private void OnEditorKeyDown(object sender, KeyEventArgs e)
        {
            if (_activeEditor == null)
                return;

            if (_activeEditor.HandlesKeyDown(e))
                return;

            switch (e.Key)
            {
                case Key.Escape:
                    e.Handled = true;
                    EndEdit(false);
                    break;
            }
        }

        public bool EndEdit(bool commitChanges)
        {
            if (!IsEditing)
                return false;

            var cellsInteractionLayer = _editingView.CellsSurface.GetInteractionLayer();
            int row = _activeRow;
            int col = _activeColumn;
            var view = _editingView;
            var editor = _activeEditor;

            if (!commitChanges)
            {
                DetachSyncBridge();
                Spread?.FormulaSuggestionManager?.Detach();

                editor.Element.KeyDown -= OnEditorKeyDown;
                editor.EndEdit();
                cellsInteractionLayer.Children.Remove(editor.Element);

                _activeEditor = null;
                _activeContext = null;
                _editingView = null;

                Spread?.RaiseCellEditEnded(new CellEditEndedEventArgs(view, row, col, false));
                cellsInteractionLayer.Focus();
                return true;
            }

            if (!editor.Validate(out string errorMessage))
            {
                editor.Element.Focus();
                return false;
            }

            object newValue = editor.GetValue();

            var endingArgs = new CellEditEndingEventArgs(view, row, col, newValue);
            if (Spread != null && !Spread.RaiseCellEditEnding(endingArgs))
            {
                editor.Element.Focus();
                return false;
            }

            var workSheet = (Worksheet)view.WorkSheet;
            var cellChangedAction = new CellChangedAction { SheetView = view };
            cellChangedAction.OldState.Value = workSheet.GetValue(row, col);
            cellChangedAction.OldState.Row = row;
            cellChangedAction.OldState.Column = col;
            cellChangedAction.OldState.Selection = view.Selection.Clone();

            try
            {
                if (newValue is string strVal)
                {
                    if (cellChangedAction.OldState.Value != null && 
                        cellChangedAction.OldState.Value.Equals(DataTypeConverter.ConvertType(newValue)))
                    {
                        return true;
                    }

                    workSheet.SetRawValue(row, col, strVal);
                }
                else
                {
                    workSheet.SetValue(row, col, newValue);
                }
            }
            catch (CalcEngineException ex)
            {
                Spread?.RaiseCalculationError(new CalcErrorEventArgs
                {
                    Exception = ex,
                    Row = row,
                    Column = col,
                    Formula = newValue?.ToString(),
                    SheetView = view
                });
                editor.Element.Focus();
                return false;
            }

            if (view.AutoSizeRows)
                view.AutoSizeRow(row);
            if (view.AutoSizeColumns)
                view.AutoSizeColumn(col);

            cellChangedAction.NewState.Value = workSheet.GetValue(row, col);
            cellChangedAction.NewState.Row = row;
            cellChangedAction.NewState.Column = col;
            cellChangedAction.NewState.Selection = view.Selection.Clone();
            Spread?.UndoRedoManager?.AddAction(cellChangedAction);

            DetachSyncBridge();
            Spread?.FormulaSuggestionManager?.Detach();

            editor.Element.KeyDown -= OnEditorKeyDown;
            editor.EndEdit();
            cellsInteractionLayer.Children.Remove(editor.Element);

            _activeEditor = null;
            _activeContext = null;
            _editingView = null;

            Spread?.RaiseCellEditEnded(new CellEditEndedEventArgs(view, row, col, true));
            cellsInteractionLayer.Focus();
            return true;
        }

        #region Formula Bar Two-Way Sync
        private void AttachSyncBridge()
        {
            var formulaBar = Spread?.FormulaTextBox;
            if (formulaBar == null || !(_activeEditor is ITextEditor textEditor))
                return;

            textEditor.TextChanged += OnInCellEditorTextChanged;
            if (formulaBar.Editor != null)
            {
                formulaBar.Editor.TextChanged += OnFormulaBarTextChanged;
            }
        }

        private void DetachSyncBridge()
        {
            var formulaBar = Spread?.FormulaTextBox;
            if (_activeEditor is ITextEditor textEditor)
            {
                textEditor.TextChanged -= OnInCellEditorTextChanged;
            }

            if (formulaBar?.Editor != null)
            {
                formulaBar.Editor.TextChanged -= OnFormulaBarTextChanged;
            }
        }

        private void OnInCellEditorTextChanged(object sender, EventArgs e)
        {
            if (_isSyncing || Spread?.FormulaTextBox == null || !(_activeEditor is ITextEditor textEditor))
                return;

            try
            {
                _isSyncing = true;
                Spread.FormulaTextBox.Text = textEditor.Text;
            }
            finally
            {
                _isSyncing = false;
            }
        }

        private void OnFormulaBarTextChanged(object sender, TextChangedEventArgs e)
        {
            if (_isSyncing || !(_activeEditor is ITextEditor textEditor) || Spread?.FormulaTextBox == null)
                return;

            try
            {
                _isSyncing = true;
                textEditor.Text = Spread.FormulaTextBox.Text;
                UpdateEditorLayout();
            }
            finally
            {
                _isSyncing = false;
            }
        }
        #endregion

        private class EditorContext : IEditorContext
        {
            public ISheetView SheetView { get; internal set; }
            public IWorksheet Worksheet { get; internal set; }
            public int Row { get; internal set; }
            public int Column { get; internal set; }
            public Rect CellBounds { get; internal set; }
            public double ZoomFactor { get; internal set; }
            public object Value { get; internal set; }
            public string Formula { get; internal set; }
            public string FormattedText { get; internal set; }
            public IStyle Style { get; internal set; }
            public IFormatter Formatter { get; internal set; }
            public EditTrigger Trigger { get; internal set; }
            public string InitialInput { get; internal set; }
        }
    }
}
