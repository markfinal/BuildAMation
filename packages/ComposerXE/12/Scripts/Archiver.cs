// <copyright file="Archiver.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>Intel package</summary>
// <author>Mark Final</author>
namespace Intel
{
    public sealed class Archiver : IntelCommon.Archiver, Opus.Core.IToolSupportsResponseFile
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