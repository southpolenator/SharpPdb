using SharpUtilities;
using System;
using System.Reflection.Metadata;

namespace SharpPdb.Managed.Portable
{
    /// <summary>
    /// Represents local constant defined in local scope in Portable PDB file.
    /// </summary>
    public class PdbLocalConstant : IPdbLocalConstant
    {
        /// <summary>
        /// Cache of <see cref="LocalConstant"/> property.
        /// </summary>
        private SimpleCacheStruct<LocalConstant> localConstantCache;

        /// <summary>
        /// Cache of <see cref="Name"/> property.
        /// </summary>
        private SimpleCacheStruct<string> nameCache;

        /// <summary>
        /// Cache of <see cref="Value"/> property.
        /// </summary>
        private SimpleCacheStruct<object> valueCache;

        /// <summary>
        /// Initializes a new instance of the <see cref="PdbLocalConstant"/> class.
        /// </summary>
        /// <param name="localScope">Local scope where this contanst is defined.</param>
        /// <param name="handle">Our metadata reader handle.</param>
        internal PdbLocalConstant(PdbLocalScope localScope, LocalConstantHandle handle)
        {
            LocalScope = localScope;
            localConstantCache = SimpleCache.CreateStruct(() => LocalScope.Function.PdbFile.Reader.GetLocalConstant(handle));
            nameCache = SimpleCache.CreateStruct(() => LocalScope.Function.PdbFile.Reader.GetString(LocalConstant.Name));
            valueCache = SimpleCache.CreateStruct<object>(() =>
            {
                var reader = LocalScope.Function.PdbFile.Reader.GetBlobReader(LocalConstant.Signature);
                SignatureTypeCode typeCode;

                while (true)
                {
                    typeCode = reader.ReadSignatureTypeCode();
                    if (typeCode == SignatureTypeCode.OptionalModifier || typeCode == SignatureTypeCode.RequiredModifier)
                        reader.ReadCompressedInteger();
                    else
                        break;
                }

                switch (typeCode)
                {
                    case SignatureTypeCode.Boolean:
                        return (short)(reader.ReadBoolean() ? 1 : 0);
                    case SignatureTypeCode.Char:
                        return reader.ReadChar();
                    case SignatureTypeCode.SByte:
                        return reader.ReadSByte();
                    case SignatureTypeCode.Byte:
                        return reader.ReadByte();
                    case SignatureTypeCode.Int16:
                        return reader.ReadInt16();
                    case SignatureTypeCode.UInt16:
                        return reader.ReadUInt16();
                    case SignatureTypeCode.Int32:
                        return reader.ReadInt32();
                    case SignatureTypeCode.UInt32:
                        return reader.ReadUInt32();
                    case SignatureTypeCode.Int64:
                        return reader.ReadInt64();
                    case SignatureTypeCode.UInt64:
                        return reader.ReadUInt64();
                    case SignatureTypeCode.Single:
                        return reader.ReadSingle();
                    case SignatureTypeCode.Double:
                        return reader.ReadDouble();
                    case SignatureTypeCode.String:
                        if (reader.RemainingBytes == 1)
                        {
                            if (reader.ReadByte() != 0xFF)
                                throw new Exception("Unexpected string constant");
                            return null;
                        }
                        if (reader.RemainingBytes % 2 != 0)
                            throw new Exception("Unexpected string constant");
                        return reader.ReadUTF16(reader.RemainingBytes);
                    case SignatureTypeCode.TypeHandle:
                    case SignatureTypeCode.Object:
                    default:
                        // We don't know how to parse value
                        return null;
                }
            });
        }

        /// <summary>
        /// Gets the local scope where this contanst is defined.
        /// </summary>
        public PdbLocalScope LocalScope { get; private set; }

        /// <summary>
        /// Gets the local constant.
        /// </summary>
        public LocalConstant LocalConstant => localConstantCache.Value;

        /// <summary>
        /// Gets the name of the local constant.
        /// </summary>
        public string Name => nameCache.Value;

        /// <summary>
        /// Gets the value of the local constant.
        /// </summary>
        public object Value => valueCache.Value;
    }
}
