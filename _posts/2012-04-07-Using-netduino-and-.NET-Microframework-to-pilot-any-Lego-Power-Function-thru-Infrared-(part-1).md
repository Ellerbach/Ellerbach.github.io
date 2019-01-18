---
layout: post
title:  "Using netduino and .NET Microframework to pilot any Lego Power Function thru Infrared (part 1)"
date: 2012-04-07 08:49:00 +0100
categories: 
author: "Laurent Ellerbach"
thumbnails: "/assets/2012-04-07-thumb.jpg"
---
I’m part of [FREELUG](http://www.freelug.org/), the French Enthusiast Lego User Group. And in this group, there are lots of discussions on Lego of course. In one of the thread someone ask the question if it was possible to pilot an Lego train using the new Power Function with a PC. The need is during expo, it makes it easier to run trains, stop them in a programmatic way.

 ![image](/assets/6404.image_2.png)

 I did a quick answer on the list saying that it can be quite easy if the protocol was not too complex and the IR technology used was close to [RC5 from Philips](http://www.sbprojects.com/knowledge/ir/rc5.php). A small oscillator behind a serial or parallel port would do the trick. [Philo](http://www.philohome.com), one of the FREELUG member answer me with a link to [the protocol](http://www.philohome.com/pf/LEGO_Power_Functions_RC.pdf). And also tell me it should not be as easy as I was thinking. And he was more than right! No real way to make this work with a simple serial or parallel port on a PC. The protocol is more complex and need quite a bit of work to implement. I’ll come later on the first explanation on how to do it.

 So I decided to see if it was possible to implement this on [netduino]({% post_url 2011-09-09-netduino-board-geek-tool-for-.NET-Microframework %}) using .NET Microframework. this board has lots of IO, analogic and digital, do implement busses like I2C but also SPI. As any project, I started my project with my friend [Bing](http://www.bing.com/). And start searching for similar projects. And I found [Mario Vernari](http://highfieldtales.wordpress.com/) who I’ve mentioned in my [previous post]({% post_url 2012-03-31-Using-2-identical-I2C-device-on-the-same-I2C-bus-(solution-working-better) %}) who was doing something similar. And we’ve exchange couple of emails to find a good way to implement an Infrared emitter using .NET Microframework. We will create the wave form in a buffer and then send it thru the MOSI port of an SPI port linked to the infrared led. So I will use [the ideas and implementation Mario explain in his blog](http://highfieldtales.wordpress.com/2012/02/07/infrared-transmitter-driver-for-netduino/) to pilot the Lego Power Function.

 I let go thru Mario article to get the basics of the IR protocol in general. And I will focus here on the specific Lego implementation and of course the specific code to make it work.

 Reading the 14 pages of the Lego protocol, we learn that the IR signals are using 38 kHz cycles. An IR Mark is 6 on and off signals as shown in the next picture

 ![image](/assets/2262.image_4.png)

 Each message will start and stop with a Start/Stop bit. This bit is 6 IR Mark and 39 pauses. So if I represent it in a binary way it will be:

 101010101010000000000000000000000000000000000000000000000000000000000000000000000000000000

 As Mario described in his post, we will use ushort to create the wave length. So in this case it will looks like

 0xFF00 0xFF00 0xFF00 0XFF00 0xFF00 0xFF00 and 39 times 0x0000

 Reality is a bit different as when using MOSI on a SPI to output a signal it is always a 1 for couple of µ seconds. So the right value to use is 0xFE00

 The low bit is working the same way, it is 6 IR Mark and 10 cycles of pause, the high one 6 IR Mark and 21 cycles of pause.

 So if I want to send the binary value 10011 I will send 6 IR Marks, 21 pauses, 6 IR Marks, 10 pauses, 6 IR Marks, 10 pauses, 6 IR Marks, 21 pauses, 6 IR Marks, 21 pauses. And I will create a ushort buffer which will contains 6 times 0xFE00, 21 times 0x0000, 6 times 0xFE00, 10 times 0x0000, 6 times 0xFE00, 10 times 0x0000, 6 times 0xFE00, 21 times 0x0000, 6 times 0xFE00, 21 times 0x0000

 All this make the Lego protocol complex compare to the RC5 and other similar protocols where the Low bit and high bits are usually the same size, the IR Mark is just inverted and the pause same size as the IR Mark.

 Now let have a look at the protocol itself.

 ![image](/assets/3252.image_6.png)

 The protocol start and stop with our Start/Stop bit describe up in the article. And then have 4 nibble. One nibble is a 4 bit data. The last nibble is used to to a check sum and is called LRC LLLL = 0xF xor Nibble 1 xor Nibble 2 xor Nibble 3.

 There are 4 channels possible going from 1 to 4 represented as CC from 0 to 3.

 a is not used in this protocol for the moment and kept for a future implementation. So it has to be set to 0.

 E is 0 for most modes except one specific mode (PWM)

 Toggle is an interesting one. The value has to change each time a new command is sent. So if the first time you send a command on a specific Channel (let say 1), it is 0, the next command send on Channel 1 will have to set Toggle as 1.

 The Power Function have different modes available (MMM):

 000 Not used in PF RC Receiver   
001 Combo direct (timeout)   
010 Single pin continuous (no timeout)   
011 Single pin timeout   
1xx Single output

 To know which mode is doing what, just refer to the protocol. I will detail the Combo direct (timeout) mode as an example for the rest of the article. It is easy to understand how it is working. The others are not much more complex and the logic is the same.

 ![image](/assets/4403.image_8.png)

 Mode (MMM) here is 001. Channel (CC) will vary form 0 to 3 depending which channel you want to pilot. So here, the Data nibble is split into 2 parts BB and AA. The documentation give this:

 B output BB,, called Red in all receivers

 00xx Float output B   
01xx Forward on output B   
10xx Backward on output B   
11xx Brake output B

 A output AA, called Blue in all receivers

 xx00 Float output A   
xx01 Forward on output A   
xx10 Backward on output A   
xx11 Brake output A

 And an interesting information is that Toggle bit is not verified on receiver. So if you don’t want to implement it, it’s possible.

 So it’s time to write some code ![Sourire](/assets/4401.wlEmoticon-smile_2.png) Let start with couple of enums to facilitate the usage:

 
```csharp
//mode 
public enum LegoMode { 
    COMBO_DIRECT_MODE = 0x1, 
    SINGLE_PIN_CONTINUOUS = 0x2, 
    SINGLE_PIN_TIMEOUT = 0x3, 
    SINGLE_OUTPUT = 0x4 
}; 
//speed 
public enum LegoSpeed { 
    RED_FLT = 0x0, 
    RED_FWD = 0x1, 
    RED_REV = 0x2, 
    RED_BRK = 0x3, 
    BLUE_FLT = 0x0, 
    BLUE_FWD = 0x4, 
    BLUE_REV = 0x8,
    BLUE_BRK = 0xC 
}; 
//channel 
public enum LegoChannel { 
    CH1 = 0x0, 
    CH2 = 0x1, 
    CH3 = 0x2, 
    CH4 = 0x3 
}; 
```

The LegoMode one will be used to setup the mode (MMM), the LegoSpeed for the AA and BB output and the LegoChannel to select the Channel.
 
```csharp
private uint[] toggle = new uint[] { 0, 0, 0, 0 }; 

public void ComboMode(LegoSpeed blue_speed, LegoSpeed red_speed, LegoChannel channel) { 
    uint nib1, nib2, nib3, nib4; 
    //set nibs 
    nib1 = toggle[(uint)channel] | (uint)channel; 
    //nib1 = (uint)channel; 
    nib2 = (uint)LegoMode.COMBO_DIRECT_MODE; 
    nib3 = (uint)blue_speed | (uint)red_speed; 
    nib4 = 0xf ^ nib1 ^ nib2 ^ nib3; 
    sendMessage((ushort)nib1, (ushort)nib2, (ushort)nib3, (ushort)nib4, (uint)channel); 
} 
```

I have defined a toggle table which will contain the value of the toggling. The function ComboModo takes as argument, the channel and the AA and BB parameters.

The code is quite straight forward, I build the 4 nibbles like in the description.

nib1 contains the Toggle plus escape (0) plus the channel. Toggle is not mandatory in this one but I’ve implemented to show you how to do it. and the values the Toggle will take will be in binary 1000 or 0000 so 8 or 0 in decimal.

nb2 is E (which is 0) and the Mode (MMM) which is 1 in our case.

nib3 combine AA and BB to select the Blue and Red orders.

nib4 is the check sum.

And then I call a function called sendMessage. I’ve build all the modes the same way, implementing simply the protocol.

Now, let have a look at the sendMessage function:

 
```csharp
private void sendMessage(ushort nib1, ushort nib2, ushort nib3, ushort nib4, uint channel) { 
    ushort code = (ushort)((nib1 << 12) | (nib2 << 8) | (nib3 << 4) | nib4); 
    for (uint i = 0; i < 6; i++) { 
        message_pause(channel, i); 
        spi_send(code); 
    } 
    if (toggle[(int)channel] == 0) 
        toggle[(int)channel] = 8; 
    else 
        toggle[(int)channel] = 0; 
} 
```

4 nibbles of 4 bits is 16 bits so a ushort. And I’m building this ushort simply with all the nibbles. OK, the protocol is a bit more complex than only sending 1 time a command. Each command has to be sent 5 times. What make the protocol not easy is that you have to wait different amount of time depending on the channel and the number of time you’ve already send a command! That is the job of the message_pause function. The spi_send send the code ![Sourire](/assets/4401.wlEmoticon-smile_2.png). The rest is about toggling the toggle bit of the channel.

That’s it for today. In the next blog post, I’ll continue to go more in the details, show the implementation of the missing functions. And when I’ll finish to explain all the protocol code, I’ll go a bit further with a way to remotely using a web page or equivalent send commands to the netduino board which will send the IR command. And if I have more time, I will also implement sensors to detect if a train or a vehicle is on a specific place. This will be extremely easy as I’ve already explain how to [use sensors like this]({% post_url 2012-02-17-Using-basic-IO-with-.NET-Microframework %}).

