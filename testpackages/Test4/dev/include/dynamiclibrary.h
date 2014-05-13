#ifndef DYNAMICLIBRARY_H
#define DYNAMICLIBRARY_H

#if defined(_WIN32)

#if defined(OPUS_DYNAMICLIBRARY)
#define API __declspec(dllexport)
#else
#define API __declspec(dllimport)
#endif

#elif defined(__unix__) || defined(__APPLE__)

#if defined(OPUS_DYNAMICLIBRARY)
#if __GNUC__ >= 4
#define API __attribute__ ((visibility("default")))
#else /* __GNUC__ */
#define API /* empty */
#endif /* __GNUC__ */
#else /* OPUS_DYNAMICLIBRARY */
#define API /* empty */
#endif /* OPUS_DYNAMICLIBRARY */

#else

#error "Unsupported platform" 

#endif

extern API int TestFunction();

#endif /* DYNAMICLIBRARY_H */
