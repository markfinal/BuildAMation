// <copyright file="Assembly.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>CSharp package</summary>
// <author>Mark Final</author>
namespace CSharp
{
    [Opus.Core.ModuleToolAssignment(typeof(ICSharpCompilerTool))]
    public abstract class Assembly :
        Opus.Core.BaseModule
    {
        public static readonly Opus.Core.LocationKey OutputFile = new Opus.Core.LocationKey("OutputAssemblyFile", Opus.Core.ScaffoldLocation.ETypeHint.File);
        public static readonly Opus.Core.LocationKey OutputDir = new Opus.Core.LocationKey("OutputAssemblyDirectory", Opus.Core.ScaffoldLocation.ETypeHint.Directory);
        public static readonly Opus.Core.LocationKey PDBFile = new Opus.Core.LocationKey("PDBFile", Opus.Core.ScaffoldLocation.ETypeHint.File);
        public static readonly Opus.Core.LocationKey PDBDir = new Opus.Core.LocationKey("PDBDirectory", Opus.Core.ScaffoldLocation.ETypeHint.Directory);
    }
}
