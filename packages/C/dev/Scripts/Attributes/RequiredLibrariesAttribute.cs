// <copyright file="RequiredLibrariesAttribute.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>C package</summary>
// <author>Mark Final</author>
namespace C
{
    [System.AttributeUsage(System.AttributeTargets.Field)]
    public sealed class RequiredLibrariesAttribute :
        Bam.Core.BaseTargetFilteredAttribute,
        Bam.Core.IFieldAttributeProcessor
    {
        void
        Bam.Core.IFieldAttributeProcessor.Execute(
            object sender,
            Bam.Core.IModule module,
            Bam.Core.Target target)
        {
            if (!Bam.Core.TargetUtilities.MatchFilters(target, this))
            {
                return;
            }

            var field = sender as System.Reflection.FieldInfo;
            string[] libraries = null;
            if (field.GetValue(module) is Bam.Core.Array<string>)
            {
                libraries = (field.GetValue(module) as Bam.Core.Array<string>).ToArray();
            }
            else if (field.GetValue(module) is string[])
            {
                libraries = field.GetValue(module) as string[];
            }
            else
            {
                throw new Bam.Core.Exception("RequiredLibrary field in {0} must be of type string[], Bam.Core.StringArray or Bam.Core.Array<string>", module.ToString());
            }

            module.UpdateOptions += delegate(Bam.Core.IModule dlgModule, Bam.Core.Target dlgTarget)
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
