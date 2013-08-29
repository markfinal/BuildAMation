// <copyright file="SymlinkDirectoryModule.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>FileUtilities package</summary>
// <author>Mark Final</author>
namespace FileUtilities
{
    public class SymlinkDirectory : SymlinkBase
    {
        public void Include(Opus.Core.Location root, params string[] pathSegments)
        {
            // don't want to recursively search for files, so just evaluate the Location
            var resolvedLocation = new Opus.Core.LocationDirectory(root, pathSegments);
            this.SourceFile.AbsolutePath = resolvedLocation.CachedPath;
        }
    }
}
