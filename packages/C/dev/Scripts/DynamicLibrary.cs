// <copyright file="DynamicLibrary.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>C package</summary>
// <author>Mark Final</author>
namespace C
{
    /// <summary>
    /// C/C++ dynamic library
    /// </summary>
    public partial class DynamicLibrary : Application
    {
        [LocalCompilerOptionsDelegate]
        protected static void DynamicLibrarySetOpusDLLPreprocessor(Opus.Core.IModule module, Opus.Core.Target target)
        {
            var compilerOptions = module.Options as ICCompilerOptions;
            compilerOptions.Defines.Add("OPUS_DYNAMICLIBRARY");
        }

        [LocalLinkerOptionsDelegate]
        protected static void DynamicLibraryEnableDLL(Opus.Core.IModule module, Opus.Core.Target target)
        {
            var linkerOptions = module.Options as ILinkerOptions;
            linkerOptions.OutputType = ELinkerOutput.DynamicLibrary;
            linkerOptions.DynamicLibrary = true;

            if (module.Options is ILinkerOptionsOSX)
            {
                (module.Options as ILinkerOptionsOSX).SuppressReadOnlyRelocations = true;
            }
        }
    }
}
