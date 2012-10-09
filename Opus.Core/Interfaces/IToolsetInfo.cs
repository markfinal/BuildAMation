// <copyright file="IToolsetInfo.cs" company="Mark Final">
//  Opus
// </copyright>
// <summary>Opus Core</summary>
// <author>Mark Final</author>
namespace Opus.Core
{
    public interface IToolsetInfo
    {
        string Version(Opus.Core.Target target);
        string InstallPath(Opus.Core.Target target);
        // TODO: could do other stuff like install location
        // bin path
        // etc
        // C.IToolInformation could expose more things like the include path, lib path etc.
    }
}