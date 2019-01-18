---
layout: post
title:  "Using XMLHttpRequest to pilot a Lego train dynamically in HTML 5"
date: 2012-06-20 13:51:25 +0100
categories: 
author: "Laurent Ellerbach"
thumbnails: "/assets/2012-06-20-thumb.jpg"
---
It’s a long time I did not write a blog post. I was very busy and had no time to write and code anything in the last weeks. I still have a lot of work but I need an intellectual break for the evening. So I do not write this post from a plane but from an hotel room. In my past [blog posts]({% post_url 2012-04-26-Using-netduino-and-.NET-Microframework-to-pilot-any-Lego-Power-Function-thru-Infrared-(part-3) %}) I’ve explained how to pilot any Lego Power System with a [Netduino]({% post_url 2011-09-09-netduino-board-geek-tool-for-.NET-Microframework %}) using .NET Microframework. 

In the [HTTP Web server I’ve implemented]({% post_url 2012-05-29-Creating-an-efficient-HTTP-Web-Server-for-.NET-Microframework-(NETMF) %}). 

Now if you want to pilot in a web interface multiple trains, and click on buttons or pictures to get an action without opening a new web page or refreshing the page, you need to do some Scripting in your HTML page. I’m not a web developer, I don’t like Scripting languages as they are not strict enough to write correct code and imply too many errors. They drastically increase your development time! I truly prefer a good language like C#, VB or even Java and I can go up to C/C++ ![Sourire](/assets/4401.wlEmoticon-smile_2.png) Now, if I want to avoid any problem, I can jump into Eiffel ![Sourire](/assets/4401.wlEmoticon-smile_2.png) OK, I won’t go up to there, I’ll stay with java script in an HTML5 page.

What I want is to call my command page in the background of the page and stay in the HTML page when I click on a button. There is a nice object in HTML which allow you to do that which is XMLHttpRequest. It is implemented in all decent browsers.

Here is the code that I generate dynamically (I’ll show you the code later) and I’ll explain you how it works:


```html
<html xmlns="http://www.w3.org/1999/xhtml"><head> 
<title></title></head><body> 
<SCRIPT language="JavaScript"> var xhr = new XMLHttpRequest(); 
function btnclicked(boxMSG, cmdSend) { 
    boxMSG.innerHTML = "Waiting"; 
    xhr.open('GET', 'singlepwm.aspx?' + cmdSend + '&sec='); 
    xhr.send(null); xhr.onreadystatechange = function () { 
        if (xhr.readyState == 4) { 
            boxMSG.innerHTML = xhr.responseText; 
        } 
    }; 
} </SCRIPT> 
<TABLE BORDER="0"><TR><TD> 
<FORM>Super train</TD><TD> 
<INPUT type="button" onClick="btnclicked(document.getElementById('train0'), 'pw=11&op=0&ch=254')" value="<"></TD><TD> 
<INPUT type="button" onClick="btnclicked(document.getElementById('train0'), 'pw=8&op=0&ch=254')" value="Stop"></TD><TD> 
<INPUT type="button" onClick="btnclicked(document.getElementById('train0'), 'pw=5&op=0&ch=254')" value=">"></TD><TD> 
<span id='train0'></span></FORM></TD></TR> 
```

In the script part of the page, I have created a simple script. Those lines of code is what is necessary to do a synchronous call of an HTTP page and display the result in the page. 

I create an XMLHttpRequest object which I call xhr. The function call btnclicked takes 2 arguments. the first one is the element of the page I will put the results of the request. And the second one is the command (the parameters) to pass to the URL which will pilot the infrared led as explain previously.

The function is very simple. First, I put “Waiting” in the element. Then I open the XMLHttpRequest object. I open it with GET and pass the overall URL.

The request is done when the send function is called. It is a synchronous call, so it get to the onreadystatechange when it is finished. 

Here, when you read the documentation, the readyState 4 mean that everything went well and you have your data back. My function return “OK” when everything is OK and “Problem” if there is a problem.

Now let have a look at the rest of the HTML page. I decided to create a form with button input. Each button input has a onClick event. This event can be linked to a script. So I will call the fucntion describe before and give a span element (here train0) and the URL. The URL is different depending if you want to train to go forward, backward or stop. And I put all this in a nice table top be able to have multiple trains.

The page with 4 trains looks like this:

![image](/assets/5807.image_2.png)

I’ve clicked on the forward button, “Wainting” is displayed in span. And as soon as the command will finish, it will either display OK or Problem. In my case, Problem will be displayed as in the Emulator, there is no SPI port!


```csharp
// Start HTML document strResp = "<!DOCTYPE html PUBLIC \"-//W3C//DTD XHTML 1.0 Transitional//EN\" \"http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd\">"; 
strResp += "<html xmlns=\"http://www.w3.org/1999/xhtml\"><head><title></title></head><body>"; 
// create the script part 
strResp += "<SCRIPT language=\"JavaScript\">"; 
strResp += "var xhr = new XMLHttpRequest(); function btnclicked(boxMSG, cmdSend) boxMSG.innerHTML=\"Waiting\";"; 
strResp += "xhr.open('GET', 'singlepwm.aspx?' + cmdSend + '&" + securityKey + "');"; 
strResp += "xhr.send(null); xhr.onreadystatechange = function() {if (xhr.readyState == 4) {boxMSG.innerHTML=xhr.responseText;}};}"; 
strResp += "</SCRIPT>"; 
strResp = WebServer.OutPutStream(response, strResp); 
// Create one section for each train 
strResp += "<TABLE BORDER=\"0\">"; 
for (byte i = 0; i < myParamRail.NumberOfTrains; i++) { 
    strResp += "<TR><TD><FORM>" + myParamRail.Trains[i].TrainName + 
        "</TD><TD><INPUT type=\"button\" onClick=\"btnclicked(document.getElementById('train" 
        + i + "'),'pw=" 
        + (16 - myParamRail.Trains[i].Speed); 
    strResp += "&op="+ myParamRail.Trains[i].RedBlue 
        + "&ch=" + (myParamRail.Trains[i].Channel - 1) 
        + "')\" value=\"<\"></TD>"; 
    strResp += "<TD><INPUT type=\"button\" onClick=\"btnclicked(document.getElementById('train" 
        + i + "'),'pw=8"; 
    strResp += "&op=" + myParamRail.Trains[i].RedBlue 
        + "&ch=" + (myParamRail.Trains[i].Channel - 1) 
        + "')\" value=\"Stop\"></TD>"; 
    strResp += "<TD><INPUT type=\"button\" onClick=\"btnclicked(document.getElementById('train" 
        + i + "'), 'pw=" + myParamRail.Trains[i].Speed; 
    strResp += "&op=" + myParamRail.Trains[i].RedBlue 
        + "&ch=" + (myParamRail.Trains[i].Channel - 1)
        + "')\" value=\">\"></TD>"; 
    strResp += "<TD><span id='train" 
        + i + "'></span></FORM></TD></TR>"; 
    strResp = WebServer.OutPutStream(response, strResp); 
} 
strResp += "</TABLE><br><a href='all.aspx?" 
    + securityKey +  "'>Display all page</a>"; 
strResp += "</body></html>";
strResp = WebServer.OutPutStream(response, strResp); 
```

The code to generate the HMTL page is here. As you see it is manually generated. The table which contains the name of the train, the backward, stop and forward button is created totally dynamically. Each train may have a different speed and use different channels so the URL is generated dynamically. 

All this takes time to generate and to output in the Stream. So it’s much better to do it only 1 time and then call a simple and small function which will return just a bit of text.

Last part, on the server header, you have to make sure you add “Cache-Control: no-cache” in the response header. If you don’t do it, only the first request will be send to the netduino board back. XMLHttpRequest will consider that the page will never expire. So I’ve modified the header code of my HTTP Server like this:

```csharp
string header = "HTTP/1.1 200 OK\r\nContent-Type: text/html; charset=utf-8\r\nCache-Control: no-cache\r\nConnection: close\r\n\r\n"; 
connection.Send(Encoding.UTF8.GetBytes(header), header.Length, SocketFlags.None); 
```

What is interesting there is that I can now pilot up to 8 Lego trains using this very simple interface and without having to refresh the page.

And the excellent news is that it is working from my Windows Phone so it’s even cool to pilot your Lego train from your Windows Phone. I haven’t tested from an Android or iPhone but I’m quite sure it will work the same way. Remember that all the code is sitting in a very small hardware with no OS, just .NET Microframework! As always, do not hesitate to send me your comments ![Sourire](/assets/4401.wlEmoticon-smile_2.png)

