#import <Foundation/Foundation.h>

int main (int argc, const char * argv[])
{
    NSAutoreleasePool * pool = [[NSAutoreleasePool alloc] init];
    (void)argc;
    (void)argv;
    NSLog(@"hello world");
    [pool drain];
    return 0;
}
