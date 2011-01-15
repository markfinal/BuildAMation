// <copyright file="RegisterTargetToolChainAttribute.cs" company="Mark Final">
//  Opus
// </copyright>
// <summary>Opus Core</summary>
// <author>Mark Final</author>
namespace Opus.Core
{
    [System.AttributeUsage(System.AttributeTargets.Assembly, AllowMultiple = true)]
    public sealed class RegisterTargetToolChainAttribute : System.Attribute
    {
        private string Initialize(string implementationName, string versionStringFunction)
        {
            string versionString = null;
            System.Type[] assemblyTypes = State.ScriptAssembly.GetTypes();
            foreach (System.Type type in assemblyTypes)
            {
                if (versionStringFunction.StartsWith(type.FullName))
                {
                    string staticFunctionName = versionStringFunction.Replace(type.FullName, null).TrimStart(new char[] { '.' });
                    var staticPropertyInfo = type.GetProperty(staticFunctionName, System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public);
                    if (null != staticPropertyInfo)
                    {
                        if (false != staticPropertyInfo.CanRead)
                        {
                            versionString = staticPropertyInfo.GetGetMethod().Invoke(null, null) as string;
                            break;
                        }
                    }
                }
            }
            if (null == versionString)
            {
                throw new Exception(System.String.Format("Cannot find the toolchain name version property '{0}'", versionStringFunction), false);
            }

            this.ImplementationName = null;
            this.VersionStringDelegate = versionStringFunction;

            return versionString;
        }

        public RegisterTargetToolChainAttribute(string toolchainName, string versionStringFunction)
        {
            string versionString = this.Initialize(null, versionStringFunction);

            State.AddCategory(toolchainName);
            State.Add<string>(toolchainName, "Version", versionString);
        }

        public RegisterTargetToolChainAttribute(string toolchainName, string implementationName, string versionStringFunction)
        {
            if (State.HasCategory(implementationName))
            {
                throw new Exception(System.String.Format("Target ToolChain '{0}' has already been registered", implementationName), false);
            }

            string versionString = this.Initialize(implementationName, versionStringFunction);

            State.AddCategory(implementationName);
            State.Add<string>(implementationName, "Version", versionString);
        }

        public string ImplementationName
        {
            get;
            private set;
        }

        public string VersionStringDelegate
        {
            get;
            private set;
        }

        public static RegisterTargetToolChainAttribute[] TargetToolChains
        {
            get
            {
                RegisterTargetToolChainAttribute[] array = State.ScriptAssembly.GetCustomAttributes(typeof(RegisterTargetToolChainAttribute), false) as Core.RegisterTargetToolChainAttribute[];

                MapToolChainClassTypesAttribute[] toolsMap = State.ScriptAssembly.GetCustomAttributes(typeof(MapToolChainClassTypesAttribute), false) as MapToolChainClassTypesAttribute[];
                if (null == toolsMap)
                {
                    throw new Exception("No tools were registered");
                }

                return array;
            }
        }
    }
}