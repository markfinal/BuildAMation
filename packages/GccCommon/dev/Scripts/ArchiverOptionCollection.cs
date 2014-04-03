// <copyright file="ArchiverOptionCollection.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>GccCommon package</summary>
// <author>Mark Final</author>
namespace GccCommon
{
    public partial class ArchiverOptionCollection : C.ArchiverOptionCollection, C.IArchiverOptions, IArchiverOptions
    {
        protected override void InitializeDefaults(Opus.Core.DependencyNode node)
        {
            base.InitializeDefaults(node);

            (this as IArchiverOptions).Command = EArchiverCommand.Replace;
            (this as IArchiverOptions).DoNotWarnIfLibraryCreated = true;

            // this must be set last, as it appears last on the command line
            (this as C.IArchiverOptions).OutputType = C.EArchiverOutput.StaticLibrary;
        }

        public ArchiverOptionCollection(Opus.Core.DependencyNode node)
            : base(node)
        {
        }

        public override Opus.Core.DirectoryCollection DirectoriesToCreate()
        {
            Opus.Core.DirectoryCollection directoriesToCreate = new Opus.Core.DirectoryCollection();

            if (null != this.LibraryFilePath)
            {
                directoriesToCreate.Add(System.IO.Path.GetDirectoryName(this.LibraryFilePath));
            }

            return directoriesToCreate;
        }
    }
}