using System;
using System.Collections.Generic;
using System.Linq;
namespace SevenZip.Pack
{
    public class ArchivePropertie
    {
        public byte PropertyType;
        public byte[] PropertyData;
    }
    public class PackInfo
    {
        public UInt64 PackPos;
        public UInt64 NumPackStreams;
        public UInt64[] PackSizes;
        public byte[] PackStreamDigests;
    }
    public class CoderInfo
    {
        public Folder[] forders;
    }
    public class Folder
    {
        public Coder[] coders;
        public UInt64 NumOutStreamsTotal
        {
            get
            {
                if (coders == null)
                    return 0;
                UInt64 i = 0;
                foreach (var c in coders)
                {
                    i += c.NumOutStreams;
                }
                return i;
            }
        }
        public UInt64 NumInStreamsTotal
        {
            get
            {
                if (coders == null)
                    return 0;
                UInt64 i = 0;
                foreach (var c in coders)
                {
                    i += c.NumInStreams;
                }
                return i;
            }
        }
        public UInt64[] packedStreams;
        public UInt64[] unpackedsizes;
    }
    public class Coder
    {
        public bool bIsComplexCoder;
        public bool bThereAreAttributes;
        public bool bReserved;
        public bool bTherearemorealternativemethods;
        public byte[] CodecId;
        public UInt64 NumInStreams = 1;
        public UInt64 NumOutStreams = 1;
        public byte[] Properties;
    }
    public class StreamInfo
    {
        public PackInfo packInfo;
        public CoderInfo coderInfo;
        public UInt32[] crcs;
    }
    public class FileInfo
    {
        public string[] names;
        public UInt64[] times;
        public UInt32[] attributes;
    }
    public class SevenzipFile
    {
        public byte verMajor;
        public byte verMinor;
        ArchivePropertie[] ArchiveProperties;
        StreamInfo streamInfo;
        FileInfo fileInfo;
        List<byte[]> datas;
        public SevenzipFile(System.IO.Stream stream)
        {
            LoadFile(stream);
        }
        public SevenzipFile(byte[] data)
        {
            using (var s = new System.IO.MemoryStream(data))
            {
                LoadFile(s);
            }
        }

        UInt32 readUint32(System.IO.Stream stream)
        {
            byte[] buf = new byte[4];
            stream.Read(buf, 0, 4);
            return BitConverter.ToUInt32(buf, 0);
        }
        UInt64 readRealUint64(System.IO.Stream stream)
        {
            byte[] buf = new byte[8];
            stream.Read(buf, 0, 8);
            return BitConverter.ToUInt64(buf, 0);
        }
        UInt64 readFalseUint64(System.IO.Stream stream)
        {
            //if (_pos >= _size)
            //    ThrowEndOfData();
            byte firstByte = (byte)stream.ReadByte();
            byte mask = 0x80;
            UInt64 value = 0;
            for (int i = 0; i < 8; i++)
            {
                if ((firstByte & mask) == 0)
                {
                    UInt64 highPart = (UInt64)firstByte & (UInt64)(mask - 1);
                    value += (highPart << (i * 8));
                    return value;
                }
                //if (_pos >= _size)
                //    ThrowEndOfData();
                value |= ((UInt64)stream.ReadByte() << (8 * i));
                mask >>= 1;
            }
            return value;
        }
        void LoadFile(System.IO.Stream stream)
        {
            var headerSignature = new byte[] { (byte)'7', (byte)'z', 0xBC, 0xAF, 0x27, 0x1C };
            var headerread = new byte[6];
            stream.Read(headerread, 0, 6);
            for (var i = 0; i < 6; i++)
            {
                if (headerread[i] != headerSignature[i])
                    throw new Exception("file not a 7z");
            }
            verMajor = (byte)stream.ReadByte(); // now = 0
            verMinor = (byte)stream.ReadByte(); // now = 2

            //读取头header
            UInt32 StartHeaderCRC = readUint32(stream);
            var pos = stream.Position;
            var headbuf = new byte[20];
            stream.Read(headbuf, 0, 20);
            var crchead = CRC.CalculateDigest(headbuf, 0, 20);
            if (crchead != StartHeaderCRC)
            {
                throw new Exception("head crc error.");
            }
            stream.Position = pos;
            UInt64 nextheaderOff = readRealUint64(stream);
            UInt64 nextheaderLen = readRealUint64(stream);
            UInt32 nextheaderCRC = readUint32(stream);
            var posData = stream.Position;

            //读取尾header
            stream.Position = (long)32 + (long)nextheaderOff;
            var nextheadbuf = new Byte[nextheaderLen];
            stream.Read(nextheadbuf, 0, (int)nextheaderLen);
            var crcnexthead = CRC.CalculateDigest(nextheadbuf, 0, (uint)nextheaderLen);
            if (crcnexthead != nextheaderCRC)
            {
                throw new Exception("nexthead crc error.");
            }
            stream.Position = (long)32 + (long)nextheaderOff;

            var kHeader = stream.ReadByte();
            if (kHeader != 0x01)
                throw new Exception("wrong header id");

            while (true)
            {
                var id = stream.ReadByte();
                if (id == 0x04)// NID::kMainStreamsInfo
                {
                    this.streamInfo = ReadStreamInfo(stream);
                }
                else if (id == 0x05)//NID::kFilesInfo
                {
                    this.fileInfo = ReadFileInfo(stream);
                }
                else if (id == 0)//0x00 = kEnd
                {
                    break;
                }
            }

            stream.Position = (int)this.streamInfo.packInfo.PackPos + posData;
            this.datas = new List<byte[]>();
            for (ulong b = 0; b < this.streamInfo.packInfo.NumPackStreams; b++)
            {
                var size = this.streamInfo.packInfo.PackSizes[b];
                byte[] data = new byte[size];
                stream.Read(data, 0, (int)size);
                this.datas.Add(data);
                var crc = CRC.CalculateDigest(data, 0, (uint)size);
            }
        }
        StreamInfo ReadStreamInfo(System.IO.Stream stream)
        {
            StreamInfo info = new StreamInfo();
            while (true)
            {
                var id = stream.ReadByte();
                if (id == 0x06)//0x06 = kPackInfo
                {
                    info.packInfo = readPackInfo(stream);
                }
                else if (id == 0x07)//=kunpackinfo
                {
                    info.coderInfo = readCoderInfo(stream);
                }
                else if (id == 0x08)
                {
                    while (true)
                    {
                        var sid = stream.ReadByte();
                        if (sid == 0x0a)
                        {
                            info.crcs = new UInt32[info.packInfo.NumPackStreams];
                            byte AllAreDefined = (byte)stream.ReadByte();
                            if (AllAreDefined == 0)
                            {
                                //for (NumStreams)
                                //    BIT Defined
                            }
                            //UINT32 CRCs[NumDefined];
                            for (ulong i = 0; i < info.packInfo.NumPackStreams; i++)
                            {
                                info.crcs[i] = readUint32(stream);
                            }
                        }
                        else if (sid == 0x00)
                        {
                            break;
                        }
                    }

                }
                else if (id == 0)//0x00 = kEnd
                {
                    break;
                }
            }
            return info;
        }

        PackInfo readPackInfo(System.IO.Stream stream)
        {
            PackInfo info = new PackInfo();
            info.PackPos = readFalseUint64(stream);
            info.NumPackStreams = readFalseUint64(stream);
            while (true)
            {
                var id = stream.ReadByte();
                if (id == 0x09)
                {
                    info.PackSizes = new UInt64[info.NumPackStreams];
                    for (var i = 0; i < (int)info.NumPackStreams; i++)
                    {
                        info.PackSizes[i] = readFalseUint64(stream);
                    }
                }
                else if (id == 0x0A)
                {

                }
                else if (id == 0)
                {
                    break;
                }
                else
                {
                    throw new Exception("unknonw format.");
                }
            }

            //[]
            //BYTE NID::kSize(0x09)
            //UINT64 PackSizes[NumPackStreams]
            //[]

            //      []
            //      BYTE NID::kCRC(0x0A)
            //PackStreamDigests[NumPackStreams]
            //[]

            //BYTE NID::kEnd
            return info;
        }

        CoderInfo readCoderInfo(System.IO.Stream stream)
        {
            CoderInfo info = new CoderInfo();
            var kFolder = stream.ReadByte();
            if (kFolder != 0x0b)
                throw new Exception("known format.");
            UInt64 NumFolders = readFalseUint64(stream);
            var External = stream.ReadByte();
            //     BYTE NID::kFolder(0x0B)//
            if (External == 0)
            {
                info.forders = new Folder[NumFolders];
                for (var i = 0; i < (int)NumFolders; i++)
                {
                    var folder = new Folder();
                    info.forders[i] = folder;

                    var NumCoders = readFalseUint64(stream);
                    folder.coders = new Coder[NumCoders];

                    for (var j = 0; j < (int)NumFolders; j++)
                    {
                        var coder = new Coder();
                        folder.coders[j] = coder;

                        var byteinfo = stream.ReadByte();
                        var count = byteinfo & 0x0f;
                        coder.bIsComplexCoder = (byteinfo & 0x10) > 0;
                        coder.bThereAreAttributes = (byteinfo & 0x20) > 0;
                        coder.bReserved = (byteinfo & 0x40) > 0;
                        coder.bTherearemorealternativemethods = (byteinfo & 0x80) > 0;
                        coder.CodecId = new byte[count];
                        stream.Read(coder.CodecId, 0, count);
                        if (coder.bIsComplexCoder)
                        {
                            coder.NumInStreams = readFalseUint64(stream);
                            coder.NumOutStreams = readFalseUint64(stream);
                        }
                        if (coder.bThereAreAttributes)
                        {
                            var PropertiesSize = readFalseUint64(stream);
                            coder.Properties = new byte[PropertiesSize];
                            stream.Read(coder.Properties, 0, (int)PropertiesSize);
                        }
                    }
                    var NumBindPairs = folder.NumOutStreamsTotal - 1;
                    for (ulong j = 0; j < NumBindPairs; j++)
                    {
                        var inindex = readFalseUint64(stream);
                        var outindex = readFalseUint64(stream);
                    }
                    var NumPackedStreams = folder.NumInStreamsTotal - NumBindPairs;
                    folder.packedStreams = new UInt64[NumPackedStreams];
                    folder.unpackedsizes = new UInt64[NumPackedStreams];
                    if (NumPackedStreams > 1)
                    {
                        for (ulong j = 0; j < NumPackedStreams; j++)
                        {
                            var Index = readFalseUint64(stream);
                        };
                    }
                }
                //BYTE ID::kCodersUnPackSize(0x0C)
                var id = stream.ReadByte();
                if (id != 0x0c)
                    throw new Exception("error format here.");
                for (var i = 0; i < info.forders.Length; i++)
                {
                    for (ulong j = 0; j < info.forders[i].NumOutStreamsTotal; j++)
                    {
                        info.forders[i].unpackedsizes[j] = readFalseUint64(stream);
                    }
                }
            }
            else if (External == 1)
            {
                throw new Exception("not write code here.");
            }
            else
            {
                throw new Exception("known format.");

            }
            while (true)
            {
                var id = stream.ReadByte();
                if (id == 0)
                {
                    break;
                }
                else
                {
                    throw new Exception("unknonw format.");
                }
            }
            return info;
        }

        FileInfo ReadFileInfo(System.IO.Stream stream)
        {
            FileInfo info = new FileInfo();
            var numfiles = readFalseUint64(stream);
            info.names = new string[numfiles];
            info.times = new UInt64[numfiles];
            info.attributes = new UInt32[numfiles];
            while (true)
            {
                byte PropertyType = (byte)stream.ReadByte();
                if (PropertyType == 0)
                    break;
                var size = readFalseUint64(stream);
                switch (PropertyType)
                {
                    case 0x12://kCTime
                    case 0x13://kATime
                    case 0x14://kMTime
                        {
                            byte AllAreDefined = (byte)stream.ReadByte();
                            if (AllAreDefined == 0)
                            {
                                //for (NumFiles)
                                //    BIT TimeDefined
                            }
                            byte External = (byte)stream.ReadByte();
                            if (External != 0)
                            {
                                var dataindex = readFalseUint64(stream);
                            }
                            for (ulong i = 0; i < numfiles; i++)
                            {
                                var Time = readRealUint64(stream);
                                info.times[i] = Time;
                            }
                        }
                        break;
                    case 0x15://kAttributes
                        {
                            byte AllAreDefined = (byte)stream.ReadByte();
                            if (AllAreDefined == 0)
                            {
                                //for (NumFiles)
                                //    BIT TimeDefined
                            }
                            byte External = (byte)stream.ReadByte();
                            if (External != 0)
                            {
                                var dataindex = readFalseUint64(stream);
                            }
                            for (ulong i = 0; i < numfiles; i++)
                            {
                                var Attributes = readUint32(stream);
                                info.attributes[i] = Attributes;
                            }
                        }
                        break;
                    case 0x11://kNames
                        {
                            byte External = (byte)stream.ReadByte();
                            if (External != 0)
                            {
                                var dataindex = readFalseUint64(stream);
                            }
                            for (ulong i = 0; i < numfiles; i++)
                            {
                                string filename = "";
                                byte[] buf = new byte[2];
                                while (true)
                                {
                                    stream.Read(buf, 0, 2);
                                    if (buf[0] == 0 && buf[1] == 0)
                                        break;
                                    char c = BitConverter.ToChar(buf, 0);
                                    filename += c.ToString();
                                }
                                info.names[i] = filename;
                            }
                        }
                        break;
                    case 0x0E://kEmptyStream
                        throw new Exception("now code here.");
                        break;
                    case 0x19:
                        break;
                    default:
                        throw new Exception("now code here.");
                        break;
                }
            }
            return info;
        }

        public byte[] Unpack(int index = 0)
        {
            var inlen = this.datas[index].Length;
            var outlen = this.streamInfo.coderInfo.forders[index].unpackedsizes[index];
            var instream = new System.IO.MemoryStream(this.datas[index]);
            var outstream = new System.IO.MemoryStream();
            SevenZip.Compression.LZMA.Decoder decoder = new Compression.LZMA.Decoder();
            decoder.SetDecoderProperties(this.streamInfo.coderInfo.forders[0].coders[index].Properties);
            decoder.Code(instream, outstream, inlen, (long)outlen, null);
            return outstream.ToArray();
        }
    }
}
