// <copyright file="WindowsExecutable.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>CSharp package</summary>
// <author>Mark Final</author>
namespace CSharp
{
    /// <summary>
    /// C# windows executable
    /// </summary>
    [Opus.Core.AssignToolForModule(typeof(Csc),
                                   typeof(ExportCscOptionsDelegateAttribute),
                                   typeof(LocalCscOptionsDelegateAttribute),
                                   "ClassCscOptions")]
    public class WindowsExecutable : Assembly
    {
        [CSharp.LocalCscOptionsDelegate]
        protected static void SetType(Opus.Core.IModule module, Opus.Core.Target target)
        {
            IOptions options = module.Options as IOptions;
            options.Target = ETarget.WindowsExecutable;
        }
    }
}