using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace WorldPacker.Classes
{
    public class WorldLoader : IWorldLoader
    {
        public List<SectionModel> LoadSections(string file)
        {
            var sectionsOffset = 0L;
            var result = BinaryUtil.FindChunk(file, 0x00034110, ref sectionsOffset);

            if (result == 0 || sectionsOffset == 0)
            {
                throw new ArgumentException("Cannot find sections chunk");
            }

            using (var reader = new BinaryReader(File.OpenRead(file)))
            {
                reader.BaseStream.Position = sectionsOffset + 8;

                var list = BinaryUtil.ReadList<SectionStruct>(reader, result);

                return list.Select(s =>
                {
                    var model = new SectionModel();

                    if (s.FileSize1 == 0)
                    {
                        model.IsSpecialSize = true;
                        s.FileSize1 = s.TPKUnknownOffset;
                        s.FileSize2 = s.TPKUnknownOffset2;
                        s.FileSize3 = s.TPKDataOffset;
                    }
                    else
                    {
                        model.IsSpecialSize = false;
                    }

                    model.SectionStruct = s;

                    return model;
                }).ToList();
            }
        }

        public void WriteSections(List<SectionModel> sections, string file)
        {
            var sectionsOffset = 0L;
            var sectionsSize = BinaryUtil.FindChunk(file, 0x00034110, ref sectionsOffset);

            if (sectionsSize == 0 || sectionsOffset == 0)
            {
                throw new ArgumentException("Cannot find sections chunk");
            }

            var fileLength = BinaryUtil.GetFileLength(file);
            var preData = new byte[sectionsOffset]; // data up until the sections chunk
            var postData = new byte[fileLength - (sectionsOffset + 8 + sectionsSize)];

            using (var reader = File.OpenRead(file))
            {
                reader.Read(preData, 0, preData.Length);
                reader.Position = sectionsOffset + 8 + sectionsSize;
                reader.Read(postData, 0, postData.Length);
            }

            using (var writer = new BinaryWriter(File.OpenWrite("output.BUN")))
            {
                writer.Write(preData, 0, preData.Length);

                // Write sections chunk
                writer.Write(0x00034110);
                writer.Write(sections.Count * 208);

                foreach (var section in sections)
                {
                    var referencedFile = StringUtil.ReplaceLastOccurrence(file, "\\", "\\STREAM")
                        .Replace(".BUN", section.SectionStruct.SubSectionID != 0 ? $"_0x{section.SectionStruct.SubSectionID:X8}.BUN" : $"_{section.SectionStruct.StreamChunkNumber}.BUN");
                    var referencedFileLength = (uint) BinaryUtil.GetFileLength(referencedFile);

                    // Build struct, write it, continue
                    var newStruct = new SectionStruct
                    {
                        TPKDataOffset2 = section.SectionStruct.TPKDataOffset2,
                        TPKLastContainerOffset = section.SectionStruct.TPKLastContainerOffset,
                        TPKLastContainerOffset2 = section.SectionStruct.TPKLastContainerOffset2,
                        TPKUnknown1 = section.SectionStruct.TPKUnknown1,
                        TPKUnknownContainerOffset = section.SectionStruct.TPKUnknownContainerOffset,
                        TPKUnknownContainerOffset2 = section.SectionStruct.TPKUnknownContainerOffset2,
                        TPKUnknownOffset = section.SectionStruct.TPKUnknownOffset,
                        TPKUnknownOffset2 = section.SectionStruct.TPKUnknownOffset2,
                        TPKDataOffset = section.SectionStruct.TPKDataOffset,
                        FileSize1 = referencedFileLength,
                        FileSize2 = referencedFileLength,
                        FileSize3 = section.SectionStruct.FileSize3,
                        Name = section.SectionStruct.Name,
                        MasterStreamChunkNumber = section.SectionStruct.MasterStreamChunkNumber,
                        MasterStreamChunkOffset = section.SectionStruct.MasterStreamChunkOffset,
                        StreamChunkHash = section.SectionStruct.StreamChunkHash,
                        StreamChunkNumber = section.SectionStruct.StreamChunkNumber,
                        UnknownChunkNumber = section.SectionStruct.UnknownChunkNumber,
                        Unknown1 = section.SectionStruct.Unknown1,
                        Unknown2 = section.SectionStruct.Unknown2,
                        Unknown3 = section.SectionStruct.Unknown3,
                        Unknown4 = section.SectionStruct.Unknown4,
                        Unknown5 = section.SectionStruct.Unknown5,
                        SubSectionID = section.SectionStruct.SubSectionID,
                        Unknown6 = new uint[16]
                    };

                    if (section.IsSpecialSize)
                    {
                        newStruct.TPKUnknownOffset = referencedFileLength;
                        newStruct.TPKUnknownOffset2 = referencedFileLength;
                        newStruct.TPKDataOffset = section.SectionStruct.FileSize3;

                        newStruct.FileSize1 = 0;
                        newStruct.FileSize2 = 0;
                        newStruct.FileSize3 = 0;
                    }

                    BinaryUtil.WriteStruct(writer, newStruct);
                }

                writer.Write(postData, 0, postData.Length);
            }
        }
    }
}
