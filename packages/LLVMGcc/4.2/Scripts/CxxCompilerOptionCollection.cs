// <copyright file="CxxCompilerOptionCollection.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>LLVMGcc package</summary>
// <author>Mark Final</author>
namespace LLVMGcc
{
    // this implementation is here because the specific version of the LLVMGcc compiler exposes a new interface
    // and because C# cannot derive from a generic type, this C++ option collection must derive from the specific
    // C option collection
    public sealed partial class CxxCompilerOptionCollection : CCompilerOptionCollection, C.ICxxCompilerOptions
    {
        protected override void SetDefaultOptionValues(Opus.Core.DependencyNode node)
        {
            base.SetDefaultOptionValues(node);

            // TODO: think I can move this to GccCommon, but it misses out the C++ include paths for some reason (see Test9-dev)
            var target = node.Target;
            var gccToolset = target.Toolset as GccCommon.Toolset;
            var cxxIncludePath = gccToolset.GccDetail.GxxIncludePath;

            if (!System.IO.Directory.Exists(cxxIncludePath))
            {
                throw new Opus.Core.Exception("llvm-g++ include path '{0}' does not exist. Is llvm-g++ installed?", cxxIncludePath);
            }

            var cCompilerOptions = this as C.ICCompilerOptions;
            cCompilerOptions.SystemIncludePaths.Add(cxxIncludePath);

            GccCommon.CxxCompilerOptionCollection.ExportedDefaults(this, node);

            // disable long long as it won't compile on C++98 and -pedantic
            if (cCompilerOptions.LanguageStandard != C.ELanguageStandard.Cxx11)
            {
                cCompilerOptions.DisableWarnings.AddUnique("long-long");
            }
        }

        public CxxCompilerOptionCollection(Opus.Core.DependencyNode node)
            : base(node)
        {
        }
    }
}
