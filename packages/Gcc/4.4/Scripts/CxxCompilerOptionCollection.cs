// <copyright file="CxxCompilerOptionCollection.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>Gcc package</summary>
// <author>Mark Final</author>
namespace Gcc
{
    // this implementation is here because the specific version of the Gcc compiler exposes a new interface
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
            var machineType = gccToolset.GccDetail.Target;
            var cxxIncludePath = gccToolset.GccDetail.GxxIncludePath;

            if (!System.IO.Directory.Exists(cxxIncludePath))
            {
                throw new Opus.Core.Exception("Gcc C++ include path '{0}' does not exist. Is g++ installed?", cxxIncludePath);
            }
            var cxxIncludePath2 = System.String.Format("{0}/{1}", cxxIncludePath, machineType);
            if (!System.IO.Directory.Exists(cxxIncludePath2))
            {
                throw new Opus.Core.Exception("Gcc C++ include path '{0}' does not exist. Is g++ installed?", cxxIncludePath2);
            }

            var cCompilerOptions = this as C.ICCompilerOptions;
            cCompilerOptions.SystemIncludePaths.Add(cxxIncludePath);
            cCompilerOptions.SystemIncludePaths.Add(cxxIncludePath2);

            GccCommon.CxxCompilerOptionCollection.ExportedDefaults(this, node);
        }

        public CxxCompilerOptionCollection(Opus.Core.DependencyNode node)
            : base(node)
        {
        }
    }
}
