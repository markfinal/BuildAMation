// <copyright file="ManageCPlusPlusObjectFile.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>VisualCCommon package</summary>
// <author>Mark Final</author>
namespace VisualCCommon
{
    /// <summary>
    /// Managed C++ object file
    /// </summary>
    public class ManagedCPlusPlusObjectFile : C.CPlusPlus.ObjectFile
    {
        [C.LocalCompilerOptionsDelegate]
        private static void ManagedCompilerOptions(Opus.Core.IModule module, Opus.Core.Target target)
        {
            ICCompilerOptions compilerOptions = module.Options as ICCompilerOptions;
            compilerOptions.CompileAsManaged = EManagedCompilation.CLR;
        }
    }

    /// <summary>
    /// Pure Managed C++ object file
    /// </summary>
    public class PureManagedCPlusPlusObjectFile : C.CPlusPlus.ObjectFile
    {
        [C.LocalCompilerOptionsDelegate]
        private static void ManagedCompilerOptions(Opus.Core.IModule module, Opus.Core.Target target)
        {
            ICCompilerOptions compilerOptions = module.Options as ICCompilerOptions;
            compilerOptions.CompileAsManaged = EManagedCompilation.PureCLR;
        }
    }

    /// <summary>
    /// Safe Managed C++ object file
    /// </summary>
    public class SafeManagedCPlusPlusObjectFile : C.CPlusPlus.ObjectFile
    {
        [C.LocalCompilerOptionsDelegate]
        private static void ManagedCompilerOptions(Opus.Core.IModule module, Opus.Core.Target target)
        {
            ICCompilerOptions compilerOptions = module.Options as ICCompilerOptions;
            compilerOptions.CompileAsManaged = EManagedCompilation.SafeCLR;
        }
    }

    /// <summary>
    /// Old Syntax Managed C++ object file
    /// </summary>
    public class OldSyntaxManagedCPlusPlusObjectFile : C.CPlusPlus.ObjectFile
    {
        [C.LocalCompilerOptionsDelegate]
        private static void ManagedCompilerOptions(Opus.Core.IModule module, Opus.Core.Target target)
        {
            ICCompilerOptions compilerOptions = module.Options as ICCompilerOptions;
            compilerOptions.CompileAsManaged = EManagedCompilation.OldSyntaxCLR;
        }
    }
}
