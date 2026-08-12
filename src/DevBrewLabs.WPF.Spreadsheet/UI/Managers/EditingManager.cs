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
    internal class EditingManager : UIManager, IEditingManager
    {
        public EditingManager(Spread spread) : base(spread)
        {
            UseCellValue = true;
        }

        public FrameworkElement ActiveEditor { get; private set; }
        public bool IsEditing => ActiveEditor != null;
        internal bool UseCellValue { get; set; }

        public void BeginEdit(int row, int column)
        {
            if (IsEditing)
                return;

            var sheetView = Spread.SheetViews.ActiveSheetView;
            var workSheet = (WorkSheet)sheetView.WorkSheet;

            var anchor = workSheet.GetSpanCellRange(row, column);
            if (anchor != default)
            {
                row = anchor.TopRow;
                column = anchor.LeftColumn;
            }

            var sheetColumn = ((Columns)workSheet.Columns).GetItem(column);

            if (sheetColumn != null && sheetColumn.Locked)
                return;

            var cellsInteractionLayer = sheetView.Spread.SheetViewPane.CellsRegion.GetInteractionLayer();
            var cellRect = sheetView.ViewPort.GetCellRect(row, column);
            cellRect.X -= sheetView.ViewPort.As<ViewPort>().LeftColumnLocation;
            cellRect.Y -= sheetView.ViewPort.As<ViewPort>().TopRowLocation;

            var sheetRow = ((Rows)workSheet.Rows).GetItem(row);
            var cellType = (BaseCellType)(workSheet.GetCellType(row, column)) ?? (BaseCellType)sheetColumn?.CellType ?? TextCellType.Default;

            var style = workSheet.GetStyle(row, column);

            if (style == null)
            {
                var styleName = workSheet.GetStyleName(row, column);
                style = !string.IsNullOrEmpty(styleName)
                    ? ((WorkBook)workSheet.WorkBook).GetNamedStyle(styleName)
                    : ((WorkBook)workSheet.WorkBook).PickStyle(sheetColumn, sheetRow, SheetRegion.Cells);
            }
            var editor = cellType.GetEditor(style);
            editor.SheetView = sheetView;
            ActiveEditor = editor;

            var formula = workSheet.GetFormula(row, column);
            if (!string.IsNullOrEmpty(formula))
            {
                editor.Text = formula;
            }
            else
            {
                var value = workSheet.GetValue(row, column);
                var formatter = workSheet.PickFormatter(sheetColumn, sheetRow);
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
            cellsInteractionLayer.Children.Add(ActiveEditor);
            UpdateEditorLayout();
            editor.Focus();
        }

        public void UpdateEditorLayout()
        {
            if (ActiveEditor is EditorBase editor && Spread?.SheetViews?.ActiveSheetView != null)
            {
                var sheetView = Spread.SheetViews.ActiveSheetView.As<SheetView>();
                var workSheet = sheetView.WorkSheet;
                double zoom = sheetView.ZoomFactor > 0 ? sheetView.ZoomFactor : 1.0;
                var viewPort = sheetView.ViewPort.As<ViewPort>();

                var cellRect = sheetView.ViewPort.GetCellRect(editor.Row, editor.Column);
                cellRect.X -= viewPort.LeftColumnLocation;
                cellRect.Y -= viewPort.TopRowLocation;
                var sheetColumn = ((Columns)workSheet.Columns).GetItem(editor.Column);
                var sheetRow = ((Rows)workSheet.Rows).GetItem(editor.Row);
                var style = workSheet.GetStyle(editor.Row, editor.Column);

                if (style == null)
                {
                    var styleName = workSheet.GetStyleName(editor.Row, editor.Column);
                    style = !string.IsNullOrEmpty(styleName)
                        ? ((WorkBook)workSheet.WorkBook).GetNamedStyle(styleName)
                        : ((WorkBook)workSheet.WorkBook).PickStyle(sheetColumn, sheetRow, SheetRegion.Cells);
                }

                var wpfStyle = style;
                editor.FontSize = (wpfStyle?.FontSize ?? 14) * zoom;
                editor.MinWidth = System.Math.Max(0, cellRect.Width * zoom - 3);

                int initialLineCount = TextUtils.GetLineCount(editor.Text);
                if (style.AllowMultiLineText && initialLineCount > 1)
                {
                    double initialLineHeight = editor.FontSize * 1.3;
                    editor.Height = System.Math.Max(cellRect.Height * zoom - 3, initialLineCount * initialLineHeight + 6);
                }
                else
                {
                    editor.Height = System.Math.Max(0, cellRect.Height * zoom - 3);
                }

                Canvas.SetLeft(ActiveEditor, cellRect.X * zoom + 1);
                Canvas.SetTop(ActiveEditor, cellRect.Y * zoom + 1);
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

        public bool EndEdit(bool commitChanges)
        {
            if (!IsEditing)
                return false;

            var sheetView = Spread.SheetViews.ActiveSheetView;
            var cellsInteractionLayer = sheetView.Spread.SheetViewPane.CellsRegion.GetInteractionLayer();

            if (!commitChanges)
            {
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
                return EndTextCellEdit(gcTextBox, sheetView, cellsInteractionLayer);
            }
            else if(ActiveEditor is NumericEditor numTextBox)
            {
                return EndNumericCellEdit(numTextBox, sheetView, cellsInteractionLayer);
            }

            return false;
        }

        private bool EndNumericCellEdit(NumericEditor numTextBox, ISheetView sheetView, InteractionLayer layer)
        {
            var workSheet = sheetView.WorkSheet;
            var cellChangedAction = new CellChangedAction() { SheetView = sheetView.As<SheetView>() };
            cellChangedAction.OldState.Value = workSheet.GetValue(numTextBox.Row, numTextBox.Column);
            cellChangedAction.OldState.Row = numTextBox.Row;
            cellChangedAction.OldState.Column = numTextBox.Column;
            cellChangedAction.OldState.Selection = sheetView.Selection.Clone();

            workSheet.SetRawValue(numTextBox.Row, numTextBox.Column, numTextBox.Text);

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

