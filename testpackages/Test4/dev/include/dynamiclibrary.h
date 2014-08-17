/*
Copyright 2010-2014 Mark Final

This file is part of BuildAMation.

BuildAMation is free software: you can redistribute it and/or modify
it under the terms of the GNU Lesser General Public License as published by
the Free Software Foundation, either version 3 of the License, or
(at your option) any later version.

BuildAMation is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU Lesser General Public License for more details.

You should have received a copy of the GNU Lesser General Public License
along with BuildAMation.  If not, see <http://www.gnu.org/licenses/>.
*/
#ifndef DYNAMICLIBRARY_H
#define DYNAMICLIBRARY_H

/* specific platform settings */
#if defined(_WIN32)
 #define API_EXPORT __declspec(dllexport)
 #define API_IMPORT __declspec(dllimport)
#elif __GNUC__ >= 4
  #define API_EXPORT __attribute__ ((visibility("default")))
#endif

/* defaults */
#ifndef API_EXPORT
 #define API_EXPORT /* empty */
#endif
#ifndef API_IMPORT
 #define API_IMPORT /* empty */
#endif

/* now define the library API based on whether it is built as static or not
   the D_<package>_<module>_STATICAPI #define is populated in all uses, including the library build itself
   the D_OPUS_DYNAMICLIBRARY_BUILD is only present for the library build when it is a dynamic library */
#if defined(D_TEST4_MYDYNAMICLIBRARY_STATICAPI)
 #define API /* empty */
#else
 #if defined(D_OPUS_DYNAMICLIBRARY_BUILD)
  #define API API_EXPORT
 #else
  #define API API_IMPORT
 #endif
#endif

extern API int TestFunction();

#endif /* DYNAMICLIBRARY_H */
