using System;
namespace ElectrumMobileXRC.Models
{
    public class InputModel
    {
        public double RiskPercentage { get; set; }
        public double CapitalSize { get; set; }
        public double TargetPrice { get; set; }
        public double EntryPrice { get; set; }
        public double StopPrice { get; set; }
    }
}
