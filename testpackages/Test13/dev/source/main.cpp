#include "myobject.h"
#include "myobject2.h"

#include <QtGui/QApplication>
#include <QtGui/QWidget>

int main(int argc, char *argv[])
{
    QApplication app(argc, argv);

    MyClass myClass;
    myClass.DoSomething();
    
    MyClass2 myClass2;
    myClass2.DoSomething();

    QWidget window;
    window.resize(250, 150);
    window.setWindowTitle("Simple example");
    window.show();

    return app.exec();
}
