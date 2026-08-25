using DevBrewLabs.WPF.Spreadsheet.Elements;
using System.Collections.Generic;
using System.Windows;

namespace DevBrewLabs.WPF.Spreadsheet.CellTypes
{
    /// <summary>
    /// Abstract base class for cell types that support interactive spinner controls (Up and Down buttons).
    /// </summary>
    public abstract class SpinnerCellType : BaseCellType
    {
        private static readonly CellElement[] _spinnerElements = new CellElement[]
        {
            SpinnerButton.Up,
            SpinnerButton.Down
        };

        /// <summary>
        /// Gets or sets whether interactive spin up and spin down buttons are displayed on the cell.
        /// </summary>
        public bool ShowSpinners { get; set; } = false;

        /// <summary>
        /// Gets the width of the spinner buttons in device-independent units (before zoom scaling).
        /// </summary>
        public virtual double SpinnerWidth => 16.0;

        /// <inheritdoc/>
        public override IEnumerable<CellElement> GetElements(ISheetView view, int row, int col)
        {
            if (ShowSpinners)
            {
                return _spinnerElements;
            }

            return base.GetElements(view, row, col);
        }

        /// <inheritdoc/>
        public override Rect GetContentRect(ISheetView view, int row, int col, Rect cellRect, double zoom)
        {
            var rect = base.GetContentRect(view, row, col, cellRect, zoom);
            if (ShowSpinners)
            {
                double width = SpinnerWidth * zoom;
                return new Rect(rect.X, rect.Y, System.Math.Max(0, rect.Width - width), rect.Height);
            }

            return rect;
        }

        /// <inheritdoc/>
        public override void OnElementClick(ISheetView view, int row, int col, CellElement element)
        {
            base.OnElementClick(view, row, col, element);

            if (element is SpinnerButton spinner)
            {
                if (view?.Spread?.EditingManager?.IsEditing == true)
                {
                    view.Spread.EditingManager.EndEdit(true);
                }

                OnSpin(view, row, col, spinner.Direction);
            }
        }

        /// <summary>
        /// Handles the spin action when a spinner button is clicked.
        /// </summary>
        /// <param name="view">The active sheet view.</param>
        /// <param name="row">The cell row index.</param>
        /// <param name="col">The cell column index.</param>
        /// <param name="direction">The spin direction (Up or Down).</param>
        public abstract void OnSpin(ISheetView view, int row, int col, SpinDirection direction);
    }
}
