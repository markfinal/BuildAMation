// <copyright file="ArchiverOptionCollection.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>GccCommon package</summary>
// <author>Mark Final</author>
namespace GccCommon
{
    public partial class ArchiverOptionCollection :
        C.ArchiverOptionCollection,
        C.IArchiverOptions,
        IArchiverOptions
    {
        protected override void
        SetDefaultOptionValues(
            Opus.Core.DependencyNode node)
        {
            base.SetDefaultOptionValues(node);

            var localArchiverOptions = this as IArchiverOptions;
            localArchiverOptions.Command = EArchiverCommand.Replace;
            localArchiverOptions.DoNotWarnIfLibraryCreated = true;

            var cArchiverOptions = this as C.IArchiverOptions;
            // this must be set last, as it appears last on the command line
            cArchiverOptions.OutputType = C.EArchiverOutput.StaticLibrary;
        }

        public
        ArchiverOptionCollection(
            Opus.Core.DependencyNode node) : base(node)
        {}
    }
}
