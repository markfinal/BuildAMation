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
    public class Executable :
        Assembly
    {
        [CSharp.LocalCscOptionsDelegate]
        protected static void
        SetType(
            Bam.Core.IModule module,
            Bam.Core.Target target)
        {
            var options = module.Options as IOptions;
            options.Target = ETarget.Executable;
        }
    }
}
