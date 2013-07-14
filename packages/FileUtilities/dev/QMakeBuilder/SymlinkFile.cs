// <copyright file="SymLinkFile.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>FileUtilities package</summary>
// <author>Mark Final</author>
namespace QMakeBuilder
{
    public sealed partial class QMakeBuilder
    {
        public object Build(FileUtilities.SymlinkFile moduleToBuild, out bool success)
        {
            Opus.Core.Log.MessageAll("TODO: Stub function for QMake support for {0}", moduleToBuild);
            success = true;
            return null;
        }
    }
}
