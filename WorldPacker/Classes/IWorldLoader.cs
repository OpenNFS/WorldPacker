using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WorldPacker.Classes
{
    /// <summary>
    /// Service to load the data from L5RA.BUN.
    /// </summary>
    public interface IWorldLoader
    {
        /// <summary>
        /// Load sections from a file.
        /// </summary>
        /// <param name="file"></param>
        /// <returns></returns>
        List<SectionModel> LoadSections(string file);

        /// <summary>
        /// Write sections to a file.
        /// </summary>
        /// <param name="sections"></param>
        /// <param name="file"></param>
        void WriteSections(List<SectionModel> sections, string file);
    }
}
