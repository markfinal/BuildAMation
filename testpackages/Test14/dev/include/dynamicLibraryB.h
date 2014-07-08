#ifndef DYNAMICLIBRARYB_H
#define DYNAMICLIBRARYB_H

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
#if defined(D_TEST14_DYNAMICLIBRARYB_STATICAPI)
#define DYNAMICLIBRARYB_API /* empty */
#else
#if defined(D_OPUS_DYNAMICLIBRARY_BUILD)
#define DYNAMICLIBRARYB_API API_EXPORT
#else
#define DYNAMICLIBRARYB_API API_IMPORT
#endif
#endif

extern DYNAMICLIBRARYB_API char *dynamicLibraryBFunction();

#endif /* DYNAMICLIBRARYB_H */
