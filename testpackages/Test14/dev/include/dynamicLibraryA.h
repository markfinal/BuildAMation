#ifndef DYNAMICLIBRARYA_H
#define DYNAMICLIBRARYA_H

#if defined(_WIN32)

#if defined(OPUS_DYNAMICLIBRARY)
#define DYNAMICLIBRARYA_API __declspec(dllexport)
#else
#define DYNAMICLIBRARYA_API __declspec(dllimport)
#endif

#elif defined(__unix__) || defined(__APPLE__)

#if defined(OPUS_DYNAMICLIBRARY)
#if __GNUC__ >= 4
#define DYNAMICLIBRARYA_API __attribute__ ((visibility("default")))
#else /* __GNUC__ */
#define DYNAMICLIBRARYA_API /* empty */
#endif /* __GNUC__ */
#else /* OPUS_DYNAMICLIBRARY */
#define DYNAMICLIBRARYA_API /* empty */
#endif /* OPUS_DYNAMICLIBRARY */

#else

#error "Unsupported platform"

#endif

extern DYNAMICLIBRARYA_API char *dynamicLibraryAFunction();

#endif /* DYNAMICLIBRARYA_H */
