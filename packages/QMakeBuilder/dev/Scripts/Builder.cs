// <copyright file="Builder.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>QMakeBuilder package</summary>
// <author>Mark Final</author>
namespace QMakeBuilder
{
    public sealed partial class QMakeBuilder : Opus.Core.IBuilder
    {
        private string EmptyConfigPriPath
        {
            get;
            set;
        }
    }
}