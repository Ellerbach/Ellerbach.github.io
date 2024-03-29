# 2012-03-31 Using 2 identical I2C device on the same I2C bus (solution working better)

[In one of my past posts](./2012-02-24-Using-2-identical-I2C-device-on-the-same-I2C-bus.md) and with whom I've exchange a bit to find other solutions help me there two. Mario is coming from the electronic side and I come from the software side.  So he can correct me when I'm wrong with my electronic ![Sourire](../assets/4401.wlEmoticon-smile_2.png). And I was quite wrong with the previous solution trying to switch on and off the power of the sensors. Mario gave me couple of good reasons:

"First off, any silicon embeds diodes, thus -even unpowering a device- there's a non-zero current flowing into. In this case, through the SCL/SDA lines, via pull-ups.

Secondly, you're using sensors, and they typically need a certain time to settle their internal state, reach the steadiness, etc. There are also "smarter" sensors, which perform a kind of auto-calibration when they are powered up. If you switch off, you'll lose those benefits."

And he gave me a tip: "use a 74HC4052". So lets go and see what it is. It's a switch for analog signals. You have 4 inputs (named Ya, a = 0 to 3) and 1 output (named Z). But they are 2 ways. So when you have selected one line, you can send signals in both ways. And there are 2 of those in the chip (naming will be nY and nZ, n = 1 and 2).

That allow to switch the overall SDA and SCL bus to the right sensor. And this will allow to pilot up to 4 identical sensors. The selection of the line is made by the S0 and S1 with the following rule:

|Input| | |Channel on|
|---|---|---|---|
|*E*|*S1*|*S0*| |
|L|L|L|nY0 and nZ|
|L|L|H|nY1 and nZ|
|L|H|L|nY2 and nZ|
|L|H|H|nY3 and nZ|
|H|X|X|none|

So regarding the code I wrote for the previous post, it will remain the same! It will work exactly the same way. What is changing is the electronic part. And here is the new design:

![image](../assets/4745.image_2.png)

Mario also give me the following advice: "Note that when you cut off a device, you should provide the pullups anyway". So That's what I did by putting the pullups for each component.

Now it's working and much better as the line switch is really fast and there is no need to wait for a long time to read the data. So thanks Mario for the tip!
