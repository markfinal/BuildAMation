// <copyright file="HeaderDependencyAction.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>C package</summary>
// <author>Mark Final</author>

[assembly: Opus.Core.RegisterAction(typeof(C.HeaderDependencyAction))]

namespace C
{
    public sealed class HeaderDependencyAction : Opus.Core.IAction
    {
        private string Toolchain
        {
            get;
            set;
        }

        string Opus.Core.IAction.CommandLineSwitch
        {
            get
            {
                return "-C.noheaderdeps";
            }
        }

        string Opus.Core.IAction.Description
        {
            get
            {
                return "Disable header dependency generation";
            }
        }

        bool Opus.Core.IAction.Execute()
        {
            if (!Opus.Core.State.HasCategory("C"))
            {
                Opus.Core.State.AddCategory("C");
            }

            Opus.Core.State.Add<bool>("C", "HeaderDependencyGeneration", false);

            return true;
        }
    }
}
