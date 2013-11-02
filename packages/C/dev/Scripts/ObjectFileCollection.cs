// <copyright file="ObjectFileCollection.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>C package</summary>
// <author>Mark Final</author>
namespace C
{
    /// <summary>
    /// C object file collection
    /// </summary>
    [Opus.Core.ModuleToolAssignment(typeof(ICompilerTool))]
    public class ObjectFileCollection : ObjectFileCollectionBase
    {
        public void Add(ObjectFile objectFile)
        {
            this.list.Add(objectFile);
        }

        protected override System.Collections.Generic.List<Opus.Core.IModule> MakeChildModules(Opus.Core.Array<Opus.Core.Location> locationList)
        {
            var objectFileList = new System.Collections.Generic.List<Opus.Core.IModule>();

            foreach (var location in locationList)
            {
                var objectFile = new ObjectFile();
                // TODO: the proxypath should have already been taken into account by now?
                objectFile.ProxyPath.Assign(this.ProxyPath);
                objectFile.SourceFile.AbsoluteLocation = location;
                objectFileList.Add(objectFile);
            }

            return objectFileList;
        }
    }
}