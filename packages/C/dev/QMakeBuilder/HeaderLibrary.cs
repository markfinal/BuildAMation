// <copyright file="HeaderLibrary.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>C package</summary>
// <author>Mark Final</author>
namespace QMakeBuilder
{
    public sealed partial class QMakeBuilder
    {
        [Opus.Core.EmptyBuildFunction]
        public object Build(C.HeaderLibrary headerLibrary, Opus.Core.DependencyNode node, out bool success)
        {
            // TODO: add a subdirs pro file with just HEADERS?
            success = true;
            return null;
        }
    }
}