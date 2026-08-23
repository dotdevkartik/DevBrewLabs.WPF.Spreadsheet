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

  <p align="center">
    A modern, modular, and blazing-fast Excel-like spreadsheet component for WPF applications.
    Built for <b>.NET 10.0</b> performance while maintaining <b>.NET Framework 4.7.2</b> compatibility.
  </p>
</div>

---

## 🌟 Why DevBrewLabs.Spreadsheet?

Building complex data grids and spreadsheet-like interfaces in WPF can be challenging and often leads to performance bottlenecks. **DevBrewLabs.Spreadsheet** solves this by combining a platform-agnostic core data engine with a highly virtualized WPF UI, delivering smooth navigation even with millions of cells.

Complete with a multi-sheet calculation engine (`DevBrewLabs.Spreadsheet.CalcEngine`) and an Excel-inspired Material 3 aesthetic, it provides everything you need to build powerful data-driven applications.

## ✨ Key Features

- 🚀 **Blazing Fast Performance**: UI virtualization ensures buttery-smooth scrolling and navigation across datasets of 1,000,000+ rows.
- 🧮 **Advanced Calculation Engine**: Robust cross-worksheet formula dependencies with a real-time recalculation engine.
- 🎨 **Modern Material 3 Aesthetic**: Beautifully designed with an Excel Green (`#107C41`) accent, light-slate surface palette, and customizable grid elements.
- 🔄 **Seamless Data Binding**: Natively bind to your POCO collections (`List<T>`) and ADO.NET `DataTable` objects with robust two-way synchronization.
- 🛠️ **Rich Cell Rendering**: Includes built-in renderers for Checkboxes, Buttons, ComboBoxes, Hyperlinks, and Text.
- 🔀 **Multi-Targeted Architecture**: Harness the power of **.NET 10** optimizations without leaving legacy **.NET 4.7.2** applications behind.
- ⚙️ **Flexible Configuration**: Choose from Item, Pixel, and Deferred scroll modes, plus multi-column sorting capabilities.

## ⚡ Unrivaled Performance

We optimized the core data engine and WPF renderer for sub-second data loading and instant UI responsiveness.

| Dataset Size | Data Volume | Load Time | Experience |
| :--- | :--- | :--- | :--- |
| **100,000 Rows** | 1,000,000 Cells | **50 - 60 ms** | ⚡ Instantaneous |
| **500,000 Rows** | 5,000,000 Cells | **150 - 200 ms** | ⚡ Ultra Fast |
| **1,000,000 Rows** | 10,000,000 Cells | **300 - 400 ms** | 🚀 Enterprise Scale |

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

        <!-- Optional: Formula Bar linked to the Spread control -->
        <sheets:FormulaTextBox Margin="8" Spread="{Binding ElementName=SpreadControl}"/>

        <!-- Main Spreadsheet Control -->
        <sheets:Spread x:Name="SpreadControl" Grid.Row="1"/>
    </Grid>
</Window>
```

### 2. Bind Your Data
Easily connect your business objects to the active worksheet.

```csharp
using DevBrewLabs.Spreadsheet.Data;

// Get your data
var customers = GetCustomerList();
var worksheet = SpreadControl.WorkBook.WorkSheets.GetSheet(0);

// Bind and map columns
worksheet.DataSource = customers;
worksheet.Columns[0].DataMap = new PropertyDataMap("Id");
worksheet.Columns[1].DataMap = new PropertyDataMap("FirstName");
worksheet.Columns[2].DataMap = new PropertyDataMap("LastName");
worksheet.Columns[3].DataMap = new PropertyDataMap("Email");
```

## 🎮 Explore the Samples

The repository includes a comprehensive **Samples Explorer** app (`SpreadsheetSampleExplorer.csproj`). Run it to see the engine in action:

- **📈 Real-Time Data**: Live stock market feed simulation with automatic recalculations.
- **🧮 Multi-Sheet Formulas**: Cross-sheet dependencies evaluating in real-time.
- **🎨 Theme Gallery**: Switch between Slate, Excel Classic, Emerald, Indigo, and Corporate themes.
- **⚡ Performance Benchmarks**: Test 1M+ row datasets directly on your machine.
- **📝 Binding & Editors**: See custom cell renderers and POCO/DataTable bindings live.

## 🤝 Contributing

We welcome contributions from the community! Whether it's a bug report, a new feature suggestion, or a pull request, your help is appreciated. 

1. Fork the repository
2. Create your feature branch (`git checkout -b feature/AmazingFeature`)
3. Commit your changes (`git commit -m 'Add some AmazingFeature'`)
4. Push to the branch (`git push origin feature/AmazingFeature`)
5. Open a Pull Request

## 📄 License

Distributed under the MIT License. See `LICENSE` for more information.
