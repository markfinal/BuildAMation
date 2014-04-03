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
        protected static void ComposerXECommonDynamicLibrarySetPositionIndependentCode(Opus.Core.IModule module, Opus.Core.Target target)
        {
            if (Opus.Core.OSUtilities.IsUnix(target))
            {
                ComposerXECommon.ICCompilerOptions compilerOptions = module.Options as ComposerXECommon.ICCompilerOptions;
                if (null != compilerOptions)
                {
                    compilerOptions.PositionIndependentCode = true;
                }
            }
        }
    }
}