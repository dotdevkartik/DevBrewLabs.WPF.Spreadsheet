using DevBrewLabs.Spreadsheet.CalcEngine;
using DevBrewLabs.Spreadsheet.Core;
using System;

namespace DevBrewLabs.Spreadsheet
{
    public interface IWorkbook : IDisposable, IStyleProvider
    {
        /// <summary>
        /// Gets or sets the name for this workbook.
        /// </summary>
        string Name { get; set; }
        /// <summary>
        /// Gets the sheet collection of this workbook.
        /// </summary>
        IWorksheets WorkSheets { get; }
        /// <summary>
        /// Gets the calculation engine of this worksheet.
        /// </summary>
        ICalcEngine CalcEngine { get; }
        /// <summary>
        /// Gets the style palette of this workbook.
        /// </summary>
        IStylePalette StylePalette { get; }
    }
}