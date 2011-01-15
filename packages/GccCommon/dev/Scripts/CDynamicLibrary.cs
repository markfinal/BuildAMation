// <copyright file="CDynamicLibrary.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>GccCommon package</summary>
// <author>Mark Final</author>
namespace C
{
    public partial class DynamicLibrary
    {
        [LocalCompilerOptionsDelegate]
        private static void DynamicLibrarySetPositionIndependentCode(Opus.Core.IModule module, Opus.Core.Target target)
        {
            if (Opus.Core.OSUtilities.IsUnix(target.Platform))
            {
                GccCommon.ICCompilerOptions compilerOptions = module.Options as GccCommon.ICCompilerOptions;
                compilerOptions.PositionIndependentCode = true;
            }
        }
    }
}