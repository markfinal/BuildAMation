// dynamic library
#ifndef DYNAMICLIBRARY_H
#define DYNAMICLIBRARY_H

#include <string>

#if defined(D_OPUS_PLATFORM_WINDOWS)

#if defined(D_OPUS_DYNAMICLIBRARY)
#define API __declspec(dllexport)
#else
#define API __declspec(dllimport)
#endif

#elif defined(D_OPUS_PLATFORM_UNIX) || defined(D_OPUS_PLATFORM_OSX)

#if defined(D_OPUS_DYNAMICLIBRARY)
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

extern API std::string LibraryFunction();

#endif // DYNAMICLIBRARY_H
