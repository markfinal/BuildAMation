// <copyright file="RequiredLibrariesAttribute.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>C package</summary>
// <author>Mark Final</author>
namespace C
{
    [System.AttributeUsage(System.AttributeTargets.Field)]
    public sealed class RequiredLibrariesAttribute : System.Attribute, Opus.Core.ITargetFilters, Opus.Core.IFieldAttributeProcessor
    {
        public RequiredLibrariesAttribute()
        {
            this.TargetFilters = new string[] { ".*-.*-.*" };
        }

        public RequiredLibrariesAttribute(string filter)
        {
            this.TargetFilters = new string[] { filter };
        }

        public RequiredLibrariesAttribute(string[] filters)
        {
            this.TargetFilters = filters;
        }

        public string[] TargetFilters
        {
            get;
            set;
        }

        void Opus.Core.IFieldAttributeProcessor.Execute(object sender, Opus.Core.IModule module, Opus.Core.Target target)
        {
            if (!target.MatchFilters(this.TargetFilters))
            {
                return;
            }

            System.Reflection.FieldInfo field = sender as System.Reflection.FieldInfo;
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
                throw new Opus.Core.Exception(System.String.Format("RequiredLibrary field in {0} must be of type string[], Opus.Core.StringArray or Opus.Core.Array<string>", module.ToString()), false);
            }

            module.UpdateOptions += delegate(Opus.Core.IModule dlgModule, Opus.Core.Target dlgTarget)
            {
                ILinkerOptions linkerOptions = dlgModule.Options as ILinkerOptions;
                foreach (string library in libraries)
                {
                    linkerOptions.Libraries.Add(library);
                }
            };
        }
    }
}
