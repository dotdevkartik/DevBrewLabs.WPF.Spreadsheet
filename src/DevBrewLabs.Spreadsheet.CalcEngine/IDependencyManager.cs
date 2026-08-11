using DevBrewLabs.Spreadsheet.CalcEngine.Parsers;
using System.Collections.Generic;

namespace DevBrewLabs.Spreadsheet.CalcEngine
{
    /// <summary>
    /// Manages cell dependencies to determine calculation order and trigger recalculations.
    /// </summary>
    internal interface IDependencyManager
    {
        /// <summary>
        /// Adds a dependency indicating that the dependent cell relies on the target cell's value.
        /// </summary>
        /// <param name="dependentCell">The cell that contains the formula.</param>
        /// <param name="targetCell">The cell that the formula references.</param>
        void SetCellDependency(CellRef dependentCell, CellRef targetCell);

        /// <summary>
        /// Adds a dependency indicating that the dependent cell relies on a range of cells.
        /// </summary>
        /// <param name="dependentCell">The cell that contains the formula.</param>
        /// <param name="targetRange">The range of cells that the formula references.</param>
        void SetRangeDependency(CellRef dependentCell, CellRangeRef targetRange);

        /// <summary>
        /// Retrieves a flat list of all cells that depend on the specified cell, traversing the entire dependency chain.
        /// </summary>
        /// <param name="sheetName">The name of the sheet containing the target cell.</param>
        /// <param name="row">The row index of the target cell.</param>
        /// <param name="column">The column index of the target cell.</param>
        /// <returns>A list of all dependent cells.</returns>
        IList<CellRef> GetDependentCells(string sheetName, int row, int column);

        /// <summary>
        /// Clears all dependencies (both single cell and range) where the given cell is the dependent.
        /// Call this before assigning a new formula to a cell to prevent stale dependency leaks.
        /// </summary>
        /// <param name="dependentCell">The cell whose dependencies should be cleared.</param>
        void ClearDependencies(CellRef dependentCell);
    }
}
