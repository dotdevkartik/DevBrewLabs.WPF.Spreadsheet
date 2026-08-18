namespace SpreadsheetSampleExplorer.Models
{
    public class StockData
    {
        public string Ticker { get; set; }
        public string Company { get; set; }
        public int Shares { get; set; }
        public double BasePrice { get; set; }
        public double CurrentPrice { get; set; }
    }
}
