using DevBrewLabs.WPF.Spreadsheet.CellTypes;
using DevBrewLabs.WPF.Spreadsheet.Enums;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace DevBrewLabs.WPF.Spreadsheet.UI.Editors
{
    /// <summary>
    /// In-place text editor for sparkline data points, displaying and parsing comma-separated numeric series.
    /// </summary>
    public class SparklineCellEditor : TextCellEditor
    {
        private readonly SparklineCellType _cellType;

        public SparklineCellEditor(SparklineCellType cellType)
        {
            _cellType = cellType;
        }

        public override void StartEdit(IEditorContext context)
        {
            base.StartEdit(context);

            if (context.Trigger != EditTrigger.DirectTyping && string.IsNullOrEmpty(context.Formula))
            {
                var points = _cellType?.ParseDataPoints(context.Value);
                if (points != null && points.Count > 0)
                {
                    Text = string.Join(", ", points.Select(p => p.ToString(CultureInfo.InvariantCulture)));
                    CaretIndex = Text.Length;
                }
            }
        }

        public override object GetValue()
        {
            if (_cellType != null && !string.IsNullOrWhiteSpace(Text))
            {
                var points = _cellType.ParseDataPoints(Text);
                if (points != null)
                {
                    var arr = new double[points.Count];
                    for (int i = 0; i < points.Count; i++)
                    {
                        arr[i] = points[i];
                    }
                    return arr;
                }
            }
            return base.GetValue();
        }
    }
}
