namespace NBitcoin
{
    /// <summary>
    /// Description of checkpointed block.
    /// </summary>
    public class CheckpointInfo
    {
        /// <summary>Hash of the checkpointed block header.</summary>
        public uint256 Hash { get; private set; }

        /// <summary>
        /// Initializes a new instance of the object.
        /// </summary>
        /// <param name="hash">Hash of the checkpointed block header.</param>
        public CheckpointInfo(uint256 hash)
        {
            this.Hash = hash;
        }
    }
}