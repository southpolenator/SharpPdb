namespace SharpPdb.Windows.GSI
{
    /// <summary>
    /// Represents element of <see cref="GlobalsStream.HashBuckets"/>.
    /// </summary>
    public struct GlobalsStreamHashBucket
    {
        /// <summary>
        /// Gets the start index of <see cref="GlobalsStream.HashRecords"/> array of current bucket.
        /// </summary>
        public int Start;

        /// <summary>
        /// Gets the end index of <see cref="GlobalsStream.HashRecords"/> array of current bucket.
        /// This index is first after the last one in this bucket.
        /// </summary>
        public int End;
    }
}
