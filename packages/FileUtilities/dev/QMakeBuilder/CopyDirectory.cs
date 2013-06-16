// <copyright file="CopyDirectory.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>FileUtilities package</summary>
// <author>Mark Final</author>
namespace QMakeBuilder
{
    public sealed partial class QMakeBuilder
    {
        public object Build(FileUtilities.CopyDirectory module, out bool success)
        {
            Opus.Core.Log.MessageAll("TODO: Stub function for QMake support for {0}", module);
            success = true;
            return null;
        }
    }
}
