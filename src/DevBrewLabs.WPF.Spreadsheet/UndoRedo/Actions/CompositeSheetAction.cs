using System.Collections.Generic;
using System.Linq;

namespace DevBrewLabs.WPF.Spreadsheet
{
    public class CompositeSheetAction : SheetAction
    {
        private readonly List<SheetAction> _actions;

        public CompositeSheetAction()
        {
            _actions = new List<SheetAction>();
        }

        public void AddAction(SheetAction action)
        {
            _actions.Add(action);
        }

        public override void Redo()
        {
            foreach (var action in _actions)
            {
                action.Redo();
            }
        }

        public override void Undo()
        {
            // Undo must happen in reverse order
            for (int i = _actions.Count - 1; i >= 0; i--)
            {
                _actions[i].Undo();
            }
        }
    }
}
