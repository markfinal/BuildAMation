// <copyright file="CObjectFileCollection.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>C package</summary>
// <author>Mark Final</author>
namespace QMakeBuilder
{
    public sealed partial class QMakeBuilder2
    {
        public object Build(C.ObjectFileCollectionBase moduleToBuild, out bool success)
        {
            var node = moduleToBuild.OwningNode;
            if (null == node.Children || 0 == node.Children.Count)
            {
                success = true;
                return null;
            }

            var data = new QMakeData(node);
            foreach (var child in node.Children)
            {
                var childData = child.Data as QMakeData;
                if (null != childData)
                {
                    data.Merge(childData);
                }
            }
            if (null != node.ExternalDependents)
            {
                foreach (var dependent in node.ExternalDependents)
                {
                    var depData = dependent.Data as QMakeData;
                    if (null != depData)
                    {
                        data.Merge(depData);
                    }
                }
            }

            success = true;
            return data;
        }
    }

    public sealed partial class QMakeBuilder
    {
        public object Build(C.ObjectFileCollectionBase objectFileCollection, out bool success)
        {
            var objectFileCollectionModule = objectFileCollection as Opus.Core.BaseModule;
            var node = objectFileCollectionModule.OwningNode;

            if ((null == node.Children) || (0 == node.Children.Count))
            {
                success = true;
                return null;
            }

            var nodeData = new NodeData();
            nodeData.Configuration = GetQtConfiguration(node.Target);

            foreach (var childNode in node.Children)
            {
                var childData = childNode.Data as NodeData;
                nodeData.Merge(childData);
            }
            if (node.ExternalDependents != null)
            {
                foreach (var externalDependent in node.ExternalDependents)
                {
                    var childData = externalDependent.Data as NodeData;
                    nodeData.Merge(childData);
                }
            }

            success = true;
            return nodeData;
        }
    }
}