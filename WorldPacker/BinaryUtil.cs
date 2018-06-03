using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;

namespace WorldPacker
{
    /// <summary>
    /// Binary file utilities.
    /// </summary>
    public static class BinaryUtil
    {
        /// <summary>
        /// Get the total length, in bytes, of a file.
        /// </summary>
        /// <param name="file"></param>
        /// <returns></returns>
        public static long GetFileLength(string file)
        {
            using (var reader = new BinaryReader(File.OpenRead(file)))
                return reader.BaseStream.Length;
        }

        /// <summary>
        /// Search for a chunk, return its size, and put its offset into a variable.
        /// </summary>
        /// <param name="file"></param>
        /// <param name="chunkId"></param>
        /// <param name="offset"></param>
        /// <returns></returns>
        public static uint FindChunk(string file, uint chunkId, ref long offset)
        {
            uint readSize = 0;

            using (var reader = new BinaryReader(File.OpenRead(file)))
            {
                while (reader.BaseStream.Position != reader.BaseStream.Length)
                {
                    var readMagic = reader.ReadUInt32();
                    readSize = reader.ReadUInt32();

                    if (readSize != 0 && readMagic == chunkId)
                    {
                        offset = reader.BaseStream.Position - 8;

                        reader.BaseStream.Position = 0;

                        return readSize;
                    }

                    if (readSize != 0)
                        reader.BaseStream.Seek(readSize, SeekOrigin.Current);
                }

                reader.BaseStream.Position = 0;
            }

            return readSize;
        }

        /// <summary>
        /// Calculate the number of bytes of padding needed in order to align to a specific boundary.
        /// </summary>
        /// <param name="num"></param>
        /// <param name="alignTo"></param>
        /// <returns></returns>
        public static long PaddingAlign(long num, int alignTo)
        {
            if (num % alignTo == 0)
            {
                return 0;
            }

            return alignTo - num % alignTo;
        }

        /// <summary>
        /// Read a C-style string from a binary file.
        /// </summary>
        /// <param name="stream"></param>
        /// <returns></returns>
        public static string ReadNullTerminatedString(BinaryReader stream)
        {
            var str = new StringBuilder();
            char ch;
            while ((ch = (char)stream.ReadByte()) != 0)
                str.Append(ch);
            return str.ToString();
        }

        /// <summary>
        /// Read a structure from a binary file.
        /// </summary>
        /// <param name="reader"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static T ReadStruct<T>(BinaryReader reader)
        {
            var bytes = reader.ReadBytes(Marshal.SizeOf(typeof(T)));

            var handle = GCHandle.Alloc(bytes, GCHandleType.Pinned);
            var theStructure = (T)Marshal.PtrToStructure(handle.AddrOfPinnedObject(), typeof(T));
            handle.Free();

            return theStructure;
        }

        /// <summary>
        /// Write a structure to a binary file.
        /// </summary>
        /// <param name="writer"></param>
        /// <param name="instance"></param>
        /// <typeparam name="T"></typeparam>
        public static void WriteStruct<T>(BinaryWriter writer, T instance)
        {
            writer.Write(MarshalStruct(instance));
        }

        /// <summary>
        /// Marshal a structure to a byte array.
        /// </summary>
        /// <param name="instance"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static byte[] MarshalStruct<T>(T instance)
        {
            var size = Marshal.SizeOf(instance);
            var arr = new byte[size];
            var ptr = Marshal.AllocHGlobal(size);

            Marshal.StructureToPtr(instance, ptr, true);
            Marshal.Copy(ptr, arr, 0, size);
            Marshal.FreeHGlobal(ptr);

            return arr;
        }

        /// <summary>
        /// Repeatedly read a struct of a given type from a binary file into a list.
        /// </summary>
        /// <param name="reader"></param>
        /// <param name="size"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static List<T> ReadList<T>(BinaryReader reader, long size)
        {
            var boundary = reader.BaseStream.Position + size;
            var items = new List<T>();
            var itemCount = size / Marshal.SizeOf(typeof(T));

            for (var i = 0; i < itemCount; i++)
                items.Add(ReadStruct<T>(reader));

            return items;
        }

        /// <summary>
        /// Skip padding bytes in a chunk.
        /// </summary>
        /// <param name="reader"></param>
        /// <param name="chunkSize"></param>
        public static void ReadPadding(BinaryReader reader, ref uint chunkSize)
        {
            uint pad = 0;

            while (reader.ReadByte() == 0x11)
            {
                pad++;
            }

            // This is a hack to get around the fact that sometimes padded chunk data actually starts with 0x11...
            // Padding is always even so if we detect uneven padding, we just jump back 2 bytes instead of 1.
            reader.BaseStream.Seek(pad % 2 == 0 ? -1 : -2, SeekOrigin.Current);

            chunkSize -= pad % 2 == 0 ? pad : pad - 1;
        }
    }
}