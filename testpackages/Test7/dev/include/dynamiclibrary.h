#ifndef DYNAMICLIBRARY_H
#define DYNAMICLIBRARY_H

typedef int (*ExportedFunction_t)(int);

#if defined(_WIN32)

#if defined(OPUS_DYNAMICLIBRARY)
#define API __declspec(dllexport)
#else
#define API __declspec(dllimport)
#endif

#elif defined(__unix__)

#if defined(OPUS_DYNAMICLIBRARY)
#if __GNUC__ >= 4
#define API __attribute__ ((visibility("default")))
#else // __GNUC__
#define API /* empty */
#endif // __GNUC__
#else // OPUS_DYNAMICLIBRARY
#define API /* empty */
#endif // OPUS_DYNAMICLIBRARY

#endif

extern API int MyTestFunction(int);

#endif // DYNAMICLIBRARY_H
