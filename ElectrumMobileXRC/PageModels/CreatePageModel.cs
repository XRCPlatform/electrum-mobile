using FreshMvvm;
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Input;
using Xamarin.Forms;

namespace ElectrumMobileXRC.PageModels
{
    public class CreatePageModel : FreshBasePageModel
    {
        public ICommand CreateWalletCommand { get; set; }
        public CreatePageModel()
        {
            CreateWalletCommand = new Command(async () =>
            {
                double riskPerc, capSize, entryPrice, stopPrice, targetPrice;

                if (double.TryParse(RiskPercentage, out riskPerc) &&
                    double.TryParse(CapitalSize, out capSize) &&
                    double.TryParse(EntryPrice, out entryPrice) &&
                    double.TryParse(StopPrice, out stopPrice) &&
                    double.TryParse(TargetPrice, out targetPrice))
                {
                    var model = new InputModel
                    {
                        RiskPercentage = riskPerc,
                        CapitalSize = capSize,
                        EntryPrice = entryPrice,
                        StopPrice = stopPrice,
                        TargetPrice = targetPrice
                    };

                    await CoreMethods.PushPageModel<ResultPageModel>(model);
                }
            });
        }
    }
}
