namespace ZipTest1
{
    public sealed class ZippedDirectory :
        Publisher.ZipModule
    {
        protected override void
        Init(
            Bam.Core.Module parent)
        {
            // note, set before the base class Init function is called
            this.Macros.Add(
                "pathtozip",
                this.CreateTokenizedString("$(packagedir)/data/dirtozip")
            );

            base.Init(parent);

            this.Macros.AddVerbatim(
                "zipoutputbasename",
                "MyZip"
            );

            this.PrivatePatch(settings =>
                {
                    var zipSettings = settings as Publisher.IZipSettings;
                    zipSettings.RecursivePaths = true;
                    zipSettings.Update = true;
                }
            );
        }
    }
}
