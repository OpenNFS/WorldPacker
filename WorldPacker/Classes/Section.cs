using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace WorldPacker.Classes
{
    public class SectionModel
    {
        /// <summary>
        /// The underlying structure.
        /// </summary>
        public SectionStruct SectionStruct { get; set; }

        /// <summary>
        /// "Special-size" mode
        /// </summary>
        public bool IsSpecialSize { get; set; }
    }

    /// <summary>
    /// The section structure. Found within the <code>10 41 03 00</code> chunk.
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Pack = 1, Size = 208)]
    public struct SectionStruct
    {
        /// <summary>
        /// The section identifier.
        /// </summary>
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 8)]
        public string Name;

        /// <summary>
        /// A rather unusual number format. A31 = 31, B245 = 1425, E474 = 4474.
        /// Probably did some weird string covnersion to int. 
        /// </summary>
        public uint StreamChunkNumber;

        public uint Unknown1;

        /// <summary>
        /// Always zero. There is no master stream.
        /// </summary>
        public uint MasterStreamChunkNumber;

        /// <see cref="MasterStreamChunkNumber"/>
        public uint MasterStreamChunkOffset;

        /// <summary>
        /// Unsure about this one...
        /// </summary>
        public uint StreamChunkHash;

        /// <summary>
        /// Sub-section identifier.
        /// For example, V161 points to 0x1B156C23 in TracksHigh.
        /// The game loads assets from STREAML5RA_0x1B156C23.BUN when it loads V161. 
        /// </summary>
        public uint SubSectionID;

        public uint Unknown2;

        /// <summary>
        /// This is just weird. I don't know what it is.
        /// </summary>
        public uint UnknownChunkNumber;

        /// <summary>
        /// Appears to just be 0x00000000 repeated.
        /// </summary>
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 12)]
        public uint[] Unknown3;

        /// <summary>
        /// Only seen 0xFFFFFFFF in this.
        /// </summary>
        public uint Unknown4;

        /// <summary>
        /// Some sort of type value.
        /// 0x01 has proper FileSize values.
        /// If not 0x01, then FileSize* = 0.
        /// </summary>
        public uint Unknown5;

        /// <summary>
        /// This points to the null chunk that comes before the first unknown TPK container.
        /// </summary>
        public uint TPKUnknownOffset;

        /// <summary>
        /// <see cref="TPKUnknownOffset"/>
        /// </summary>
        public uint TPKUnknownOffset2;

        /// <summary>
        /// Offset to the TPK data chunk.
        /// </summary>
        public uint TPKDataOffset;

        /// <summary>
        /// Points to the 2nd unknown TPK container.
        /// </summary>
        public uint TPKUnknownContainerOffset;

        /// <see cref="TPKUnknownContainerOffset"/>
        public uint TPKUnknownContainerOffset2;

        /// <see cref="TPKDataOffset"/>
        public uint TPKDataOffset2;

        public uint TPKLastContainerOffset;
        public uint TPKLastContainerOffset2;
        public uint TPKUnknown1;

        /// <summary>
        /// This can be 0x00. In that case, the size values can be found after Unknown4+Unknown5.
        /// </summary>
        public uint FileSize1;
        public uint FileSize2;
        public uint FileSize3;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 16)]
        public uint[] Unknown6;
    }
}
