// <copyright file="Phonon.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>Qt package</summary>
// <author>Mark Final</author>
namespace Qt
{
    public sealed class Phonon : QtCommon.Phonon
    {
        public Phonon()
            : base(typeof(Qt.Toolset), false)
        {
        }
    }
}
