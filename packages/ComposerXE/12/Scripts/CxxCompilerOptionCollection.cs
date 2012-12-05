// <copyright file="CxxCompilerOptionCollection.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>ComposerXE package</summary>
// <author>Mark Final</author>
namespace ComposerXE
{
    // this implementation is here because the specific version of the ComposerXE compiler exposes a new interface
    // and because C# cannot derive from a generic type, this C++ option collection must derive from the specific
    // C option collection
    public sealed partial class CxxCompilerOptionCollection : CCompilerOptionCollection, C.ICxxCompilerOptions
    {
        protected override void InitializeDefaults(Opus.Core.DependencyNode node)
        {
            base.InitializeDefaults(node);

#if false
            Opus.Core.Target target = node.Target;

            CCompiler compilerInstance = C.CompilerFactory.GetTargetInstance(target, C.ClassNames.CCompilerTool) as CCompiler;

            string cppIncludePath = compilerInstance.GxxIncludePath(target);
            if (!System.IO.Directory.Exists(cppIncludePath))
            {
                throw new Opus.Core.Exception(System.String.Format("ComposerXE C++ include path '{0}' does not exist. Is g++ installed?", cppIncludePath), false);
            }
            string cppIncludePath2 = System.String.Format("{0}/{1}", cppIncludePath, compilerInstance.MachineType(target));
            if (!System.IO.Directory.Exists(cppIncludePath2))
            {
                throw new Opus.Core.Exception(System.String.Format("ComposerXE C++ include path '{0}' does not exist. Is g++ installed?", cppIncludePath2), false);
            }

            this.SystemIncludePaths.Add(null, cppIncludePath);
            this.SystemIncludePaths.Add(null, cppIncludePath2);
#endif

            ComposerXECommon.CxxCompilerOptionCollection.ExportedDefaults(this, node);
        }

        public CxxCompilerOptionCollection()
            : base()
        {
        }

        public CxxCompilerOptionCollection(Opus.Core.DependencyNode node)
            : base(node)
        {
        }
    }
}
