---
layout: post
title:  "Adding Bluetooth support to a NETMF board (.NET Microframework)"
date: 2013-04-29 02:59:39 +0100
categories: 
---
I recently bought a very cheap Bluetooth adaptor for my Netduino. I wanted to test how easy/hard it is to support Bluetooth. I see lots of advantages with Bluetooth for a near field communication like piloting easily a robot with a Phone without the need of other network or Infrared. Also Bluetooth is a secured communication with a peering.

So I bought [this cheap Bluetooth](http://dx.com/p/jy-mcu-arduino-bluetooth-wireless-serial-port-module-104299) adaptor for $8.20. It does expose itself to the world with a serial port on one side and as a normal Bluetooth device on the other side. Communication is supported with a serial port from one side to the other. On a PC, Phone or whatever device, it creates a serial port. So communication is basically very transparent and assimilated to a serial port from end to end.

![JY-MCU Arduino Bluetooth Wireless Serial Port Module](http://img.dxcdn.com/productimages/sku_104299_1.jpg)![JY-MCU Arduino Bluetooth Wireless Serial Port Module](http://img.dxcdn.com/productimages/sku_104299_3.jpg)

When I received it, I was impatient to test it. First step was to peer it with a PC. I looked at the documentation and found the default name for this device was “linvor” and found out the passkey was 1234. After cabling it with 3.3V (my board support 3.3V to 12V alimentation) and the ground, and approximately 1 minutes, I peered it!

New step was to write a bit of code to test all this. I decided to do a very basic echo program. So whatever it will receive, it will send it back to the calling program. On the netduino board, I’ll use the COM1 (pins D0 and D1). I found also in less than 1 minute that the default configuration was 9600 bauds, 8 bits, no parity and 1 bit stop. So I wrote this very simple code for the test, very hard to do more basic than that:

 
```csharp
using System; 
using System.Net; 
using System.Net.Sockets; 
using System.Threading; 
using Microsoft.SPOT; 
using Microsoft.SPOT.Hardware; 
using SecretLabs.NETMF.Hardware; 
using SecretLabs.NETMF.Hardware.Netduino; 
using System.Text; 
using System.IO.Ports; 
namespace Bluetooth { 
    public class Program {
         static SerialPort serial;
        public static void Main() { 
            // initialize the serial port for COM1 (pins D0 and D1)  
            serial = new SerialPort(SerialPorts.COM1, 9600, Parity.None, 8, StopBits.One); 
            // open the serial-port, so we can send and receive data  
            serial.Open(); // add an event-handler for handling incoming data  
            serial.DataReceived += new SerialDataReceivedEventHandler(serial_DataReceived); 
            //wait until the end of the Universe :-) 
            Thread.Sleep(Timeout.Infinite); 
        } 
        static void serial_DataReceived(object sender, SerialDataReceivedEventArgs e) { 
            // create a single byte array  
            byte[] bytes = new byte[1]; 
            // as long as there is data waiting to be read  
            while (serial.BytesToRead > 0) { 
                // read a single byte  
                serial.Read(bytes, 0, bytes.Length); 
                // send the same byte back  
                serial.Write(bytes, 0, bytes.Length); 
            } 
        } 
    }
} 
```

I launch a simple serial port program like the old Hyper terminal on the PC where I peered the Bluetooth device and ran the test. Good surprise, I selected the port created on my PC (was port 6), open the port. The Bluetooth device went from a red blinking led to a always on led showing the device was correctly peered. Sounds good so far! So I typed “bonjour” and send it. instantly I get the “bonjour” back. 

So cool it’s working! I wanted to know more about the cheap and what can be setup, changes like the name of the device, the pin, the baud rate, etc. I used my preferred search engine Bing and quickly found out that it’s possible to change lots of things by sending couple of AT command. Those commands were used at the old age of modems ![Sourire](/assets/4401.wlEmoticon-smile_2.png) It just remembered me that!

Even if there are lots of cheap existing like the one I bought, most support exactly the same commands. I found a good documentation [there](http://www.cutedigi.com/pub/Bluetooth/BMX_Bluetooth_quanxin.pdf). It’s not the same cheap and the AT commands are a bit different but I quickly found out that most were working. So I’ve decided to test if it was working. All what you have to do is send the commands when the device is not peered. You can do it either with a USB to serial FTDI cheap or directly from the board. I did it directly from the Netduino by modifying  a bit the code to send the commands. I found the most interesting commands were the following:

* AT+NAMEnewname\r\n to change the device name, you get an answer 
* AT+PINxxxx\r\n to change the pin code, default is 1234 
* AT+BAUDx\r\n where X goes from 1 to 8 (1 = 1200 to 8 = 115200) to change the baud rate  

I send couple of commands to test and it worked just perfectly ![Sourire](/assets/4401.wlEmoticon-smile_2.png) So I renamed the device to LaurelleBT instead of linvor. As the device was already peered, Windows did not had to reinstall drivers or cut the communication, it was just about changing the displayed name:

![image](/assets/1072.image_1E70ED88.png)

So that’s it! In 5 minutes I had a working Bluetooth module on my board. I was positively surprised and I’ll buy more for sure! Next step it to mount it on a robot and pilot it from a Windows Phone or Windows 8 device.

