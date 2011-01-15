// <copyright file="CHeaderLibrary.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>C package</summary>
// <author>Mark Final</author>
namespace VSSolutionBuilder
{
    public sealed partial class VSSolutionBuilder
    {
        [Opus.Core.EmptyBuildFunction]
        public object Build(C.HeaderLibrary headerLibrary, Opus.Core.DependencyNode node, out bool success)
        {
            // TODO: find the files and add to the appropriate collection

#if false
            System.Reflection.BindingFlags fieldBindingFlags = System.Reflection.BindingFlags.Instance |
                                                               System.Reflection.BindingFlags.Public |
                                                               System.Reflection.BindingFlags.NonPublic;
            System.Reflection.FieldInfo[] fields = headerLibrary.GetType().GetFields(fieldBindingFlags);
            foreach (System.Reflection.FieldInfo field in fields)
            {
                var headerFileAttributes = field.GetCustomAttributes(typeof(C.HeaderFilesAttribute), false);
                if (headerFileAttributes.Length > 0)
                {
                    Opus.Core.FileCollection headerFileCollection = field.GetValue(headerLibrary) as Opus.Core.FileCollection;
                    foreach (string headerPath in headerFileCollection)
                    {
                        if (!projectData.HeaderFiles.Contains(headerPath))
                        {
                            ProjectFile headerFile = new ProjectFile(headerPath);
                            projectData.HeaderFiles.Add(headerFile);
                        }
                    }
                }
            }
#endif

            success = true;
            return null;
        }
    }
}