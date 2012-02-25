// <copyright file="Intel.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>Intel package</summary>
// <author>Mark Final</author>
[assembly: Opus.Core.RegisterTargetToolChain("C", "intel", "Intel.Toolchain.VersionString")]

[assembly: Opus.Core.MapToolChainClassTypes("C", "intel", C.ClassNames.ArchiverTool, typeof(Intel.Archiver), typeof(Intel.ArchiverOptionCollection))]
[assembly: Opus.Core.MapToolChainClassTypes("C", "intel", C.ClassNames.CCompilerTool, typeof(Intel.CCompiler), typeof(Intel.CCompilerOptionCollection))]
[assembly: Opus.Core.MapToolChainClassTypes("C", "intel", C.ClassNames.CPlusPlusCompilerTool, typeof(Intel.CPlusPlusCompiler), typeof(Intel.CPlusPlusCompilerOptionCollection))]
[assembly: Opus.Core.MapToolChainClassTypes("C", "intel", C.ClassNames.LinkerTool, typeof(Intel.Linker), typeof(Intel.LinkerOptionCollection))]
[assembly: Opus.Core.MapToolChainClassTypes("C", "intel", C.ClassNames.Toolchain, typeof(Intel.Toolchain), typeof(Intel.ToolchainOptionCollection))]

namespace Intel
{
}
