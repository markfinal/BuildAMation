// <copyright file="HeaderLibrary.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>C package</summary>
// <author>Mark Final</author>
namespace NativeBuilder
{
    public sealed partial class NativeBuilder
    {
        [Opus.Core.EmptyBuildFunction]
        public object Build(C.HeaderLibrary headerLibrary, out bool success)
        {
            success = true;
            return null;
        }
    }
}