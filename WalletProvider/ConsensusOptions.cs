using NBitcoin;

namespace WalletProvider.Entities
{
    /// <summary>
    /// A set of options with default values of the Bitcoin network
    /// This can be easily overridable for alternative networks (i.e BRhodium)
    /// Capital style param nameing is kept to mimic core
    /// </summary>
    public class PowConsensusOptions : NBitcoin.Consensus.ConsensusOptions
    {
        /// <summary>The maximum allowed size for a serialized block, in bytes (only for buffer size limits).</summary>
        public int MaxBlockSerializedSize { get; set; }

        /// <summary>The maximum allowed weight for a block, see BIP 141 (network rule)</summary>
        public int MaxBlockWeight { get; set; }

        public int WitnessScaleFactor { get; set; }

        public int SerializeTransactionNoWitness { get; set; }

        /// <summary>
        /// Changing the default transaction version requires a two step process:
        /// <list type="bullet">
        /// <item>Adapting relay policy by bumping <see cref="MaxStandardVersion"/>,</item>
        /// <item>and then later date bumping the default CURRENT_VERSION at which point both CURRENT_VERSION and
        /// <see cref="MaxStandardVersion"/> will be equal.</item>
        /// </list>
        /// </summary>
        public int MaxStandardVersion { get; set; }

        /// <summary>The maximum weight for transactions we're willing to relay/mine.</summary>
        public int MaxStandardTxWeight { get; set; }

        public int MaxBlockBaseSize { get; set; }

        /// <summary>The maximum allowed number of signature check operations in a block (network rule).</summary>
        public int MaxBlockSigopsCost { get; set; }

        public long MaxMoney { get; set; }

        /// <summary>
        /// How many blocks should be on top of the block with coinbase transaction until it's outputs are considered spendable.
        /// </summary>
        public long CoinbaseMaturity { get; set; }

        public Money ProofOfWorkReward { get; set; }

        /// <summary>Maximal length of reorganization that the node is willing to accept, or 0 to disable long reorganization protection.</summary>
        public uint MaxReorgLength { get; set; }

        /// <summary>
        /// Initializes the default values.
        /// </summary>
        public PowConsensusOptions()
        {
            //https://bitcoin.stackexchange.com/questions/69468/what-is-the-current-maximum-bitcoin-block-size-in-mb
            //Block weight = Base size * 3 + Total size. (rationale[3])

            this.MaxBlockBaseSize = 4 * 1000 * 1000;
            this.MaxBlockWeight = this.MaxBlockBaseSize;
            this.MaxBlockSerializedSize = this.MaxBlockWeight;
            this.WitnessScaleFactor = 1;
            this.SerializeTransactionNoWitness = 0x40000000;
            this.MaxStandardVersion = 2;
            this.MaxStandardTxWeight = this.MaxBlockWeight / 10;
            this.MaxBlockSigopsCost = 160000;
            this.MaxMoney = 2100000 * Money.COIN;
            this.CoinbaseMaturity = 10;
            this.ProofOfWorkReward = Money.Coins((decimal)2.5);

            // No long reorg protection on PoW.
            this.MaxReorgLength = 0;
        }

        public PowConsensusOptions TestPowConsensusOptions()
        {
            var production = this;
            production.CoinbaseMaturity = 1;
            return production;
        }
        public PowConsensusOptions RegTestPowConsensusOptions()
        {
            var production = this;
            production.CoinbaseMaturity = 6;//one is unsuitible as precludes maturity based tests
            this.MaxBlockSigopsCost = 3000;
            return production;
        }
    }

    public static class ConsensusExtentions
    {
        public static T Option<T>(this NBitcoin.Consensus item)
            where T : NBitcoin.Consensus.ConsensusOptions
        {
            return item.Options as T;
        }
    }
}
