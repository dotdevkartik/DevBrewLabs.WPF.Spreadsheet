using DevBrewLabs.Spreadsheet.Formatters;

namespace DevBrewLabs.Spreadsheet.Core
{
    /// <summary>
    /// Holds a snapshot of raw cell data for a single row within a column.
    /// </summary>
    internal struct CellData
    {
        public object Value;
        public string Formula;
        public ushort StyleId;
        public string StyleName;
        public IFormatter Formatter;
        public ICellType CellType;
        public IDataMap DataMap;
        public int RowSpan;
        public int ColumnSpan;
        public bool Locked;
        public object MetaData;
        public bool IsEmpty => Value == null && Formula == null && StyleId == 0 && StyleName == null && Formatter == null && CellType == null && DataMap == null && RowSpan == 0 && ColumnSpan == 0 && !Locked && MetaData == null;
    }

    /// <summary>
    /// Columnar data storage for a single column across all rows.
    /// </summary>
    internal class ColumnData
    {
        public int ColumnIndex { get; }

        private readonly ChunkedArray<object> _values;
        private readonly ChunkedArray<string> _formulas;
        private readonly ChunkedArray<ushort> _styleIds;
        private readonly ChunkedArray<string> _styleNames;
        private readonly ChunkedArray<IFormatter> _formatters;
        private readonly ChunkedArray<ICellType> _cellTypes;
        private readonly ChunkedArray<IDataMap> _dataMaps;
        private readonly ChunkedArray<int> _rowSpans;
        private readonly ChunkedArray<int> _columnSpans;
        private readonly ChunkedArray<bool> _locked;
        private readonly ChunkedArray<object> _metaData;

        public ColumnData(int columnIndex)
        {
            ColumnIndex = columnIndex;
            _values = new ChunkedArray<object>();
            _formulas = new ChunkedArray<string>();
            _styleIds = new ChunkedArray<ushort>();
            _styleNames = new ChunkedArray<string>();
            _formatters = new ChunkedArray<IFormatter>();
            _cellTypes = new ChunkedArray<ICellType>();
            _dataMaps = new ChunkedArray<IDataMap>();
            _rowSpans = new ChunkedArray<int>();
            _columnSpans = new ChunkedArray<int>();
            _locked = new ChunkedArray<bool>();
            _metaData = new ChunkedArray<object>();
        }

        #region Values
        public object GetValue(int row) => _values.GetValue(row);

        public void SetValue(int row, object value)
        {
            value = DataTypeConverter.ConvertType(value);
            if (value == null) _values.Remove(row);
            else _values.SetValue(row, value);
        }
        #endregion

        #region Formulas
        public string GetFormula(int row) => _formulas.GetValue(row);

        public void SetFormula(int row, string formula)
        {
            if (string.IsNullOrEmpty(formula)) _formulas.Remove(row);
            else _formulas.SetValue(row, formula);
        }
        #endregion

        #region Styles
        public ushort GetStyleId(int row) => _styleIds.GetValue(row);

        public void SetStyleId(int row, ushort styleId)
        {
            if (styleId == StylePalette.DefaultStyleId) _styleIds.Remove(row);
            else _styleIds.SetValue(row, styleId);
        }

        public string GetStyleName(int row) => _styleNames.GetValue(row);

        public void SetStyleName(int row, string styleName)
        {
            if (string.IsNullOrEmpty(styleName)) _styleNames.Remove(row);
            else _styleNames.SetValue(row, styleName);
        }
        #endregion

        #region Formatters
        public IFormatter GetFormatter(int row) => _formatters.GetValue(row);

        public void SetFormatter(int row, IFormatter formatter)
        {
            if (formatter == null) _formatters.Remove(row);
            else _formatters.SetValue(row, formatter);
        }
        #endregion

        #region CellTypes
        public ICellType GetCellType(int row) => _cellTypes.GetValue(row);

        public void SetCellType(int row, ICellType cellType)
        {
            if (cellType == null) _cellTypes.Remove(row);
            else _cellTypes.SetValue(row, cellType);
        }
        #endregion

        #region DataMaps
        public IDataMap GetDataMap(int row) => _dataMaps.GetValue(row);

        public void SetDataMap(int row, IDataMap dataMap)
        {
            if (dataMap == null) _dataMaps.Remove(row);
            else _dataMaps.SetValue(row, dataMap);
        }
        #endregion

        #region Spans & Locked
        public int GetRowSpan(int row) => _rowSpans.GetValue(row);

        public void SetRowSpan(int row, int span)
        {
            if (span == 0) _rowSpans.Remove(row);
            else _rowSpans.SetValue(row, span);
        }

        public int GetColumnSpan(int row) => _columnSpans.GetValue(row);

        public void SetColumnSpan(int row, int span)
        {
            if (span == 0) _columnSpans.Remove(row);
            else _columnSpans.SetValue(row, span);
        }

        public bool GetLocked(int row) => _locked.GetValue(row);

        public void SetLocked(int row, bool locked)
        {
            if (!locked) _locked.Remove(row);
            else _locked.SetValue(row, locked);
        }

        #region MetaData
        public object GetMetaData(int row) => _metaData.GetValue(row);

        public void SetMetaData(int row, object metaData)
        {
            if (metaData == null) _metaData.Remove(row);
            else _metaData.SetValue(row, metaData);
        }
        #endregion
        #endregion

        #region CellData Operations
        internal CellData GetCellData(int row)
        {
            return new CellData
            {
                Value = GetValue(row),
                Formula = GetFormula(row),
                StyleId = GetStyleId(row),
                StyleName = GetStyleName(row),
                Formatter = GetFormatter(row),
                CellType = GetCellType(row),
                DataMap = GetDataMap(row),
                RowSpan = GetRowSpan(row),
                ColumnSpan = GetColumnSpan(row),
                Locked = GetLocked(row),
                MetaData = GetMetaData(row)
            };
        }

        internal void SetCellData(int row, CellData data)
        {
            SetValue(row, data.Value);
            SetFormula(row, data.Formula);
            SetStyleId(row, data.StyleId);
            SetStyleName(row, data.StyleName);
            SetFormatter(row, data.Formatter);
            SetCellType(row, data.CellType);
            SetDataMap(row, data.DataMap);
            SetRowSpan(row, data.RowSpan);
            SetColumnSpan(row, data.ColumnSpan);
            SetLocked(row, data.Locked);
            SetMetaData(row, data.MetaData);
        }

        internal void ClearRow(int row)
        {
            _values.Remove(row);
            _formulas.Remove(row);
            _styleIds.Remove(row);
            _styleNames.Remove(row);
            _formatters.Remove(row);
            _cellTypes.Remove(row);
            _dataMaps.Remove(row);
            _rowSpans.Remove(row);
            _columnSpans.Remove(row);
            _locked.Remove(row);
            _metaData.Remove(row);
        }

        public bool HasRowData(int row)
        {
            return _values.ContainsKey(row) || _formulas.ContainsKey(row) || _styleIds.ContainsKey(row) ||
                   _styleNames.ContainsKey(row) || _formatters.ContainsKey(row) || _cellTypes.ContainsKey(row) ||
                   _dataMaps.ContainsKey(row) || _rowSpans.ContainsKey(row) || _columnSpans.ContainsKey(row) ||
                   _locked.ContainsKey(row) || _metaData.ContainsKey(row);
        }

        public void Clear()
        {
            _values.Clear();
            _formulas.Clear();
            _styleIds.Clear();
            _styleNames.Clear();
            _formatters.Clear();
            _cellTypes.Clear();
            _dataMaps.Clear();
            _rowSpans.Clear();
            _columnSpans.Clear();
            _locked.Clear();
            _metaData.Clear();
        }
        #endregion
    }
}
