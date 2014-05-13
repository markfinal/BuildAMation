#using <mscorlib.dll>

using namespace System;

ref class ManagedTest
{
public:
    void Test()
    {
        Console::WriteLine((String^)"Hello from managed C++");
    }
};

void __stdcall DoManagedStuff()
{
    ManagedTest ^instance = gcnew ManagedTest();
    instance->Test();
}