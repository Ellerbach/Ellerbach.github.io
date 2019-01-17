---
layout: post
title:  "A low cost humidity sensor for my sprinkler system"
date: 2012-06-24 08:23:47 +0100
categories: 
---
I’ve [developed my own sprinkler system]({% post_url 2012-05-06-Managing-my-Sprinklers-from-the-Cloud %}) which embed a web server and allow me to control it remotely where ever I am. I can program when and how long I will sprinkler on which circuit. I have 3 circuits but my system can work with a large number of circuits.

What I want to do is to be able to add intelligence into my netduino board. This .NET Microframework (NETMF) board runs without any OS. So now Windows, no Linux, no Mac, nothing! Directly .NET on the chip. Not a full .NET of course but all what is necessary to be able to [pilot IO]({% post_url 2012-02-17-Using-basic-IO-with-.NET-Microframework %}), [have a web server]({% post_url 2012-05-29-Creating-an-efficient-HTTP-Web-Server-for-.NET-Microframework-(NETMF) %}), etc. All this in an embedded board smallest of the size of a credit card. 

Part of my project is to be able to measure the soil humidity. So I’ve decided to develop my own sensor. The basic idea is to measure the conductivity (or resistor) of the soil. Any object/material has it’s own resistance. The more conductive it is, the smallest the resistor is and the less conductive, the higher the resistor is. And it does apply to anything. Metals are usually excellent resistors with a very low resistance of less than 1 Ω. And something like a plastic will have more than 1 MΩ resistor. So if you apply a huge voltage, you’ll get a very small current.

The rule you have to know to do some electronic is U = R x I where U is the voltage (tension in volt V), R is the resistor (in ohm Ω) and I is the intensity of the current (in ampere, A). So I will measure the resistor 'of the soil and I will determine if it is dry or humid.

Let start wit a bit of theory there regarding soil conductivity. It is possible to measure the soil conductivity with a [Tellurometer](http://en.wikipedia.org/wiki/Tellurometer). Soil conductivity is measured by this specific sensor and the resistance of the soil is determined. In my case what will interest me is to be able to measure the difference of conductivity between a humid and a dry soil at the same place. It just need to have 2 stick of copper or any other metal put into the soil and have a current going thru one stick and measuring the difference of voltage from the other.

When a soil is humid the resistor decrease and when it is dry, it does increase. So imagine I will build something like a voltmeter put into the soil and I will measure the resistance. As my netduino has an analogic input I will use it for this purpose. What I measure here, is a voltage so indirectly this variance or resistance. As per the [light sensor]({% post_url 2012-02-17-Using-basic-IO-with-.NET-Microframework %}), I’ll use the same principle:

![image](/assets/5518.image_6.png)

So I will measure the voltage of R3. R3 is a high value of 10K to do a pull down. It is a high resistor which will create a small current between the ground and A0. If I don’t put any resistor,I won’t be able to measure any intensity. And if I place A0 on the ogher side of my sensor and remove R3, I will use more current than in this design. It is possible to do the same as for the light sensor but in my case it will be a bit less efficient I guess.

R1 is here to reduce a bit the current and I will have to adjust this value regarding of my current soil.

The code is extremely simple:

 
```csharp
SecretLabs.NETMF.Hardware.AnalogInput SoilSensor =  
 new SecretLabs.NETMF.Hardware.AnalogInput(Pins.GPIO_PIN_A0); 
int lSoilSensorReading = 0; while (true) { 
    SoilSensorReading = SoilSensor.Read(); 
    Debug.Print(SoilSensorReading.ToString()); 
    Thread.Sleep(500); 
} 
```

I create an analogic input on port A0. And then I read the value every 500 milliseconds. And that’s it!

I’ve done the test with real soil, one is very humid, one a bit humid and one is very dry.

I get the following results:

* very humid = 650 
* a bit humid = 630 
* very dry = 550  And here is the picture of the prototype:

![WP_000792](/assets/2860.WP_000792_2.jpg)

As the analogic port has 1024 values going from 0 to 1023 on 3.3V, I have an amplitude of 100 values which represent a variance of approximately 0.32V.

So with this prototype I have a difference of 0.32V between dry and very humid with this specific soil.

I’m sure I can change a bit the sensitivity to use a broader range of the analogic input. I can do like for the light sensor an remove the R3 resistor and measure directly the tension between the sensor and the ground. I can also change R1 to a value close to the middle of the resistance of the soil. I can also change the alimentation value to 5V or so. 

That was just a first experiment! Just to prove it is working ![Sourire](/assets/4401.wlEmoticon-smile_2.png) Now, I need to improve a bit the system and see how far I can go. Any feedback from an electronic guy welcome.

