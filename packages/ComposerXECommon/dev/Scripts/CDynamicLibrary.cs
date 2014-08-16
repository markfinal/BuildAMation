// <copyright file="CDynamicLibrary.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>ComposerXECommon package</summary>
// <author>Mark Final</author>
namespace C
{
    public partial class DynamicLibrary
    {
        [LocalCompilerOptionsDelegate]
        protected static void
        ComposerXECommonDynamicLibrarySetPositionIndependentCode(
            Bam.Core.IModule module,
            Bam.Core.Target target)
        {
            if (Bam.Core.OSUtilities.IsUnix(target))
            {
                var compilerOptions = module.Options as ComposerXECommon.ICCompilerOptions;
                if (null != compilerOptions)
                {
                    compilerOptions.PositionIndependentCode = true;
                }
            }
        }
    }
}
