namespace DevBrewLabs.Spreadsheet
{
    public class TopLeft : ITopLeft
    {
        public IWorkSheet WorkSheet { get; }
        public string StyleName { get; set; }
        public IStyle Style { get; set; }

        internal TopLeft(IWorkSheet workSheet)
        {
            WorkSheet = workSheet;
        }
    }
}
