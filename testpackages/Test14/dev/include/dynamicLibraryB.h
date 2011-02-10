#ifndef DYNAMICLIBRARYB_H
#define DYNAMICLIBRARYB_H

#if defined(OPUS_DYNAMICLIBRARY)
extern __declspec(dllexport) char *dynamicLibraryBFunction();
#else
extern __declspec(dllimport) char *dynamicLibraryBFunction();
#endif

#endif // DYNAMICLIBRARYB_H
