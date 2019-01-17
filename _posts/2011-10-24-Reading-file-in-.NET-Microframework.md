---
layout: post
title:  "Reading file in .NET Microframework"
date: 2011-10-24 21:51:04 +0100
categories: 
---

For the one reading my articles, you know I’m developing in .NET Microframework an application to be able to [switch on and off led in my Lego City]({% post_url 2011-10-11-Lighting-my-Lego-city-using-.NET-Microframework %}). In the past post, I’ve explain how to setup a [web server with HTTP]({% post_url 2011-09-12-Implementing-a-simple-HTTP-server-in-.NET-Microframework %}), generate dynamic pages, handle parameters. And in the [last one I show how to overlay 2 images]({% post_url 2011-10-11-Lighting-my-Lego-city-using-.NET-Microframework %}) and make the one on top clickable. In this article, I’ll explain how to read a file from the file system. I need this to be able to store images on an SD card for example and push them thru HTTP on the client. But I also want to have a setup file with the position of the led to display on the map.

 .NET Microframework offer basic IO with basic file system. It has to be a FAT format sitting on SD or equivalent. For the netduino, you have the [netduino plus version](http://www.netduino.com/netduinoplus/specs.htm) which offers a SD card reader. And the basic IO are already implemented. So you can easily read and write a file. I have to admit I get couple of problems with SD card and have hard time to find one which was working all the time. Also, it looks like there is an hardware/firmware problem with SD card. But it’s about to be fixed. I never get too many problems to read the SD card but mainly to write on it. Here, my need is about reading and not writing.

 In the HTTP Server example, there is a good example of how to read a file and send it over HTTP. Here is the function (a bit modified from the original sample for my own purpose):

```csharp
static void SendFile(HttpListenerResponse response, string strFilePath) { 
    FileStream fileToServe = null; 
    try { 
        fileToServe = new FileStream(strFilePath, FileMode.Open, FileAccess.Read); 
        long fileLength = fileToServe.Length; 
        // Once we know the file length, set the content length. 
        //response.ContentLength64 = fileLength; 
        // Send HTTP headers. Content lenght is ser 
        Debug.Print("File length " + fileLength); 
        // Now loops sending all the data. 
        byte[] buf = new byte[BUFFER_SIZE]; 
        for (long bytesSent = 0; 
        bytesSent < fileLength; ) { 
            // Determines amount of data left. 
            long bytesToRead = fileLength - bytesSent; 
            bytesToRead = bytesToRead < BUFFER_SIZE ? bytesToRead : BUFFER_SIZE; 
            // Reads the data. 
            fileToServe.Read(buf, 0, (int)bytesToRead); 
            // Writes data to browser 
            response.OutputStream.Write(buf, 0, (int)bytesToRead); 
            System.Threading.Thread.Sleep(50); 
            // Updates bytes read. 
            bytesSent += bytesToRead; 
        } 
        fileToServe.Close(); 
    } catch (Exception e) { 
        if (fileToServe != null) { 
            fileToServe.Close(); 
        } 
        throw e; 
    } 
}
```
 The function takes the HTTP response object and the file name of the file to send over HTTP. 

 Then the code is quite simple. It first create a FileStream. And as it is well developed, you have a nice try catch ![Sourire](/assets/4401.wlEmoticon-smile_2.png). When you try to access files or resources, always use a try catch. You never know what can happen. You may have the user removing the support on where the file are like the SD card, the file may be corrupted, already access by another thread, etc.

 The file is open on readonly so if there is any other thread which want to access it also in read only, it is possible:

 
```csharp
fileToServe = new FileStream(strFilePath, FileMode.Open, FileAccess.Read); 
long fileLength = fileToServe.Length;
```
 The size of the file is stored into a variable. It will be necessary because the size of the memory in a netduino is very limited and you can’t open the file totally, put it in memory and send it as you’ll probably do a regular OS like Windows or Linux. Here, there is no operating system, no page file, and very very limited resources to only couple of kilo bytes. Yes, kilo bytes, not mega and far away giga!

 
```csharp
byte[] buf = new byte[BUFFER_SIZE]; 
for (long bytesSent = 0; 
bytesSent < fileLength; ) { 
    // Determines amount of data left. 
    long bytesToRead = fileLength - bytesSent; 
    bytesToRead = bytesToRead < BUFFER_SIZE ? bytesToRead : BUFFER_SIZE; 
    // Reads the data. 
    fileToServe.Read(buf, 0, (int)bytesToRead); 
    // Writes data to browser 
    response.OutputStream.Write(buf, 0, (int)bytesToRead); 
    System.Threading.Thread.Sleep(50); 
    // Updates bytes read. 
    bytesSent += bytesToRead; 
} 
```
 Reading the file will be done by slice. We are creating a buffer of the BUFFER_SIZE size. Here in netduino the maximum size is 1024. So the file will be read by slide of 1K and send over the response object. the loop is simple, it just read the file up to the end by slice of 1024 bytes.

 and to allow the system to do something else, it is paused couple of miliseconds. So be aware sending large file over HTTP in a netduino and any other .NET Microframework environment will require lot of loop like this and will take time. 

 The rest of the code is just about closing the file. if you are sure your file will be less than 1K, you don’t need the loop, you’ll just need to create a buffer of the right size and read all.

 So we’ve seen the basic of reading a file in .NET Microframework. In a next post we will see how to use this to read a setup file.

