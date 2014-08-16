// <copyright file="Library.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>CSharp package</summary>
// <author>Mark Final</author>
namespace CSharp
{
    /// <summary>
    /// C# library
    /// </summary>
    public class Library :
        Assembly
    {
        [CSharp.LocalCscOptionsDelegate]
        protected static void
        SetType(
            Bam.Core.IModule module,
            Bam.Core.Target target)
        {
            var options = module.Options as IOptions;
            options.Target = ETarget.Library;
        }
    }
}
