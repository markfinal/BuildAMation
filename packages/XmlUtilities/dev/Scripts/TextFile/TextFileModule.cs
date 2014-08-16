// <copyright file="TextFileModule.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>XmlUtilities package</summary>
// <author>Mark Final</author>
namespace XmlUtilities
{
    [Bam.Core.ModuleToolAssignment(typeof(ITextFileTool))]
    public class TextFileModule :
        Bam.Core.BaseModule
    {
        public static readonly Bam.Core.LocationKey OutputFile = new Bam.Core.LocationKey("TextFile", Bam.Core.ScaffoldLocation.ETypeHint.File);
        public static readonly Bam.Core.LocationKey OutputDir = new Bam.Core.LocationKey("TextFileDir", Bam.Core.ScaffoldLocation.ETypeHint.Directory);

        public
        TextFileModule()
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
