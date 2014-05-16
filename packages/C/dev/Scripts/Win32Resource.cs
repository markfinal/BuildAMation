// <copyright file="Win32Resource.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>C package</summary>
// <author>Mark Final</author>
namespace C
{
    /// <summary>
    /// C/C++ console application
    /// </summary>
    [Opus.Core.ModuleToolAssignment(typeof(IWinResourceCompilerTool))]
    public class Win32Resource : Opus.Core.BaseModule
    {
        public static readonly Opus.Core.LocationKey OutputDir = new Opus.Core.LocationKey("Win32ResourceOutputDirectory", Opus.Core.ScaffoldLocation.ETypeHint.Directory);
        public static readonly Opus.Core.LocationKey OutputFile = new Opus.Core.LocationKey("Win32ResourceOutputFile", Opus.Core.ScaffoldLocation.ETypeHint.File);

        public Opus.Core.Location ResourceFileLocation
        {
            get;
            set;
        }

        public void Include(Opus.Core.Location baseLocation, string pattern)
        {
            this.ResourceFileLocation = new Opus.Core.ScaffoldLocation(baseLocation, pattern, Opus.Core.ScaffoldLocation.ETypeHint.File);
        }
    }
}