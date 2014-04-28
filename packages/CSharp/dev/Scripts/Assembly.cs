// <copyright file="Assembly.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>CSharp package</summary>
// <author>Mark Final</author>
namespace CSharp
{
    [Opus.Core.ModuleToolAssignment(typeof(ICSharpCompilerTool))]
    public abstract class Assembly : Opus.Core.BaseModule
    {
        public static readonly Opus.Core.LocationKey AssemblyLocationKey = new Opus.Core.LocationKey("Assembly");
    }
}
