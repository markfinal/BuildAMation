// <copyright file="RequiredLibrariesAttribute.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>C package</summary>
// <author>Mark Final</author>
namespace C
{
    [System.AttributeUsage(System.AttributeTargets.Field)]
    public sealed class RequiredLibrariesAttribute :
        Opus.Core.BaseTargetFilteredAttribute,
        Opus.Core.IFieldAttributeProcessor
    {
        void
        Opus.Core.IFieldAttributeProcessor.Execute(
            object sender,
            Opus.Core.IModule module,
            Opus.Core.Target target)
        {
            if (!Opus.Core.TargetUtilities.MatchFilters(target, this))
            {
                return;
            }

            var field = sender as System.Reflection.FieldInfo;
            string[] libraries = null;
            if (field.GetValue(module) is Opus.Core.Array<string>)
            {
                libraries = (field.GetValue(module) as Opus.Core.Array<string>).ToArray();
            }
            else if (field.GetValue(module) is string[])
            {
                libraries = field.GetValue(module) as string[];
            }
            else
            {
                throw new Opus.Core.Exception("RequiredLibrary field in {0} must be of type string[], Opus.Core.StringArray or Opus.Core.Array<string>", module.ToString());
            }

            module.UpdateOptions += delegate(Opus.Core.IModule dlgModule, Opus.Core.Target dlgTarget)
            {
                var linkerOptions = dlgModule.Options as ILinkerOptions;
                foreach (var library in libraries)
                {
                    linkerOptions.Libraries.Add(library);
                }
            };
        }
    }
}
