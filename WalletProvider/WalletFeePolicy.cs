using System;
using NBitcoin;
using WalletProvider.Entities;
using WalletProvider.Interfaces;

namespace WalletProvider
{
    public class WalletFeePolicy : IWalletFeePolicy
    {
        /// <summary>Maximum transaction fee.</summary>
        private readonly Money maxTxFee;

        /// <summary>
        ///  Fees smaller than this (in satoshi) are considered zero fee (for transaction creation)
        ///  Override with -mintxfee
        /// </summary>
        private readonly FeeRate minTxFee;

        /// <summary>
        ///  If fee estimation does not have enough data to provide estimates, use this fee instead.
        ///  Has no effect if not using fee estimation
        ///  Override with -fallbackfee
        /// </summary>
        private readonly FeeRate fallbackFee;

        /// <summary>
        /// Transaction fee set by the user
        /// </summary>
        private FeeRate payTxFee;

        /// <summary>
        /// Min Relay Tx Fee
        /// </summary>
        private readonly FeeRate minRelayTxFee;

        /// <summary>
        /// Constructs a wallet fee policy.
        /// </summary>
        /// <param name="nodeSettings">Settings for the the node.</param>
        public WalletFeePolicy(Network network)
        {
            this.minTxFee = new FeeRate(network.MinTxFee);
            this.fallbackFee = new FeeRate(network.FallbackFee);
            this.payTxFee = new FeeRate(0);
            this.maxTxFee = new Money(0.1M, MoneyUnit.XRC);
            this.minRelayTxFee = new FeeRate(network.MinRelayTxFee);
        }

        /// <inheritdoc />
        public void Start()
        {
            return;
        }

        /// <inheritdoc />
        public void Stop()
        {
            return;
        }

        /// <inheritdoc />
        public Money GetRequiredFee(int txBytes)
        {
            return Math.Max(this.minTxFee.GetFee(txBytes), this.minRelayTxFee.GetFee(txBytes));
        }

        /// <inheritdoc />
        public Money GetMinimumFee(int txBytes, int confirmTarget)
        {
            // payTxFee is the user-set global for desired feerate
            return this.GetMinimumFee(txBytes, confirmTarget, this.payTxFee.GetFee(txBytes));
        }

        /// <inheritdoc />
        public Money GetMinimumFee(int txBytes, int confirmTarget, Money targetFee)
        {
            Money nFeeNeeded = targetFee;
            // User didn't set: use -txconfirmtarget to estimate...
            if (nFeeNeeded == 0)
            {
                int estimateFoundTarget = confirmTarget;

                //TODO: this.blockPolicyEstimator.EstimateSmartFee?
                // ... unless we don't have enough mempool data for estimatefee, then use fallbackFee
                if (nFeeNeeded == 0)
                    nFeeNeeded = this.fallbackFee.GetFee(txBytes);
            }
            // prevent user from paying a fee below minRelayTxFee or minTxFee
            nFeeNeeded = Math.Max(nFeeNeeded, this.GetRequiredFee(txBytes));
            // But always obey the maximum
            if (nFeeNeeded > this.maxTxFee)
                nFeeNeeded = this.maxTxFee;
            return nFeeNeeded;
        }

        /// <inheritdoc />
        public FeeRate GetFeeRate(FeeType feeType)
        {
            //TODO: this.blockPolicyEstimator.EstimateSmartFee?
            switch (feeType)
            {
                case FeeType.VeryLow:
                    return new FeeRate(Money.Satoshis(10000));

                case FeeType.Low:
                    return new FeeRate(Money.Satoshis(20000));

                case FeeType.High:
                    return new FeeRate(Money.Satoshis(50000));

                case FeeType.VeryHigh:
                    return new FeeRate(Money.Satoshis(100000));

                default:
                    return new FeeRate(Money.Satoshis(30000));
            }
        }

        /// <inheritdoc />
        public void SetPayTxFee(Money feePerK)
        {
            this.payTxFee = new FeeRate(feePerK);
        }

        /// <inheritdoc />
        public FeeRate GetPayTxFee()
        {
            return this.payTxFee;
        }
    }
}
