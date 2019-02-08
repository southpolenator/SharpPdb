using SharpPdb.Windows.Utility;
using System.Collections.Generic;

namespace SharpPdb.Windows.DebugSubsections
{
    /// <summary>
    /// Represents parsed CV_LinesSection structure.
    /// </summary>
    public class LinesSubsection : DebugSubsection
    {
        /// <summary>
        /// Flags enumeration in CV_Line structure.
        /// </summary>
        public enum LineFlags : uint
        {
            /// <summary>
            /// Line where statement/expression starts
            /// </summary>
            LineNumberStart = 0x00ffffff,

            /// <summary>
            /// Delta to line where statement ends (optional)
            /// </summary>
            LineEndDelta = 0x7f000000,

            /// <summary>
            /// <c>true</c> if a statement <see cref="LineNumberStart"/>, else an expression line number.
            /// </summary>
            Statement = 0x80000000,
        }

        /// <summary>
        /// Represents parsed CV_Line and CV_Column structures combined.
        /// </summary>
        public struct Line
        {
            /// <summary>
            /// The IL offset of this line.
            /// </summary>
            public uint Offset;

            /// <summary>
            /// Line number start.
            /// </summary>
            public uint LineStart;

            /// <summary>
            /// Line number end.
            /// </summary>
            public uint LineEnd;

            /// <summary>
            /// Column number start.
            /// </summary>
            public uint ColumnStart;

            /// <summary>
            /// Column number end.
            /// </summary>
            public uint ColumnEnd;

            /// <summary>
            /// <c>true</c> if a statement at <see cref="LineStart"/>, else an expression line.
            /// </summary>
            public bool Statement;
        }

        /// <summary>
        /// Represents parsed CV_SourceFile structure.
        /// </summary>
        public struct SourceFile
        {
            /// <summary>
            /// Index to a file in checksum section.
            /// </summary>
            public uint Index;

            /// <summary>
            /// Array of lines for this file.
            /// </summary>
            public Line[] Lines;
        }

        /// <summary>
        /// Array of <see cref="DebugSubsectionKind"/> that this class can read.
        /// </summary>
        public static readonly DebugSubsectionKind[] Kinds = new DebugSubsectionKind[]
        {
            DebugSubsectionKind.Lines,
        };

        /// <summary>
        /// Gets the offset portion of the procedure address.
        /// </summary>
        public uint CodeOffset { get; private set; }

        /// <summary>
        /// Gets the segment portion of the procedure address.
        /// </summary>
        public ushort Segment { get; private set; }

        /// <summary>
        /// Gets the flags.
        /// </summary>
        public LinesSubsectionFlags Flags { get; private set; }

        /// <summary>
        /// TODO: Figure out what this represents?
        /// </summary>
        public uint Cod { get; private set; }

        /// <summary>
        /// Gets the array of source files.
        /// </summary>
        public SourceFile[] Files { get; private set; }

        /// <summary>
        /// Reads <see cref="LinesSubsection"/> from the stream.
        /// </summary>
        /// <param name="reader">Stream binary reader.</param>
        /// <param name="kind">Debug subsection kind.</param>
        /// <param name="dataLength">Debug subsection length.</param>
        public static LinesSubsection Read(IBinaryReader reader, DebugSubsectionKind kind, uint dataLength)
        {
            long positionEnd = reader.Position + dataLength;
            LinesSubsection linesSubsection = new LinesSubsection
            {
                Kind = kind,
                CodeOffset = reader.ReadUint(),
                Segment = reader.ReadUshort(),
                Flags = (LinesSubsectionFlags)reader.ReadUshort(),
                Cod = reader.ReadUint(),
            };
            bool columnsAvailable = (linesSubsection.Flags & LinesSubsectionFlags.LinesHaveColumns) == LinesSubsectionFlags.LinesHaveColumns;
            List<SourceFile> files = new List<SourceFile>();

            while (reader.Position < positionEnd)
            {
                SourceFile file = new SourceFile
                {
                    Index = reader.ReadUint(),
                    Lines = new Line[reader.ReadUint()],
                };
                uint size = reader.ReadUint();

                for (int i = 0; i < file.Lines.Length; i++)
                {
                    file.Lines[i].Offset = reader.ReadUint();
                    LineFlags flags = (LineFlags)reader.ReadUint();
                    file.Lines[i].LineStart = (uint)(flags & LineFlags.LineNumberStart);
                    file.Lines[i].LineEnd = file.Lines[i].LineStart + ((uint)(flags & LineFlags.LineEndDelta) >> 24);
                    file.Lines[i].Statement = (flags & LineFlags.Statement) == LineFlags.Statement;
                }
                if (columnsAvailable)
                {
                    for (int i = 0; i < file.Lines.Length; i++)
                    {
                        file.Lines[i].ColumnStart = reader.ReadUshort();
                        file.Lines[i].ColumnEnd = reader.ReadUshort();
                    }
                }
                files.Add(file);
            }
            linesSubsection.Files = files.ToArray();
            return linesSubsection;
        }
    }
}
