#ifndef MYOBJECT_H
#define MYOBJECT_H

#include <QtCore/QObject>

class MyClass : public QObject
{
    Q_OBJECT
    
public:
    MyClass(QObject *parent2 = 0);
    ~MyClass();
    
signals:
    void mySignal();
    
public slots:
    void mySlot();
};

#endif // MYOBJECT_H
