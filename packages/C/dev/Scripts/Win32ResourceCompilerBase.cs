// <copyright file="Win32ResoureCompilerBase.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>C package</summary>
// <author>Mark Final</author>
namespace C
{
    [System.AttributeUsage(System.AttributeTargets.Method)]
    public class ExportWin32ResourceCompilerOptionsDelegateAttribute : System.Attribute
    {
    }

    [System.AttributeUsage(System.AttributeTargets.Method)]
    public class LocalWin32ResourceCompilerOptionsDelegateAttribute : System.Attribute
    {
    }

    public abstract class Win32ResourceCompilerBase : Opus.Core.ITool
    {
        public abstract string Executable(Opus.Core.Target target);

        public abstract string InputFileSwitch
        {
            get;
        }

        public abstract string OutputFileSwitch
        {
            get;
        }
    }
}
