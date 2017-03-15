#region License
// Copyright (c) 2010-2017, Mark Final
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
using Bam.Core;
namespace ConfigurationTest1
{
#if BAM_FEATURE_MODULE_CONFIGURATION
    // this interface is intentionally read only
    // it is used to query what the current configuration for the module is
    interface IConfigureLibrary :
        Bam.Core.IModuleConfiguration
    {
        bool UseFunkyNewFeature
        {
            get;
        }
    }

    // this class is not abstract, and implements the interface above, additionally implementing a setter for the
    // properties in that interface
    // it is used by the user at the top-most level to set the desired configuration for the module
    // the instance of this class is created just prior to the module's Init() function being called
    sealed class ConfigureLibrary :
        IConfigureLibrary
    {
        public ConfigureLibrary(
            Bam.Core.Environment buildEnv)
        {
            this.UseFunkyNewFeature = false;
        }

        public bool UseFunkyNewFeature
        {
            get;
            set;
        }
    }
#endif

    class ConfigurableLibrary :
        C.StaticLibrary
#if BAM_FEATURE_MODULE_CONFIGURATION
        ,Bam.Core.IHasModuleConfiguration
#endif
    {
#if BAM_FEATURE_MODULE_CONFIGURATION
        System.Type IHasModuleConfiguration.ReadOnlyInterfaceType
        {
            get
            {
                return typeof(IConfigureLibrary);
            }
        }

        System.Type IHasModuleConfiguration.WriteableClassType
        {
            get
            {
                return typeof(ConfigureLibrary);
            }
        }
#endif

        protected override void
        Init(
            Bam.Core.Module parent)
        {
            base.Init(parent);

            this.CreateHeaderContainer("$(packagedir)/include/configurablelibrary/*.h");
            var source = this.CreateCSourceContainer("$(packagedir)/source/library.c");

            this.PublicPatch((settings, appliedTo) =>
                {
                    var compiler = settings as C.ICommonCompilerSettings;
                    if (null != compiler)
                    {
                        compiler.IncludePaths.AddUnique(this.CreateTokenizedString("$(packagedir)/include/configurablelibrary"));
                    }
                });

#if BAM_FEATURE_MODULE_CONFIGURATION
            var config = this.Configuration as IConfigureLibrary;
            if (null != config)
            {
                if (config.UseFunkyNewFeature)
                {
                    source.PrivatePatch(settings =>
                        {
                            var compiler = settings as C.ICommonCompilerSettings;
                            compiler.PreprocessorDefines.Add("D_ENABLE_FUNKY_NEW_FEATURE");
                        });
                    this.CompileAgainst<FunkyFeatureLibrary>(source);
                }
            }
        }
#endif
    }
}
