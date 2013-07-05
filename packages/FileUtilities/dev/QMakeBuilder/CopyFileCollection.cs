// <copyright file="CopyFileCollection.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>FileUtilities package</summary>
// <author>Mark Final</author>
namespace QMakeBuilder
{
    public sealed partial class QMakeBuilder2
    {
        [Opus.Core.EmptyBuildFunction]
        public object Build(FileUtilities.CopyFileCollection moduleToBuild, out bool success)
        {
            success = true;
            return null;
        }
    }

    public sealed partial class QMakeBuilder
    {
        [Opus.Core.EmptyBuildFunction]
        public object Build(FileUtilities.CopyFileCollection module, out bool success)
        {
            Opus.Core.Log.MessageAll("TODO: Stub function for QMake support for {0}", module);
            success = false;
            return null;
        }
    }
}
