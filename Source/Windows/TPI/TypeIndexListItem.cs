namespace SharpPdb.Windows.TPI
{
    /// <summary>
    /// Represents chained list of type indexes.
    /// </summary>
    public class TypeIndexListItem
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TypeIndexListItem"/> class.
        /// </summary>
        /// <param name="typeIndex">Type index.</param>
        /// <param name="next">Next item in the list.</param>
        public TypeIndexListItem(TypeIndex typeIndex, TypeIndexListItem next)
        {
            TypeIndex = typeIndex;
            Next = next;
        }

        /// <summary>
        /// Gets the type index.
        /// </summary>
        public TypeIndex TypeIndex { get; private set; }

        /// <summary>
        /// Gets the next item in the list.
        /// </summary>
        public TypeIndexListItem Next { get; internal set; }
    }
}
