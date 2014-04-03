// <copyright file="QtDeclarative.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>Qt package</summary>
// <author>Mark Final</author>
namespace Qt
{
    public sealed class Declarative : QtCommon.Declarative
    {
        public Declarative()
            : base(typeof(Qt.Toolset), false)
        {
        }
    }
}
