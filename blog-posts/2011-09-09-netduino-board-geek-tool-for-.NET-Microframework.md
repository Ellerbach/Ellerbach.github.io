# 2011-09-09 netduino board: geek tool for .NET Microframework

In the past, long time ago, I've been a developer. I loved to develop low level code like drivers. And I love embedded and robots and all those stuff. So I have a natural attraction for .NET Microframework. Of course I know it was existing but I never really touch it. And discussing with [Pierre Cauchois](https://www.linkedin.com/in/pierrecauchois/), he told me he bought a [netduino board](https://www.netduino.com/) and automate his cooler!

 I had a look at it and just figure that was exactly what I needed to start my automation sprinkler project! So I bought the [netduino plus](https://www.netduino.com/netduinoplus/specs.htm) version with the micro sd card reader and the network connection. So 50€ later and couple of days I've received this gadget ![Sourire](../assets/4401.wlEmoticon-smile_2.png)

 What I love with netduino is that it's an open source design, really simple and efficient, the community is really active and it has all what I needed in terms of IO. And of course, I'll be able to reuse my .NET C# skills. OK, last time I've coded in C# was 1 year ago to automate my CANON EOS and be able to control it. But after 2 hours of code, C# skills come back very fast ![Sourire](../assets/4401.wlEmoticon-smile_2.png) And good news with .NET Microframework is that the number of class is quite small! so it's very easy to understand, very efficient classes and pretty much all what you need.

 So I've started by downloading and installing all the SDK (I'm using Visual Studio Ultimate but it's working with express versions). and downloaded the first [example](https://www.netduino.com/projects/) to make the embedded led blinking. And é minutes later, it was working ![Sourire](../assets/4401.wlEmoticon-smile_2.png) At this time I was an happy geek ![Rire](../assets/0842.wlEmoticon-openmouthedsmile_2.png)

 So I started to look at other samples that are available and played with the HHTPServer one. And decided to derive from this project to build my own pages and program my sprinklers. On the other side, I've done quite a bit retro engineering on my sprinklers (Gardena model). they are bi stable electro valve using 9V small impulsions. So I know I have to use 2 digital output to pilot open and close for each sprinkler. And I'll first concentrate and the soft and then on the hardware. By the way, the hardware question is still open, so if anyone want help, I'll appreciate it.

 When searching if someone already did this, I found [Mike Linnen](https://www.protosystem.net/) who has done an amazing integration with Windows Azure and Windows Phone 7. check it, it's really cool!

 In the next post, I'll go thru couple of lines of code!
