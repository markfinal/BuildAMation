// <copyright file="CxxCompilerOptionCollection.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>Mingw package</summary>
// <author>Mark Final</author>
namespace Mingw
{
    public sealed partial class CxxCompilerOptionCollection : MingwCommon.CxxCompilerOptionCollection
    {
        public CxxCompilerOptionCollection()
            : base()
        {
        }

        public CxxCompilerOptionCollection(Opus.Core.DependencyNode node)
            : base(node)
        {
        }

        protected override void InitializeDefaults(Opus.Core.DependencyNode node)
        {
            base.InitializeDefaults(node);

            // TODO: can this be moved to MingwCommon? (difference in root C folders)
            Opus.Core.Target target = node.Target;
            MingwCommon.Toolset mingwToolset = target.Toolset as MingwCommon.Toolset;

            // using [0] as we want the one in the root include folder
            string cppIncludePath = System.IO.Path.Combine(mingwToolset.MingwDetail.IncludePaths[0], "c++");
            (this as C.ICCompilerOptions).SystemIncludePaths.AddAbsoluteDirectory(cppIncludePath, false);

            string cppIncludePath2 = System.IO.Path.Combine(cppIncludePath, mingwToolset.MingwDetail.Version);
            (this as C.ICCompilerOptions).SystemIncludePaths.AddAbsoluteDirectory(cppIncludePath2, false);

            // TODO: commenting these two lines out reveals an error on Mingw Test9-dev
            string cppIncludePath3 = System.IO.Path.Combine(cppIncludePath2, mingwToolset.MingwDetail.Target);
            (this as C.ICCompilerOptions).SystemIncludePaths.AddAbsoluteDirectory(cppIncludePath3, false);
        }
    }
}
