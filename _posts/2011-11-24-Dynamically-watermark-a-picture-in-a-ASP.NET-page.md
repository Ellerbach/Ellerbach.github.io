---
layout: post
title:  "Dynamically watermark a picture in a ASP.NET page"
date: 2011-11-24 09:29:54 +0100
categories: 
---
In a [previous post]({% post_url 2011-11-15-Writing-a-generic-ASP.NET-handler-to-return-an-image-and-control-the-image %}), I’ve explain how to use generic ASP.NET handlers to display a picture. The concept is easy to use and perfect to manipulate any kind of document you want to output in your web server. There equivalent of this in technologies like PHP or Java but it’s so easy to do using ASP.NET. In my previous post, I’ve used VB, so I’ll continue with VB ![Sourire](/assets/4401.wlEmoticon-smile_2.png) Don’t worry, it’s as easy to do in C#!

 The overall code to watermark an image dynamically using the generic handler and the minimum manipulation code is here:

 
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
        Dim MaCamera As New Camera  
        Dim MyUri As New Uri("file://c:/Temp/image.jpg") 
        Dim returnValue As Stream 
        Try 
            returnValue = instanceHTTP.OpenRead(MyUri) 
            Dim MyImage As System.Drawing.Image = System.Drawing.Image.FromStream(returnValue) 
            Dim MyText As String = Now.ToString("yyy-MM-dd HH-mm") 
            Dim MyGraphics As Graphics = Graphics.FromImage(MyImage) 
            Dim MyFont As Font = New Font(FontFamily.GenericSerif, 12) 
            MyGraphics.DrawString(MyText, MyFont, Brushes.White, 100, 100)
            MyImage.Save(context.Response.OutputStream, System.Drawing.Imaging.ImageFormat.Jpeg) 
            MyGraphics.Dispose() 
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

I will not explain again what I’ve explain in the previous post on the way handlers are working. So I’ll just concentrate on the image manipulation and how to write text in an image dynamically.
 
```vb
returnValue = instanceHTTP.OpenRead(MyUri) 
Dim MyImage As System.Drawing.Image = System.Drawing.Image.FromStream(returnValue)
 Dim MyText As String = Now.ToString("yyy-MM-dd HH-mm") 
 Dim MyGraphics As Graphics = Graphics.FromImage(MyImage) 
 Dim MyFont As Font = New Font(FontFamily.GenericSerif, 12) 
 MyGraphics.DrawString(MyText, MyFont, Brushes.White, 100, 100) 
 MyImage.Save(context.Response.OutputStream, System.Drawing.Imaging.ImageFormat.Jpeg) 
 MyGraphics.Dispose() MyImage.Dispose()
```

I first open a URI which contains an image. In my case a static image sitting on the hard drive but the idea here is to take it from a webcam for example. The URI is open as a stream and passed to an Image object. 

The idea here is to watermark the image with the date. So I create a simple string containing the date and time in a dedicated formatting. I like this formatting "yyy-MM-dd HH-mm" as it’s easy to sort especially with files. It does return a text like “2011-11-24 10-47”.

Then I create a graphics object pointing on the image. This object is directly link to the image we have loaded and any modification made on the graphics object will impact the image. It’s the basic concept of pointers used here. 

In order to write text, you’ll have to choose a font. Again, nothing complicated here, choose your preferred font and the size. A good Serif and a 12 size will be enough in my example.

And let the magic work using the DrawString method to draw in the graphics, so in the image your text. The 100 and 100 params are the X and Y starting point where to write the text. It starts on the upper left corner of the image. You can also use predefined points. But for the example, I keep it very simple.

And as explain in the previous post, you just have to save your watermarked picture into the output stream and you’re done. Or almost done, don’t forget to dispose the graphics objects, better to do it especially if you want to scale up. The garbage collector will thank you ![Sourire](/assets/4401.wlEmoticon-smile_2.png)

Final result will look like that:

[![image](/assets/6708.image_thumb.png)](/assets/5226.image_2.png)

And of course, you can do more, much more using this handler. You can add shapes, transform the color schemas, apply filters, etc. .NET is full or very efficient graphics manipulations. so just use them! In my case I use it to tag pictures with date and time as in this example but also to control when I can access an image on some webcam. I do not expose them directly to the web. They are only accessible thru handlers like this. And only to specific persons (with a login and password) and on specific time. Maybe more in a next example!

As always, feedback welcome ![Sourire](/assets/4401.wlEmoticon-smile_2.png) The code and blog post written in a plane from Paris to Zagreb ![Sourire](/assets/4401.wlEmoticon-smile_2.png)

