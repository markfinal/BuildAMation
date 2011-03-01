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
    [Opus.Core.AssignToolForModule(typeof(Linker),
                                   typeof(ExportLinkerOptionsDelegateAttribute),
                                   typeof(LocalLinkerOptionsDelegateAttribute),
                                   ClassNames.LinkerToolOptions)]
    public partial class DynamicLibrary : Application
    {
        [LocalCompilerOptionsDelegate]
        protected static void DynamicLibrarySetOpusDLLPreprocessor(Opus.Core.IModule module, Opus.Core.Target target)
        {
            ICCompilerOptions compilerOptions = module.Options as ICCompilerOptions;
            compilerOptions.Defines.Add("OPUS_DYNAMICLIBRARY");
        }

        [LocalLinkerOptionsDelegate]
        protected static void DynamicLibraryEnableDLL(Opus.Core.IModule module, Opus.Core.Target target)
        {
            ILinkerOptions linkerOptions = module.Options as ILinkerOptions;
            linkerOptions.OutputType = ELinkerOutput.DynamicLibrary;
            linkerOptions.DynamicLibrary = true;
        }
    }
}