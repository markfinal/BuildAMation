// <copyright file="ArchiverOptionCollection.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>MingwCommon package</summary>
// <author>Mark Final</author>
namespace MingwCommon
{
    public partial class ArchiverOptionCollection :
        C.ArchiverOptionCollection,
        C.IArchiverOptions,
        IArchiverOptions
    {
        protected override void
        SetDefaultOptionValues(
            Bam.Core.DependencyNode node)
        {
            base.SetDefaultOptionValues(node);

            var localArchiverOptions = this as IArchiverOptions;
            localArchiverOptions.Command = EArchiverCommand.Replace;
            localArchiverOptions.DoNotWarnIfLibraryCreated = true;

            // this must be set last, as it appears last on the command line
            (this as C.IArchiverOptions).OutputType = C.EArchiverOutput.StaticLibrary;
        }

        public
        ArchiverOptionCollection(
            Bam.Core.DependencyNode node) : base(node)
        {}
    }
}
