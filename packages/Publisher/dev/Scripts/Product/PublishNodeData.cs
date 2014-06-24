// <copyright file="PublishNodeData.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>Publisher package</summary>
// <author>Mark Final</author>
namespace Publisher
{
    public class PublishNodeData
    {
        public PublishNodeData(
            System.Type moduleType,
            Opus.Core.LocationKey key)
        {
            this.ModuleType = moduleType;
            this.Key = key;
        }

        public System.Type ModuleType
        {
            get;
            private set;
        }

        public Opus.Core.LocationKey Key
        {
            get;
            private set;
        }
    }
}
