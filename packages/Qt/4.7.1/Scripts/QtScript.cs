// <copyright file="QtScript.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>Qt package</summary>
// <author>Mark Final</author>
namespace Qt
{
    public sealed class Script : QtCommon.Script
    {
        public Script()
            : base(typeof(Qt.Toolset), false)
        {
        }
    }
}
