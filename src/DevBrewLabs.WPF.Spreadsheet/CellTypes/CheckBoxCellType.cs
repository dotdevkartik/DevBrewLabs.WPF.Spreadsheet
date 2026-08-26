using DevBrewLabs.Spreadsheet;
using DevBrewLabs.Spreadsheet.Drawing;
using DevBrewLabs.Spreadsheet.Formatters;
using DevBrewLabs.WPF.Spreadsheet.Rendering;
using DevBrewLabs.WPF.Spreadsheet.UI.Editors;
using System;
using System.Windows;
using System.Windows.Media;

namespace DevBrewLabs.WPF.Spreadsheet.CellTypes
{
    public class CheckBoxCellType : BaseCellType
    {
        internal static Size CheckBoxSize { get; }

        static CheckBoxCellType()
        {
            CheckBoxSize = new Size(11, 11);
        }

        private DrawingPen _pen;
        private DrawingPen _markPen;

        public bool IsThreeState { get; set; }

        public CheckBoxCellType()
        {
            _pen = new DrawingPen(DrawingColor.Black, 0.75);
            _markPen = new DrawingPen(DrawingColor.Black, 1.5);
            IsThreeState = false;
        }

        public override void DrawCell(IRenderContext renderContext, object value, IStyle style, IFormatter formatter, Rect cellRect)
        {
            var scaledCheckBoxSize = new Size(CheckBoxSize.Width * renderContext.Zoom, CheckBoxSize.Height * renderContext.Zoom);
            var checkBoxRect = cellRect.ToCellCheckBoxRect(scaledCheckBoxSize);
            var halfPenWidth = _pen.Thickness / 2;
            GuidelineSet guidelines = new GuidelineSet();
            guidelines.GuidelinesX.Add(checkBoxRect.Left + halfPenWidth);
            guidelines.GuidelinesX.Add(checkBoxRect.Right + halfPenWidth);
            guidelines.GuidelinesY.Add(checkBoxRect.Top + halfPenWidth);
            guidelines.GuidelinesY.Add(checkBoxRect.Bottom + halfPenWidth);
            renderContext.PushGuidelineSet(guidelines);

            base.DrawCell(renderContext, value, style, formatter, cellRect);

            renderContext.DrawRectangle(null, _pen, checkBoxRect);
            DrawMark(renderContext, checkBoxRect, value);
            renderContext.Pop();
        }

        /// <summary>
        /// Draws checkbox mark
        /// </summary>
        /// <param name="ctx"></param>
        /// <param name="checkBoxRect"></param>
        /// <param name="value"></param>
        private void DrawMark(IRenderContext renderContext, Rect checkBoxRect, object value)
        {
            if(IsThreeState && value == null)
            {
                checkBoxRect.Inflate(-2, -2);
                renderContext.DrawRectangle(DrawingColor.Black, null, checkBoxRect);
            }
            else if(value != null && Convert.ToBoolean(value))
            {
                var bottom = new Point(checkBoxRect.Left + checkBoxRect.Width / 2, checkBoxRect.Bottom - 1.5);
                renderContext.DrawLine(_markPen, new Point(checkBoxRect.Left + 1.5, checkBoxRect.Top + checkBoxRect.Height / 2),
                    bottom);
                renderContext.DrawLine(_markPen, bottom, new Point(checkBoxRect.Right - 1.5, checkBoxRect.Top + 1.5));
            }
        }

        public override bool SupportsEditing => false;
    }
}

