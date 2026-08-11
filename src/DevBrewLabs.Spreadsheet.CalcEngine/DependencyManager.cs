using DevBrewLabs.Spreadsheet.CalcEngine.Parsers;
using System;
using System.Collections.Generic;

namespace DevBrewLabs.Spreadsheet.CalcEngine
{
    /// <summary>
    /// Implementation of <see cref="IDependencyManager"/> that manages cell dependencies 
    /// using an <see cref="IDataAdapter"/> to store and retrieve metadata.
    /// </summary>
    internal class DependencyManager : IDependencyManager
    {
        private readonly IDataAdapter _provider;
        private readonly Dictionary<string, List<(CellRangeRef Range, CellRef Dependent)>> _rangeDependencies;

        /// <summary>
        /// Initializes a new instance of the <see cref="DependencyManager"/> class.
        /// </summary>
        /// <param name="provider">The data provider used to store metadata.</param>
        public DependencyManager(IDataAdapter provider)
        {
            _provider = provider ?? throw new ArgumentNullException(nameof(provider));
            _rangeDependencies = new Dictionary<string, List<(CellRangeRef, CellRef)>>();
        }

        /// <inheritdoc />
        public void SetCellDependency(CellRef dependentCell, CellRef targetCell)
        {
            if (dependentCell == null) throw new ArgumentNullException(nameof(dependentCell));
            if (targetCell == null) throw new ArgumentNullException(nameof(targetCell));

            if (string.IsNullOrEmpty(targetCell.SheetName))
            {
                targetCell = new CellRef(targetCell.Row, targetCell.Column, dependentCell.SheetName);
            }

            var dependentSet = GetDependencySet(targetCell.SheetName, targetCell.Row, targetCell.Column, true);
            dependentSet.Add(dependentCell);
        }

        /// <inheritdoc />
        public void SetRangeDependency(CellRef dependentCell, CellRangeRef targetRange)
        {
            if (dependentCell == null) throw new ArgumentNullException(nameof(dependentCell));
            if (targetRange == null) throw new ArgumentNullException(nameof(targetRange));

            string sheetName = string.IsNullOrEmpty(targetRange.SheetName) ? dependentCell.SheetName : targetRange.SheetName;

            if (!_rangeDependencies.TryGetValue(sheetName, out var rangeList))
            {
                rangeList = new List<(CellRangeRef, CellRef)>();
                _rangeDependencies[sheetName] = rangeList;
            }

            rangeList.Add((targetRange, dependentCell));
        }

        /// <inheritdoc />
        public void ClearDependencies(CellRef dependentCell)
        {
            if (dependentCell == null) throw new ArgumentNullException(nameof(dependentCell));

            if (_provider.GetMetadata(dependentCell.SheetName, dependentCell.Row, dependentCell.Column) is CalcCellMetaInfo metaInfo)
            {
                if (metaInfo.Dependencies != null)
                {
                    foreach (var dep in metaInfo.Dependencies)
                    {
                        if (dep is CellRef cellRef)
                        {
                            string targetSheet = string.IsNullOrEmpty(cellRef.SheetName) ? dependentCell.SheetName : cellRef.SheetName;
                            var targetSet = GetDependencySet(targetSheet, cellRef.Row, cellRef.Column, false);
                            
                            if (targetSet != null)
                            {
                                var toRemove = new List<CellRef>();
                                foreach (var c in targetSet)
                                {
                                    if (c.Row == dependentCell.Row && c.Column == dependentCell.Column && c.SheetName == dependentCell.SheetName)
                                        toRemove.Add(c);
                                }
                                foreach (var c in toRemove) targetSet.Remove(c);
                            }
                        }
                        else if (dep is CellRangeRef rangeRef)
                        {
                            string targetSheet = string.IsNullOrEmpty(rangeRef.SheetName) ? dependentCell.SheetName : rangeRef.SheetName;
                            if (_rangeDependencies.TryGetValue(targetSheet, out var rangeList))
                            {
                                rangeList.RemoveAll(x => 
                                    x.Dependent.Row == dependentCell.Row && 
                                    x.Dependent.Column == dependentCell.Column && 
                                    x.Dependent.SheetName == dependentCell.SheetName &&
                                    x.Range.TopRow == rangeRef.TopRow &&
                                    x.Range.BottomRow == rangeRef.BottomRow &&
                                    x.Range.LeftColumn == rangeRef.LeftColumn &&
                                    x.Range.RightColumn == rangeRef.RightColumn
                                );
                            }
                        }
                    }
                }
            }
        }

        /// <inheritdoc />
        public IList<CellRef> GetDependentCells(string sheetName, int row, int column)
        {
            var dependents = new List<CellRef>();
            var dependentsSetQueue = new Queue<ISet<CellRef>>();
            
            var initialSet = GetDependencySetForCell(sheetName, row, column);
            if (initialSet != null && initialSet.Count > 0)
            {
                dependentsSetQueue.Enqueue(initialSet);
            }

            var visited = new HashSet<string>();

            while (dependentsSetQueue.Count > 0)
            {
                var currentSet = dependentsSetQueue.Dequeue();
                foreach (var dependent in currentSet)
                {
                    string cellKey = $"{dependent.SheetName}!{dependent.Row},{dependent.Column}";
                    if (!visited.Add(cellKey))
                        continue;

                    dependents.Add(dependent);
                    var nestedDependentSet = GetDependencySetForCell(dependent.SheetName, dependent.Row, dependent.Column);
                    if (nestedDependentSet != null && nestedDependentSet.Count > 0)
                    {
                        dependentsSetQueue.Enqueue(nestedDependentSet);
                    }
                }
            }

            return dependents;
        }

        private ISet<CellRef> GetDependencySetForCell(string sheetName, int row, int column)
        {
            var result = new HashSet<CellRef>();

            var exactSet = GetDependencySet(sheetName, row, column, false);
            if (exactSet != null)
            {
                foreach (var dep in exactSet)
                    result.Add(dep);
            }

            if (_rangeDependencies.TryGetValue(sheetName, out var rangeList))
            {
                foreach (var item in rangeList)
                {
                    if (row >= item.Range.TopRow && row <= item.Range.BottomRow &&
                        column >= item.Range.LeftColumn && column <= item.Range.RightColumn)
                    {
                        result.Add(item.Dependent);
                    }
                }
            }

            return result;
        }

        /// <summary>
        /// Retrieves the dependency set for a cell, optionally creating an empty set if it doesn't exist.
        /// </summary>
        private ISet<CellRef> GetDependencySet(string sheetName, int row, int column, bool createEmptySetIfNull)
        {
            if (_provider.GetMetadata(sheetName, row, column) is CalcCellMetaInfo metaInfo)
            {
                if (metaInfo.Dependents == null && createEmptySetIfNull)
                {
                    metaInfo.Dependents = new HashSet<CellRef>();
                }
                return metaInfo.Dependents;
            }

            if (createEmptySetIfNull)
            {
                metaInfo = new CalcCellMetaInfo
                {
                    Dependents = new HashSet<CellRef>()
                };
                _provider.SetMetadata(sheetName, row, column, metaInfo);
                return metaInfo.Dependents;
            }

            return null;
        }
    }
}
