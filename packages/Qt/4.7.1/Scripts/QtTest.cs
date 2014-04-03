// <copyright file="QtTest.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>Qt package</summary>
// <author>Mark Final</author>
namespace Qt
{
    public sealed class Test : QtCommon.Test
    {
        public Test()
            : base(typeof(Qt.Toolset), false)
        {
        }
    }
}
