namespace MsgExtMultiParamCSharp.Models
{
    public class StockData
    {
        public string CompanyName { get; set; }
        public string PrimaryExchange { get; set; }
        public string Symbol { get; set; }
        public long LatestUpdate { get; set; }
        public double Change { get; set; }
        public double ChangePercent { get; set; }
        public double LatestPrice { get; set; }
        public double Open { get; set; }
        public double High { get; set; }
        public double Low { get; set; }
    }
}
