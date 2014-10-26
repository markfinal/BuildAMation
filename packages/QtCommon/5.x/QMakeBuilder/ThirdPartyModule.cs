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
            QtCommon.ThirdPartyModule moduleToBuild,
            out bool success)
        {
            var node = moduleToBuild.OwningNode;
            var data = new QMakeData(node);

            var moduleType = moduleToBuild.GetType();
            var className = moduleType.FullName.Replace(moduleType.Namespace, string.Empty).Trim('.').ToLower();
            data.QtModules.Add(className);

            success = true;
            return data;
        }
    }
}
