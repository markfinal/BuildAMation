#ifndef DYNAMICLIBRARYB_H
#define DYNAMICLIBRARYB_H

#if defined(_WIN32)

#if defined(OPUS_DYNAMICLIBRARY)
#define DYNAMICLIBRARYB_API __declspec(dllexport)
#else
#define DYNAMICLIBRARYB_API __declspec(dllimport)
#endif

#elif defined(__unix__) || defined(__APPLE__)

#if defined(OPUS_DYNAMICLIBRARY)
#if __GNUC__ >= 4
#define DYNAMICLIBRARYB_API __attribute__ ((visibility("default")))
#else // __GNUC__
#define DYNAMICLIBRARYB_API /* empty */
#endif // __GNUC__
#else // OPUS_DYNAMICLIBRARY
#define DYNAMICLIBRARYB_API /* empty */
#endif // OPUS_DYNAMICLIBRARY

#else

#error "Unsupported platform"

#endif

extern DYNAMICLIBRARYB_API char *dynamicLibraryBFunction();

#endif // DYNAMICLIBRARYB_H
