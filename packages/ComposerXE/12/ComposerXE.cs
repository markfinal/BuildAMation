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
[assembly: Opus.Core.MapToolChainClassTypes("C", "intel", C.ClassNames.Toolchain, typeof(ComposerXE.Toolchain), typeof(ComposerXE.ToolchainOptionCollection))]

namespace ComposerXE
{
}
