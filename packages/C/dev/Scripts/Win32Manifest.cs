// <copyright file="Win32Manifest.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>C package</summary>
// <author>Mark Final</author>
namespace C
{
    /// <summary>
    /// Windows manifest management
    /// </summary>
    [Bam.Core.ModuleToolAssignment(typeof(IWinManifestTool))]
    public class Win32Manifest :
        Bam.Core.BaseModule
    {
        public static readonly Bam.Core.LocationKey OutputDir = new Bam.Core.LocationKey("Win32ManifestOutputDirectory", Bam.Core.ScaffoldLocation.ETypeHint.Directory);
        public static readonly Bam.Core.LocationKey OutputFile = new Bam.Core.LocationKey("Win32ManifestOutputFile", Bam.Core.ScaffoldLocation.ETypeHint.File);

        public Bam.Core.Location BinaryFileLocation
        {
            get
            {
                return this.Locations[OutputFile];
            }
        }
    }
}
