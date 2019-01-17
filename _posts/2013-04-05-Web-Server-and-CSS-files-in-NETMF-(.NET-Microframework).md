---
layout: post
title:  "Web Server and CSS files in NETMF (.NET Microframework)"
date: 2013-04-05 01:16:37 +0100
categories: 
---
It’s been a long time I did not write anything on my blog. Not that I haven’t developed anything but just because I did not take the time to write proper articles. I’ve continue to add features to my Lego city by piloting the trains but also the switches. And I’ll try to write articles to explain how to do that for all the features.

But I will start with the modification of my Web Server to support CSS file. I did couple of demonstration of my development and each time I show the interface people were telling to me I need to work with a designer. And that’s what I finally did ![Sourire](/assets/4401.wlEmoticon-smile_2.png) I worked with [Michel Rousseau](https://www.linkedin.com/in/michel-rousseau-4b628920/) who is designer at Microsoft in the French team. And I gave him a challenge: “Design this simple web page without changing the code too much and keep it less than couple of K without any image”. Michel is used to design Windows 8 and Windows Phone apps but not very very simple page like the one I had.

And he has done an excellent job! Here is the view before and after:

![image](/assets/6215.image_1103F768.png)![image](/assets/0116.image_050497E5.png)

Now I had to implement this in my code. As the brief was to have minimal effect on the code, I was expecting to implement this quickly. Reality was a bit different. It took me a bit more time than expected for the following reasons:


* I had to implement in [my basic web server]({% post_url 2012-05-29-Creating-an-efficient-HTTP-Web-Server-for-.NET-Microframework-(NETMF) %}) a function to be able to download a file (the CSS one) 
* To read and download a file from an SD, you have to do it by chunk as the buffer size is limited (in the case of my [Netduino](http://www.netduino.com/) 1024 bit) 
* Modify the main code to care about downloaded file and also add the lines of code to support CSS 
* But the main issue was that I’ve discovered that to be able to have a CSS file, you need to have the specific type “text/css”. This is to avoid cross domain fishing and other hacking  So let see how to implement this step by step. So let start with the reading part of the file and how to send it. As explained in the last point, a CSS file has to have the correct mime type in the header. In fact, Internet Explorer and most of the other browsers such as Chrome and Firefox does not need the mime type to determine what kind of fire you are downloading. They do it with the mime type and/or with the extension. Most of the time, it’s just with the extension and reading the header of the file. But for security reason, it’s better if you have to determine correctly the type matching with the extension and the header of the file. And for CSS, it is forced like this to reinforce the security in Internet Explorer 8, 9 and 10. 

So as I had to implement this feature for CSS, I made a simple function to support some types I’ll use in other creation:

```csharp
public static void SendFileOverHTTP(Socket response, string strFilePath) { 
    string ContentType = "text/html"; 
    //determine the type of file for the http header 
    if (strFilePath.IndexOf(".cs") != -1 
        || strFilePath.IndexOf(".txt") != -1 
        || strFilePath.IndexOf(".csproj") != -1 ) { 
            ContentType = "text/plain"; 
        } 
    if (strFilePath.IndexOf(".jpg") != -1 
        || strFilePath.IndexOf(".bmp") != -1 
        || strFilePath.IndexOf(".jpeg") != -1 ) { 
            ContentType = "image"; 
        } 
    if (strFilePath.IndexOf(".htm") != -1 
        || strFilePath.IndexOf(".html") != -1 ) { 
            ContentType = "text/html"; 
        } 
    if (strFilePath.IndexOf(".mp3") != -1) {
            ContentType = "audio/mpeg"; 
        } 
    if (strFilePath.IndexOf(".css") != -1) { 
            ContentType = "text/css"; 
        } 
    string strResp = "HTTP/1.1 200 OK\r\nContent-Type: "
        + ContentType 
        + "; charset=UTF-8\r\nCache-Control: no-cache\r\nConnection: close\r\n\r\n"; 
    OutPutStream(response, strResp);
}
```

So very simple and straight forward code. I do determine the extension of the file I want to read and create a ContentType variable with the right format. Then I build the HTTP header and send the header. Very simple, efficient and straight forward code. We are doing Embedded code, it’s not the code I would do in a normal development of a real web server. But it does the job there!

Next step is reading the file and outputting it to the Socket. And I do this in the same function, right after this first part:


```csharp
FileStream fileToServe = null; 
try { 
    fileToServe = new FileStream(strFilePath, FileMode.Open, FileAccess.Read); 
    long fileLength = fileToServe.Length; 
    // Now loops sending all the data. 
    byte[] buf = new byte[MAX_BUFF]; 
    for (long bytesSent = 0; bytesSent < fileLength; ) { 
        // Determines amount of data left. 
        long bytesToRead = fileLength - bytesSent; 
        bytesToRead = bytesToRead < MAX_BUFF ? bytesToRead : MAX_BUFF; 
        // Reads the data. 
        fileToServe.Read(buf, 0, (int)bytesToRead); 
        // Writes data to browser 
        response.Send(buf, 0, (int)bytesToRead, SocketFlags.None); 
        System.Threading.Thread.Sleep(100); 
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

First step is to create a FileStream and create the Stream with the path of the file and read the length of the file. MAX_BUFF = 1024 and is the maximum size of a buffer. It depends on the .NET Microframework Platform. And we will start a loop to read part of the file and send it.

The System.Threading.Thread.Sleep(100) is necessary to allow some time for the system and other tasks. If you don’t put it and have other tasks, the risk is that the memory will get full very quickly and you’ll block all the code.

In my Web Server I have an event raised when an HTTP request is done. Here is an example of code you can place in your handling function to manage on one side the files to be downloaded from the SD and on the other side a page you’ll generate dynamically like in ASP, ASP.NET, PHP or any other dynamic language:


```csharp
//PageCSS 
if (strFilePath.Length >= pageCSS.Length) { 
    if (strFilePath.Substring(0, pageCSS.Length).ToLower() == pageCSS) { 
        string strDefaultDir = ""; 
        if (Microsoft.SPOT.Hardware.SystemInfo.IsEmulator) 
            strDefaultDir = "WINFS"; 
        else 
            strDefaultDir = "SD"; 
        WebServer.SendFileOverHTTP(response, strDefaultDir + "\\" + pageCSS);
        return; 
    } 
} 
//HTTP header 
strResp = "HTTP/1.1 200 OK\r\nContent-Type: text/html; charset=utf-8\r\nCache-Control: no-cache\r\nConnection: close\r\n\r\n"; 
strResp = WebServer.OutPutStream(response, strResp); 
// Page util 
if (strFilePath.Length >= pageUtil.Length) { 
    if (strFilePath.Substring(0, pageUtil.Length).ToLower() == pageUtil) { 
        ProcessUtil(response, strFilePath); 
        return; 
    } 
}
```

Here PageCSS = the file name of the file you are looking for (including the path if in sub directory) so something like “page.css” and pageUtil = name of a page you will generate dynamically (including the path subdirectory) so something like “util.aspx”

strFilePath = full URL including the parameters so something like “util.apsx?bd=3;tc=4”

so code looks for the name of the page in the URL and brunch it to a either the SendFileOverHTTP function we’ve just look at or another function in the case of a dynamic page.

You’ll note also that the is a case in the file part as the default directory is not the same if you are in the emulator or on a real board. The path depend also from the Platform. In the case of my Netduino, it’s CD. For emulator, it’s always WINFS.

And please do all your file reading brunching before the HTTP header part. As for files, the header is already send out. Which is not the case for the dynamic generated page. As you can send whatever you want including images, text, binary files and you’ll need to set it up correctly.

Now, lets have a look at the CSS file:

```css
@charset "utf-8"; /* CSS Document */ 
body { font-family: "Lucida Console", Monaco, monospace; font-size: 16px; color: #09C; text-align: center; margin-left: 0px; margin-top: 0px; margin-right: 0px; margin-bottom: 0px; } 
a { font-family: "Lucida Console", Monaco, monospace; font-size: 14px; color: #09F; } 
td { font-family: Arial, Helvetica, sans-serif; color: #004262; font-size: 14px; } 
input { font-family: "Lucida Console", Monaco, monospace; font-size: 16px; color: #09F; background-color: #FFF; -webkit-transition: all 0s linear 0s; -moz-transition: all 0s linear 0s; -ms-transition: all 0s linear 0s; -o-transition: all 0s linear 0s; transition: all 0s linear 0s; border: 1px none #FFF; } h1 { font-family: Arial, Helvetica, sans-serif; color: #FFF; background-color: #006699; font-size: 24px; border: thick solid #006699; } 
td { font-family: Arial, Helvetica, sans-serif; text-align: left; font-size: 16px; } 
footer { color: #09C; } 
input:hover { background-color: #09F; color: #FFF; } 
input:active { background-color: #003; } 
```

As you can see, it is very simple CSS that Michel did. It is just over righting the normal styles with colors and fonts. Nothing really complex but complicated to do something nice in only few lines of code! Well done Michel ![Sourire](/assets/4401.wlEmoticon-smile_2.png)

Now implementation in the rest of the code and all the pages is quite straight forward and simple, here is an example:

```csharp
// Start HTML document 
strResp += "<!DOCTYPE html PUBLIC \"-//W3C//DTD XHTML 1.0 Transitional//EN\" \"http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd\">"; 
strResp += "<html xmlns=\"http://www.w3.org/1999/xhtml\"><head><title>Gestion des trains</title>"; 
//this is the css to make it nice :-) 
strResp += "<link href=\"" + pageCSS + "?" + securityKey + "\" rel=\"stylesheet\" type=\"text/css\" />"; 
strResp += "<meta http-equiv=\"Content-Type\" content=\"text/html; charset=utf-8\"/></head><body>"; 
strResp += "<meta http-equiv=\"Cache-control\" content=\"no-cache\"/>"; 
strResp = WebServer.OutPutStream(response, strResp);
```

The HTML page is build and the CSS page is added with a parameter (a security key). And this line including the CSS is the only line I have to add to go from the original design to the nice new one!

So that’s it for this part. I’ll try to find some time to write additional examples.

