// <copyright file="Gcc.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>Gcc package</summary>
// <author>Mark Final</author>
[assembly: Opus.Core.RegisterTargetToolChain("C", "gcc", "Gcc.Toolchain.VersionString")]

[assembly: Opus.Core.MapToolChainClassTypes("C", "gcc", C.ClassNames.ArchiverTool, typeof(Gcc.Archiver), typeof(Gcc.ArchiverOptionCollection))]
[assembly: Opus.Core.MapToolChainClassTypes("C", "gcc", C.ClassNames.CCompilerTool, typeof(Gcc.CCompiler), typeof(Gcc.CCompilerOptionCollection))]
[assembly: Opus.Core.MapToolChainClassTypes("C", "gcc", C.ClassNames.CPlusPlusCompilerTool, typeof(Gcc.CPlusPlusCompiler), typeof(Gcc.CPlusPlusCompilerOptionCollection))]
[assembly: Opus.Core.MapToolChainClassTypes("C", "gcc", C.ClassNames.LinkerTool, typeof(Gcc.Linker), typeof(Gcc.LinkerOptionCollection))]
[assembly: Opus.Core.MapToolChainClassTypes("C", "gcc", C.ClassNames.Toolchain, typeof(Gcc.Toolchain), typeof(Gcc.ToolchainOptionCollection))]

[assembly: Opus.Core.RegisterToolset("gcc", typeof(Gcc.Toolset))]

namespace Gcc
{
}
