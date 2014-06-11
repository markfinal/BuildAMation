// <copyright file="Win32Manifest.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>C package</summary>
// <author>Mark Final</author>
namespace C
{
    /// <summary>
    /// C/C++ console application
    /// </summary>
    [Opus.Core.ModuleToolAssignment(typeof(IWinManifestTool))]
    public class Win32Manifest : Opus.Core.BaseModule
    {
        public static readonly Opus.Core.LocationKey OutputDir = new Opus.Core.LocationKey("Win32ManifestOutputDirectory", Opus.Core.ScaffoldLocation.ETypeHint.Directory);
        public static readonly Opus.Core.LocationKey OutputFile = new Opus.Core.LocationKey("Win32ManifestOutputFile", Opus.Core.ScaffoldLocation.ETypeHint.File);

        public Opus.Core.Location BinaryFileLocation
        {
            get
            {
                return this.Locations[OutputFile];
            }
        }
    }
}