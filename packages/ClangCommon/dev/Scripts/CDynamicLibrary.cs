// <copyright file="CDynamicLibrary.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>ClangCommon package</summary>
// <author>Mark Final</author>
namespace C
{
    public partial class DynamicLibrary
    {
        [LocalCompilerOptionsDelegate]
        protected static void
        ClangCommonDynamicLibrarySetPositionIndependentCode(
            Bam.Core.IModule module,
            Bam.Core.Target target)
        {
            var compilerOptions = module.Options as ClangCommon.ICCompilerOptions;
            if (null != compilerOptions)
            {
                compilerOptions.PositionIndependentCode = true;
            }
        }
    }
}
