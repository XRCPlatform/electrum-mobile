using NBitcoin;
using WalletProvider.Entities;

namespace WalletProvider.Interfaces
{
    public interface IWalletFeePolicy
    {
        void Start();

        void Stop();

        Money GetRequiredFee(int txBytes);

        Money GetMinimumFee(int txBytes, int confirmTarget);

        Money GetMinimumFee(int txBytes, int confirmTarget, Money targetFee);

        FeeRate GetFeeRate(FeeType feeType);

        void SetPayTxFee(Money feePerK);

        FeeRate GetPayTxFee();
    }
}
