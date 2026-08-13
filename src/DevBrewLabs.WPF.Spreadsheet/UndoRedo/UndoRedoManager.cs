using System.Collections.Generic;

namespace DevBrewLabs.WPF.Spreadsheet
{
    public class UndoRedoManager : IUndoRedoManager
    {
        private readonly LinkedList<SheetAction> _undoStack;
        private readonly LinkedList<SheetAction> _redoStack;
        private readonly Spread _spread;

        private int _groupDepth = 0;
        private CompositeSheetAction _currentGroup;

        public int MaxCapacity { get; set; } = 100;

        public UndoRedoManager(Spread spread)
        {
            _spread = spread;
            _undoStack = new LinkedList<SheetAction>();
            _redoStack = new LinkedList<SheetAction>();
        }

        public void BeginGroup()
        {
            if (_groupDepth == 0)
            {
                _currentGroup = new CompositeSheetAction();
            }
            _groupDepth++;
        }

        public void EndGroup()
        {
            if (_groupDepth > 0)
            {
                _groupDepth--;
                if (_groupDepth == 0 && _currentGroup != null)
                {
                    PushToUndo(_currentGroup);
                    _currentGroup = null;
                }
            }
        }

        public void AddAction(SheetAction action)
        {
            if (_groupDepth > 0 && _currentGroup != null)
            {
                _currentGroup.AddAction(action);
            }
            else
            {
                PushToUndo(action);
            }
        }

        private void PushToUndo(SheetAction action)
        {
            _undoStack.AddLast(action);

            if (_undoStack.Count > MaxCapacity)
            {
                _undoStack.RemoveFirst();
            }

            if (_redoStack.Count > 0)
            {
                _redoStack.Clear();
            }
        }

        public void Redo()
        {
            if (_redoStack.Count > 0)
            {
                var action = _redoStack.Last.Value;
                _redoStack.RemoveLast();
                
                action.Redo();
                
                _undoStack.AddLast(action);
                if (_undoStack.Count > MaxCapacity)
                {
                    _undoStack.RemoveFirst();
                }
            }
        }

        public void Undo()
        {
            if (_undoStack.Count > 0)
            {
                var action = _undoStack.Last.Value;
                _undoStack.RemoveLast();
                
                action.Undo();
                
                _redoStack.AddLast(action);
                if (_redoStack.Count > MaxCapacity)
                {
                    _redoStack.RemoveFirst();
                }
            }
        }
    }
}
