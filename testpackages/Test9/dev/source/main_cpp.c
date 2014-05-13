#include <string>
#include <vector>
#include <iostream>

class MyClass
{
public:
	MyClass()
	{
	}

	~MyClass()
	{
	}
};

int main()
{
	try
	{
		MyClass instance;
        std::string hello("Hello");
        std::cout << hello;
        std::vector<int> *a = new std::vector<int>;
        a->push_back(1);
        for (std::vector<int>::iterator i = a->begin(); i != a->end(); ++i)
        {
            std::cout << *i;
        }
        delete a;
	}
	catch (...)
	{
		return -1;
	}
	return 0;
}
