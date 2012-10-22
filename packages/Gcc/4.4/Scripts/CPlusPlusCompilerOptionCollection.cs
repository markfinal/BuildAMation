// <copyright file="CPlusPlusCompilerOptionCollection.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>Gcc package</summary>
// <author>Mark Final</author>
namespace Gcc
{
    // this implementation is here because the specific version of the Gcc compiler exposes a new interface
    // and because C# cannot derive from a generic type, this C++ option collection must derive from the specific
    // C option collection
    public sealed partial class CPlusPlusCompilerOptionCollection : CCompilerOptionCollection, C.ICPlusPlusCompilerOptions
    {
        protected override void SetDelegates(Opus.Core.DependencyNode node)
        {
            base.SetDelegates(node);

            this["ExceptionHandler"].PrivateData = new GccCommon.PrivateData(GccCommon.CPlusPlusCompilerOptionCollection.ExceptionHandlerCommandLine);
        }

        protected override void InitializeDefaults(Opus.Core.DependencyNode node)
        {
            base.InitializeDefaults(node);

            Opus.Core.Target target = node.Target;

            // NEW STYLE
#if true
            Opus.Core.IToolsetInfo info = Opus.Core.ToolsetInfoFactory.CreateToolsetInfo(typeof(Gcc.ToolsetInfo));
            GccCommon.IGCCInfo gccInfo = info as GccCommon.IGCCInfo;
            
            string cppIncludePath = gccInfo.GxxIncludePath(target);
            string machineType = gccInfo.MachineType(target);
#else
            CCompiler compilerInstance = C.CompilerFactory.GetTargetInstance(target, C.ClassNames.CCompilerTool) as CCompiler;

            string cppIncludePath = compilerInstance.GxxIncludePath(target);
            string machineType = compilerInstance.MachineType(target);
#endif
            if (!System.IO.Directory.Exists(cppIncludePath))
            {
                throw new Opus.Core.Exception(System.String.Format("Gcc C++ include path '{0}' does not exist. Is g++ installed?", cppIncludePath), false);
            }
            string cppIncludePath2 = System.String.Format("{0}/{1}", cppIncludePath, machineType);
            if (!System.IO.Directory.Exists(cppIncludePath2))
            {
                throw new Opus.Core.Exception(System.String.Format("Gcc C++ include path '{0}' does not exist. Is g++ installed?", cppIncludePath2), false);
            }

            (this as C.ICCompilerOptions).SystemIncludePaths.AddAbsoluteDirectory(cppIncludePath, false);
            (this as C.ICCompilerOptions).SystemIncludePaths.AddAbsoluteDirectory(cppIncludePath2, false);

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
