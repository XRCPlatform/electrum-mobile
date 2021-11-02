using System.Collections.Generic;
using static ElectrumXClient.Response.BlockchainTransactionGetMerkleResponse;
using static ElectrumXClient.Response.BlockchainTransactionGetResponse;

namespace WalletProvider.Entities
{
    public class WalletTransaction
    {
        public HdAddress Address { get; set; }
        public BlockchainTransactionGetResult BlockchainTransaction { get; set; }
        public List<string> BlockchainTransactionMerkle { get; set; }
        public TransactionData Transaction { get; set; }

        public WalletTransaction(
            HdAddress address,
            BlockchainTransactionGetResult blockchainTransaction,
            BlockchainTransactionGetMerkleResult blockchainTransactionMerkle)
        {
            Address = address;
            BlockchainTransaction = blockchainTransaction;
            BlockchainTransactionMerkle = blockchainTransactionMerkle.Merkle;
        }

        public WalletTransaction(HdAddress address, TransactionData transaction)
        {
            Address = address;
            Transaction = transaction;
        }
    }
}
