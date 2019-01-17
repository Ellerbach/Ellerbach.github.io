---
layout: post
title:  "Writing a generic ASP.NET handler to return an image and control the image"
date: 2011-11-15 23:07:36 +0100
categories: 
---
In one of my project, I want to integrate the picture from an IP camera to a web site. The idea is to be able to put a IP camera in my [Lego room]({% post_url 2011-09-12-Implementing-a-simple-HTTP-server-in-.NET-Microframework %}) and see it for real. Especially to be able to control it. this project is linked to the one to light my Lego city. Instead of having the static image of the city, I want to be able to show the real time image. For this project, I’m using a [netduino and .NET Microframework]({% post_url 2011-09-09-netduino-board-geek-tool-for-.NET-Microframework %}).

Most IP camera do propose a simple URL where you can get the image. So it is very easy to integrate. But what if you want to control when to display the picture? Let say you don’t want to display the picture over night or just in small period of time? Or you want to dynamically add something to the picture like a layer? Well, IP camera are not smart enough to do it. So you’ll have to write some code. 

In ASP.NET you cant easily write a generic handler with the .ashx extension. that’s what I’ll do. And I’ll do it in VB.NET, that will change from my previous post that I did in C#. And no, I won’t do it in C++, php or Java ![Tire la langue](/assets/3036.wlEmoticon-smilewithtongueout_2.png). Yes, I do love VB too. Not only C#. As started developing a very long ago when the only thing you had on a PC was BASICA, the BASIC that was implemented in the IBM PC ROM. My father had a PC at home he was using for his own business.I was allow to use it but as I did not get any floppy disk, the only thing I was able to do was coding. And that’s at this time at 10 I started to code… in BASIC. and since then I never stopped coding ![Sourire](/assets/4401.wlEmoticon-smile_2.png) 

The simple code is the following one:

```vb
<%@ WebHandler Language="VB" Class="Image" %> 
Imports System 
Imports System.Web 
Imports System.Net 
Imports System.IO 
Imports System.Drawing 
Imports System.Drawing.Imaging 
Imports System.Drawing.Drawing2D
Public Class Image : Implements IHttpHandler 
    Public Sub ProcessRequest(ByVal context As HttpContext) 
    Implements IHttpHandler.ProcessRequest 
        context.Response.ContentType = "image/jpeg" 
        context.Response.AddHeader("Cache-Control", "private,must-revalidate,post-check=1,pre-check=2,no-cache") 
        Dim instanceHTTP As New WebClient 
        Dim MyUri As New Uri("http://yourcameraurl/image.jpg") 
        Dim returnValue As Stream 
        Try 
            returnValue = instanceHTTP.OpenRead(MyUri) 
            Dim MyImage As System.Drawing.Image = System.Drawing.Image.FromStream(returnValue) 
            MyImage.Save(context.Response.OutputStream, System.Drawing.Imaging.ImageFormat.Jpeg)  
            MyImage.Dispose()  
        Catch ex As Exception 
            context.Response.Write("Error") 
        End Try 
    End Sub 
    Public ReadOnly Property IsReusable() As Boolean 
    Implements IHttpHandler.IsReusable 
        Get 
            Return False 
        End Get 
    End Property 
End Class 
```

On top the page, you have to declare that it’s a WebHandler and implement IHttpHandler. When you’ve done that, add the ProcessRequest function as following:

```vb
 Public Sub ProcessRequest(ByVal context As HttpContext) Implements IHttpHandler.ProcessRequest 
```

And also the read only property IsReusable. This function basically allow you to control if you want the ProcessRequest function to be reusable without executing the code or not. Here, the idea is to refresh the image each time the function is called so the property will return false.

The ProcessRequest function gives you a HttpContext object. This object allow you to get many information like the brower type, the IP request address and more. But it also allow you to write in the response object. And that’s what interest us here as we want to send back an image.

 
```vb
context.Response.ContentType = "image/jpeg" 
context.Response.AddHeader("Cache-Control", "private,must-revalidate,post-check=1,pre-check=2,no-cache")
```
Those 2 just set the content type as an image so the browser will know how to interpret the content before having it at all. It does this interpretation automatically based on the extension of the file usually even if the type is not set. But here, it is mandatory to define as the extension of you file is .ASHX. And is it not interpreted as an image by any browser ![Sourire](/assets/4401.wlEmoticon-smile_2.png) and the .ASHX can virtually return anything, it can be images, text, videos, PowerPoint, Excel file, Word docs, zip or whatever you want. So don’t forget this line! Still the browser (at least Internet Explorer ![Sourire](/assets/4401.wlEmoticon-smile_2.png)) is smart enough to figure out that when you are in the “Error” returned in the stream is text and not a jpg…

the second line is about adding a header. Here, I’m not really sure I do it correctly but I try to specify that the browser can’t cache the image as I want always a fresh image.

 
```vb
Dim instanceHTTP As New WebClient  
Dim MyUri As New Uri("http://yourcameraurl/image.jpg")
Dim returnValue As Stream
```
The instanceHTTP variable will be used to download the image from the web cam. It’s a WebClient which allow to do this kind of operation. It can be done synchronously or asynchronously. 

The MyURI is used to create the URI that will be used to download the picture. So put in the string the address of your webcam. It is quite easy to find, just open your webcam in a browser, usually it display a page which includes the picture of your image. Right click on the image and you get the URL. 

Then we will need a stream object to put the image in and resend to the response object. that’s what returnValue is.

 
```vb
 Try 
    returnValue = instanceHTTP.OpenRead(MyUri) 
    Dim MyImage As System.Drawing.Image = System.Drawing.Image.FromStream(returnValue) 
    MyImage.Save(context.Response.OutputStream, System.Drawing.Imaging.ImageFormat.Jpeg)
    MyImage.Dispose()  
Catch ex As Exception 
    context.Response.Write("Error") 
End Try
```

I know you are a good developer so I know when you are trying to access critical resources or resources that may not be present that you are using a try catch section. Good, you are a great developer ![Sourire](/assets/4401.wlEmoticon-smile_2.png). The fact is that trying to download a picture from a webcam may not work. The webcam may be down, your kids may have switch it off or your cat or your mother or whatever can happen. 
 
```vb
returnValue = instanceHTTP.OpenRead(MyUri)
```

 The 3 variables created previously are used in this line of code. It does open a stream (returnValue) from the WebClient (instanceHTTP) which has a URI (MyURI). That’s done, we are ready to download the picture from the camera.

```vb
Dim MyImage As System.Drawing.Image = System.Drawing.Image.FromStream(returnValue)
```

 So I create an Image object which I get from the camera thru the function FromStream. On this image, I can do whatever I want. Add text, change the size, colors or do any manipulation.
 
```vb
MyImage.Save(context.Response.OutputStream, System.Drawing.Imaging.ImageFormat.Jpeg)
```

Here, I just save the image as a jpeg in the Response object. And I don’t forget to dispose the image as I don’t need it anymore.

And that’s it! And you can image returning video or file you have created on the flight. Or pages or whatever you want. It is as simple to use handlers in ASP.NET. This implementation as it is as not really a usage. It is much simple to directly use the address of the camera. The only usage you can see is to secure your internal network and the access to the camera. By doing this you are impersonalizing the access to the camera. It is view as a black box from your web server.

I’ll try to do couple of more post to show examples on how you can create thumbnails on the flight using this technic or maybe how you can restrict access regarding hours. I’m just a marketing guy with a technical background doing code in planes and enjoying it ![Sourire](/assets/4401.wlEmoticon-smile_2.png) So any feedback on the code an those articles is welcome. And I see in my stats you are more and more to read them!

