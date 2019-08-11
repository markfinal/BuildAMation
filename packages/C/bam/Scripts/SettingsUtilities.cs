#region License
// Copyright (c) 2010-2019, Mark Final
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
    /// <summary>
    /// Utility class for use with settings
    /// </summary>
    public static class SettingsUtilities
    {
        /// <summary>
        /// Get the intersection between two bool values.
        /// </summary>
        /// <param name="lhs">First value</param>
        /// <param name="rhs">Second value</param>
        /// <returns>Intersection</returns>
        public static bool?
        Intersect(
            this bool? lhs,
            bool? rhs)
        {
            return (lhs == rhs) ? lhs : null;
        }

        /// <summary>
        /// Get the complement between two bool values
        /// </summary>
        /// <param name="lhs">First value</param>
        /// <param name="rhs">Second value</param>
        /// <returns>Complement</returns>
        public static bool?
        Complement(
            this bool? lhs,
            bool? rhs)
        {
            return (lhs != rhs) ? lhs : null;
        }

        /// <summary>
        /// Get the intersection between two EBits
        /// </summary>
        /// <param name="lhs">First value</param>
        /// <param name="rhs">Second value</param>
        /// <returns>Intersection</returns>
        public static EBit?
        Intersect(
            this EBit? lhs,
            EBit? rhs)
        {
            return (lhs == rhs) ? lhs : null;
        }

        /// <summary>
        /// Get the complement between two EBits
        /// </summary>
        /// <param name="lhs">First value</param>
        /// <param name="rhs">Second value</param>
        /// <returns>Complement</returns>
        public static EBit?
        Complement(
            this EBit? lhs,
            EBit? rhs)
        {
            return (lhs != rhs) ? lhs : null;
        }

        /// <summary>
        /// Get the intersection between two EOptimization
        /// </summary>
        /// <param name="lhs">First value</param>
        /// <param name="rhs">Second value</param>
        /// <returns>Intersection</returns>
        public static EOptimization?
        Intersect(
            this EOptimization? lhs,
            EOptimization? rhs)
        {
            return (lhs == rhs) ? lhs : null;
        }

        /// <summary>
        /// Get the complement between two EOptimization
        /// </summary>
        /// <param name="lhs">First value</param>
        /// <param name="rhs">Second value</param>
        /// <returns>Complement</returns>
        public static EOptimization?
        Complement(
            this EOptimization? lhs,
            EOptimization? rhs)
        {
            return (lhs != rhs) ? lhs : null;
        }

        /// <summary>
        /// Get the intersection between two ETargetLanguage
        /// </summary>
        /// <param name="lhs">First value</param>
        /// <param name="rhs">Second value</param>
        /// <returns>Intersection</returns>
        public static ETargetLanguage?
        Intersect(
            this ETargetLanguage? lhs,
            ETargetLanguage? rhs)
        {
            return (lhs == rhs) ? lhs : null;
        }

        /// <summary>
        /// Get the complement between two ETargetLanguage
        /// </summary>
        /// <param name="lhs">First value</param>
        /// <param name="rhs">Second value</param>
        /// <returns>Complement</returns>
        public static ETargetLanguage?
        Complement(
            this ETargetLanguage? lhs,
            ETargetLanguage? rhs)
        {
            return (lhs != rhs) ? lhs : null;
        }

        /// <summary>
        /// Get the intersection between two strings
        /// </summary>
        /// <param name="lhs">First value</param>
        /// <param name="rhs">Second value</param>
        /// <returns>Intersection</returns>
        public static string
        Intersect(
            this string lhs,
            string rhs)
        {
            return lhs.Equals(rhs, System.StringComparison.Ordinal) ? lhs : null;
        }

        /// <summary>
        /// Get the complement between two strings
        /// </summary>
        /// <param name="lhs">First value</param>
        /// <param name="rhs">Second value</param>
        /// <returns>Complement</returns>
        public static string
        Complement(
            this string lhs,
            string rhs)
        {
            return (lhs != rhs) ? lhs : null;
        }

        /// <summary>
        /// Get the intersection between two ECharacterSet
        /// </summary>
        /// <param name="lhs">First value</param>
        /// <param name="rhs">Second value</param>
        /// <returns>Intersection</returns>
        public static ECharacterSet?
        Intersect(
            this ECharacterSet? lhs,
            ECharacterSet? rhs)
        {
            return (lhs == rhs) ? lhs : null;
        }

        /// <summary>
        /// Get the complement between two ECharacterSet
        /// </summary>
        /// <param name="lhs">First value</param>
        /// <param name="rhs">Second value</param>
        /// <returns>Complement</returns>
        public static ECharacterSet?
        Complement(
            this ECharacterSet? lhs,
            ECharacterSet? rhs)
        {
            return (lhs != rhs) ? lhs : null;
        }

        /// <summary>
        /// Get the intersection between two ELanguageStandard
        /// </summary>
        /// <param name="lhs">First value</param>
        /// <param name="rhs">Second value</param>
        /// <returns>Intersection</returns>
        public static ELanguageStandard?
        Intersect(
            this ELanguageStandard? lhs,
            ELanguageStandard? rhs)
        {
            return (lhs == rhs) ? lhs : null;
        }

        /// <summary>
        /// Get the complement between two ELanguageStandard
        /// </summary>
        /// <param name="lhs">First value</param>
        /// <param name="rhs">Second value</param>
        /// <returns>Complement</returns>
        public static ELanguageStandard?
        Complement(
            this ELanguageStandard? lhs,
            ELanguageStandard? rhs)
        {
            return (lhs != rhs) ? lhs : null;
        }

        /// <summary>
        /// Get the intersection between two EExceptionHandler
        /// </summary>
        /// <param name="lhs">First value</param>
        /// <param name="rhs">Second value</param>
        /// <returns>Intersection</returns>
        public static Cxx.EExceptionHandler?
        Intersect(
            this Cxx.EExceptionHandler? lhs,
            Cxx.EExceptionHandler? rhs)
        {
            return (lhs == rhs) ? lhs : null;
        }

        /// <summary>
        /// Get the complement between two EExceptionHandler
        /// </summary>
        /// <param name="lhs">First value</param>
        /// <param name="rhs">Second value</param>
        /// <returns>Complement</returns>
        public static Cxx.EExceptionHandler?
        Complement(
            this Cxx.EExceptionHandler? lhs,
            Cxx.EExceptionHandler? rhs)
        {
            return (lhs != rhs) ? lhs : null;
        }

        /// <summary>
        /// Get the intersection between two ELanguageStandard
        /// </summary>
        /// <param name="lhs">First value</param>
        /// <param name="rhs">Second value</param>
        /// <returns>Intersection</returns>
        public static Cxx.ELanguageStandard?
        Intersect(
            this Cxx.ELanguageStandard? lhs,
            Cxx.ELanguageStandard? rhs)
        {
            return (lhs == rhs) ? lhs : null;
        }

        /// <summary>
        /// Get the complement between two ELanguageStandard
        /// </summary>
        /// <param name="lhs">First value</param>
        /// <param name="rhs">Second value</param>
        /// <returns>Complement</returns>
        public static Cxx.ELanguageStandard?
        Complement(
            this Cxx.ELanguageStandard? lhs,
            Cxx.ELanguageStandard? rhs)
        {
            return (lhs != rhs) ? lhs : null;
        }

        /// <summary>
        /// Get the intersection between two EStandardLibrary
        /// </summary>
        /// <param name="lhs">First value</param>
        /// <param name="rhs">Second value</param>
        /// <returns>Intersection</returns>
        public static Cxx.EStandardLibrary?
        Intersect(
            this Cxx.EStandardLibrary? lhs,
            Cxx.EStandardLibrary? rhs)
        {
            return (lhs == rhs) ? lhs : null;
        }

        /// <summary>
        /// Get the complement between two EStandardLibrary
        /// </summary>
        /// <param name="lhs">First value</param>
        /// <param name="rhs">Second value</param>
        /// <returns>Complement</returns>
        public static Cxx.EStandardLibrary?
        Complement(
            this Cxx.EStandardLibrary? lhs,
            Cxx.EStandardLibrary? rhs)
        {
            return (lhs != rhs) ? lhs : null;
        }
    }
}
