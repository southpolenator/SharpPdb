using SharpUtilities;
using System.Collections.Generic;

namespace SharpPdb.Windows.SymbolRecords
{
    /// <summary>
    /// Base class for all symbol record classes.
    /// </summary>
    public class SymbolRecord
    {
        /// <summary>
        /// Cache for <see cref="Parent"/> property.
        /// </summary>
        private SimpleCacheStruct<SymbolRecord> parentCache;

        /// <summary>
        /// Cache for <see cref="Children"/> property.
        /// </summary>
        private SimpleCacheStruct<SymbolRecord[]> childrenCache;

        /// <summary>
        /// Initializes a new instance of the <see cref="SymbolRecord"/> class.
        /// </summary>
        public SymbolRecord()
        {
            parentCache = SimpleCache.CreateStruct(GetParent);
            childrenCache = SimpleCache.CreateStruct(GetChildren);
        }

        /// <summary>
        /// Gets the symbol stream that contains this symbol record.
        /// </summary>
        public SymbolStream SymbolStream { get; protected set; }

        /// <summary>
        /// Gets the index in symbol stream <see cref="SymbolStream.References"/> array.
        /// </summary>
        public int SymbolStreamIndex { get; protected set; }

        /// <summary>
        /// Type of the symbol record.
        /// </summary>
        public SymbolRecordKind Kind { get; protected set; }

        /// <summary>
        /// Gets the parent symbol record.
        /// </summary>
        /// <remarks>
        /// For symbol records that don't have ParentOffset field, parent symbol record will
        /// be automatically updated during evaluation of <see cref="Children"/>.
        /// </remarks>
        public SymbolRecord Parent
        {
            get
            {
                return parentCache.Value;
            }

            private set
            {
                parentCache.Value = value;
            }
        }

        /// <summary>
        /// Gets the child symbol records. Block like symbol records have children.
        /// </summary>
        public SymbolRecord[] Children => childrenCache.Value;

        /// <summary>
        /// Gets end position of the children subrecords.
        /// </summary>
        /// <returns>Position in the binary stream.</returns>
        protected virtual long GetChildrenEndPosition()
        {
            return 0;
        }

        /// <summary>
        /// Gets the position in the symbol stream of the parent symbol record.
        /// </summary>
        /// <returns>Position in the symbol stream of the parent symbol record.</returns>
        protected virtual long GetParentPosition()
        {
            return 0;
        }

        /// <summary>
        /// Gets the parent symbol record if exists.
        /// </summary>
        /// <returns></returns>
        private SymbolRecord GetParent()
        {
            long parentPosition = GetParentPosition();

            if (parentPosition <= 0)
                return null;
            return SymbolStream.GetSymbolRecordByOffset(parentPosition);
        }

        /// <summary>
        /// Getd end index of the children subrecords.
        /// </summary>
        private int GetChildrenEndIndex()
        {
            long endPosition = GetChildrenEndPosition();
            var references = SymbolStream.References;
            int endIndex = SymbolStreamIndex + 1;

            while (endIndex < references.Count && references[endIndex].DataOffset < endPosition)
                endIndex++;
            return endIndex;
        }

        /// <summary>
        /// Gets the children subrecords.
        /// </summary>
        private SymbolRecord[] GetChildren()
        {
            int startIndex = SymbolStreamIndex + 1;
            int endIndex = GetChildrenEndIndex();
            int capacity = endIndex - startIndex;

            if (capacity <= 0)
                return new SymbolRecord[0];

            List<SymbolRecord> children = new List<SymbolRecord>(capacity);

            for (int i = startIndex; i < endIndex; )
            {
                SymbolRecord child = SymbolStream[i];

                children.Add(child);
                child.Parent = this;
                i = child.GetChildrenEndIndex();
            }
            return children.ToArray();
        }
    }
}
