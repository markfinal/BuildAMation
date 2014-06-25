#ifndef UNUSED_MACROS_H
#define UNUSED_MACROS_H

#if defined(__GNUC__)
#define UNUSEDARG(_ARGUMENT) _ARGUMENT __attribute__ ((unused))
#else
#define UNUSEDARG(_ARGUMENT) /* do nothing */
#endif

#endif /* UNUSED_MACROS_H */
