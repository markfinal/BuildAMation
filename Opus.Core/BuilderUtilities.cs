// <copyright file="BuilderUtilities.cs" company="Mark Final">
//  Opus
// </copyright>
// <summary>Opus Core</summary>
// <author>Mark Final</author>
namespace Opus.Core
{
    public static class BuilderUtilities
    {
        public static void SetBuilderPackage()
        {
            if (null == State.BuilderName)
            {
                return;
            }

            var builderPackageName = System.String.Format("{0}Builder", State.BuilderName);
            foreach (var package in State.PackageInfo)
            {
                if (builderPackageName == package.Name)
                {
                    package.IsBuilder = true;
                    State.BuilderPackage = package;
                    return;
                }
            }

            throw new Exception("Builder package '{0}' was not specified as a dependency", builderPackageName);
        }

        public static void CreateBuilderInstance()
        {
            if (null == State.BuilderName)
            {
                throw new Exception("Name of the Builder has not been specified");
            }

            if (null == State.ScriptAssembly)
            {
                throw new Exception("Script assembly has not been set");
            }

            IBuilder builderInstance = null;
            var attributes = State.ScriptAssembly.GetCustomAttributes(typeof(DeclareBuilderAttribute), false) as DeclareBuilderAttribute[];
            foreach (var attribute in attributes)
            {
                if (attribute.Name == State.BuilderName)
                {
                    builderInstance = BuilderFactory.CreateBuilder(attribute.Type);
                    break;
                }
            }
            if (null == builderInstance)
            {
                throw new Exception("Unsupported builder '{0}'. Please double check the spelling as the name is case sensitive", State.BuilderName);
            }

            State.BuilderInstance = builderInstance;
        }
    }
}