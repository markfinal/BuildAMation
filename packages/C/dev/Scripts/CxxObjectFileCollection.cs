// <copyright file="CxxObjectFileCollection.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>C package</summary>
// <author>Mark Final</author>
namespace C.Cxx
{
    /// <summary>
    /// C++ object file collection
    /// </summary>
    [Opus.Core.ModuleToolAssignment(typeof(ICxxCompilerTool))]
    public class ObjectFileCollection : ObjectFileCollectionBase
    {
        public void Add(ObjectFile objectFile)
        {
            this.list.Add(objectFile);
        }

        protected override System.Collections.Generic.List<Opus.Core.IModule> MakeChildModules(Opus.Core.Array<Opus.Core.Location> pathList)
        {
            var objectFileList = new System.Collections.Generic.List<Opus.Core.IModule>();

            foreach (var path in pathList)
            {
                var objectFile = new ObjectFile();
                objectFile.ProxyPath.Assign(this.ProxyPath);
                objectFile.SourceFile.AbsoluteLocation = path;
                objectFileList.Add(objectFile);
            }

            return objectFileList;
        }
    }
}