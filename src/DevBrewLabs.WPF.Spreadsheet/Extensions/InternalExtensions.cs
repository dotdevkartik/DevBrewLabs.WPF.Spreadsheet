using DevBrewLabs.Spreadsheet;
using DevBrewLabs.Spreadsheet.Formatters;
using DevBrewLabs.WPF.Spreadsheet.UI.Interaction;
using System.Windows;

namespace DevBrewLabs.WPF.Spreadsheet
{
    internal static class InternalExtensions
    {
        public static T As<T>(this object obj)
        {
            return (T)obj;
        }

        internal static bool ContainsOrIntersectsWith(this Rect source, Rect rect)
        {
            return source.Contains(rect) || source.IntersectsWith(rect);
        }

        internal static void EnsureFree(this InteractionLayer layer)
        {
            if (!layer.IsAttached)
                return;

            layer.DetachFromRegion();
            layer.ReleaseMouseCapture();
            layer.InvalidateVisual();
        }

        /// <summary>
        /// Gets the style applied on cell.
        /// </summary>
        /// <param name="workSheet"></param>
        /// <param name="rowIndex"></param>
        /// <param name="columnIndex"></param>
        /// <param name="row"></param>
        /// <param name="column"></param>
        /// <returns></returns>
        public static IStyle GetCellStyle(this IWorkSheet workSheet, int rowIndex, int columnIndex, IRow row, IColumn column)
        {
            IStyle style = workSheet.GetStyle(rowIndex, columnIndex);

            if (style != null)
            {
                return style;
            }

            var styleName = workSheet.GetStyleName(rowIndex, columnIndex);

            if (!string.IsNullOrEmpty(styleName))
            {
                return workSheet.WorkBook.GetNamedStyle(styleName);
            }

            if(column?.Style != null)
            {
                return column.Style;
            }

            if (!string.IsNullOrEmpty(column?.StyleName))
            {
                return workSheet.WorkBook.GetNamedStyle(column.StyleName);
            }

            if (row?.Style != null)
            {
                return row.Style;
            }

            if (!string.IsNullOrEmpty(row?.StyleName))
            {
                return workSheet.WorkBook.GetNamedStyle(row.StyleName);
            }

            return workSheet.WorkBook.GetNamedStyle(StyleKeys.DefaultSheetStyleKey);
        }

        /// <summary>
        /// Gets the style applied on column header cell.
        /// </summary>
        /// <param name="workSheet"></param>
        /// <param name="rowIndex"></param>
        /// <param name="columnIndex"></param>
        /// <param name="row"></param>
        /// <param name="column"></param>
        /// <returns></returns>
        public static IStyle GetColumnHeaderCellStyle(this IWorkSheet workSheet, int rowIndex, int columnIndex, IRow row, IColumn column)
        {
            IStyle style = workSheet.ColumnHeaders.GetStyle(rowIndex, columnIndex);

            if (style != null)
            {
                return style;
            }

            var styleName = workSheet.ColumnHeaders.GetStyleName(rowIndex, columnIndex);

            if (!string.IsNullOrEmpty(styleName))
            {
                return workSheet.WorkBook.GetNamedStyle(styleName);
            }

            if(column?.Style != null)
            {
                return column.Style;
            }

            if (!string.IsNullOrEmpty(column?.StyleName))
            {
                return workSheet.WorkBook.GetNamedStyle(column.StyleName);
            }

            if (row?.Style != null)
            {
                return row.Style;
            }

            if (!string.IsNullOrEmpty(row?.StyleName))
            {
                return workSheet.WorkBook.GetNamedStyle(row.StyleName);
            }

            return workSheet.WorkBook.GetNamedStyle(StyleKeys.DefaultColumnHeaderStyleKey);
        }

        /// <summary>
        /// Gets the style applied on row header cell.
        /// </summary>
        /// <param name="workSheet"></param>
        /// <param name="rowIndex"></param>
        /// <param name="columnIndex"></param>
        /// <param name="row"></param>
        /// <param name="column"></param>
        /// <returns></returns>
        public static IStyle GetRowHeaderCellStyle(this IWorkSheet workSheet, int rowIndex, int columnIndex, IRow row, IColumn column)
        {
            IStyle style = workSheet.RowHeaders.GetStyle(rowIndex, columnIndex);

            if (style != null)
            {
                return style;
            }

            var styleName = workSheet.RowHeaders.GetStyleName(rowIndex, columnIndex);

            if (!string.IsNullOrEmpty(styleName))
            {
                return workSheet.WorkBook.GetNamedStyle(styleName);
            }

            if(column?.Style != null)
            {
                return column.Style;
            }

            if (!string.IsNullOrEmpty(column?.StyleName))
            {
                return workSheet.WorkBook.GetNamedStyle(column.StyleName);
            }

            if(row?.Style != null)
            {
                return row.Style;
            }

            if (!string.IsNullOrEmpty(row?.StyleName))
            {
                return workSheet.WorkBook.GetNamedStyle(row.StyleName);
            }

            return workSheet.WorkBook.GetNamedStyle(StyleKeys.DefaultRowHeaderStyleKey);
        }

        public static IStyle GetTopLeftStyle(this IWorkSheet sheet)
        {
            if (sheet.TopLeft.Style != null)
            {
                return sheet.TopLeft.Style;
            }

            if (!string.IsNullOrEmpty(sheet.TopLeft.StyleName))
            {
                return sheet.WorkBook.GetNamedStyle(sheet.TopLeft.StyleName);
            }

            return sheet.WorkBook.GetNamedStyle(StyleKeys.DefaultTopLeftStyleKey);
        }

        /// <summary>
        /// Gets the formatter applied on cell.
        /// </summary>
        /// <param name="sheet"></param>
        /// <param name="rowIndex"></param>
        /// <param name="columnIndex"></param>
        /// <param name="row"></param>
        /// <param name="column"></param>
        /// <returns></returns>
        public static IFormatter GetCellFormatter(this IWorkSheet sheet, int rowIndex, int columnIndex, IRow row, IColumn column)
        {
            IFormatter formatter = sheet.GetFormatter(rowIndex, columnIndex);

            if (formatter != null)
            {
                return formatter;
            }

            if (column?.Formatter != null)
            {
                return column.Formatter;
            }

            if (row?.Formatter != null)
            {
                return row.Formatter;
            }

            return GeneralFormatter.Default;
        }
    }
}
