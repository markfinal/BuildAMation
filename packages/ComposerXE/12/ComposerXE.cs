// <copyright file="ComposerXE.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>ComposerXE package</summary>
// <author>Mark Final</author>
[assembly: Opus.Core.RegisterTargetToolChain("C", "intel", "ComposerXE.Toolchain.VersionString")]

[assembly: Opus.Core.MapToolChainClassTypes("C", "intel", C.ClassNames.ArchiverTool, typeof(ComposerXE.Archiver), typeof(ComposerXE.ArchiverOptionCollection))]
[assembly: Opus.Core.MapToolChainClassTypes("C", "intel", C.ClassNames.CCompilerTool, typeof(ComposerXE.CCompiler), typeof(ComposerXE.CCompilerOptionCollection))]
[assembly: Opus.Core.MapToolChainClassTypes("C", "intel", C.ClassNames.CPlusPlusCompilerTool, typeof(ComposerXE.CPlusPlusCompiler), typeof(ComposerXE.CPlusPlusCompilerOptionCollection))]
[assembly: Opus.Core.MapToolChainClassTypes("C", "intel", C.ClassNames.LinkerTool, typeof(ComposerXE.Linker), typeof(ComposerXE.LinkerOptionCollection))]
#if false
[assembly: Opus.Core.MapToolChainClassTypes("C", "intel", C.ClassNames.Toolchain, typeof(ComposerXE.Toolchain), typeof(ComposerXE.ToolchainOptionCollection))]
#endif

#if false
[assembly: C.RegisterToolchain(
    "intel",
    typeof(ComposerXE.Toolset),
    typeof(ComposerXE.CCompiler), typeof(ComposerXE.CCompilerOptionCollection),
    typeof(ComposerXE.CPlusPlusCompiler), typeof(ComposerXE.CPlusPlusCompilerOptionCollection),
    null, null,
    null, null,
    null, null)]
#endif

namespace ComposerXE
{
    public class Toolset : Opus.Core.IToolset, C.ICompilerInfo
    {
        #region IToolset Members

        string Opus.Core.IToolset.BinPath(Opus.Core.BaseTarget baseTarget)
        {
            throw new System.NotImplementedException();
        }

        Opus.Core.StringArray Opus.Core.IToolset.Environment
        {
            get { throw new System.NotImplementedException(); }
        }

        string Opus.Core.IToolset.InstallPath(Opus.Core.BaseTarget baseTarget)
        {
            throw new System.NotImplementedException();
        }

        string Opus.Core.IToolset.Version(Opus.Core.BaseTarget baseTarget)
        {
            return "12";
        }

        #endregion

        #region ICompilerInfo Members

        string C.ICompilerInfo.PreprocessedOutputSuffix
        {
            get
            {
                return ".i";
            }
        }

        string C.ICompilerInfo.ObjectFileSuffix
        {
            get
            {
                return ".o";
            }
        }

        string C.ICompilerInfo.ObjectFileOutputSubDirectory
        {
            get
            {
                return "obj";
            }
        }

        Opus.Core.StringArray C.ICompilerInfo.IncludePaths(Opus.Core.Target target)
        {
            throw new System.NotImplementedException();
        }

        #endregion
    }
}
