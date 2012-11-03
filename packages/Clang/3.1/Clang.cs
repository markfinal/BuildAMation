// <copyright file="CCompiler.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>Clang package</summary>
// <author>Mark Final</author>
[assembly: C.RegisterToolchain(
    "clang",
    typeof(Clang.Toolset),
    typeof(Clang.CCompiler), typeof(Clang.CCompilerOptionCollection),
    typeof(Clang.CxxCompiler), typeof(Clang.CxxCompilerOptionCollection),
    null, null,
    null, null,
    null, null)]

namespace Clang
{
    // Add modules here
}
