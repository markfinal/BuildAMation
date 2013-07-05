// <copyright file="HeaderLibrary.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>C package</summary>
// <author>Mark Final</author>
namespace QMakeBuilder
{
    public sealed partial class QMakeBuilder
    {
        public object Build(C.HeaderLibrary headerLibrary, out bool success)
        {
            var headerLibraryModule = headerLibrary as Opus.Core.BaseModule;
            var node = headerLibraryModule.OwningNode;

            var nodeData = new NodeData();
            nodeData.Configuration = GetQtConfiguration(node.Target);

            var proFilePath = QMakeBuilder.GetProFilePath(node);
            System.IO.Directory.CreateDirectory(System.IO.Path.GetDirectoryName(proFilePath));
            nodeData.ProFilePathName = proFilePath;

            using (var proFileWriter = new System.IO.StreamWriter(proFilePath))
            {
                proFileWriter.WriteLine("# --- Written by Opus");

                {
                    var relativePriPathName = Opus.Core.RelativePathUtilities.GetPath(this.DisableQtPriPathName, proFilePath);
                    proFileWriter.WriteLine("include({0})", relativePriPathName.Replace('\\', '/'));
                }

                proFileWriter.WriteLine("TEMPLATE = subdirs");

                // headers
                {
                    var fieldBindingFlags = System.Reflection.BindingFlags.Instance |
                                            System.Reflection.BindingFlags.Public |
                                            System.Reflection.BindingFlags.NonPublic;
                    var fields = headerLibrary.GetType().GetFields(fieldBindingFlags);
                    foreach (var field in fields)
                    {
                        var headerFileAttributes = field.GetCustomAttributes(typeof(C.HeaderFilesAttribute), false);
                        if (headerFileAttributes.Length > 0)
                        {
                            var headerFileCollection = field.GetValue(headerLibrary) as Opus.Core.FileCollection;
                            if (headerFileCollection.Count > 0)
                            {
                                var headersStatement = new System.Text.StringBuilder();
                                headersStatement.AppendFormat("{0}:HEADERS += ", nodeData.Configuration);
                                foreach (string headerPath in headerFileCollection)
                                {
                                    headersStatement.AppendFormat("\\\n\t{0}", headerPath.Replace('\\', '/'));
                                }
                                proFileWriter.WriteLine(headersStatement.ToString());
                            }
                        }
                    }
                }
            }

            success = true;
            return nodeData;
        }
    }
}