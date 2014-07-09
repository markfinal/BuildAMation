// <copyright file="PublishDependency.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>Publisher package</summary>
// <author>Mark Final</author>
namespace Publisher
{
    public class PublishDependency
    {
        public PublishDependency(
            Opus.Core.LocationKey key) :
            this(key, string.Empty)
        {
        }

        public PublishDependency(
            Opus.Core.LocationKey key,
            string subdirectory)
        {
            this.Key = key;
            this.SubDirectory = subdirectory;
        }

        public Opus.Core.LocationKey Key
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
