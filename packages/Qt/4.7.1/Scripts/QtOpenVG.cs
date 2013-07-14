// <copyright file="QtOpenVG.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>Qt package</summary>
// <author>Mark Final</author>
namespace Qt
{
    public sealed class OpenVG : QtCommon.OpenVG
    {
        public OpenVG()
            : base(typeof(Qt.Toolset), false)
        {
        }
    }
}
