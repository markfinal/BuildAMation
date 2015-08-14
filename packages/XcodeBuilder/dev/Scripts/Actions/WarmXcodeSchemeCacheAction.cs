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

[assembly: Bam.Core.RegisterAction(typeof(XcodeBuilder.WarmXcodeSchemeCacheAction))]

namespace XcodeBuilder
{
    [Bam.Core.PreambleAction]
    public sealed class WarmXcodeSchemeCacheAction :
        Bam.Core.IAction
    {
        public
        WarmXcodeSchemeCacheAction()
        {
            if (!Bam.Core.State.HasCategory("XcodeBuilder"))
            {
                Bam.Core.State.AddCategory("XcodeBuilder");
            }
            Bam.Core.State.Add<bool>("XcodeBuilder", "WarmSchemeCache", false);
        }

        string Bam.Core.IAction.CommandLineSwitch
        {
            get
            {
                return "-warmschemecache";
            }
        }

        string Bam.Core.IAction.Description
        {
            get
            {
                return "Warms Xcode project scheme caches, in order to use xcodebuild on a container workspace without loading it into the Xcode UI";
            }
        }

        bool
        Bam.Core.IAction.Execute()
        {
            Bam.Core.State.Set("XcodeBuilder", "WarmSchemeCache", true);

            Bam.Core.Log.DebugMessage("Xcode project scheme caches will be warmed at the end of the build");

            return true;
        }

        #region ICloneable Members

        object
        System.ICloneable.Clone()
        {
            return this.MemberwiseClone();
        }

        #endregion
    }
}
