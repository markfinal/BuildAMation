// <copyright file="CxxCompilerOptionCollection.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>Mingw package</summary>
// <author>Mark Final</author>
namespace Mingw
{
    public sealed partial class CxxCompilerOptionCollection :
        MingwCommon.CxxCompilerOptionCollection
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

            // using [0] as we want the one in the root include folder
            var cppIncludePath = System.IO.Path.Combine(mingwToolset.MingwDetail.IncludePaths[0], "c++");
            cCompilerOptions.SystemIncludePaths.Add(cppIncludePath);

            var cppIncludePath2 = System.IO.Path.Combine(cppIncludePath, mingwToolset.MingwDetail.Version);
            cCompilerOptions.SystemIncludePaths.Add(cppIncludePath2);

            // TODO: commenting these two lines out reveals an error on Mingw Test9-dev
            var cppIncludePath3 = System.IO.Path.Combine(cppIncludePath2, mingwToolset.MingwDetail.Target);
            cCompilerOptions.SystemIncludePaths.Add(cppIncludePath3);
        }
    }
}
