# DevBrewLabs.Spreadsheet - High-Performance WPF Spreadsheet & Calculation Engine

[![Build Status](https://img.shields.io/badge/build-passing-brightgreen.svg)](https://github.com/kartikdeepsagar/DevBrewLabs.WPF.Spreadsheet)
[![Platform](https://img.shields.io/badge/platform-WPF-blue.svg)](https://dotnet.microsoft.com/)
[![Target Framework](https://img.shields.io/badge/.NET-10.0%20%7C%204.7.2%20%7C%20Standard%202.0-512BD4.svg)](https://dotnet.microsoft.com/)
[![License](https://img.shields.io/badge/license-MIT-green.svg)](LICENSE)

**DevBrewLabs.Spreadsheet** is a modular, high-performance, Excel-like spreadsheet component for WPF applications. It combines a platform-agnostic core spreadsheet data engine, a multi-sheet calculation engine (`DevBrewLabs.Spreadsheet.CalcEngine`), and a modern WPF view control (`Spread`) featuring an Excel-inspired Material 3 aesthetic and multi-target support for **.NET 10.0** and **.NET Framework 4.7.2**.

![Spread Explorer Preview](src/docs/spread_preview.png)

---

## ✨ Features at a Glance

- 🚀 **High Performance Grid**: Virtualized rendering supporting smooth navigation and virtual scrolling across 1,000,000+ data rows.
- ⚡ **Multi-Targeted .NET 10 & .NET Framework**: Built for modern **.NET 10** performance optimizations while preserving legacy **.NET Framework 4.7.2** compatibility.
- 🧮 **Multi-Sheet Calculation Engine**: Cross-worksheet formula dependencies with real-time recalculation engine powered by `DevBrewLabs.Spreadsheet.CalcEngine`.
- 🎨 **Materialist & Modern Theme**: Excel Green (`#107C41`) accent styling, light-slate surface palette, customizable gridlines, headers, and row striping.
- 📊 **Two-Way Data Binding**: Native binding to C# POCO collections (`List<T>`) and ADO.NET `DataTable` objects.
- 🔘 **Rich Custom Cell Renderers**: Built-in renderers for Checkbox, Button, ComboBox, Hyperlink, and Text cells.
- 🔃 **Range Sorting Engine**: Multi-column ascending and descending sorting algorithms.
- 📜 **Configurable Scroll Modes**: Support for **Item**, **Pixel**, and **Deferred** scroll modes.

---

## ⚡ Performance Benchmarks

The core data engine (`DevBrewLabs.Spreadsheet`) and virtualized WPF renderer (`Spread`) are optimized for sub-second data loading and ultra-fast navigation across massive datasets.

| Dataset Size | Data Volume (Cells) | Load Time | Performance Rating |
| :--- | :--- | :--- | :--- |
| **100,000 Rows** | 1,000,000 Cells | **50 - 60 ms** | ⚡ Instantaneous |
| **500,000 Rows** | 5,000,000 Cells | **~250 ms** | ⚡ Ultra Fast |
| **1,000,000 Rows** | 10,000,000 Cells | **500 ms - 700 ms** | 🚀 Sub-Second Virtualized |

> **Note:** Benchmarks measure engine loading and first UI frame render in the **Performance** sample of the `SpreadsheetSampleExplorer` application (excludes data generation time).

---

## 🏗️ Architecture & Multi-Targeting

The project is architected with strict separation of concerns into multi-targeted assemblies:

```
src/
├── DevBrewLabs.Spreadsheet/              # Core data engine (netstandard2.0;net10.0)
├── DevBrewLabs.Spreadsheet.CalcEngine/          # Expression parser & calculation engine (netstandard2.0;net10.0)
├── DevBrewLabs.WPF.Spreadsheet/          # WPF UI control (net472;net10.0-windows)
└── Samples/                    # Modern Samples Explorer application (net472;net10.0-windows)
```

| Assembly | Target Frameworks | Target Audience |
| :--- | :--- | :--- |
| **DevBrewLabs.Spreadsheet** | `netstandard2.0;net10.0` | Platform Agnostic Core Engine |
| **DevBrewLabs.Spreadsheet.CalcEngine** | `netstandard2.0;net10.0` | Formula Evaluation Engine |
| **DevBrewLabs.WPF.Spreadsheet** | `net472;net10.0-windows` | Modern & Legacy WPF Control |
| **Samples Explorer** | `net472;net10.0-windows` | Showcase & Benchmark App |

---

## 🚀 Getting Started

### 1. Adding `Spread` to XAML

```xaml
<Window x:Class="SpreadDemo.MainWindow"
        xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
        xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
        xmlns:sheets="http://schemas.devbrewlabs.com/2026/wpf/spreadsheet"
        Title="Spreadsheet Demo" Height="600" Width="900">
    <Grid>
        <Grid.RowDefinitions>
            <RowDefinition Height="Auto"/>
            <RowDefinition Height="*"/>
        </Grid.RowDefinitions>

        <!-- Formula Bar -->
        <sheets:FormulaTextBox Margin="8" Spread="{Binding ElementName=spreadControl}"/>

        <!-- Main Spreadsheet Control -->
        <sheets:Spread x:Name="spreadControl" Grid.Row="1"/>
    </Grid>
</Window>
```

### 2. Data Binding Example

```csharp
using DevBrewLabs.Spreadsheet.Data;

// Bind a List<Customer> to the active worksheet
var customers = GetCustomerList();
var worksheet = spreadControl.WorkBook.WorkSheets.GetSheet(0);

worksheet.DataSource = customers;
worksheet.Columns[0].DataMap = new PropertyDataMap("Id");
worksheet.Columns[1].DataMap = new PropertyDataMap("FirstName");
worksheet.Columns[2].DataMap = new PropertyDataMap("LastName");
worksheet.Columns[3].DataMap = new PropertyDataMap("Email");
```

---

## 💻 Samples Explorer

Run `SpreadsheetSampleExplorer.csproj` to explore interactive feature demonstrations:

- **Formula Bar & Editor**: Real-time formula editing linked to spreadsheet cell selection.
- **Multi-Sheet Formulas**: Cross-sheet formula evaluation with real-time dependency recalculations.
- **Real-Time Data & Portfolio**: Live stock market feed simulation with automatic formula recalculations and dynamic cell styling.
- **Data Binding**: Compare POCO `List<T>` vs. ADO.NET `DataTable` two-way bindings.
- **Grid Styling & Themes**: Live theme switcher (Slate, Excel Classic Green, Emerald, Indigo, Corporate) and 4-quadrant grid showcase.
- **Performance**: Benchmark dataset load times (100k, 500k, 1M+ rows) and compare Item, Pixel, and Deferred scroll modes.

---

## 🤝 Contributing

Contributions are welcome! Feel free to open issues, submit pull requests, or propose new spreadsheet features and calculation engine capabilities.

## 📄 License

This project is licensed under the [MIT License](LICENSE).
