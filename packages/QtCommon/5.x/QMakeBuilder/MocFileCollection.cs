#region License
// <copyright>
//  Mark Final
// </copyright>
// <author>Mark Final</author>
#endregion // License
namespace QMakeBuilder
{
    public sealed partial class QMakeBuilder
    {
        public object
        Build(
            QtCommon.MocFileCollection moduleToBuild,
            out bool success)
        {
            var node = moduleToBuild.OwningNode;
            var data = new QMakeData(node);
            foreach (var child in node.Children)
            {
                var childData = child.Data as QMakeData;
                if (null != childData)
                {
                    data.Merge(childData);
                }
            }

            success = true;
            return data;
        }
    }
}
