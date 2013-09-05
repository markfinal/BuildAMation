// <copyright file="PostExecute.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>XCodeBuilder package</summary>
// <author>Mark Final</author>
namespace XCodeBuilder
{
    public sealed partial class XCodeBuilder : Opus.Core.IBuilderPostExecute
    {
        private void WriteRoot(System.IO.TextWriter writer)
        {
            writer.WriteLine("// !$*UTF8*$!");
            writer.WriteLine("{");
            writer.WriteLine("\tarchiveVersion = 1;");
            writer.WriteLine("\tclasses = {");
            writer.WriteLine("\t};");
            writer.WriteLine("\tobjectVersion = 46;");
            writer.WriteLine("\tobjects = {");
            (this.Project as IWriteableNode).Write(writer);
            writer.WriteLine("\t};");
            writer.WriteLine("\trootObject = {0} /* Project object */;", this.Project.UUID);
            writer.WriteLine("}");
        }

#region IBuilderPostExecute Members

        void Opus.Core.IBuilderPostExecute.PostExecute(Opus.Core.DependencyNodeCollection executedNodes)
        {
            // cannot write a Byte-Ordering-Mark (BOM) into the project file
            var encoding = new System.Text.UTF8Encoding(false);
            using (var projectFile = new System.IO.StreamWriter(this.ProjectPath, false, encoding) as System.IO.TextWriter)
            {
                this.WriteRoot(projectFile);
            }

            Opus.Core.Log.MessageAll("XCode project written to '{0}'", this.ProjectPath);
        }

#endregion
    }
}
