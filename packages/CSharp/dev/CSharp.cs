// <copyright file="CSharp.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>CSharp package</summary>
// <author>Mark Final</author>
[assembly: Opus.Core.MapToolChainClassTypes("CSharp", "dotnet", "ClassCsc", typeof(CSharp.Csc), typeof(CSharp.OptionCollection))]

[assembly: Opus.Core.RegisterToolset("csharp", typeof(CSharp.Toolset))]

namespace CSharp
{
}
