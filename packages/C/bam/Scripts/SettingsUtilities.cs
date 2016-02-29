#region License
// Copyright (c) 2010-2016, Mark Final
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
namespace C
{
    public static class SettingsUtilities
    {
        public static bool?
        Intersect(
            this bool? lhs,
            bool? rhs)
        {
            return (lhs == rhs) ? lhs : null;
        }

        public static bool?
        Complement(
            this bool? lhs,
            bool? rhs)
        {
            return (lhs != rhs) ? lhs : null;
        }

        public static EBit?
        Intersect(
            this EBit? lhs,
            EBit? rhs)
        {
            return (lhs == rhs) ? lhs : null;
        }

        public static EBit?
        Complement(
            this EBit? lhs,
            EBit? rhs)
        {
            return (lhs != rhs) ? lhs : null;
        }

        public static ECompilerOutput?
        Intersect(
            this ECompilerOutput? lhs,
            ECompilerOutput? rhs)
        {
            return (lhs == rhs) ? lhs : null;
        }

        public static ECompilerOutput?
        Complement(
            this ECompilerOutput? lhs,
            ECompilerOutput? rhs)
        {
            return (lhs != rhs) ? lhs : null;
        }

        public static EOptimization?
        Intersect(
            this EOptimization? lhs,
            EOptimization? rhs)
        {
            return (lhs == rhs) ? lhs : null;
        }

        public static EOptimization?
        Complement(
            this EOptimization? lhs,
            EOptimization? rhs)
        {
            return (lhs != rhs) ? lhs : null;
        }

        public static ETargetLanguage?
        Intersect(
            this ETargetLanguage? lhs,
            ETargetLanguage? rhs)
        {
            return (lhs == rhs) ? lhs : null;
        }

        public static ETargetLanguage?
        Complement(
            this ETargetLanguage? lhs,
            ETargetLanguage? rhs)
        {
            return (lhs != rhs) ? lhs : null;
        }

        public static string
        Intersect(
            this string lhs,
            string rhs)
        {
            return (lhs == rhs) ? lhs : null;
        }

        public static string
        Complement(
            this string lhs,
            string rhs)
        {
            return (lhs != rhs) ? lhs : null;
        }

        public static ECharacterSet?
        Intersect(
            this ECharacterSet? lhs,
            ECharacterSet? rhs)
        {
            return (lhs == rhs) ? lhs : null;
        }

        public static ECharacterSet?
        Complement(
            this ECharacterSet? lhs,
            ECharacterSet? rhs)
        {
            return (lhs != rhs) ? lhs : null;
        }

        public static ELanguageStandard?
        Intersect(
            this ELanguageStandard? lhs,
            ELanguageStandard? rhs)
        {
            return (lhs == rhs) ? lhs : null;
        }

        public static ELanguageStandard?
        Complement(
            this ELanguageStandard? lhs,
            ELanguageStandard? rhs)
        {
            return (lhs != rhs) ? lhs : null;
        }

        public static Cxx.EExceptionHandler?
        Intersect(
            this Cxx.EExceptionHandler? lhs,
            Cxx.EExceptionHandler? rhs)
        {
            return (lhs == rhs) ? lhs : null;
        }

        public static Cxx.EExceptionHandler?
        Complement(
            this Cxx.EExceptionHandler? lhs,
            Cxx.EExceptionHandler? rhs)
        {
            return (lhs != rhs) ? lhs : null;
        }

        public static Cxx.ELanguageStandard?
        Intersect(
            this Cxx.ELanguageStandard? lhs,
            Cxx.ELanguageStandard? rhs)
        {
            return (lhs == rhs) ? lhs : null;
        }

        public static Cxx.ELanguageStandard?
        Complement(
            this Cxx.ELanguageStandard? lhs,
            Cxx.ELanguageStandard? rhs)
        {
            return (lhs != rhs) ? lhs : null;
        }

        public static Cxx.EStandardLibrary?
        Intersect(
            this Cxx.EStandardLibrary? lhs,
            Cxx.EStandardLibrary? rhs)
        {
            return (lhs == rhs) ? lhs : null;
        }

        public static Cxx.EStandardLibrary?
        Complement(
            this Cxx.EStandardLibrary? lhs,
            Cxx.EStandardLibrary? rhs)
        {
            return (lhs != rhs) ? lhs : null;
        }
    }
}
