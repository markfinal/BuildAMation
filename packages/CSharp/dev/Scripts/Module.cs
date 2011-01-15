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
    [Opus.Core.AssignToolForModule(typeof(Csc),
                                   typeof(ExportCscOptionsDelegateAttribute),
                                   typeof(LocalCscOptionsDelegateAttribute),
                                   "ClassCscOptions")]
    public class Module : Assembly
    {
        [CSharp.LocalCscOptionsDelegate]
        protected static void SetType(Opus.Core.IModule module, Opus.Core.Target target)
        {
            OptionCollection options = module.Options as OptionCollection;
            options.Target = ETarget.Module;
        }
    }
}