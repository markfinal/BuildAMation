// <copyright file="Module.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>CSharp package</summary>
// <author>Mark Final</author>
namespace CSharp
{
    /// <summary>
    /// C# module
    /// </summary>
    public class Module :
        Assembly
    {
        [CSharp.LocalCscOptionsDelegate]
        protected static void
        SetType(
            Opus.Core.IModule module,
            Opus.Core.Target target)
        {
            var options = module.Options as IOptions;
            options.Target = ETarget.Module;
        }
    }
}
