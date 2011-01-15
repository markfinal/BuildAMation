// <copyright file="CPlusPlusCompilerOptionCollection.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>Gcc package</summary>
// <author>Mark Final</author>
namespace Gcc
{
    public sealed partial class CPlusPlusCompilerOptionCollection : GccCommon.CPlusPlusCompilerOptionCollection
    {
        protected override void InitializeDefaults(Opus.Core.DependencyNode node)
        {
            base.InitializeDefaults(node);

            CCompiler compilerInstance = C.CompilerFactory.GetTargetInstance(node.Target, C.ClassNames.CCompilerTool) as CCompiler;
            string cppIncludePath = System.IO.Path.Combine(compilerInstance.IncludeDirectoryPaths(node.Target)[0], "c++");
            cppIncludePath = System.IO.Path.Combine(cppIncludePath, "4.4");
            this.SystemIncludePaths.Add(null, cppIncludePath);
            this.SystemIncludePaths.Add(null, System.IO.Path.Combine(cppIncludePath, "i486-linux-gnu"));
        }

        public CPlusPlusCompilerOptionCollection()
            : base()
        {
        }

        public CPlusPlusCompilerOptionCollection(Opus.Core.DependencyNode node)
            : base(node)
        {
        }
    }
}
