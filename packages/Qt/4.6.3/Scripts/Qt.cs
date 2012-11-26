// <copyright file="Qt.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>Qt package</summary>
// <author>Mark Final</author>
namespace Qt
{
    public sealed class Qt : QtCommon.QtCommon
    {
        public Qt(Opus.Core.Target target)
            : base(typeof(Toolset), target)
        {
        }
    }
}