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
            Opus.Core.DependencyNode node = headerLibrary.OwningNode;

            NodeData nodeData = new NodeData();
            nodeData.Configuration = GetQtConfiguration(node.Target);

            string proFilePath = QMakeBuilder.GetProFilePath(node);
            System.IO.Directory.CreateDirectory(System.IO.Path.GetDirectoryName(proFilePath));
            nodeData.ProFilePathName = proFilePath;

            using (System.IO.TextWriter proFileWriter = new System.IO.StreamWriter(proFilePath))
            {
                proFileWriter.WriteLine("# --- Written by Opus");

                {
                    string relativePriPathName = Opus.Core.RelativePathUtilities.GetPath(this.DisableQtPriPathName, proFilePath);
                    proFileWriter.WriteLine("include({0})", relativePriPathName.Replace('\\', '/'));
                }

                proFileWriter.WriteLine("TEMPLATE = subdirs");

                // headers
                {
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
                            if (headerFileCollection.Count > 0)
                            {
                                System.Text.StringBuilder headersStatement = new System.Text.StringBuilder();
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