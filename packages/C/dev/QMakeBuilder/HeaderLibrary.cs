// <copyright file="HeaderLibrary.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>C package</summary>
// <author>Mark Final</author>
namespace QMakeBuilder
{
    public sealed partial class QMakeBuilder
    {
        public object
        Build(
            C.HeaderLibrary moduleToBuild,
            out bool success)
        {
            var data = new QMakeData(moduleToBuild.OwningNode);

            // find headers
            var fieldBindingFlags = System.Reflection.BindingFlags.Instance |
                                    System.Reflection.BindingFlags.Public |
                                    System.Reflection.BindingFlags.NonPublic;
            var fields = moduleToBuild.GetType().GetFields(fieldBindingFlags);
            foreach (var field in fields)
            {
                var headerFileAttributes = field.GetCustomAttributes(typeof(C.HeaderFilesAttribute), false);
                if (headerFileAttributes.Length > 0)
                {
                    var headerFileCollection = field.GetValue(moduleToBuild) as Bam.Core.FileCollection;
                    data.Headers.AddRangeUnique(headerFileCollection.ToStringArray());
                }
            }

            data.Output = QMakeData.OutputType.HeaderLibrary;

            success = true;
            return data;
        }
    }
}
