// <copyright file="ObjectFileCollection.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>C package</summary>
// <author>Mark Final</author>
namespace XCodeBuilder
{
    public sealed partial class XCodeBuilder
    {
        [Opus.Core.EmptyBuildFunction]
        public object Build(C.ObjectFileCollectionBase moduleToBuild, out bool success)
        {
            success = true;
            return null;
        }
    }
}
