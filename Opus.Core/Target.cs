// <copyright file="Target.cs" company="Mark Final">
//  Opus
// </copyright>
// <summary>Opus Core</summary>
// <author>Mark Final</author>
namespace Opus.Core
{
    // TODO: instead of storing the toolchain name, store the TYPE of the targetted tool
    // this allows extraction of the versioning information easily
    public sealed class Target //: System.ICloneable, System.IComparable
    {
        // NEW STYLE
#if true
        private static System.Collections.Generic.Dictionary<int, System.Collections.Generic.Dictionary<IToolset, Target>> map = new System.Collections.Generic.Dictionary<int, System.Collections.Generic.Dictionary<IToolset, Target>>();
        private static System.Collections.Generic.Dictionary<int, Target> mapNullToolset = new System.Collections.Generic.Dictionary<int, Target>();
#else
        private static System.Collections.Generic.Dictionary<int, System.Collections.Generic.Dictionary<string, Target>> map = new System.Collections.Generic.Dictionary<int, System.Collections.Generic.Dictionary<string, Target>>();
#endif

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

        // TODO: Make this completely private
        public string Toolchain
        {
            get;
            private set;
        }

        public IToolset Toolset
        {
            get;
            private set;
        }

        // NEW STYLE
#if true
#else
        public static Target GetInstance(BaseTarget baseTarget, string toolchain)
        {
            Target target = null;
            if (!map.ContainsKey(baseTarget.HashKey))
            {
                map[baseTarget.HashKey] = new System.Collections.Generic.Dictionary<string, Target>();
            }
            if (!map[baseTarget.HashKey].ContainsKey(toolchain))
            {
                target = map[baseTarget.HashKey][toolchain] = new Target(baseTarget, toolchain);
            }
            else
            {
                target = map[baseTarget.HashKey][toolchain];
            }

            return target;
        }
#endif

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

        // NEW STYLE
#if true
#else
        private Target(BaseTarget baseTarget, string toolchain)
        {
            this.BaseTarget = baseTarget;
            this.Toolchain = toolchain;
            System.Text.StringBuilder builder = new System.Text.StringBuilder();
            builder.AppendFormat("{0}{1}{2}", baseTarget.ToString(), BaseTarget.ToStringSeparator, toolchain);
            this.Key = builder.ToString();
        }
#endif

        private Target(BaseTarget baseTarget)
        {
            this.BaseTarget = baseTarget;
            this.Toolchain = null;
            this.Toolset = null;
            this.Key = baseTarget.ToString();
        }

        private Target(BaseTarget baseTarget, IToolset toolset)
        {
            this.BaseTarget = baseTarget;
            this.Toolchain = toolset.GetType().Namespace;
            this.Toolset = toolset;
            System.Text.StringBuilder builder = new System.Text.StringBuilder();
            builder.AppendFormat("{0}{1}{2}", baseTarget.ToString(), BaseTarget.ToStringSeparator, this.Toolchain);
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

        // NEW STYLE
        public bool HasToolsetType(System.Type toolsetType)
        {
            // TODO: investigate this for ThirdPartyModule types
            if (null == this.Toolset)
            {
                return false;
            }

            bool hasToolset = toolsetType.IsAssignableFrom(this.Toolset.GetType());
            return hasToolset;
        }

        // TODO: remove this
        public bool HasToolchain(string toolchain)
        {
            bool hasToolchain = System.Text.RegularExpressions.Regex.IsMatch(this.Toolchain.ToLower(), toolchain.ToLower());
            return hasToolchain;
        }

        public override string ToString()
        {
            return this.Key;
        }
    }
}