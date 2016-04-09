using Bam.Core;
namespace LinkPrebuiltLibrary
{
    sealed class TestApp :
        C.ConsoleApplication
    {
        protected override void
        Init(
            Bam.Core.Module parent)
        {
            base.Init(parent);

            var source = this.CreateCSourceContainer("$(packagedir)/source/*.c");

            this.PrivatePatch(settings =>
                {
                    var linker = settings as C.ICommonLinkerSettings;
                    if (this.BuildEnvironment.Platform.Includes(Bam.Core.EPlatform.OSX))
                    {
                        linker.Libraries.AddUnique("-lcurses");
                    }
                    else if (this.BuildEnvironment.Platform.Includes(Bam.Core.EPlatform.Linux))
                    {
                        linker.Libraries.AddUnique("-ldl");
                    }
                    else
                    {
                        if (this.Linker is VisualCCommon.LinkerBase)
                        {
                            linker.Libraries.AddUnique("Ws2_32.lib");
                        }
                        else
                        {
                            linker.Libraries.AddUnique("-lws2_32");
                        }
                    }
                });

            if (this.Linker is VisualCCommon.LinkerBase)
            {
                this.CompileAndLinkAgainst<WindowsSDK.WindowsSDK>(source);
            }
        }
    }
}
