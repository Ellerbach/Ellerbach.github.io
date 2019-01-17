---
layout: post
title:  "Display overlay images in HTML and javascript with .NET Microframework"
date: 2011-10-22 01:29:59 +0100
---
In my current project of [lighting my Lego city]({% post_url 2011-10-11-Lighting-my-Lego-city-using-.NET-Microframework %}), I’m working on a simple web interface that will allow thru a HTTP web page to display an image and small lamp icons on overlay. In my previous project on automate my sprinklers, [I’ve implemented a HTTP web server]({% post_url 2011-09-12-Implementing-a-simple-HTTP-server-in-.NET-Microframework %}) in my [netduino]({% post_url 2011-09-09-netduino-board-geek-tool-for-.NET-Microframework %}). It’s working like any Apache or IIS with kind of dynamic pages like ASP, php or Java. of course the implementation is much smaller and do cover only the very basic functions like GET request and sending file over.

 So as for the sprinkler project, I started with the HTTP Server example provided in the .NET Microframework SDK. I’ve removed what I did not need (same as for the Sprinkler project) and added couple of features like conversion, GET parameter management. Now I have the base, I was looking at a way to display images on overlap. my HTML bases are quite old, last time I did a page by hand was for the sprinkler project but not really with images. So of course, I remember the <img> tag and the map attribute to create an image that can bi clicked in multiple areas but that was not really what I wanted. I wanted multiple real images to be displayed on top of each other and having the ability to click on them.

 I rapidly found the <span> tag which allow to display anything on anyplace on top of HTML code. So it was looking perfect for my usage. The way to use it is to create an area and place HTML code inside with a position on the page. As an example:


```html
<span style='position: absolute;margin-left:158px; margin-top:59px; width:55px; height:44px; top:20px; left:50px;'><a href="/"><img border=0 width=55 height=44 src="lampoff.png"></a></span> 
```
 This span will be positioned in an absolute position on the page, starting from 158 pixels from the left side of the page and 59 from the top. Then you have to add another 20 pixels from the top and 50 from the left. The size of this span will be 55 pixels width and 44 pixels height. And it will contain an &lt;a&gt; tag with an image. The size of the image is 55 pixels width and 44 pixels height. the question you may ask is why is the span control taking 2 parameters for each position? one called margin-left + left and for vertical positioning margin-top + top. So good question ![Sourire](/assets/4401.wlEmoticon-smile_2.png) the idea there is to allow to position first the span based on a control on the page and then offer the possibility to add an offset. the top/left is the first positioning and the margin-top/margin-left the second one. You want a real example? OK, so lets go with my final page which looks like this:

 ![image](/assets/4428.image_2.png)

 As you can see, I’ve positioned some lamps on the main image. And on top of the image there is some text. Imagine than in my code, I want to add more text before, of change the font size. It will change the position of the main picture. So the position of the lamp icons will change too. If I hard code the position of those lamp icons with absolute numbers, I’ll have to change all of them if I do a modification on the page.

 Now, If I can get dynamically the position of the main image and then use the possibility to use the margin to adjust the position on the main image, the only thing I have to care of is really the position on the picture. Well, the bad news there is that in HTML there is no other way than doing javascript to get a position of an object. So I had to code in javascript, 2 functions. 1 which will give me the absolute top position of the image and one for the left position. I have to admit I did not write javascript for such a long time… And I also have to admit, it’s not a language I like very much. No strong typing, no real way to debug correctly, no real language structure and less productivity than with languages like C#, VB, Java, C/C++. But I did it, and here is the result:


```javascript
<script langague="Javascript"> // function to return the absolute Top and Left position function findTop(iobj) { ttop = 0; while (iobj)   
{ ttop += iobj.offsetTop; iobj = iobj.offsetParent; }   
return ttop; } function findLeft(iobj) { tleft = 0; while (iobj)   
{ tleft += iobj.offsetLeft; iobj = iobj.offsetParent; }   
return tleft; } //to use it if the id of your image is MyImage //findTop(document.all.MyImage); </script> 
```

 The function findTop will take the object you want the top position in argument. In HTML you can have imbricated objects so you have to take care of this to get the real position of an object. So I did a very simple recursive loop to sum the top position of all the parents objects up to the point there won’t be any object. And I simply return the sum of those positions. I did the exact same function for the left position. Now question is: how to use it? well, that’s very simple ![Sourire](/assets/4401.wlEmoticon-smile_2.png) Taking the previous example, it will be:


```javascript
<span style='position: absolute;margin-left:158px; margin-top:59px; width:55px; height:44px; top:findTop(document.all.MyImage);   
left:findLeft(document.all.MyImage);'><a href="/">  
<img border=0 width=55 height=44 src="lampoff.png"></a></span> 
```
 So rather than have a number after the left and top attributes, I just call the functions. It assume that the name of the picture is MyImage. To give a name to an object, just use the id attribute:


```javascript
<img alt="Map of the city" id=MyImage src="ville.jpg" /> 
```
 and that’s it! the overall code for the page I generate automatically looks like:


```html
<HTML><BODY>netduino Lego City  
<p>HTTP Method: GET<br>   
Requested URL: "/default.aspx?id=0;lg=True<br>   
HTTP Version: 1.1"<p>  
<script langague="Javascript"> // You don't need to worry about this function findTop(iobj) { ttop = 0; while (iobj)   
{ ttop += iobj.offsetTop; iobj = iobj.offsetParent; }   
return ttop; } function findLeft(iobj) { tleft = 0; while (iobj)   
{ tleft += iobj.offsetLeft; iobj = iobj.offsetParent; }   
return tleft; } </script> <p> Click the lamp to switch on or off </p><span style='position:absolute;margin-left:158;   
margin-top:59;width:26px;height:45px; top:findTop(document.all.MyImage);   
left:findLeft(document.all.MyImage);'>  
<a href='/default.aspx?id=0;lg=False'>  
<img border=0 width=26 height=45 src="/WINFS/lampon.png"></a></span>  
<span style='position:absolute;margin-left:208;   
margin-top:300;width:26px;height:45px; top:findTop(document.all.MyImage);   
left:findLeft(document.all.MyImage);'>  
<a href='/default.aspx?id=1;lg=True'>  
<img border=0 width=26 height=45 src="/WINFS/lampoff.png"></a></span>  
<span style='position:absolute;margin-left:10;   
margin-top:10;width:26px;height:45px; top:findTop(document.all.MyImage);   
left:findLeft(document.all.MyImage);'>  
<a href='/default.aspx?id=2;lg=True'>  
<img border=0 width=26 height=45 src="/WINFS/lampoff.png"></a></span>  
<span style='position:absolute;margin-left:700;   
margin-top:550;width:26px;height:45px; top:findTop(document.all.MyImage);   
left:findLeft(document.all.MyImage);'>  
<a href='/default.aspx?id=3;lg=True'>  
<img border=0 width=26 height=45 src="/WINFS/lampoff.png"></a></span>  
<img alt="" src="/WINFS/ville.jpg" /></p></BODY></HTML> 
```
 The /WINFS/ path is due to the fact I was running this sample thru the emulator. If you run it on the netduino board, then the SD path is /SD/. 

 So I have created all what I need to display an image, put some other pictures as overlap, have the possibility to click on them. Now the next step is about generating dynamically this page from the netduino which I did and I will post explanations in a next post. Of course, there is the possibility to do all this with CSS. It’s working exactly the same way. My need is very simple so I won’t use CSS there but maybe to make my page very sexy in the future. Enjoy this code and post written in a plane ![Rire](/assets/0842.wlEmoticon-openmouthedsmile_2.png)

