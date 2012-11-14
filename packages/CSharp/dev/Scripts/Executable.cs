// <copyright file="Executable.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>CSharp package</summary>
// <author>Mark Final</author>
namespace CSharp
{
    /// <summary>
    /// C# executable
    /// </summary>
    public class Executable : Assembly
    {
        [CSharp.LocalCscOptionsDelegate]
        protected static void SetType(Opus.Core.IModule module, Opus.Core.Target target)
        {
            IOptions options = module.Options as IOptions;
            options.Target = ETarget.Executable;
        }
    }
}