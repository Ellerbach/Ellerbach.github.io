---
layout: post
title:  "Using 2 identical I2C device on the same I2C bus"
date: 2012-02-24 09:12:36 +0100
categories: 
---
If you know a bit about I2C bus, it is impossible to use 2 identical devices with the same address on the bus. [Read my previous article]({% post_url 2012-02-21-Using-one-temperature-sensor-with-I2C-protocol-and-.NET-Micro-framework-on-netduino-board %}) to understand more on how it’s working. But as always, you can find trick to make it works.

In my case, I’m using a TC74 I2C temperature sensor from Microchip. Their bus address is the same (0x4D). Plugging 2 on the same bus will create a redundancy but that’s all what you’ll get. If you want to place them in 2 different locations, you’ll never be sure which one will give you the right temperature. 

I get the idea of powering on one sensor and powering the other one off to make sure only one of the device will be on and will respond to the requests. To do that I’m using one digital IO. In the state high (1), one of the sensor will be on and on state low (0) the other will be on. 

So I’ve decided to do the following hardware implementation:

![image](/assets/4505.image_2.png)

and how it looks like for real ![Sourire](/assets/4401.wlEmoticon-smile_2.png)

![WP_000164](/assets/4380.WP_000164_2.jpg)

When D0 will be high (1), the TC74 #1 will be alimented and when it will be low (0), the TC74 #2 will be alimented. Both transistors are playing the role of switch there. Each TC74 need approximately 300 ms to get fully initialized. So in the code, before accessing any of the sensor, right after switching, we will have to wait a bit. But overall, this simple and smart solution will work with more than 2 sensors, if you need a third or a fourth one, just add another IO and do a bit of logic. And that’s it!

The .NET Microframework (NETMF) code is very simple, based on the same example as the previous post, it will looks like this:


```csharp
public static void Main() { 
    TC74Device MyTC74 = new TC74Device(0x4D); //0x4D 
    OutputPort MySelect = new OutputPort(Pins.GPIO_PIN_D0, false); 
    Thread.Sleep(1000); 
    byte MyData; 
    sbyte MyTemp; MyTC74.Init(); 
    MySelect.Write(!MySelect.Read()); 
    Thread.Sleep(1000); 
    MyTC74.Init(); 
    while (MyTC74.IsReady()) { 
        MyTemp = MyTC74.ReadTemperature(); 
        Debug.Print("Temperature :" + MyTemp); 
        MyData = MyTC74.ReadRegister(); 
        Debug.Print("Register :" + MyData); 
        Thread.Sleep(1000); 
        MySelect.Write(!MySelect.Read()); 
        Thread.Sleep(1000); 
        //MyTC74.Standby(true); 
    } 
}
```

So nothing really different from the [previous post]({% post_url 2012-02-21-Using-one-temperature-sensor-with-I2C-protocol-and-.NET-Micro-framework-on-netduino-board %}) except that a digital IO is created and the state is changed every time in the infinite loop. And there are a 1s sleep before access any of the sensor. As for the [netduino]({% post_url 2011-09-09-netduino-board-geek-tool-for-.NET-Microframework %}), the component is the same, it is declared one time. But in terms of programming, we know we have 2 different sensors ![Sourire](/assets/4401.wlEmoticon-smile_2.png) Up to you to create a class to select which sensor you want to measure and play with the sleep if needed.

I did put the sensors outside (I’m on vacations in the mountains) and run the program. I put my fingers on one of the sensor so the temperature get higher. And the result is the following:

Temperature :-4   
Register :64   
Temperature :21   
Register :64   
Temperature :-4   
Register :64   
Temperature :23   
Register :64   
Temperature :-4   
Register :64   
Temperature :24   
Register :64   
Temperature :-4   
Register :64   
Temperature :23   
Register :64   
Temperature :-4   
Register :64   
Temperature :21   
Register :64

So as you can see, the outside temperature is –4 and my fingers warmed up the sensor to 21-23 degrees. It was cold so I did not wait the full time to get to the 37 degrees or so it should be ![Sourire](/assets/4401.wlEmoticon-smile_2.png)

I hope you’ll enjoy the trick ![Sourire](/assets/4401.wlEmoticon-smile_2.png) Feedback from electronic guys welcome.

