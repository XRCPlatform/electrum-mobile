using System;

namespace WalletProvider
{
    public class WalletException : Exception
    {
        public WalletException(string message) : base(message)
        {
        }
    }
}
