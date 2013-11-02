// <copyright file="SymlinkFileModule.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>FileUtilities package</summary>
// <author>Mark Final</author>
namespace FileUtilities
{
    public class SymlinkFile : SymlinkBase
    {
        public void Include(Opus.Core.Location baseLocation, string pattern)
        {
            this.SourceFileLocation = new Opus.Core.ScaffoldLocation(baseLocation, pattern, Opus.Core.ScaffoldLocation.ETypeHint.File);
        }
    }
}
