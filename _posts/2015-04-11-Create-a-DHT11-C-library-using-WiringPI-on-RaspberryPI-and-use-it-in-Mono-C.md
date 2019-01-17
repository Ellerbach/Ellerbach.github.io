---
layout: post
title:  "Create a DHT11 C library using WiringPI on RaspberryPI and use it in Mono C#"
date: 2015-04-11 09:40:05 +0100
categories: 
---
I’m using various boards like RaspberryPI (1 and 2) as well as Arduino and Netduino. I have to say I’m a big fan of C#, so I try to use C# as much as I can. Based on the excellent [WiringPI](http://wiringpi.com/) framework, I’ve ported equivalent of .NET Microframework classes to RaspberryPI (see [previous post here]({% post_url 2014-10-25-.NET-Microframework-on-RaspberryPi-(Part-2) %}), code not fully updated with latest Wiring PI version, just need to find some time to update it). All is using Mono on Linux.

But as .NET is a managed code runtime, there are some limitations when it come to do some very short timing operations as managed code can’t guaranty those easily. In order to have those operations working, you’ll need to build some native C code. But wait, I know you’re a fan of C# like me and you don’t want to build your full project in C! It’s like the old time when we had the excellent Visual Basic to build all the graphical interfaces, fast development for anything related to interfaces but lack of performance. At this time, most of the code were don in a C/C++ dll and imported with the famous dllimport into the VB code.

Well, we will do exactly the same here but replacing the old VB by the modern C# and for anything that need to be hard real time, we’ll put that into a C dll and import it into C#. But wait, here, our RaspberryPI is running Linux. So how to make that possible? In Linux as well, there are equivalent of dll, those are called library. They start with lib and have .so extensions. They are the exact same as dll but in the Linux world with the same advantages and same weaknesses.

Then, let start by the library first. I wanted to find some code that already worked with the WiringPI framework as I’m using it ([see this article]({% post_url 2013-06-21-.NET-Microframework-on-RaspberryPi-(Part-1) %}) on how to install it on a RaspberryPI). And I fond [this great example](http://www.rpiblog.com/2012/11/interfacing-temperature-and-humidity.html) which was almost what I wanted. I just needed to adapt it a bit to make it working. 

So I created (using my preferred tool Visual Studio), a normal C++ project. My main idea is to use it as an editor. I need 2 files: the library header and the core code. Both are important, and if you want to build a library you must have both. Let start with the header (called DHT11library.h in my code), here is the code:

```cpp
#ifndef _DHTLIB 
#define _DHTLIB 
#ifdef __cplusplus 
extern "C" { 
#endif
extern bool InitDHT(int pinval); 
extern float getTemp(); 
extern float getHumidity(); 
extern bool dht11_read_val(); 
#ifdef __cplusplus 
} 
#endif 
#endif 
```

What is the most important here is to have the entry point of the library properly declared in this header with extern. Now, the rest of the code in the file DHT11library.cpp:

```csharp
#include <wiringPi.h>
#include <stdio.h>
#include <stdlib.h>
#include <stdint.h>
#include "DHT11library.h"
#define MAX_TIME 85
int DHT11PIN; 
int dht11_val[5] = { 0, 0, 0, 0, 0 }; 
bool isinit = false; 

bool InitDHT(int pinval) 
{ 
    if (wiringPiSetup() == -1) 
    { 
        isinit = false; 
        return isinit; 
    } 
    DHT11PIN = pinval; 
    // initialize pin 
    isinit = true; 
    return isinit; 
} 

float getTemp() 
{ 
    return (float)(dht11_val[2] + dht11_val[3] / 10); 
} 

float getHumidity() 
{ 
    return (float)(dht11_val[0] + dht11_val[1] / 10); 
} 

bool dht11_read_val() 
{ 
    if (!isinit) 
        return false; 
    uint8_t lststate = HIGH; 
    uint8_t counter = 0; 
    uint8_t j = 0, i; 
    float farenheit; 
    for (i = 0; i < 5; i++) 
    dht11_val[i] = 0; 
    pinMode(DHT11PIN, OUTPUT); 
    digitalWrite(DHT11PIN, LOW); 
    delay(18); 
    digitalWrite(DHT11PIN, HIGH); 
    delayMicroseconds(40); 
    pinMode(DHT11PIN, INPUT); 
    for (i = 0; i < MAX_TIME; i++) 
    { 
        counter = 0; 
        while (digitalRead(DHT11PIN) == lststate){ 
            counter++; 
            delayMicroseconds(1); 
            if (counter == 255) 
                break; 
        } 
        lststate = digitalRead(DHT11PIN); 
        if (counter == 255) 
        break; 
        // top 3 transistions are ignored   
        if ((i >= 4) && (i % 2 == 0)){ 
            dht11_val[j / 8] <<= 1; 
            if (counter>16) 
                dht11_val[j / 8] |= 1; 
            j++; 
        } 
    } 
    // verify cheksum and print the verified data   
    if ((j >= 40) && (dht11_val[4] == ((dht11_val[0] + dht11_val[1] + dht11_val[2] + dht11_val[3]) & 0xFF))) 
    { 
        if ((dht11_val[0] == 0) && (dht11_val[2] == 0)) 
            return false; 
        return true; 
    } 
    return false; 
} 
```

As you can see, the code is very similar to the example code I found. I just added 3 functions to initialize the pin I’ll use and return both the temperature and humidity. Every time I’ll need to read the DHT11, I’ll call the dht11_read_val and then call both functions to return the temperature and humidity.

Now, this is where you have to pay attention to make sure you’ll build the library correctly. You need to copy both file the DHT11library.cpp and .h into the Raspberry (I’m using a Samba share for this), then using PyuTTY or another way, connect to the Raspberry, go to the directory where you have placed the 2 files and compile the library:

```bash
sudo gcc -o libDHT11.so DHT11library.cpp -L/usr/local/lib -lwiringPi –shared
```

_-o_ command will allow you to define the name of the library, on Linux, name need to start with lib and finish with so. At least that’s the only way I managed to make it working. I’m not a Linux expect so I may have miss something.

_-L and –lwinringPI _reference the rest of the library. You must have compiled and installed the WiringPI framework before of course.

_-shared _is the command to tell gcc to build a library

If everything goes well, you’ll get your libDHT11.so compiled and ready to use. So let move to C# now. You can create a normal C# project using Visual Studio, .NET 4.0 type is recommended to work on Mono.

I have created a class that embedded the C library and then allow to be used like a normal class:

```csharp
using System; 
using System.Collections.Generic; 
using System.Linq; 
using System.Runtime.InteropServices; 
using System.Text; 
using RaspberryPiNETMF; 
using Microsoft.SPOT.Hardware; 
using System.Timers; 

namespace SerreManagement 
{ 
    public class NewDataArgs 
    { 
        public NewDataArgs(float temp, float hum) 
        { Temperature = temp; Humidity = hum; } 
        public float Temperature { get; private set; } 
        public float Humidity { get; private set; } 
        } 

        class DHT11 
        { 
        //bool InitDHT(int pinval) 
        [DllImport("libDHT11.so", EntryPoint = "InitDHT")] 
        static extern bool InitDHT(int pinval); 
        //float getTemp() 
        [DllImport("libDHT11.so", EntryPoint = "getTemp")] 
        static extern float getTemp(); 
        //float getHumidity() 
        [DllImport("libDHT11.so", EntryPoint = "getHumidity")] 
        static extern float getHumidity(); 
        //bool dht11_read_val() 
        [DllImport("libDHT11.so", EntryPoint = "dht11_read_val")] 
        static extern bool dht11_read_val(); 

        // private values 
        private Cpu.Pin mPin; 
        private int mSec; 
        private Timer mTimer = new Timer(); 
        public delegate void NewData(object sender, NewDataArgs e); 
        public event NewData EventNewData; 
        // to get temperature and humidity 
        public float Temperature { get; internal set; } 
        public float Humidity { get; internal set; } 

        public DHT11(Cpu.Pin pin, int seconds = 0) 
        { 
            mPin = pin; 
            mSec = seconds; 
            if (!InitDHT((int)pin)) 
                throw new Exception("Error initalizing DHT11"); 
            mTimer.Elapsed += mTimer_Elapsed; 
        } 

        void mTimer_Elapsed(object sender, ElapsedEventArgs e) 
        { 
            if (dht11_read_val()) 
            { 
                Temperature = getTemp(); 
                Humidity = getHumidity(); 
                if (EventNewData != null) 
                    EventNewData(this, new NewDataArgs(Temperature, Humidity)); 
            } 
        } 

        public void Start() 
        { 
            if (mSec != 0) 
            { 
                mTimer.Interval = mSec * 1000; 
                mTimer.Start(); 
            } 
        } 

        public void Stop() 
        { 
            mTimer.Stop(); 
        } 

        public bool ReadDHT11() 
        { 
            return (dht11_read_val()); 
        } 
    } 
}
```

The way to import the library is simple, it’s like for a Windows dll, it’s just the name changing. Be very careful if you build more complex library because the type conversion as well as pointers conversion may not be strait forward, you’ll need to use marshaling:

```csharp
//bool InitDHT(int pinval) 
[DllImport("libDHT11.so", EntryPoint = "InitDHT")] 
static extern bool InitDHT(int pinval); 
```

This is working with any kind of already existing Linux library, the only thing you need is the header file with the definition in order to find the right entry points as well as the exact types used.

Then usage of the class is really simple:

```csharp
DHT11 mDHT; 
mDHT = new DHT11(Cpu.Pin.Pin_P1_18, 30); 
mDHT.EventNewData += mDHT_EventNewData; 
mDHT.Start();
void mDHT_EventNewData(object sender, NewDataArgs e) 
{ 
    Console.WriteLine("Temp: " + e.Temperature + ", Hum: " + e.Humidity); 
} 
```

In our case, the DHT11 is linked to the Pin 18 on a Raspberry and the call for temperature and humidity is done every 30 seconds. The event is raised and the data can be read. You can then create an exe with Visual Studio, and run it on the RPI. Don’t forget to copy all the dll you’ll need as well as the libDHT11.so one. It is a dll so it has to be present with the exe. If not present, you’ll get an exception

Results looks like:

Temp: 23, Hum: 34

The DHT11 is very simple and the temperature and humidity is not precise, you don’t have anything after the decimal point. the DHT22 is much better, it’s very easy to adapt this code to it, the difference with DHT11 is very tinny.