// <copyright file="ObjectFile.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>C package</summary>
// <author>Mark Final</author>
namespace C
{
    /// <summary>
    /// C object file
    /// </summary>
    [Opus.Core.ModuleToolAssignment(typeof(ICompilerTool))]
    public class ObjectFile : Opus.Core.BaseModule
    {
        private static readonly Opus.Core.LocationKey SourceFileLocationKey = new Opus.Core.LocationKey("SourceFile");
        public static readonly Opus.Core.LocationKey ObjectFileLocationKey = new Opus.Core.LocationKey("ObjectFile");

        public Opus.Core.Location SourceFileLocation
        {
            get
            {
                return this.Locations[SourceFileLocationKey];
            }

            set
            {
                this.Locations[SourceFileLocationKey] = value;
            }
        }

        public void Include(Opus.Core.Location baseLocation, string pattern)
        {
            this.SourceFileLocation = new Opus.Core.ScaffoldLocation(baseLocation, pattern, Opus.Core.ScaffoldLocation.ETypeHint.File);
        }
    }
}