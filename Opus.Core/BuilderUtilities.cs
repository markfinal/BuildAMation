﻿// <copyright file="BuilderUtilities.cs" company="Mark Final">
//  Opus
// </copyright>
// <summary>Opus Core</summary>
// <author>Mark Final</author>
namespace Opus.Core
{
    public static class BuilderUtilities
    {
        public static void EnsureBuilderPackageExists()
        {
            if (null == State.BuilderName)
            {
                throw new Exception("Name of the Builder has not been specified", false);
            }

            string builderPackageName = System.String.Format("{0}Builder", State.BuilderName);
            foreach (PackageInformation package in State.PackageInfo)
            {
                if (builderPackageName == package.Name)
                {
                    package.IsBuilder = true;
                    State.BuilderPackage = package;
                    return;
                }
            }

            throw new Exception(System.String.Format("Builder package '{0}' was not specified as a dependency", builderPackageName), false);
        }

        public static void CreateBuilderInstance()
        {
            if (null == State.BuilderName)
            {
                throw new Exception("Name of the Builder has not been specified", false);
            }

            if (null == State.ScriptAssembly)
            {
                throw new Exception("Script assembly has not been set", false);
            }

            IBuilder builderInstance = null;
            DeclareBuilderAttribute[] attributes = State.ScriptAssembly.GetCustomAttributes(typeof(DeclareBuilderAttribute), false) as DeclareBuilderAttribute[];
            foreach (DeclareBuilderAttribute attribute in attributes)
            {
                if (attribute.Name == State.BuilderName)
                {
                    builderInstance = BuilderFactory.CreateBuilder(attribute.Type);
                    break;
                }
            }
            if (null == builderInstance)
            {
                throw new Exception(System.String.Format("Unsupported builder '{0}'. Please double check the spelling as the name is case sensitive", State.BuilderName));
            }

            State.BuilderInstance = builderInstance;
        }
    }
}