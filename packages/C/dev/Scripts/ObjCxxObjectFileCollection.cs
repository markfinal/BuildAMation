// <copyright file="ObjCxxObjectFileCollection.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>C package</summary>
// <author>Mark Final</author>
namespace C.ObjCxx
{
    /// <summary>
    /// ObjectiveC++ object file collection
    /// </summary>
    [Bam.Core.ModuleToolAssignment(typeof(IObjCxxCompilerTool))]
    public class ObjectFileCollection :
        ObjectFileCollectionBase
    {
        public void
        Add(
            ObjectFile objectFile)
        {
            this.list.Add(objectFile);
        }

        protected override System.Collections.Generic.List<Bam.Core.IModule>
        MakeChildModules(
            Bam.Core.LocationArray locationList)
        {
            var objectFileList = new System.Collections.Generic.List<Bam.Core.IModule>();

            foreach (var location in locationList)
            {
                var objectFile = new ObjectFile();
                objectFile.SourceFileLocation = location;
                objectFileList.Add(objectFile);
            }

            return objectFileList;
        }
    }
}
