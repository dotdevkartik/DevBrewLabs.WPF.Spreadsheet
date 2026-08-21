using System;

namespace DevBrewLabs.WPF.Spreadsheet.UI.Managers
{
    internal abstract class UIManager : IDisposable
    {
        protected Spread Spread { get; private set; }

        public UIManager(Spread spread)
        {
            Spread = spread;
        }

        public virtual void Dispose()
        {
            Spread = null;
        }
    }
}
