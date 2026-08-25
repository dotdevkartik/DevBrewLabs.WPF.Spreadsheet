using DevBrewLabs.Spreadsheet;
using DevBrewLabs.Spreadsheet.CalcEngine;
using DevBrewLabs.Spreadsheet.Utils;
using DevBrewLabs.WPF.Spreadsheet.UI.Editors;
using DevBrewLabs.WPF.Spreadsheet.UI.Interaction;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using DevBrewLabs.WPF.Spreadsheet.CellTypes;

namespace DevBrewLabs.WPF.Spreadsheet.UI.Managers
{
    internal class EditingManager : UIManager
    {
        private SheetView _editingView;

        public EditingManager(Spread spread) : base(spread)
        {
            UseCellValue = true;
        }

        public FrameworkElement ActiveEditor { get; private set; }
        public bool IsEditing => ActiveEditor != null;
        internal bool UseCellValue { get; set; }

        public void BeginEdit(SheetView sheetView, int row, int column)
        {
            if (IsEditing)
                return;

            _editingView = sheetView;
            var workSheet = (Worksheet)sheetView.WorkSheet;

            var anchor = workSheet.GetSpanCellRange(row, column);
            if (anchor != default)
            {
                row = anchor.TopRow;
                column = anchor.LeftColumn;
            }

            var sheetColumn = ((Columns)workSheet.Columns).GetItem(column);
            var sheetRow = ((Rows)workSheet.Rows).GetItem(row);

            bool locked = workSheet.GetLocked(row, column) || 
                (sheetRow != null && sheetRow.Locked) || 
                (sheetColumn != null && sheetColumn.Locked);

            if (locked)
                return;

            var cellsInteractionLayer = _editingView.CellsSurface.GetInteractionLayer();
            var cellRect = _editingView.ViewPort.GetCellRect(row, column);
            cellRect.X -= _editingView.ViewPort.As<ViewPort>().LeftColumnLocation;
            cellRect.Y -= _editingView.ViewPort.As<ViewPort>().TopRowLocation;

            var cellType = (BaseCellType)(workSheet.GetCellType(row, column)) ?? (BaseCellType)sheetColumn?.CellType ?? TextCellType.Default;

            var style = workSheet.GetCellStyle(row, column, sheetRow, sheetColumn);
            var editor = cellType.GetEditor(style);
            editor.SheetView = _editingView;
            ActiveEditor = editor;

            var formula = workSheet.GetFormula(row, column);
            if (!string.IsNullOrEmpty(formula))
            {
                editor.Text = formula;
            }
            else
            {
                var value = workSheet.GetValue(row, column);
                var formatter = workSheet.GetCellFormatter(row, column, sheetRow, sheetColumn);
                editor.Text = formatter?.Format(value) ?? value?.ToString() ?? "";
            }

            if (!UseCellValue)
                editor.Text = "";

            editor.CaretIndex = editor.Text.Length;

            if (editor is TextEditor gcTextBox)
            {
                gcTextBox.AcceptsReturn = style.AllowMultiLineText;
            }

            editor.Row = row;
            editor.Column = column;
            editor.KeyDown += OnEditorKeyDown;
            Spread.FormulaSuggestionManager.Attach(editor);
            cellsInteractionLayer.Children.Add(ActiveEditor);
            UpdateEditorLayout();
            editor.Focus();
        }

        public void UpdateEditorLayout()
        {
            if (ActiveEditor is EditorBase editor && Spread?.Sheets?.ActiveSheet != null)
            {
                var sheetView = Spread.Sheets.ActiveSheet.As<SheetView>();
                var workSheet = sheetView.WorkSheet;
                double zoom = sheetView.ZoomFactor > 0 ? sheetView.ZoomFactor : 1.0;
                var viewPort = sheetView.ViewPort.As<ViewPort>();

                var cellRect = sheetView.ViewPort.GetCellRect(editor.Row, editor.Column);
                cellRect.X -= viewPort.LeftColumnLocation;
                cellRect.Y -= viewPort.TopRowLocation;
                var sheetColumn = ((Columns)workSheet.Columns).GetItem(editor.Column);
                var sheetRow = ((Rows)workSheet.Rows).GetItem(editor.Row);
                var style = workSheet.GetCellStyle(editor.Row, editor.Column, sheetRow, sheetColumn);

                var scaledCellRect = new Rect(
                    cellRect.X * zoom,
                    cellRect.Y * zoom,
                    cellRect.Width * zoom,
                    cellRect.Height * zoom);

                var cellType = (workSheet.GetCellType(editor.Row, editor.Column) ?? sheetColumn?.CellType) as BaseCellType ?? TextCellType.Default;
                var contentRect = cellType.GetContentRect(sheetView, editor.Row, editor.Column, scaledCellRect, zoom);

                editor.FontSize = (style?.FontSize ?? 14) * zoom;
                double availableWidth = System.Math.Max(0, contentRect.Width - 3);
                editor.MinWidth = availableWidth;

                int initialLineCount = TextUtils.GetLineCount(editor.Text);
                if (style.AllowMultiLineText && initialLineCount > 1)
                {
                    double initialLineHeight = editor.FontSize * 1.3;
                    editor.Height = System.Math.Max(contentRect.Height - 3, initialLineCount * initialLineHeight + 6);
                }
                else
                {
                    editor.Height = System.Math.Max(0, contentRect.Height - 3);
                }

                Canvas.SetLeft(ActiveEditor, contentRect.X + 1);
                Canvas.SetTop(ActiveEditor, contentRect.Y + 1);
            }
        }

        private void OnEditorKeyDown(object sender, KeyEventArgs e)
        {
            switch(e.Key)
            {
                case Key.Escape:
                    EndEdit(false);
                    break;
            }
        }

        public bool IsShowingFormulaSuggestion => Spread?.FormulaSuggestionManager?.IsOpen == true;

        public bool EndEdit(bool commitChanges)
        {
            if (!IsEditing)
                return false;

            var cellsInteractionLayer = _editingView.CellsSurface.GetInteractionLayer();

            if (!commitChanges)
            {
                Spread.FormulaSuggestionManager.Detach();
                if (ActiveEditor != null)
                {
                    ActiveEditor.KeyDown -= OnEditorKeyDown;
                    cellsInteractionLayer.Children.Remove(ActiveEditor);
                    ActiveEditor = null;
                }
                return true;
            }

            if (ActiveEditor is TextEditor gcTextBox)
            {
                return EndTextCellEdit(gcTextBox, _editingView, cellsInteractionLayer);
            }
            else if(ActiveEditor is NumericEditor numTextBox)
            {
                return EndNumericCellEdit(numTextBox, _editingView, cellsInteractionLayer);
            }

            return false;
        }

        private bool EndNumericCellEdit(NumericEditor numTextBox, ISheetView sheetView, InteractionLayer layer)
        {
            Spread.FormulaSuggestionManager.Detach();
            var workSheet = sheetView.WorkSheet;
            var cellChangedAction = new CellChangedAction() { SheetView = sheetView.As<SheetView>() };
            cellChangedAction.OldState.Value = workSheet.GetValue(numTextBox.Row, numTextBox.Column);
            cellChangedAction.OldState.Row = numTextBox.Row;
            cellChangedAction.OldState.Column = numTextBox.Column;
            cellChangedAction.OldState.Selection = sheetView.Selection.Clone();

            workSheet.SetRawValue(numTextBox.Row, numTextBox.Column, numTextBox.Text);

            if (sheetView.AutoSizeRows)
                sheetView.AutoSizeRow(numTextBox.Row);
            if (sheetView.AutoSizeColumns)
                sheetView.AutoSizeColumn(numTextBox.Column);

            cellChangedAction.NewState.Value = workSheet.GetValue(numTextBox.Row, numTextBox.Column);
            cellChangedAction.NewState.Row = numTextBox.Row;
            cellChangedAction.NewState.Column = numTextBox.Column;
            cellChangedAction.NewState.Selection = sheetView.Selection.Clone();

            Spread.UndoRedoManager.AddAction(cellChangedAction);

            layer.Children.Remove(ActiveEditor);
            ActiveEditor.KeyDown -= OnEditorKeyDown;
            ActiveEditor = null;
            layer.Focus();
            return true;
        }

        private bool EndTextCellEdit(TextEditor gcTextBox, ISheetView sheetView, InteractionLayer layer)
        {
            var workSheet = sheetView.WorkSheet;
            var cellChangedAction = new CellChangedAction() { SheetView = sheetView.As<SheetView>() };
            cellChangedAction.OldState.Value = workSheet.GetValue(gcTextBox.Row, gcTextBox.Column);
            cellChangedAction.OldState.Row = gcTextBox.Row;
            cellChangedAction.OldState.Column = gcTextBox.Column;
            cellChangedAction.OldState.Selection = sheetView.Selection.Clone();

            try
            {
                workSheet.SetRawValue(gcTextBox.Row, gcTextBox.Column, gcTextBox.Text);
            }
            catch (CalcEngineException ex)
            {
                sheetView.Spread.RaiseCalculationError(new CalcErrorEventArgs()
                {
                    Exception = ex,
                    Row = gcTextBox.Row,
                    Column = gcTextBox.Column,
                    Formula = gcTextBox.Text,
                    SheetView = sheetView
                });
                ActiveEditor.Focus();
                return false;
            }

            Spread.FormulaSuggestionManager.Detach();
            if (sheetView.AutoSizeRows)
                sheetView.AutoSizeRow(gcTextBox.Row);
            if (sheetView.AutoSizeColumns)
                sheetView.AutoSizeColumn(gcTextBox.Column);

            // We add undo/redo regardless of formula or value to support full history
            cellChangedAction.NewState.Value = workSheet.GetValue(gcTextBox.Row, gcTextBox.Column);
            cellChangedAction.NewState.Row = gcTextBox.Row;
            cellChangedAction.NewState.Column = gcTextBox.Column;
            cellChangedAction.NewState.Selection = sheetView.Selection.Clone();
            Spread.UndoRedoManager.AddAction(cellChangedAction);

            layer.Children.Remove(ActiveEditor);
            ActiveEditor.KeyDown -= OnEditorKeyDown;
            ActiveEditor = null;
            layer.Focus();
            return true;
        }
    }
}

