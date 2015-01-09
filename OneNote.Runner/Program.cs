using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OneNote.Runner
{
    public class FileChunkReference64x32
    {
        public FileChunkReference64x32(Byte[] bytes)
        {
            if (bytes.Length != 12)
                throw new ArgumentException("Byte array length must be 12");

            Stp = BitConverter.ToUInt64(bytes, 0);
            Cb = BitConverter.ToUInt32(bytes, 8);
        }

        /// <summary>
        /// Location of the reference in the file
        /// </summary>
        public UInt64 Stp { get; set; }

        /// <summary>
        /// Size of the referenced data, in bytes
        /// </summary>
        public UInt32 Cb { get; set; }

        public static FileChunkReference64x32 Nil = new FileChunkReference64x32(new byte[] { 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0, 0, 0, 0 });
        public static FileChunkReference64x32 Zero = new FileChunkReference64x32(new byte[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 });

        public override Boolean Equals(Object obj)
        {
            var fcr = obj as FileChunkReference64x32;
            return fcr == this;
        }

        public override Int32 GetHashCode()
        {
            return base.GetHashCode();
        }

        public static Boolean operator ==(FileChunkReference64x32 a, FileChunkReference64x32 b)
        {
            return a.Stp == b.Stp && a.Cb == b.Cb;
        }

        public static Boolean operator !=(FileChunkReference64x32 a, FileChunkReference64x32 b)
        {
            return !(a.Stp == b.Stp && a.Cb == b.Cb);
        }
    }

    public class FileChunkReference32
    {
        public FileChunkReference32(Byte[] bytes)
        {
            if (bytes.Length != 8)
                throw new ArgumentException("Byte array length must be 8");

            Stp = BitConverter.ToUInt32(bytes, 0);
            Cb = BitConverter.ToUInt32(bytes, 4);
        }

        /// <summary>
        /// Location of the reference in the file
        /// </summary>
        public UInt32 Stp { get; set; }

        /// <summary>
        /// Size of the referenced data, in bytes
        /// </summary>
        public UInt32 Cb { get; set; }

        public static FileChunkReference32 Nil = new FileChunkReference32(new byte[] { 0xff, 0xff, 0xff, 0xff, 0, 0, 0, 0 });
        public static FileChunkReference32 Zero = new FileChunkReference32(new byte[] { 0, 0, 0, 0, 0, 0, 0, 0 });

        public override Boolean Equals(Object obj)
        {
            var fcr = obj as FileChunkReference32;
            return fcr == this;
        }

        public override Int32 GetHashCode()
        {
            return base.GetHashCode();
        }

        public static Boolean operator ==(FileChunkReference32 a, FileChunkReference32 b)
        {
            return a.Stp == b.Stp && a.Cb == b.Cb;
        }

        public static Boolean operator !=(FileChunkReference32 a, FileChunkReference32 b)
        {
            return !(a.Stp == b.Stp && a.Cb == b.Cb);
        }
    }

    public class OneFileHeader
    {
        public OneFileHeader(Stream s)
        {
            var guidBuff = new Byte[16];
            var ffvBuff = new Byte[4];
            var fcr32Buff = new Byte[8];
            var fcr64x32Buff = new Byte[12];
            var cBuff = new Byte[4];

            var aUInt32Buff = new Byte[4];
            var aUInt64Buff = new Byte[8];

            s.Read(guidBuff, 0, 16);
            FileType = new Guid(guidBuff);

            s.Read(guidBuff, 0, 16);
            FileId = new Guid(guidBuff);

            s.Read(guidBuff, 0, 16);
            LegacyId = new Guid(guidBuff);

            s.Read(guidBuff, 0, 16);
            Format = new Guid(guidBuff);

            s.Read(aUInt32Buff, 0, 4);
            LastCodeThatWroteToThisFile = BitConverter.ToUInt32(ffvBuff, 0);

            s.Read(aUInt32Buff, 0, 4);
            OldestCodeThatHasWrittenToThisFile = BitConverter.ToUInt32(ffvBuff, 0);

            s.Read(aUInt32Buff, 0, 4);
            NewestCodeThatHasWrittenToThisFile = BitConverter.ToUInt32(ffvBuff, 0);

            s.Read(aUInt32Buff, 0, 4);
            OldestCodeThatMayReadThisFile = BitConverter.ToUInt32(ffvBuff, 0);

            s.Read(fcr32Buff, 0, 8);
            LegacyFreeChunkList = new FileChunkReference32(fcr32Buff);
            if (LegacyFreeChunkList != FileChunkReference32.Zero)
                return;

            s.Read(fcr32Buff, 0, 8);
            LegacyTransactionLog = new FileChunkReference32(fcr32Buff);
            if (LegacyTransactionLog != FileChunkReference32.Nil)
                return;

            s.Read(aUInt32Buff, 0, 4);
            TransactionsInLog = BitConverter.ToUInt32(cBuff, 0);

            s.Read(aUInt32Buff, 0, 4);
            LegacyExpectedFileLength = BitConverter.ToUInt32(cBuff, 0);

            s.Read(fcr32Buff, 0, 8); //skip over rgbPlaceholder

            s.Read(fcr32Buff, 0, 8);
            LegacyFileNodeListRoot = new FileChunkReference32(fcr32Buff);
            if (LegacyFileNodeListRoot != FileChunkReference32.Nil)
                return;

            s.Read(fcr32Buff, 0, 8); //skip over cbLegacyFreeSpaceInFreeChunkList,fNeedsDefrag, fRepairedFile, fNeedsGarbageCollect and fHasNoEmbeddedFileObjects 

            s.Read(guidBuff, 0, 16);
            Ancestor = new Guid(guidBuff);

            s.Read(aUInt32Buff, 0, 4);
            CrcName = BitConverter.ToUInt32(aUInt32Buff, 0);

            s.Read(fcr64x32Buff, 0, 12);
            HashedChunkList = new FileChunkReference64x32(fcr64x32Buff);

            s.Read(fcr64x32Buff, 0, 12);
            TransactionLog = new FileChunkReference64x32(fcr64x32Buff);
            if (TransactionLog == FileChunkReference64x32.Nil || TransactionLog == FileChunkReference64x32.Zero)
                return;

            s.Read(fcr64x32Buff, 0, 12);
            FileNodeListRoot = new FileChunkReference64x32(fcr64x32Buff);

            s.Read(fcr64x32Buff, 0, 12);
            FreeChunkList = new FileChunkReference64x32(fcr64x32Buff);

            s.Read(aUInt64Buff, 0, 8);
            ExpectedFileLength = BitConverter.ToUInt64(aUInt64Buff, 0);

            s.Read(aUInt64Buff, 0, 8);
            FreeSpaceInFreeChunkList = BitConverter.ToUInt64(aUInt64Buff, 0);

            s.Read(guidBuff, 0, 16);
            FileVersion = new Guid(guidBuff);

            s.Read(aUInt64Buff, 0, 8);
            FileVersionGeneration = BitConverter.ToUInt64(aUInt64Buff, 0);

            s.Read(guidBuff, 0, 16);
            DenyReadFileVersion = new Guid(guidBuff);

            s.Read(new byte[28], 0, 28); //skip over grfDebugLogFlags, fcrDebugLog and fcrAllocVerificationFreeChunkList 

            s.Read(aUInt32Buff, 0, 4);
            BuildNumberCreated = BitConverter.ToUInt32(aUInt32Buff, 0);

            s.Read(aUInt32Buff, 0, 4);
            BuildNumberLastWroteToThisFile = BitConverter.ToUInt32(aUInt32Buff, 0);

            s.Read(aUInt32Buff, 0, 4);
            BuildNumberOldestWritten = BitConverter.ToUInt32(aUInt32Buff, 0);

            s.Read(aUInt32Buff, 0, 4);
            BuildNumberNewestWritten = BitConverter.ToUInt32(aUInt32Buff, 0);
        }

        public Guid FileType { get; set; }
        public Guid FileId { get; set; }
        public Guid LegacyId { get; set; }
        public Guid Format { get; set; }

        public UInt32 LastCodeThatWroteToThisFile { get; set; }
        public UInt32 OldestCodeThatHasWrittenToThisFile { get; set; }
        public UInt32 NewestCodeThatHasWrittenToThisFile { get; set; }
        public UInt32 OldestCodeThatMayReadThisFile { get; set; }

        public FileChunkReference32 LegacyFreeChunkList { get; set; }
        public FileChunkReference32 LegacyTransactionLog { get; set; }

        public UInt32 TransactionsInLog { get; set; }
        public UInt32 LegacyExpectedFileLength { get; set; }

        public FileChunkReference32 LegacyFileNodeListRoot { get; set; }

        public Guid Ancestor { get; set; }

        public UInt32 CrcName { get; set; }

        public FileChunkReference64x32 HashedChunkList { get; set; }
        public FileChunkReference64x32 TransactionLog { get; set; }
        public FileChunkReference64x32 FileNodeListRoot { get; set; }
        public FileChunkReference64x32 FreeChunkList { get; set; }

        public UInt64 ExpectedFileLength { get; set; }
        public UInt64 FreeSpaceInFreeChunkList { get; set; }

        public Guid FileVersion { get; set; }
        public UInt64 FileVersionGeneration { get; set; }

        public Guid DenyReadFileVersion { get; set; }

        public UInt32 BuildNumberCreated { get; set; }
        public UInt32 BuildNumberLastWroteToThisFile { get; set; }
        public UInt32 BuildNumberOldestWritten { get; set; }
        public UInt32 BuildNumberNewestWritten { get; set; }
    }

    class Program
    {
        static void Main(string[] args)
        {
            var onePath = @"C:\dev\OneNote.Runner\OneNote.Runner\files\2013test\Untitled Section.one"; // @"C:\dev\OneNote.Runner\OneNote.Runner\files\TestOneNote0.one";
            var tocPath = @"C:\dev\OneNote.Runner\OneNote.Runner\files\2013test\Open Notebook.onetoc2";
            
            
            using (var fs = new FileStream(tocPath, FileMode.Open))
            {
                var header = new OneFileHeader(fs);
            }
        }
    }
}
