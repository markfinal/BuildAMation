// <copyright file="QtScriptTools.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>Qt package</summary>
// <author>Mark Final</author>
namespace Qt
{
    public sealed class ScriptTools : QtCommon.ScriptTools
    {
        public ScriptTools()
            : base(typeof(Qt.Toolset), false)
        {
        }
    }
}
