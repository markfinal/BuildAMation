// <copyright file="Win32Resource.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>C package</summary>
// <author>Mark Final</author>
namespace C
{
    /// <summary>
    /// Windows resource to be compiled and embedded into a binary
    /// </summary>
    [Bam.Core.ModuleToolAssignment(typeof(IWinResourceCompilerTool))]
    public class Win32Resource :
        Bam.Core.BaseModule
    {
        public static readonly Bam.Core.LocationKey OutputDir = new Bam.Core.LocationKey("Win32ResourceOutputDirectory", Bam.Core.ScaffoldLocation.ETypeHint.Directory);
        public static readonly Bam.Core.LocationKey OutputFile = new Bam.Core.LocationKey("Win32ResourceOutputFile", Bam.Core.ScaffoldLocation.ETypeHint.File);

        public Bam.Core.Location ResourceFileLocation
        {
            get;
            set;
        }

        public void
        Include(
            Bam.Core.Location baseLocation,
            string pattern)
        {
            this.ResourceFileLocation = new Bam.Core.ScaffoldLocation(baseLocation, pattern, Bam.Core.ScaffoldLocation.ETypeHint.File);
        }
    }
}
