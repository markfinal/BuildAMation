// <copyright file="Archiver.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>Gcc package</summary>
// <author>Mark Final</author>
namespace Gcc
{
    public sealed class Archiver : GccCommon.Archiver, Opus.Core.IToolSupportsResponseFile
    {
        public Archiver(Opus.Core.Target target)
            : base(target)
        {
        }

        string Opus.Core.IToolSupportsResponseFile.Option
        {
            get
            {
                return "@";
            }
        }
    }
}