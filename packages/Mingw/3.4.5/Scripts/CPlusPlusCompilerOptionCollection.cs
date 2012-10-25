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

            // NEW STYLE
#if true
            Opus.Core.IToolsetInfo info = Opus.Core.ToolsetInfoFactory.CreateToolsetInfo(typeof(Mingw.ToolsetInfo));

            string cppIncludePath = System.IO.Path.Combine((info as C.ICompilerInfo).IncludePaths(node.Target)[0], "c++");
#else
            CCompiler compilerInstance = C.CompilerFactory.GetTargetInstance(node.Target, C.ClassNames.CCompilerTool) as CCompiler;

            string cppIncludePath = System.IO.Path.Combine((compilerInstance as C.ICompiler).IncludeDirectoryPaths(node.Target)[0], "c++");
#endif
            cppIncludePath = System.IO.Path.Combine(cppIncludePath, "3.4.5");
            (this as C.ICCompilerOptions).SystemIncludePaths.AddAbsoluteDirectory(cppIncludePath, false);
            (this as C.ICCompilerOptions).SystemIncludePaths.AddAbsoluteDirectory(System.IO.Path.Combine(cppIncludePath, "mingw32"), false);
        }
    }
}
