// <copyright file="ObjectFileCollection.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>C package</summary>
// <author>Mark Final</author>
namespace XCodeBuilder
{
    public sealed partial class XCodeBuilder
    {
        public object Build(C.ObjectFileCollectionBase moduleToBuild, out bool success)
        {
            Opus.Core.Log.MessageAll("ObjectFileCollectionBase");
            //var data = new XCodeNodeData();
            success = true;
            //return data;
            return null;
        }
    }
}
