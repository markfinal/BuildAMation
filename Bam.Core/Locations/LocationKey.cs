// <copyright file="LocationKey.cs" company="Mark Final">
//  Opus
// </copyright>
// <summary>Opus Core</summary>
// <author>Mark Final</author>
namespace Bam.Core
{
    /// <summary>
    /// Abstraction of the key to the LocationMap
    /// </summary>
    public sealed class LocationKey
    {
        public
        LocationKey(
            string identifier,
            ScaffoldLocation.ETypeHint type)
        {
            this.Identifier = identifier;
            this.Type = type;
        }

        private string Identifier
        {
            get;
            set;
        }

        public ScaffoldLocation.ETypeHint Type
        {
            get;
            private set;
        }

        public bool IsFileKey
        {
            get
            {
                bool isFileKey = (this.Type == ScaffoldLocation.ETypeHint.File);
                return isFileKey;
            }
        }

        public bool IsDirectoryKey
        {
            get
            {
                bool isDirectoryKey = (this.Type == ScaffoldLocation.ETypeHint.Directory);
                return isDirectoryKey;
            }
        }

        public bool IsSymlinkKey
        {
            get
            {
                bool isSymlinkKey = (this.Type == ScaffoldLocation.ETypeHint.Symlink);
                return isSymlinkKey;
            }
        }

        public override string
        ToString()
        {
            return this.Identifier;
        }
    }
}
