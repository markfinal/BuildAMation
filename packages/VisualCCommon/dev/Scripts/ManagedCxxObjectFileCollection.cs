// <copyright file="ManagedCxxObjectFileCollection.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>VisualCCommon package</summary>
// <author>Mark Final</author>
namespace VisualCCommon
{
    /// <summary>
    /// Managed C++ object file collection
    /// </summary>
    public class ManagedCxxObjectFileCollection :
        C.Cxx.ObjectFileCollection
    {
        [C.LocalCompilerOptionsDelegate]
        private static void
        ManagedCompilerOptions(
            Bam.Core.IModule module,
            Bam.Core.Target target)
        {
            var compilerOptions = module.Options as ICCompilerOptions;
            compilerOptions.CompileAsManaged = EManagedCompilation.CLR;
        }
    }

    /// <summary>
    /// Pure Managed C++ object file collection
    /// </summary>
    public class PureManagedCxxObjectFileCollection :
        C.Cxx.ObjectFileCollection
    {
        [C.LocalCompilerOptionsDelegate]
        private static void
        ManagedCompilerOptions(
            Bam.Core.IModule module,
            Bam.Core.Target target)
        {
            var compilerOptions = module.Options as ICCompilerOptions;
            compilerOptions.CompileAsManaged = EManagedCompilation.PureCLR;
        }
    }

    /// <summary>
    /// Safe Managed C++ object file collection
    /// </summary>
    public class SafeManagedCxxObjectFileCollection :
        C.Cxx.ObjectFileCollection
    {
        [C.LocalCompilerOptionsDelegate]
        private static void
        ManagedCompilerOptions(
            Bam.Core.IModule module,
            Bam.Core.Target target)
        {
            var compilerOptions = module.Options as ICCompilerOptions;
            compilerOptions.CompileAsManaged = EManagedCompilation.SafeCLR;
        }
    }

    /// <summary>
    /// Old Syntax Managed C++ object file collection
    /// </summary>
    public class OldSyntaxManagedCxxObjectFileCollection :
        C.Cxx.ObjectFileCollection
    {
        [C.LocalCompilerOptionsDelegate]
        private static void
        ManagedCompilerOptions(
            Bam.Core.IModule module,
            Bam.Core.Target target)
        {
            var compilerOptions = module.Options as ICCompilerOptions;
            compilerOptions.CompileAsManaged = EManagedCompilation.OldSyntaxCLR;
        }
    }
}
