#ifndef DYNAMICLIBRARYA_H
#define DYNAMICLIBRARYA_H

#if defined(OPUS_DYNAMICLIBRARY)
extern __declspec(dllexport) char *dynamicLibraryAFunction();
#else
extern __declspec(dllimport) char *dynamicLibraryAFunction();
#endif

#endif // DYNAMICLIBRARYA_H
