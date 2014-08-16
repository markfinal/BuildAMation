// <copyright file="CxxCompilerOptionCollection.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>Mingw package</summary>
// <author>Mark Final</author>
namespace Mingw
{
    // this implementation is here because the specific version of the Mingw compiler exposes a new interface
    // and because C# cannot derive from a generic type, this C++ option collection must derive from the specific
    // C option collection
    public sealed partial class CxxCompilerOptionCollection :
        CCompilerOptionCollection,
        C.ICxxCompilerOptions
    {
        public
        CxxCompilerOptionCollection(
            Bam.Core.DependencyNode node) : base(node)
        {}

        protected override void
        SetDefaultOptionValues(
            Bam.Core.DependencyNode node)
        {
            base.SetDefaultOptionValues(node);

            // TODO: can this be moved to MingwCommon? (difference in root C folders)
            var target = node.Target;
            var mingwToolset = target.Toolset as MingwCommon.Toolset;

            var cCompilerOptions = this as C.ICCompilerOptions;

            // using [1] as we want the one in the lib folder
            var cppIncludePath = System.IO.Path.Combine(mingwToolset.MingwDetail.IncludePaths[1], "c++");
            cCompilerOptions.SystemIncludePaths.Add(cppIncludePath);

            var cppIncludePath2 = System.IO.Path.Combine(cppIncludePath, mingwToolset.MingwDetail.Target);
            cCompilerOptions.SystemIncludePaths.Add(cppIncludePath2);

            MingwCommon.CxxCompilerOptionCollection.ExportedDefaults(this, node);
        }
    }
}
