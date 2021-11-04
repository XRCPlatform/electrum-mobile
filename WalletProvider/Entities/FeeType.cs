using System;

namespace WalletProvider.Entities
{
    /// <summary>
    /// An indicator of how fast a transaction will be accepted in a block.
    /// </summary>
    public enum FeeType
    {
        /// <summary>
        /// VeryLow.
        /// </summary>
        VeryLow = 0,

        /// <summary>
        /// Slow.
        /// </summary>
        Low = 10,

        /// <summary>
        /// Avarage.
        /// </summary>
        Medium = 50,

        /// <summary>
        /// Fast.
        /// </summary>
        High = 100,

        /// <summary>
        /// VeryHigh.
        /// </summary>
        VeryHigh = 150
    }

    public static class FeeParser
    {
        public static FeeType Parse(string value)
        {
            bool isParsed = Enum.TryParse<FeeType>(value, true, out var result);
            if (!isParsed)
            {
                throw new FormatException($"FeeType {value} is not a valid FeeType");
            }

            return result;
        }

        /// <summary>
        /// Map a fee type to the number of confirmations
        /// </summary>
        public static int ToConfirmations(this FeeType fee)
        {
            switch (fee)
            {
                case FeeType.VeryLow:
                    return 40;

                case FeeType.Low:
                    return 30;

                case FeeType.Medium:
                    return 20;

                case FeeType.High:
                    return 10;

                case FeeType.VeryHigh:
                    return 5;
            }

            throw new WalletException("Invalid fee");
        }
    }
}
