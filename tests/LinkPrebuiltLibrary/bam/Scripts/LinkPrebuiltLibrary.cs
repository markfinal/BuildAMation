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

            this.CreateCSourceContainer("$(packagedir)/source/*.c");

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
                }
            });
        }
    }
}
