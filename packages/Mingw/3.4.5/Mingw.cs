// <copyright file="Mingw.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>Mingw package</summary>
// <author>Mark Final</author>
[assembly: Opus.Core.RegisterTargetToolChain("C", "mingw", "Mingw.Toolchain.GetVersion")]
[assembly: Opus.Core.MapToolChainClassTypes("C", "mingw", C.ClassNames.ArchiverTool, typeof(MingwCommon.Archiver), typeof(Mingw.ArchiverOptionCollection))]
#if false
[assembly: Opus.Core.MapToolChainClassTypes("C", "mingw", C.ClassNames.CCompilerTool, typeof(Mingw.CCompiler), typeof(Mingw.CCompilerOptionCollection))]
#endif
[assembly: Opus.Core.MapToolChainClassTypes("C", "mingw", C.ClassNames.CPlusPlusCompilerTool, typeof(Mingw.CPlusPlusCompiler), typeof(Mingw.CPlusPlusCompilerOptionCollection))]
[assembly: Opus.Core.MapToolChainClassTypes("C", "mingw", C.ClassNames.LinkerTool, typeof(Mingw.Linker), typeof(Mingw.LinkerOptionCollection))]
#if false
[assembly: Opus.Core.MapToolChainClassTypes("C", "mingw", C.ClassNames.Toolchain, typeof(Mingw.Toolchain), typeof(Mingw.ToolchainOptionCollection))]
#endif
[assembly: Opus.Core.MapToolChainClassTypes("C", "mingw", C.ClassNames.Win32ResourceCompilerTool, typeof(MingwCommon.Win32ResourceCompiler), typeof(C.Win32ResourceCompilerOptionCollection))]

[assembly: C.RegisterToolchain(
    "mingw",
    typeof(Mingw.Toolset),
    typeof(MingwCommon.CCompiler), typeof(Mingw.CCompilerOptionCollection),
    typeof(Mingw.CPlusPlusCompiler), typeof(Mingw.CPlusPlusCompilerOptionCollection),
    typeof(Mingw.Linker), typeof(Mingw.LinkerOptionCollection),
    typeof(MingwCommon.Archiver), typeof(Mingw.ArchiverOptionCollection),
    typeof(MingwCommon.Win32ResourceCompiler), typeof(C.Win32ResourceCompilerOptionCollection))]

namespace Mingw
{
}
