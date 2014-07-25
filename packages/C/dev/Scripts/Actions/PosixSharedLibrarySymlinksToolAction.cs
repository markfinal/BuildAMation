// <copyright file="PosixSharedLibrarySymlinksToolAction.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>C package</summary>
// <author>Mark Final</author>

[assembly: Opus.Core.RegisterAction(typeof(C.PosixSharedLibrarySymlinksToolAction))]

namespace C
{
    [Opus.Core.PreambleAction]
    public sealed class PosixSharedLibrarySymlinksToolAction :
        Opus.Core.IActionWithArguments
    {
        public
        PosixSharedLibrarySymlinksToolAction()
        {
            if (!Opus.Core.State.HasCategory("C"))
            {
                Opus.Core.State.AddCategory("C");
            }

            if (!Opus.Core.State.Has("C", "ToolToToolsetName"))
            {
                var map = new System.Collections.Generic.Dictionary<System.Type, string>();
                Opus.Core.State.Add("C", "ToolToToolsetName", map);
            }
        }

        private string PosixSharedLibrarySymlinksTool
        {
            get;
            set;
        }

        void
        Opus.Core.IActionWithArguments.AssignArguments(
            string arguments)
        {
            this.PosixSharedLibrarySymlinksTool = arguments;
        }

        string Opus.Core.IAction.CommandLineSwitch
        {
            get
            {
                return "-C.SOSymLinks";
            }
        }

        string Opus.Core.IAction.Description
        {
            get
            {
                return "Assign the Posix shared library symlinks tool used.";
            }
        }

        bool
        Opus.Core.IAction.Execute()
        {
            Opus.Core.Log.DebugMessage("Posix shared library symlinks tool is '{0}'", this.PosixSharedLibrarySymlinksTool);

            var map = Opus.Core.State.Get("C", "ToolToToolsetName") as System.Collections.Generic.Dictionary<System.Type, string>;
            map[typeof(IPosixSharedLibrarySymlinksTool)] = this.PosixSharedLibrarySymlinksTool;

            return true;
        }

        #region ICloneable Members

        object
        System.ICloneable.Clone()
        {
            return this.MemberwiseClone();
        }

        #endregion
    }
}
