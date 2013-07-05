// <copyright file="ThirdPartyModule.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>QtCommon package</summary>
// <author>Mark Final</author>
namespace QMakeBuilder
{
    public sealed partial class QMakeBuilder2
    {
        public object Build(QtCommon.ThirdPartyModule moduleToBuild, out bool success)
        {
            var node = moduleToBuild.OwningNode;
            var data = new QMakeData(node);

            var moduleType = moduleToBuild.GetType();
            string className = moduleType.FullName.Replace(moduleType.Namespace, string.Empty).Trim('.').ToLower();
            data.QtModules.Add(className);

            success = true;
            return data;
        }
    }
}