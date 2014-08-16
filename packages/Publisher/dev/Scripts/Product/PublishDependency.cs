// <copyright file="PublishDependency.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>Publisher package</summary>
// <author>Mark Final</author>
namespace Publisher
{
    public class PublishDependency
    {
        public
        PublishDependency(
            Bam.Core.LocationKey key) : this(key, string.Empty)
        {}

        public
        PublishDependency(
            Bam.Core.LocationKey key,
            string subdirectory)
        {
            this.Key = key;
            this.SubDirectory = subdirectory;
        }

        public Bam.Core.LocationKey Key
        {
            get;
            private set;
        }

        public string SubDirectory
        {
            get;
            private set;
        }
    }
}
