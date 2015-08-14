#region License
// Copyright (c) 2010-2015, Mark Final
// All rights reserved.
//
// Redistribution and use in source and binary forms, with or without
// modification, are permitted provided that the following conditions are met:
//
// * Redistributions of source code must retain the above copyright notice, this
//   list of conditions and the following disclaimer.
//
// * Redistributions in binary form must reproduce the above copyright notice,
//   this list of conditions and the following disclaimer in the documentation
//   and/or other materials provided with the distribution.
//
// * Neither the name of BuildAMation nor the names of its
//   contributors may be used to endorse or promote products derived from
//   this software without specific prior written permission.
//
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS"
// AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE
// IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE
// DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT HOLDER OR CONTRIBUTORS BE LIABLE
// FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL
// DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR
// SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER
// CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY,
// OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE
// OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
#endregion // License
namespace Bam.Core
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

        public static Target
        GetInstance(
            BaseTarget baseTarget,
            IToolset toolset)
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

        private
        Target(
            BaseTarget baseTarget,
            IToolset toolset)
        {
            this.BaseTarget = baseTarget;
            this.Toolset = toolset;
            var builder = new System.Text.StringBuilder();
            builder.AppendFormat("{0}{1}{2}", baseTarget.ToString(), BaseTarget.ToStringSeparator, this.Toolset.GetType().Namespace);
            this.Key = builder.ToString();
        }

        public static explicit operator BaseTarget(
            Target target)
        {
            return target.BaseTarget;
        }

        public bool
        HasPlatform(
            EPlatform platforms)
        {
            return this.BaseTarget.HasPlatform(platforms);
        }

        public bool
        HasConfiguration(
            EConfiguration configurations)
        {
            return this.BaseTarget.HasConfiguration(configurations);
        }

        public bool
        HasToolsetType(
            System.Type toolsetType)
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

        public override string
        ToString()
        {
            return this.Key;
        }

        public string
        ToolsetName(
            char formatter)
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
