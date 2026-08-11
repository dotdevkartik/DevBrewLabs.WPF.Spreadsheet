using System;

namespace DevBrewLabs.Spreadsheet.CalcEngine
{
    public delegate void ValueChangedEventHandler(ValueChangedEventArgs args);
    public delegate void FormulaChangedEventHandler(FormulaChangedEventArgs args);

    public interface IDataAdapter
    {
        event ValueChangedEventHandler ValueChanged;
        event FormulaChangedEventHandler FormulaChanged;
        void SetMetadata(string sheetName, int row, int column, object data);
        object GetMetadata(string sheetName, int row, int column);
        object[,] GetRangeValue(string sheetName, int rowIndex, int columnIndex, int rowCount, int columnCount);
        object GetValue(string sheetName, int rowIndex, int columnIndex);
        string GetFormula(string sheetName, int row, int column);
    }

    public class ValueChangedEventArgs : EventArgs
    {
        public int Row { get; set; }
        public int Column { get; set; }
        public string SheetName { get; set; }
        public object OldValue { get; set; }
        public object NewValue { get; set; }
    }

    public class FormulaChangedEventArgs : EventArgs
    {
        public int Row { get; set; }
        public int Column { get; set; }
        public string SheetName { get; set; }
        public string OldFormula { get; set; }
        public string NewFormula { get; set; }
    }
}
