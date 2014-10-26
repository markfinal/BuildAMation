#region License
// <copyright>
//  Mark Final
// </copyright>
// <author>Mark Final</author>
#endregion // License
namespace NativeBuilder
{
    public sealed partial class NativeBuilder
    {
        [Bam.Core.EmptyBuildFunction]
        public object
        Build(
            QtCommon.MocFileCollection moduleToBuild,
            out bool success)
        {
            success = true;
            return null;
        }
    }
}
