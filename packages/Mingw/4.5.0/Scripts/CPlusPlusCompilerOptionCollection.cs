// <copyright file="CPlusPlusCompilerOptionCollection.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>Mingw package</summary>
// <author>Mark Final</author>
namespace Mingw
{
    public sealed partial class CPlusPlusCompilerOptionCollection : MingwCommon.CPlusPlusCompilerOptionCollection
    {
        public CPlusPlusCompilerOptionCollection()
            : base()
        {
        }

        public CPlusPlusCompilerOptionCollection(Opus.Core.DependencyNode node)
            : base(node)
        {
        }

        protected override void InitializeDefaults(Opus.Core.DependencyNode node)
        {
            base.InitializeDefaults(node);

            CCompiler compilerInstance = C.CompilerFactory.GetTargetInstance(node.Target, C.ClassNames.CCompilerTool) as CCompiler;

            string cppIncludePath = System.IO.Path.Combine(compilerInstance.IncludeDirectoryPaths(node.Target)[1], "c++");
            this.SystemIncludePaths.Add(null, cppIncludePath);
            this.SystemIncludePaths.Add(null, System.IO.Path.Combine(cppIncludePath, "mingw32"));
        }
    }
}
