using SharpUtilities;
using System;
using System.IO;
using System.Linq;
using Xunit;

namespace SharpPdb.Windows.MSF.Tests
{
    public class SuperBlockTests
    {
        [Fact]
        public void Validate()
        {
            SuperBlock superBlock = CreateSuperBlock(SuperBlock.Magic, 512, 1, 4, 0, 0, 1);

            superBlock.Validate();
            Assert.Equal(SuperBlock.Magic, superBlock.MagicString);
        }

        [Fact]
        public void ValidateFail()
        {
            Assert.ThrowsAny<Exception>(() => CreateSuperBlock("Bad magic", 512, 1, 4, 0, 0, 1).Validate());
            Assert.ThrowsAny<Exception>(() => CreateSuperBlock(SuperBlock.Magic, 128, 1, 4, 0, 0, 1).Validate());
            Assert.ThrowsAny<Exception>(() => CreateSuperBlock(SuperBlock.Magic, 512, 1, 4, 3, 0, 1).Validate());
            Assert.ThrowsAny<Exception>(() => CreateSuperBlock(SuperBlock.Magic, 512, 1, 4, 512000, 0, 1).Validate());
            Assert.ThrowsAny<Exception>(() => CreateSuperBlock(SuperBlock.Magic, 512, 1, 4, 0, 0, 0).Validate());
            Assert.ThrowsAny<Exception>(() => CreateSuperBlock(SuperBlock.Magic, 512, 0, 4, 0, 0, 1).Validate());
        }

        [Fact]
        public void IsValidBlockSize()
        {
            Assert.False(SuperBlock.IsValidBlockSize(128));
            Assert.False(SuperBlock.IsValidBlockSize(256));
            Assert.True(SuperBlock.IsValidBlockSize(512));
            Assert.True(SuperBlock.IsValidBlockSize(1024));
            Assert.True(SuperBlock.IsValidBlockSize(2048));
            Assert.True(SuperBlock.IsValidBlockSize(4096));
            Assert.False(SuperBlock.IsValidBlockSize(8192));
        }

        private static SuperBlock CreateSuperBlock(string magic, uint blockSize, uint freeBlockMapBlock, uint numBlocks, uint numDirectoryBytes, uint unknown, uint blockMapAddr)
        {
            return CreateSuperBlock(SerializeSuperBlock(magic, blockSize, freeBlockMapBlock, numBlocks, numDirectoryBytes, unknown, blockMapAddr));
        }

        private static byte[] SerializeSuperBlock(string magic, uint blockSize, uint freeBlockMapBlock, uint numBlocks, uint numDirectoryBytes, uint unknown, uint blockMapAddr)
        {
            using (MemoryStream stream = new MemoryStream())
            using (BinaryWriter writer = new BinaryWriter(stream))
            {
                if (magic.Length < SuperBlock.Magic.Length)
                    magic += new string(' ', SuperBlock.Magic.Length - magic.Length);
                writer.Write(magic.ToCharArray().Select(c => (byte)c).ToArray(), 0, SuperBlock.Magic.Length);
                writer.Write(blockSize);
                writer.Write(freeBlockMapBlock);
                writer.Write(numBlocks);
                writer.Write(numDirectoryBytes);
                writer.Write(unknown);
                writer.Write(blockMapAddr);
                writer.Flush();
                return stream.ToArray();
            }
        }

        private static unsafe SuperBlock CreateSuperBlock(byte[] bytes)
        {
            fixed (byte* memory = bytes)
            {
                MemoryBinaryReader reader = new MemoryBinaryReader(memory, bytes.Length);

                return SuperBlock.Read(reader);
            }
        }
    }
}
