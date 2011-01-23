// <copyright file="HeaderLibrary.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>C package</summary>
// <author>Mark Final</author>
namespace C
{
    /// <summary>
    /// C/C++ header only library
    /// </summary>
    [Opus.Core.AssignToolForModule]
    public class HeaderLibrary : Opus.Core.IModule
    {
        public event Opus.Core.UpdateOptionCollectionDelegate UpdateOptions;

        public Opus.Core.BaseOptionCollection Options
        {
            get;
            set;
        }

        public void ExecuteOptionUpdate(Opus.Core.Target target)
        {
            if (this.UpdateOptions != null)
            {
                this.UpdateOptions(this, target);
            }
        }
    }
}