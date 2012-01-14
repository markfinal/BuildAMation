// <copyright file="CObjectFileCollection.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>C package</summary>
// <author>Mark Final</author>
namespace QMakeBuilder
{
    public sealed partial class QMakeBuilder
    {
        public object Build(C.ObjectFileCollectionBase objectFileCollection, out bool success)
        {
            Opus.Core.DependencyNode node = objectFileCollection.OwningNode;

            NodeData nodeData = new NodeData();
            nodeData.Configuration = GetQtConfiguration(node.Target);

            foreach (Opus.Core.DependencyNode childNode in node.Children)
            {
                NodeData childData = childNode.Data as NodeData;
                nodeData.Merge(childData);
            }
            if (node.ExternalDependents != null)
            {
                foreach (Opus.Core.DependencyNode externalDependent in node.ExternalDependents)
                {
                    NodeData childData = externalDependent.Data as NodeData;
                    nodeData.Merge(childData);
                }
            }

            success = true;
            return nodeData;
        }
    }
}