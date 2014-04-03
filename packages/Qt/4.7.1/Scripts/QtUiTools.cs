// <copyright file="QtUiTools.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>Qt package</summary>
// <author>Mark Final</author>
namespace Qt
{
    public sealed class UiTools : QtCommon.UiTools
    {
        public UiTools()
            : base(typeof(Qt.Toolset), false)
        {
        }
    }
}
