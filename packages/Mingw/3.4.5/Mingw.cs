// <copyright file="Mingw.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>Mingw package</summary>
// <author>Mark Final</author>
[assembly: Opus.Core.RegisterTargetToolChain("C", "mingw", "Mingw.Toolchain.GetVersion")]

[assembly: Opus.Core.RegisterToolset("mingw", typeof(Mingw.Toolset))]

namespace Mingw
{
}
