// <copyright file="SymlinkDirectoryModule.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>FileUtilities package</summary>
// <author>Mark Final</author>
namespace FileUtilities
{
    public class SymlinkDirectory : SymlinkBase
    {
        public void Include(Opus.Core.Location baseLocation, string pattern)
        {
            // TODO: is this right now?
            // don't want to recursively search for files, so just evaluate the Location
            this.SourceFile.Include(baseLocation, pattern, Opus.Core.ScaffoldLocation.ETypeHint.Directory);
        }
    }
}
