#region License
// Copyright (c) 2010-2018, Mark Final
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
namespace C.DefaultSettings
{
    public static partial class DefaultSettingsExtensions
    {
        public static void
        Defaults(
            this C.ICommonHasCompilerPreprocessedOutputPath settings,
            Bam.Core.Module module)
        {
            if (module is Bam.Core.IModuleGroup)
            {
                return;
            }
            if (module is ObjectFileBase)
            {
                settings.PreprocessedOutputPath = null;
            }
            else
            {
                throw new Bam.Core.Exception(
                    "Module type {0} is not recognised to be able to determine its PreprocessedOutputPath",
                    module.GetType().ToString()
                );
            }
        }

        public static void
        Empty(
            this C.ICommonHasCompilerPreprocessedOutputPath settings)
        {
            settings.PreprocessedOutputPath = null;
        }

        public static void
        Intersect(
            this C.ICommonHasCompilerPreprocessedOutputPath shared,
            C.ICommonHasCompilerPreprocessedOutputPath other)
        {
            if (shared.PreprocessedOutputPath != null && other.PreprocessedOutputPath != null)
            {
                shared.PreprocessedOutputPath = shared.PreprocessedOutputPath.Equals(other.PreprocessedOutputPath) ? shared.PreprocessedOutputPath : null;
            }
            else
            {
                shared.PreprocessedOutputPath = null;
            }
        }

        public static void
        Delta(
            this C.ICommonHasCompilerPreprocessedOutputPath delta,
            C.ICommonHasCompilerPreprocessedOutputPath lhs,
            C.ICommonHasCompilerPreprocessedOutputPath rhs)
        {
            if (lhs.PreprocessedOutputPath != null && rhs.PreprocessedOutputPath != null)
            {
                delta.PreprocessedOutputPath = !lhs.PreprocessedOutputPath.Equals(rhs.PreprocessedOutputPath) ? lhs.PreprocessedOutputPath : null;
            }
            else
            {
                delta.PreprocessedOutputPath = null;
            }
        }

        public static void
        Clone(
            this C.ICommonHasCompilerPreprocessedOutputPath settings,
            C.ICommonHasCompilerPreprocessedOutputPath other)
        {
            settings.PreprocessedOutputPath = other.PreprocessedOutputPath;
        }
    }
}
