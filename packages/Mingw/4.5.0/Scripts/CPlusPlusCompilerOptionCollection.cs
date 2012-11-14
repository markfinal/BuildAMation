// <copyright file="CPlusPlusCompilerOptionCollection.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>Mingw package</summary>
// <author>Mark Final</author>
namespace Mingw
{
    // this implementation is here because the specific version of the Mingw compiler exposes a new interface
    // and because C# cannot derive from a generic type, this C++ option collection must derive from the specific
    // C option collection
    public sealed partial class CPlusPlusCompilerOptionCollection : CCompilerOptionCollection, C.ICPlusPlusCompilerOptions
    {
        protected override void SetDelegates(Opus.Core.DependencyNode node)
        {
            base.SetDelegates(node);

            this["ExceptionHandler"].PrivateData = new MingwCommon.PrivateData(MingwCommon.CPlusPlusCompilerOptionCollection.ExceptionHandlerCommandLine);
        }

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

            // NEW STYLE
#if true
            Opus.Core.IToolset info = Opus.Core.ToolsetFactory.CreateToolset(typeof(Mingw.Toolset));

            C.ICompilerTool compilerTool = info.Tool(typeof(C.ICompilerTool)) as C.ICompilerTool;
            string cppIncludePath = System.IO.Path.Combine(compilerTool.IncludePaths(node.Target)[0], "c++");
#else
            CCompiler compilerInstance = C.CompilerFactory.GetTargetInstance(node.Target, C.ClassNames.CCompilerTool) as CCompiler;

            string cppIncludePath = System.IO.Path.Combine(compilerInstance.IncludeDirectoryPaths(node.Target)[1], "c++");
#endif
            (this as C.ICCompilerOptions).SystemIncludePaths.AddAbsoluteDirectory(cppIncludePath, false);
            (this as C.ICCompilerOptions).SystemIncludePaths.AddAbsoluteDirectory(System.IO.Path.Combine(cppIncludePath, "mingw32"), false);

            MingwCommon.CPlusPlusCompilerOptionCollection.ExportedDefaults(this, node);
        }
    }
}
