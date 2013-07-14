// <copyright file="QtDBus.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>Qt package</summary>
// <author>Mark Final</author>
namespace Qt
{
    public sealed class DBus : QtCommon.DBus
    {
        public DBus()
            : base(typeof(Qt.Toolset), false)
        {
        }
    }
}
