// <copyright file="PublishDirectory.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>Publisher package</summary>
// <author>Mark Final</author>
namespace Publisher
{
    public class PublishDirectory
    {
        public
        PublishDirectory(
            Bam.Core.Location root,
            string directory) : this(root, directory, null)
        {}

        public
        PublishDirectory(
            Bam.Core.Location root,
            string directory,
            string renamedLeaf)
        {
            this.Root = root;
            this.Directory = directory;
            this.DirectoryLocation = new Bam.Core.ScaffoldLocation(root, directory, Bam.Core.ScaffoldLocation.ETypeHint.Directory, Bam.Core.Location.EExists.WillExist);
            this.RenamedLeaf = renamedLeaf;
        }

        public Bam.Core.Location Root
        {
            get;
            private set;
        }

        public string Directory
        {
            get;
            private set;
        }

        public Bam.Core.Location DirectoryLocation
        {
            get;
            private set;
        }

        public string RenamedLeaf
        {
            get;
            private set;
        }
    }
}
