// Automatically generated by Opus v0.00
namespace Test7
{
    // Define module classes here
    class ExplicitDynamicLibrary : C.DynamicLibrary
    {
        public ExplicitDynamicLibrary()
        {
            this.sourceFile.SetRelativePath(this, "source", "dynamiclibrary.c");
            this.sourceFile.UpdateOptions += SetIncludePaths;
        }

        [C.ExportCompilerOptionsDelegate]
        private void SetIncludePaths(Opus.Core.IModule module, Opus.Core.Target target)
        {
            C.ICCompilerOptions compilerOptions = module.Options as C.ICCompilerOptions;
            compilerOptions.IncludePaths.Include(this, "include");
        }

        [Opus.Core.SourceFiles]
        C.ObjectFile sourceFile = new C.ObjectFile();

        [Opus.Core.DependentModules(Platform=Opus.Core.EPlatform.Windows, Toolchains=new string[]{"visualc"})]
        Opus.Core.TypeArray winVCDependents = new Opus.Core.TypeArray(
            typeof(WindowsSDK.WindowsSDK)
        );

        [C.RequiredLibraries(Platform = Opus.Core.EPlatform.Windows, Toolchains = new string[] { "visualc" })]
        Opus.Core.StringArray libraries = new Opus.Core.StringArray(
            "KERNEL32.lib"
        );
    }
}
