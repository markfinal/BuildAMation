// <copyright file="MocFileCollection.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>QtCommon package</summary>
// <author>Mark Final</author>
namespace QMakeBuilder
{
    public sealed partial class QMakeBuilder
    {
        public object Build(QtCommon.MocFileCollection mocFileCollection, out bool success)
        {
            Opus.Core.IModule mocFileCollectionModule = mocFileCollection as Opus.Core.IModule;
            Opus.Core.DependencyNode node = mocFileCollectionModule.OwningNode;

            NodeData nodeData = new NodeData();
            nodeData.Configuration = GetQtConfiguration(node.Target);

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