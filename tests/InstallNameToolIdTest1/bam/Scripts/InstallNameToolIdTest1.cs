namespace InstallNameToolIdTest1
{
    class DynamicLib :
        C.DynamicLibrary
    {
        protected override void
        Init(
            Bam.Core.Module parent)
        {
            base.Init(parent);

            this.CreateCSourceContainer("$(packagedir)/source/*.c");
        }
    }

    sealed class SetId :
        Publisher.IdNameOSX
    {
        protected override void
        Init(
            Bam.Core.Module parent)
        {
            base.Init(parent);

            this.Source = Bam.Core.Graph.Instance.FindReferencedModule<DynamicLib>();

            this.PrivatePatch(settings =>
                {
                    var install_name = settings as Publisher.IInstallNameToolSettings;
                    install_name.NewName = "@loader_path/libDynamicLib.1.dylib";
                });
        }
    }
}
