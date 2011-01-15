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
    [Opus.Core.AssignToolForModule(typeof(Csc),
                                   typeof(ExportCscOptionsDelegateAttribute),
                                   typeof(LocalCscOptionsDelegateAttribute),
                                   "ClassCscOptions")]
    public class Executable : Assembly
    {
        [CSharp.LocalCscOptionsDelegate]
        protected static void SetType(Opus.Core.IModule module, Opus.Core.Target target)
        {
            OptionCollection options = module.Options as OptionCollection;
            options.Target = ETarget.Executable;
        }
    }
}