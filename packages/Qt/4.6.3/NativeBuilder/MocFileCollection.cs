// <copyright file="MocFileCollection.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>Qt package</summary>
// <author>Mark Final</author>
namespace NativeBuilder
{
    public sealed partial class NativeBuilder
    {
        [Opus.Core.EmptyBuildFunction]
        public object Build(Qt.MocFileCollection mocFileCollection, out bool success)
        {
            success = true;
            return null;
        }
    }
}