using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace ElectrumMobileXRC.Services
{
    public class DbWalletHelper
    {
        public bool IsWalletInit { get; set; }
        public string SerializedWallet { get; set; }

        private ConfigDbService _configDb { get; set; }

        public DbWalletHelper(ConfigDbService configDb)
        {
            _configDb = configDb;

            IsWalletInit = false;
        }

        internal async Task LoadFromDbAsync()
        {
            var walletInit = await _configDb.Get(DbConfig.CFG_WALLETINIT);

            if ((walletInit == null) || (string.IsNullOrEmpty(walletInit.Value)) || walletInit.Value != DbConfig.CFG_TRUE)
            {
                IsWalletInit = false;
            }
            else
            {
                IsWalletInit = true;

                var serializedWallet = await _configDb.Get(DbConfig.CFG_WALLETMETADATA);
                SerializedWallet = serializedWallet.Value;
            }

            return;
        }

        internal async Task UpdateWalletAsync(string serializedWallet)
        {
            await _configDb.Add(DbConfig.CFG_WALLETINIT, DbConfig.CFG_TRUE);
            await _configDb.Add(DbConfig.CFG_WALLETMETADATA, serializedWallet);
        }
    }
}
