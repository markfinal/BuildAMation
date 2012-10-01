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
    [Opus.Core.AssignToolForModule(typeof(Csc),
                                   typeof(ExportCscOptionsDelegateAttribute),
                                   typeof(LocalCscOptionsDelegateAttribute),
                                   "ClassCscOptions")]
    public class Library : Assembly
    {
        [CSharp.LocalCscOptionsDelegate]
        protected static void SetType(Opus.Core.IModule module, Opus.Core.Target target)
        {
            IOptions options = module.Options as IOptions;
            options.Target = ETarget.Library;
        }
    }
}