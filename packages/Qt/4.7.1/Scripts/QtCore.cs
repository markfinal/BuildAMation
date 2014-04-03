// <copyright file="QtCore.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>Qt package</summary>
// <author>Mark Final</author>
namespace Qt
{
    public sealed class Core : QtCommon.Core
    {
        public Core()
            : base(typeof(Qt.Toolset), false)
        {
        }
    }
}
