// <copyright file="QtSql.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>Qt package</summary>
// <author>Mark Final</author>
namespace Qt
{
    public sealed class Sql : QtCommon.Sql
    {
        public Sql()
            : base(typeof(Qt.Toolset), false)
        {
        }
    }
}
