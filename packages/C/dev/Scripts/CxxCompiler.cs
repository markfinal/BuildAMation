// <copyright file="CxxCompiler.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>C package</summary>
// <author>Mark Final</author>
namespace C
{
    [Opus.Core.LocalAndExportTypesAttribute(typeof(LocalCompilerOptionsDelegateAttribute),
                                            typeof(ExportCompilerOptionsDelegateAttribute))]
    public abstract class CxxCompiler
    {
        public abstract Opus.Core.StringArray IncludeDirectoryPaths(Opus.Core.Target target);

        public abstract Opus.Core.StringArray IncludePathCompilerSwitches
        {
            get;
        }
    }
}
