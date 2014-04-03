// <copyright file="QtOpenGL.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>Qt package</summary>
// <author>Mark Final</author>
namespace Qt
{
    public sealed class OpenGL : QtCommon.OpenGL
    {
        public OpenGL()
            : base(typeof(Qt.Toolset), false)
        {
        }
    }
}
