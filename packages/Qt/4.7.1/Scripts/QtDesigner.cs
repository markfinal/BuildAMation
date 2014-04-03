// <copyright file="QtDesigner.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>Qt package</summary>
// <author>Mark Final</author>
namespace Qt
{
    public sealed class Designer : QtCommon.Designer
    {
        public Designer()
            : base(typeof(Qt.Toolset), false)
        {
        }
    }
}
