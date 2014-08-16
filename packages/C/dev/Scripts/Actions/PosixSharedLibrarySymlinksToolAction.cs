// <copyright file="PosixSharedLibrarySymlinksToolAction.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>C package</summary>
// <author>Mark Final</author>

[assembly: Bam.Core.RegisterAction(typeof(C.PosixSharedLibrarySymlinksToolAction))]

namespace C
{
    [Bam.Core.PreambleAction]
    public sealed class PosixSharedLibrarySymlinksToolAction :
        Bam.Core.IActionWithArguments
    {
        public
        PosixSharedLibrarySymlinksToolAction()
        {
            if (!Bam.Core.State.HasCategory("C"))
            {
                Bam.Core.State.AddCategory("C");
            }

            if (!Bam.Core.State.Has("C", "ToolToToolsetName"))
            {
                var map = new System.Collections.Generic.Dictionary<System.Type, string>();
                Bam.Core.State.Add("C", "ToolToToolsetName", map);
            }
        }

        private string PosixSharedLibrarySymlinksTool
        {
            get;
            set;
        }

        void
        Bam.Core.IActionWithArguments.AssignArguments(
            string arguments)
        {
            this.PosixSharedLibrarySymlinksTool = arguments;
        }

        string Bam.Core.IAction.CommandLineSwitch
        {
            get
            {
                return "-C.SOSymLinks";
            }
        }

        string Bam.Core.IAction.Description
        {
            get
            {
                return "Assign the Posix shared library symlinks tool used.";
            }
        }

        bool
        Bam.Core.IAction.Execute()
        {
            Bam.Core.Log.DebugMessage("Posix shared library symlinks tool is '{0}'", this.PosixSharedLibrarySymlinksTool);

            var map = Bam.Core.State.Get("C", "ToolToToolsetName") as System.Collections.Generic.Dictionary<System.Type, string>;
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
