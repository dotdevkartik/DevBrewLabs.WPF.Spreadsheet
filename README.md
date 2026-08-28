<div align="center">
  <img src="src/docs/spread_preview.png" alt="DevBrewLabs.Spreadsheet Preview" width="100%" />

  <br />
  <br />

  <h1>DevBrewLabs.Spreadsheet</h1>
  <p><b>High-Performance WPF Spreadsheet & Calculation Engine</b></p>
  
  [![Build Status](https://img.shields.io/badge/build-passing-brightgreen.svg?style=for-the-badge)](https://github.com/kartikdeepsagar/DevBrewLabs.WPF.Spreadsheet)
  [![Platform](https://img.shields.io/badge/platform-WPF-blue.svg?style=for-the-badge)](https://dotnet.microsoft.com/)
  [![Target Framework](https://img.shields.io/badge/.NET_10.0_%7C_4.7.2_%7C_Standard_2.0-512BD4.svg?style=for-the-badge&logo=dotnet)](https://dotnet.microsoft.com/)
  [![License](https://img.shields.io/badge/license-MIT-green.svg?style=for-the-badge)](LICENSE)

  > [!NOTE]
  > **STATUS: ACTIVE DEVELOPMENT** 🚧  
  > This component is currently under active development. We are iterating fast, focusing on stabilizing existing features and improving performance before a stable v1.0 release. You might encounter bugs - feel free to contribute!
</div>

---

## 🌟 Why DevBrewLabs.Spreadsheet?

Building complex data grids and spreadsheet-like interfaces in WPF can be challenging and often leads to performance bottlenecks. **DevBrewLabs.Spreadsheet** solves this by combining a platform-agnostic core data engine with a highly virtualized WPF UI, delivering smooth navigation even with millions of cells.

Complete with a multi-sheet calculation engine (`DevBrewLabs.Spreadsheet.CalcEngine`) and a modern Material 3 aesthetic, it provides everything you need to build powerful data-driven applications.

## ✨ Features Showcase

All features shown here are actively demonstrated in the `SpreadsheetSampleExplorer` project. Run the samples to see them in action!

### 🟢 Essentials
- **Testing (Formula Bar & Editor)**: Interactive formula bar linked with spreadsheet cell selection.
  ```xml
  <sheets:FormulaTextBox Margin="8" Spread="{Binding ElementName=SpreadControl}"/>
  ```
- **Cell Types**: Demonstration of Checkbox, Button, ComboBox, and Text cell types.
  ```csharp
  sheet.Cells[0, 0].CellType = new CheckBoxCellType { IsThreeState = true };
  sheet.Cells[1, 0].CellType = new ButtonCellType { Text = "Execute" };
  ```
- **Performance**: Benchmark dataset load times, rendering speed, and compare scroll modes.
  ```csharp
  spread.SuspendUpdates = true;
  // Load 1,000,000 rows quickly without triggering layout passes
  spread.SuspendUpdates = false;
  ```
- **Data Sorting Engine**: Fast ascending and descending multi-range data sorting.
  ```csharp
  var options = new SortOptions();
  options.SortLevels.Add(new SortInfo(colIndex: 1, ascending: true));
  sheet.Sort(new CellRange(0, 0, 100, 5), options);
  ```

### 📊 Data & Calculations
- **Multi-Sheet Formulas**: Cross-worksheet formula engine with real-time dependency recalculation.
  ```csharp
  sheet.Cells[0, 0].Formula = "SUM(Sheet2!B4:E4)";
  ```
- **List & DataTable Binding**: Automatic two-way binding to custom C# object lists and DataTables.
  ```csharp
  worksheet.DataSource = customers; // Binds List<T> or DataTable
  ```
- **Excel-like AutoFilter**: Filter down spreadsheet data quickly using dynamic header dropdowns.
  ```csharp
  sheet.AutoFilter.SetRange(new CellRange(0, 0, 500, 5));
  ```
- **Portfolio Dashboard**: Industry-grade dashboard with custom formatters, spanning, and live streams.
  ```csharp
  // Efficient real-time updates for high-frequency streaming
  sheet.Cells[1, 1].Value = liveTicker.CurrentPrice; 
  ```

### 🎨 Appearance & Styling
- **Cell Spanning**: Merge and unmerge cells, spanning multiple rows and columns.
  ```csharp
  // Spans 2 rows and 7 columns starting at cell (1,1)
  worksheet.AddSpan(startRow: 1, startColumn: 1, rowCount: 2, columnCount: 7); 
  ```
- **Grid Styling & Themes**: Custom cell background colors, borders, fonts, and gridlines.
  ```csharp
  sheet.Cells[0, 0].BackColor = Colors.LightSlateGray;
  sheet.Cells[0, 0].ForeColor = Colors.White;
  ```
- **Worksheet Zooming**: Zoom in and out on active worksheet using slider, presets, or Ctrl+MouseWheel.
  ```csharp
  spread.ZoomFactor = 1.5; // Zoom to 150%
  ```
- **Spread Properties**: Configure and toggle various spread properties such as scroll mode, gridlines, and resize behaviors.
  ```csharp
  spread.ScrollMode = ScrollMode.Item; // or ScrollMode.Pixel
  spread.ShowGridLines = false;
  ```

## ⚡ Unrivaled Performance

We optimized the core data engine and WPF renderer for sub-second data loading and instant UI responsiveness.

| Dataset Size | Data Volume | Load Time | Experience |
| :--- | :--- | :--- | :--- |
| **100,000 Rows** | 1,000,000 Cells | **50 - 60 ms** | ⚡ Instantaneous |
| **500,000 Rows** | 5,000,000 Cells | **150 - 200 ms** | ⚡ Ultra Fast |
| **1,000,000 Rows** | 10,000,000 Cells | **300 - 400 ms** | 🚀 Unmatched Scale |

*(Benchmarks measure engine loading and first UI frame render on standard hardware.)*

## 🏗️ Architecture

Built with a strict separation of concerns to maximize reusability across different application layers:

| Package / Assembly | Target Frameworks | Role |
| :--- | :--- | :--- |
| 📦 **DevBrewLabs.Spreadsheet** | `netstandard2.0;net10.0` | Platform Agnostic Core Data Engine |
| 🧮 **DevBrewLabs.Spreadsheet.CalcEngine** | `netstandard2.0;net10.0` | Formula Parser & Evaluation Engine |
| 🎨 **DevBrewLabs.WPF.Spreadsheet** | `net472;net10.0-windows` | Modern WPF UI Control (`Spread`) |
| 📱 **Samples Explorer** | `net472;net10.0-windows` | Interactive Showcase & Benchmark App |

## 🚀 Quick Start

### 1. Add to XAML
Import the namespace and drop the `Spread` control into your WPF Window.

```xml
<Window x:Class="SpreadDemo.MainWindow"
        xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
        xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
        xmlns:sheets="http://schemas.devbrewlabs.com/2026/wpf/spreadsheet"
        Title="Spreadsheet Demo" Height="600" Width="900">
    <Grid>
        <sheets:Spread x:Name="SpreadControl" />
    </Grid>
</Window>
```

### 2. Bind Your Data
Easily connect your business objects to the active worksheet.

```csharp
using DevBrewLabs.Spreadsheet.Data;

var customers = GetCustomerList();
var worksheet = SpreadControl.WorkBook.WorkSheets.GetSheet(0);

worksheet.DataSource = customers;
worksheet.Columns[0].DataMap = new PropertyDataMap("FirstName");
```

## 🛣️ Future Roadmap

- Stabilizing existing features.
- Adding Excel I/O

## 🤝 Contributing

We welcome contributions from the community! Whether it's a bug report, a new feature suggestion, or a pull request, your help is appreciated. 

## 📄 License

Distributed under the MIT License. See `LICENSE` for more information.
