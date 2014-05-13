#ifndef MYOBJECT2_H
#define MYOBJECT2_H

#include <QtCore/QObject>

class MyClass2 : public QObject
{
    Q_OBJECT
    
public:
    MyClass2(QObject *parent = 0);
    ~MyClass2();

    void DoSomething();
    
signals:
    void mySignal();
    
public slots:
    void mySlot();
};

#endif // MYOBJECT2_H
