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

            Opus.Core.Target target = node.Target;

            CCompiler compilerInstance = C.CompilerFactory.GetTargetInstance(target, C.ClassNames.CCompilerTool) as CCompiler;

            string cppIncludePath = System.String.Format("{0}/c++/4.1", compilerInstance.IncludeDirectoryPaths(target)[0]);
            if (!System.IO.Directory.Exists(cppIncludePath))
            {
                throw new Opus.Core.Exception(System.String.Format("Gcc C++ include path '{0}' does not exist", cppIncludePath), false);
            }
            string cppIncludePath2 = System.String.Format("{0}/{1}", cppIncludePath, compilerInstance.MachineType(target));
            if (!System.IO.Directory.Exists(cppIncludePath2))
            {
                throw new Opus.Core.Exception(System.String.Format("Gcc C++ include path '{0}' does not exist", cppIncludePath2), false);
            }

            this.SystemIncludePaths.Add(null, cppIncludePath);
            this.SystemIncludePaths.Add(null, cppIncludePath2);
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
