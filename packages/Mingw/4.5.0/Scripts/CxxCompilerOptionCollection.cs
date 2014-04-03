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
    public sealed partial class CxxCompilerOptionCollection : CCompilerOptionCollection, C.ICxxCompilerOptions
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

            // using [1] as we want the one in the lib folder
            string cppIncludePath = System.IO.Path.Combine(mingwToolset.MingwDetail.IncludePaths[1], "c++");
            (this as C.ICCompilerOptions).SystemIncludePaths.Add(cppIncludePath);

            string cppIncludePath2 = System.IO.Path.Combine(cppIncludePath, mingwToolset.MingwDetail.Target);
            (this as C.ICCompilerOptions).SystemIncludePaths.Add(cppIncludePath2);

            MingwCommon.CxxCompilerOptionCollection.ExportedDefaults(this, node);
        }
    }
}
