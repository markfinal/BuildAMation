// <copyright file="Target.cs" company="Mark Final">
//  Opus
// </copyright>
// <summary>Opus Core</summary>
// <author>Mark Final</author>
namespace Opus.Core
{
    public sealed class Target
    {
        private static System.Collections.Generic.Dictionary<int, System.Collections.Generic.Dictionary<IToolset, Target>> map = new System.Collections.Generic.Dictionary<int, System.Collections.Generic.Dictionary<IToolset, Target>>();

        public string Key
        {
            get;
            private set;
        }

        private BaseTarget BaseTarget
        {
            get;
            set;
        }

        public IToolset Toolset
        {
            get;
            private set;
        }

        public static Target GetInstance(BaseTarget baseTarget, IToolset toolset)
        {
            if (null == toolset)
            {
                throw new Exception("Toolset interface must not be null with the base target '{0}'", baseTarget.ToString());
            }

            Target target = null;
            if (!map.ContainsKey(baseTarget.HashKey))
            {
                map[baseTarget.HashKey] = new System.Collections.Generic.Dictionary<IToolset, Target>();
                target = map[baseTarget.HashKey][toolset] = new Target(baseTarget, toolset);
                return target;
            }

            if (map[baseTarget.HashKey].ContainsKey(toolset))
            {
                target = map[baseTarget.HashKey][toolset];
            }
            else
            {
                target = map[baseTarget.HashKey][toolset] = new Target(baseTarget, toolset);
            }

            return target;
        }

        private Target(BaseTarget baseTarget, IToolset toolset)
        {
            this.BaseTarget = baseTarget;
            this.Toolset = toolset;
            var builder = new System.Text.StringBuilder();
            builder.AppendFormat("{0}{1}{2}", baseTarget.ToString(), BaseTarget.ToStringSeparator, this.Toolset.GetType().Namespace);
            this.Key = builder.ToString();
        }

        public static explicit operator BaseTarget(Target target)
        {
            return target.BaseTarget;
        }

        public bool HasPlatform(EPlatform platforms)
        {
            return this.BaseTarget.HasPlatform(platforms);
        }

        public bool HasConfiguration(EConfiguration configurations)
        {
            return this.BaseTarget.HasConfiguration(configurations);
        }

        public bool HasToolsetType(System.Type toolsetType)
        {
            if (null == this.Toolset)
            {
                if (null == toolsetType)
                {
                    return true;
                }
                return false;
            }
            else if (null == toolsetType)
            {
                return false;
            }

            var hasToolset = toolsetType.IsAssignableFrom(this.Toolset.GetType());
            return hasToolset;
        }

        public override string ToString()
        {
            return this.Key;
        }

        public string ToolsetName(char formatter)
        {
            var text = this.Toolset.GetType().Namespace;
            if (formatter == 'u')
            {
                return text.ToUpper();
            }
            else if (formatter == 'l')
            {
                return text.ToLower();
            }
            else if (formatter == 'p')
            {
                return StringUtilities.CapitalizeFirstLetter(text);
            }
            else if (formatter == '=')
            {
                return text;
            }
            else
            {
                throw new Exception("Unknown format specifier '%0'", formatter);
            }
        }
    }
}