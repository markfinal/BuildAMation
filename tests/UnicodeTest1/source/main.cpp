/*
Copyright (c) 2010-2019, Mark Final
All rights reserved.

Redistribution and use in source and binary forms, with or without
modification, are permitted provided that the following conditions are met:

* Redistributions of source code must retain the above copyright notice, this
  list of conditions and the following disclaimer.

* Redistributions in binary form must reproduce the above copyright notice,
  this list of conditions and the following disclaimer in the documentation
  and/or other materials provided with the distribution.

* Neither the name of BuildAMation nor the names of its
  contributors may be used to endorse or promote products derived from
  this software without specific prior written permission.

THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS"
AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE
IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE
DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT HOLDER OR CONTRIBUTORS BE LIABLE
FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL
DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR
SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER
CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY,
OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE
OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
*/

#include <tchar.h>
#include <type_traits>

int main()
{
    // see https://msdn.microsoft.com/en-us/library/7dzey6h6.aspx

#if defined(D_COMPILE_AS_NON_UNICODE)
    static_assert(sizeof(_TCHAR) == sizeof(char), "_TCHAR should be the same size as char in non-unicode builds");
    static_assert(sizeof(_TCHAR) < sizeof(wchar_t), "_TCHAR should be the smaller than wchar_t in non-unicode builds");
    //static_assert(!std::is_signed<_TXCHAR>::value, "_TXCHAR must be signed in non-unicode builds");
    static_assert(sizeof(_TXCHAR) == sizeof(char), "_TCHAR should be the same size as char in non-unicode builds");
#elif defined(D_COMPILE_AS_UNICODE)
    static_assert(sizeof(_TCHAR) > sizeof(char), "_TCHAR should be the bigger than char in unicode builds");
    static_assert(sizeof(_TCHAR) == sizeof(wchar_t), "_TCHAR should be the same size as wchar_t in unicode builds");
#elif defined(D_COMPILE_AS_MULTIBYTE)
    static_assert(sizeof(_TCHAR) == sizeof(char), "_TCHAR should be the same size as char in multibyte builds");
    static_assert(sizeof(_TCHAR) < sizeof(wchar_t), "_TCHAR should be the smaller than wchar_t in multibyte builds");
    static_assert(!std::is_signed<_TXCHAR>::value, "_TXCHAR must be unsigned in multibyte builds");
    static_assert(sizeof(_TXCHAR) == sizeof(unsigned char), "_TCHAR should be the same size as unsigned char in multibyte builds");
#else
    #error Unknown character set
#endif

    return 0;
}
