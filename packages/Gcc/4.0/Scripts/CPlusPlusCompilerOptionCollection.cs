// <copyright file="CPlusPlusCompilerOptionCollection.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>Gcc package</summary>
// <author>Mark Final</author>
namespace Gcc
{
    // this implementation is here because the specific version of the Mingw compiler exposes a new interface
    // and because C# cannot derive from a generic type, this C++ option collection must derive from the specific
    // C option collection
    public sealed partial class CPlusPlusCompilerOptionCollection : CCompilerOptionCollection, C.ICPlusPlusCompilerOptions
    {
        protected override void SetDelegates(Opus.Core.DependencyNode node)
        {
            base.SetDelegates(node);

            this["ExceptionHandler"].PrivateData = new MingwCommon.PrivateData(GccCommon.CPlusPlusCompilerOptionCollection.ExceptionHandlerCommandLine);
        }

        protected override void InitializeDefaults(Opus.Core.DependencyNode node)
        {
            base.InitializeDefaults(node);

            Opus.Core.Target target = node.Target;

            CCompiler compilerInstance = C.CompilerFactory.GetTargetInstance(target, C.ClassNames.CCompilerTool) as CCompiler;

            string cppIncludePath = compilerInstance.GxxIncludePath(target);
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

            GccCommon.CPlusPlusCompilerOptionCollection.ExportedDefaults(this, node);
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
