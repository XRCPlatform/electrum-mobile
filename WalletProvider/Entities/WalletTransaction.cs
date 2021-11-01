using static ElectrumXClient.Response.BlockchainTransactionGetResponse;

namespace WalletProvider.Entities
{
    public class WalletTransaction
    {
        public HdAddress Address { get; set; }
        public BlockchainTransactionGetResult BlockchainTransaction { get; set; }
        public TransactionData Transaction { get; set; }

        public WalletTransaction(HdAddress address, BlockchainTransactionGetResult blockchainTransaction)
        {
            Address = address;
            BlockchainTransaction = blockchainTransaction;
        }

        public WalletTransaction(HdAddress address, TransactionData transaction)
        {
            Address = address;
            Transaction = transaction;
        }
    }
}
