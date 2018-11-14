#region License
// Copyright (c) 2010-2018, Mark Final
// All rights reserved.
//
// Redistribution and use in source and binary forms, with or without
// modification, are permitted provided that the following conditions are met:
//
// * Redistributions of source code must retain the above copyright notice, this
//   list of conditions and the following disclaimer.
//
// * Redistributions in binary form must reproduce the above copyright notice,
//   this list of conditions and the following disclaimer in the documentation
//   and/or other materials provided with the distribution.
//
// * Neither the name of BuildAMation nor the names of its
//   contributors may be used to endorse or promote products derived from
//   this software without specific prior written permission.
//
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS"
// AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE
// IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE
// DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT HOLDER OR CONTRIBUTORS BE LIABLE
// FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL
// DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR
// SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER
// CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY,
// OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE
// OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
#endregion // License
namespace Bam.Core
{
    internal class TarFile :
        System.IDisposable
    {
        private readonly System.IO.Stream tarStream;

        const int HEADERSIZE = 512;
        const int NAMESIZ = 100;
        const int TUNMLEN = 32;
        const int TGNMLEN = 32;

        const int REGTYPE = 0;
        const int LINKTYPE = 1;
        const int SYMLINK = 2;
        const int DIRTYPE = 5;
        const string GLOBALEXTENDEDHEADER = "g";

        class Header
        {
            public enum Type
            {
                Unknown,
                File,
                Directory,
                Link,
                Symlink,
                GlobalExtendedHeader
            }

            public class EndOfStreamException :
                Exception
            {}

            public class BadFormattingException :
                Exception
            {
                public BadFormattingException(
                    System.FormatException ex,
                    string message)
                    :
                    base(ex, message)
                {}
            }

            string
            NullTerminatedCharArrayToString(char[] array)
            {
                int index = 0;
                while (index < array.Length && array[index] != '\0')
                {
                    ++index;
                }
                return new string(array, 0, index).Trim();
            }

            public Header(
                System.IO.Stream stream)
            {
                char[]
                ReadChars(
                    System.IO.Stream localStream,
                    int length,
                    ref int bytesRead)
                {
                    var bytes = new byte[length];
                    var read = localStream.Read(bytes, 0, length);
                    if (0 == read)
                    {
                        return null;
                    }
                    bytesRead += read;
                    return System.Text.Encoding.ASCII.GetChars(bytes);
                }

                int
                CharArrayOctalToInt(char[] array)
                {
                    var asString = NullTerminatedCharArrayToString(array);
                    if (System.String.IsNullOrEmpty(asString))
                    {
                        return 0;
                    }
                    try
                    {
                        return System.Convert.ToInt32(asString, 8);
                    }
                    catch (System.FormatException ex)
                    {
                        throw new BadFormattingException(ex, $"Unable to parse '{asString}' as an octal number");
                    }
                }

                Type
                CharArrayToType(char[] array)
                {
                    var asString = NullTerminatedCharArrayToString(array);
                    if (System.String.IsNullOrEmpty(asString))
                    {
                        return 0;
                    }
                    if (asString == GLOBALEXTENDEDHEADER)
                    {
                        return Type.GlobalExtendedHeader;
                    }
                    var asInt = System.Convert.ToInt32(asString, 8);
                    switch (asInt)
                    {
                        case DIRTYPE:
                            return Type.Directory;
                        case LINKTYPE:
                            return Type.Link;
                        case SYMLINK:
                            return Type.Symlink;
                        case REGTYPE:
                            return Type.File;
                        default:
                            return Type.Unknown;
                    }
                }

                int
                PadSizeUpToHeaderMultiple(int size)
                {
                    if (0 == size)
                    {
                        return 0;
                    }
                    var newSize = ((size + HEADERSIZE) / HEADERSIZE) * HEADERSIZE;
                    return newSize;
                }

                this.stream = stream;
                int offset = 0;
                this.name = ReadChars(stream, NAMESIZ, ref offset);         // 100
                if (null == this.name)
                {
                    throw new EndOfStreamException();
                }
                this.mode = ReadChars(stream, 8, ref offset);               // 108
                this.uid = ReadChars(stream, 8, ref offset);                // 116
                this.gid = ReadChars(stream, 8, ref offset);                // 124
                this.size = ReadChars(stream, 12, ref offset);              // 136
                this.mtime = ReadChars(stream, 12, ref offset);             // 148
                this.chksum = ReadChars(stream, 8, ref offset);             // 156
                this.typeFlag = ReadChars(stream, 1, ref offset);           // 157
                this.linkname = ReadChars(stream, NAMESIZ, ref offset);     // 257
                this.magic = ReadChars(stream, 8, ref offset);              // 265
                this.uname = ReadChars(stream, TUNMLEN, ref offset);        // 297
                this.gname = ReadChars(stream, TGNMLEN, ref offset);        // 329
                this.devmajor = ReadChars(stream, 8, ref offset);           // 337
                this.devminor = ReadChars(stream, 8, ref offset);           // 345
                this.prefix = ReadChars(stream, 155, ref offset);           // 500

                // cannot just seek ahead, as not all Stream implementations offer it
                this.padding = new byte[HEADERSIZE - offset];
                stream.Read(this.padding, 0, HEADERSIZE - offset);

                this.realSize = CharArrayOctalToInt(this.size);
                this.paddedSize = PadSizeUpToHeaderMultiple(this.realSize);

                this.type = CharArrayToType(this.typeFlag);
            }

            private void
            SkipData(
                int skipSize)
            {
                if (0 == skipSize)
                {
                    return;
                }
                // cannot just seek ahead, as not all Stream implementations offer it
                var temp = new byte[skipSize];
                this.stream.Read(temp, 0, skipSize);
            }

            public void
            WriteToDisk(
                string baseDir)
            {
                string tarPath = null;
                if (NullTerminatedCharArrayToString(this.magic).StartsWith("ustar", System.StringComparison.Ordinal))
                {
                    var prefixString = NullTerminatedCharArrayToString(this.prefix);
                    if (!System.String.IsNullOrEmpty(prefixString))
                    {
                        tarPath = System.IO.Path.Combine(prefixString, NullTerminatedCharArrayToString(this.name));
                    }
                }
                if (null == tarPath)
                {
                    tarPath = NullTerminatedCharArrayToString(this.name);
                }
                if (System.String.IsNullOrEmpty(tarPath))
                {
                    return;
                }
                var path = System.IO.Path.GetFullPath(tarPath, baseDir);
                var parentDir = System.IO.Path.GetDirectoryName(path);
                if (!System.IO.Directory.Exists(parentDir))
                {
                    System.IO.Directory.CreateDirectory(parentDir);
                }

                if (this.IsSymLink)
                {
                    var link = new Mono.Unix.UnixSymbolicLinkInfo(path);
                    if (System.IO.File.Exists(path))
                    {
                        link.Delete(); // equivalent to ln -s -f
                    }

                    var linkPath = NullTerminatedCharArrayToString(this.linkname);
                    var targetPath = System.IO.Path.GetFullPath(linkPath, parentDir);
                    link.CreateSymbolicLinkTo(targetPath);
                }
                else
                {
                    var buffer = new byte[this.realSize];
                    this.stream.Read(buffer, 0, this.realSize);

                    using (var writerStream = System.IO.File.OpenWrite(path))
                    {
                        writerStream.Write(buffer, 0, this.realSize);
                    }
                    var pad = this.paddedSize - this.realSize;
                    this.SkipData(pad);
                }
            }

            public bool IsFile => this.type == Type.File;
            public bool IsDir => this.type == Type.Directory;
            public bool IsSymLink => this.type == Type.Symlink;

            private readonly System.IO.Stream stream;
            private readonly char[] name;
            private readonly char[] mode;
            private readonly char[] uid;
            private readonly char[] gid;
            private readonly char[] size;
            private readonly char[] mtime;
            private readonly char[] chksum;
            private readonly char[] typeFlag;
            private readonly char[] linkname;
            private readonly char[] magic;
            private readonly char[] uname;
            private readonly char[] gname;
            private readonly char[] devmajor;
            private readonly char[] devminor;
            private readonly char[] prefix;
            private readonly byte[] padding;

            private readonly int realSize;
            private readonly int paddedSize;
            private readonly Type type;
        }

        public TarFile(
            System.IO.FileStream stream)
        {
            stream.Seek(0, System.IO.SeekOrigin.Begin);
            var magicNumber = new byte[2];
            stream.Read(magicNumber, 0, 2);
            var gzipCompressed = false;
            if (magicNumber[0] == 0x1f && magicNumber[1] == 0x8b)
            {
                gzipCompressed = true;
            }

            stream.Seek(0, System.IO.SeekOrigin.Begin);
            if (gzipCompressed)
            {
                var temp = System.IO.Path.GetTempFileName();
                var decompressedStream = System.IO.File.Create(temp);
                this.tarStream = new System.IO.Compression.GZipStream(stream, System.IO.Compression.CompressionMode.Decompress);
            }
            else
            {
                this.tarStream = stream;
            }
        }

        public void
        Export(
            string baseDir)
        {
            try
            {
                while (true)
                {
                    var entry = new Header(this.tarStream);
                    if (entry.IsDir)
                    {
                        continue;
                    }
                    entry.WriteToDisk(baseDir);
                }
            }
            catch (Header.EndOfStreamException)
            {
                // done
            }
        }

        void System.IDisposable.Dispose()
        {}
    }
}
