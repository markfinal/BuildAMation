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
        private static System.Collections.Generic.Dictionary<int, System.Collections.Generic.Dictionary<string, Target>> map = new System.Collections.Generic.Dictionary<int, System.Collections.Generic.Dictionary<string, Target>>();

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

        public static Target GetInstance(BaseTarget baseTarget, string toolchain, IToolset toolset)
        {
            Target target = null;
            if (!map.ContainsKey(baseTarget.HashKey))
            {
                map[baseTarget.HashKey] = new System.Collections.Generic.Dictionary<string, Target>();
            }
            if (!map[baseTarget.HashKey].ContainsKey(toolchain))
            {
                // TODO: change this to an exception
                if (null == toolset)
                {
                    target = map[baseTarget.HashKey][toolchain] = new Target(baseTarget, toolchain);
                }
                else
                {
                    target = map[baseTarget.HashKey][toolchain] = new Target(baseTarget, toolset);
                }
            }
            else
            {
                target = map[baseTarget.HashKey][toolchain];
            }

            return target;
        }

        private Target(BaseTarget baseTarget, string toolchain)
        {
            this.BaseTarget = baseTarget;
            this.Toolchain = toolchain;
            System.Text.StringBuilder builder = new System.Text.StringBuilder();
            builder.AppendFormat("{0}{1}{2}", baseTarget.ToString(), BaseTarget.ToStringSeparator, toolchain);
            this.Key = builder.ToString();
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