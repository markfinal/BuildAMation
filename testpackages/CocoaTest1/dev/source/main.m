#include <Cocoa/Cocoa.h>

int main(int argc, char *argv[])
{
    NSAutoreleasePool* pool =[[NSAutoreleasePool alloc] init];
    [NSApplication sharedApplication];

    (void)argc;
    (void)argv;
    NSRunAlertPanel(@"Testing Message Box",
                    @"Hello, world!",
                    @"OK", NULL, NULL);
    [pool release];
    return 0;
}
