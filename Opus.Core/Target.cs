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
        private static System.Collections.Generic.Dictionary<int, Target> mapNullToolset = new System.Collections.Generic.Dictionary<int, Target>();

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

        private static Target GetInstance(BaseTarget baseTarget)
        {
            Target target = null;
            if (mapNullToolset.ContainsKey(baseTarget.HashKey))
            {
                target = mapNullToolset[baseTarget.HashKey];
            }
            else
            {
                target = mapNullToolset[baseTarget.HashKey] = new Target(baseTarget);
            }

            return target;
        }

        public static Target GetInstance(BaseTarget baseTarget, IToolset toolset)
        {
            if (null == toolset)
            {
                return GetInstance(baseTarget);
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

        private Target(BaseTarget baseTarget)
        {
            this.BaseTarget = baseTarget;
            this.Toolset = null;
            this.Key = baseTarget.ToString();
        }

        private Target(BaseTarget baseTarget, IToolset toolset)
        {
            this.BaseTarget = baseTarget;
            this.Toolset = toolset;
            System.Text.StringBuilder builder = new System.Text.StringBuilder();
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

            bool hasToolset = toolsetType.IsAssignableFrom(this.Toolset.GetType());
            return hasToolset;
        }

        public override string ToString()
        {
            return this.Key;
        }

        public string ToolsetName(char formatter)
        {
            string text = this.Toolset.GetType().Namespace;
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
                // Pascal case
                return BaseTarget.CapitalizeFirstLetter(text);
            }
            else if (formatter == '=')
            {
                return text;
            }
            else
            {
                throw new Exception(System.String.Format("Unknown format specifier '%0'", formatter), false);
            }
        }
    }
}