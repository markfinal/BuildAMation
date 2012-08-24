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

        private Target(BaseTarget baseTarget, string toolchain)
        {
            this.BaseTarget = baseTarget;
            this.Toolchain = toolchain;
            this.Key = baseTarget.ToString() + "-" + toolchain; // TODO: simplify or remove
        }

        public static explicit operator BaseTarget(Target target)
        {
            return target.BaseTarget;
        }

        public bool HasToolchain(string toolchain)
        {
            bool hasToolchain = System.Text.RegularExpressions.Regex.IsMatch(this.Toolchain.ToLower(), toolchain.ToLower());
            return hasToolchain;
        }

        // THESE ARE TO BE REMOVED - THEY ARE ONLY TO EASE MIGRATION
        public EPlatform Platform
        {
            get
            {
                return this.BaseTarget.PlatformTOREMOVE;
            }
        }

        public EConfiguration Configuration
        {
            get
            {
                return this.BaseTarget.ConfigurationTOREMOVE;
            }
        }

        public bool MatchFilters(ITargetFilters filterInterface)
        {
            return TargetUtilities.MatchFilters(this, filterInterface);
        }

        public bool HasPlatform(EPlatform platforms)
        {
            return this.BaseTarget.HasPlatform(platforms);
        }

        public bool HasConfiguration(EConfiguration configurations)
        {
            return this.BaseTarget.HasConfiguration(configurations);
        }

        public override string ToString()
        {
            return this.Key;
        }
    }
}