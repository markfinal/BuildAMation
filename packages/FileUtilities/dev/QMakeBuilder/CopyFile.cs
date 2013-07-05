// <copyright file="CopyFile.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>FileUtilities package</summary>
// <author>Mark Final</author>
namespace QMakeBuilder
{
    public sealed partial class QMakeBuilder2
    {
        public object Build(FileUtilities.CopyFile moduleToBuild, out bool success)
        {
            success = true;
            return null;
        }
    }

    public sealed partial class QMakeBuilder
    {
        public object Build(FileUtilities.CopyFile module, out bool success)
        {
            var owningNode = module.OwningNode;
            var besideModuleType = module.BesideModuleType;
            if (null == besideModuleType)
            {
                Opus.Core.Log.MessageAll("QMake support for copying to arbitrary locations is unavailable");
                success = true;
                return null;
            }

            var besideModuleNode = Opus.Core.ModuleUtilities.GetNode(besideModuleType, (Opus.Core.BaseTarget)owningNode.Target);

            Opus.Core.Log.MessageAll("TODO: Stub function for QMake support for {0}", module);

            success = true;
            return null;
        }
    }
}
