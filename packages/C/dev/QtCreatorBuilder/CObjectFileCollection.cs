// <copyright file="CObjectFileCollection.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>C package</summary>
// <author>Mark Final</author>
namespace QtCreatorBuilder
{
    public sealed partial class QtCreatorBuilder
    {
        public object Build(C.ObjectFileCollectionBase objectFileCollection, Opus.Core.DependencyNode node, out bool success)
        {
            NodeData nodeData = new NodeData();

            foreach (Opus.Core.DependencyNode childNode in node.Children)
            {
                NodeData childData = childNode.Data as NodeData;
                nodeData.Merge(childData);
            }

            success = true;
            return nodeData;
        }
    }
}