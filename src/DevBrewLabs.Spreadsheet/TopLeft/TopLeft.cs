namespace DevBrewLabs.Spreadsheet
{
    public class TopLeft : ITopLeft
    {
        public IWorksheet WorkSheet { get; }
        public string StyleName { get; set; }
        public IStyle Style { get; set; }

        internal TopLeft(IWorksheet workSheet)
        {
            WorkSheet = workSheet;
        }
    }
}
