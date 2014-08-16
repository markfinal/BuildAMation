// <copyright file="Assembly.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>CSharp package</summary>
// <author>Mark Final</author>
namespace CSharp
{
    [Bam.Core.ModuleToolAssignment(typeof(ICSharpCompilerTool))]
    public abstract class Assembly :
        Bam.Core.BaseModule
    {
        public static readonly Bam.Core.LocationKey OutputFile = new Bam.Core.LocationKey("OutputAssemblyFile", Bam.Core.ScaffoldLocation.ETypeHint.File);
        public static readonly Bam.Core.LocationKey OutputDir = new Bam.Core.LocationKey("OutputAssemblyDirectory", Bam.Core.ScaffoldLocation.ETypeHint.Directory);
        public static readonly Bam.Core.LocationKey PDBFile = new Bam.Core.LocationKey("PDBFile", Bam.Core.ScaffoldLocation.ETypeHint.File);
        public static readonly Bam.Core.LocationKey PDBDir = new Bam.Core.LocationKey("PDBDirectory", Bam.Core.ScaffoldLocation.ETypeHint.Directory);
    }
}
