// <copyright file="QtGui.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>Qt package</summary>
// <author>Mark Final</author>
namespace Qt
{
    public sealed class Gui : QtCommon.Gui
    {
        public Gui()
            : base(typeof(Qt.Toolset), false)
        {
        }
    }
}
