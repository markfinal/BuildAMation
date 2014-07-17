// <copyright file="TextFileModule.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>XmlUtilities package</summary>
// <author>Mark Final</author>
namespace XmlUtilities
{
    [Opus.Core.ModuleToolAssignment(typeof(ITextFileTool))]
    public class TextFileModule : Opus.Core.BaseModule
    {
        public static readonly Opus.Core.LocationKey OutputFile = new Opus.Core.LocationKey("TextFile", Opus.Core.ScaffoldLocation.ETypeHint.File);
        public static readonly Opus.Core.LocationKey OutputDir = new Opus.Core.LocationKey("TextFileDir", Opus.Core.ScaffoldLocation.ETypeHint.Directory);

        public TextFileModule()
        {
            this.Content = new System.Text.StringBuilder();
        }

        public System.Text.StringBuilder Content
        {
            get;
            private set;
        }
    }
}
