using SharpPdb.Windows.TypeRecords;
using SharpUtilities;
using System;

namespace SharpPdb.Windows.Utility
{
    /// <summary>
    /// Extension methods for <see cref="IBinaryReader"/>.
    /// </summary>
    public static class BinaryReaderExtensions
    {
        /// <summary>
        /// Reads encoded constant from the stream.
        /// </summary>
        /// <param name="reader">Stream binary reader.</param>
        /// <returns>Mostly integer, but also carries type info, so it is returned as object.</returns>
        public static object ReadEncodedConstant(this IBinaryReader reader)
        {
            ushort type = reader.ReadUshort();

            if (type < (ushort)TypeLeafKind.LF_NUMERIC)
                return type;

            switch ((TypeLeafKind)type)
            {
                case TypeLeafKind.LF_CHAR:
                    return (sbyte)reader.ReadByte();
                case TypeLeafKind.LF_SHORT:
                    return reader.ReadShort();
                case TypeLeafKind.LF_USHORT:
                    return reader.ReadUshort();
                case TypeLeafKind.LF_LONG:
                    return reader.ReadInt();
                case TypeLeafKind.LF_ULONG:
                    return reader.ReadUint();
                case TypeLeafKind.LF_QUADWORD:
                    return reader.ReadLong();
                case TypeLeafKind.LF_UQUADWORD:
                    return reader.ReadUlong();
                case TypeLeafKind.LF_REAL32:
                    return reader.ReadFloat();
                case TypeLeafKind.LF_REAL64:
                    return reader.ReadDouble();
                case TypeLeafKind.LF_VARSTRING:
                    return reader.ReadBString();
                case TypeLeafKind.LF_DECIMAL:
                    return reader.ReadDecimal();
                case TypeLeafKind.LF_DATE:
                    return new DateTime(reader.ReadLong());
            }

            throw new NotImplementedException();
        }
    }
}
