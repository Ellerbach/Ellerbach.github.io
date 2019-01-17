---
layout: post
title:  "Creating dynamically a web page using .NET Micro framework"
date: 2011-12-07 08:42:46 +0100
categories: 
---
In a [previous post]({% post_url 2011-10-24-Reading-file-in-.NET-Microframework %}), I’ve explain how to read a file using .NET Microframework, how to [create a setup file and load it]({% post_url 2011-11-04-Read-a-setup-file-in-.NET-Microframework %}) (and write it also), how to [implement a web server using HTTP]({% post_url 2011-09-12-Implementing-a-simple-HTTP-server-in-.NET-Microframework %}). The idea is to be able to click on an image and a led will switch on or off in the Lego city. I’ll show in this post how to generate dynamically using files and code the page containing the overlay images from this post.

```csharp
HttpListenerRequest request = context.Request; 
HttpListenerResponse response = context.Response; 
// decode params string 
strParam = request.RawUrl; 
ParamPage MyParamPage = new ParamPage(); 
bool bLight = false; int iID= -1; 
Param[] Params = decryptParam(strParam); 
if (Params != null) 
    for (int i = 0; i < Params.Length; i++) { 
        //on cherche le paramètre 
        strMonth int j = Params[i].Name.ToLower().IndexOf(MyParamPage.id); 
        if (j == 0) { 
            Convert.ToInt(Params[i].Value, out iID); 
        } 
        j = Params[i].Name.ToLower().IndexOf(MyParamPage.lg); 
        if (j == 0) { 
            Convert.ToBool(Params[i].Value, out bLight); 
        } 
    }
```

 This first part of the code is very similar to what I’ve explained in this post. The page parameters are analyzed, they are converted into real values. Here, each light has an id and a status. They can be on or off. Again, more information in some of my previous posts here.
 
```csharp
// if the ID value is valid, just light up the light :-) 
if ((iID != -1) && (iID < myLegoLight.Count)) { 
    ((LegoLight)myLegoLight[iID]).Light = bLight; 
}
```

 As it’s a web page and anyone can access it, there are a minimum of integrity to do ![Sourire](/assets/4401.wlEmoticon-smile_2.png) I just check if the id is in the limit and load the object. More information on the LegoLight class here.
 
```csharp
string strResp = "<HTML><BODY>netduino Lego City<p>"; *
// Print requested verb, URL and version.. Adds information from the request. 
strResp += "HTTP Method: " + request.HttpMethod + "<br> Requested URL: \"" + request.RawUrl 
    + "<br> HTTP Version: " + request.ProtocolVersion 
    + "\"<p>"; strResp = OutPutStream(response, strResp); 
//send first part of the HTML File 
SendFile(response, strDefaultDir + "\\page1.ht");
```

 I create a string with the first part of the HTML page and for debug purpose, I add couple of information and send the string to the response object. Again, everything is explained there.

 The SendFile function is the one described in the post explaining how to read a file. Details here. strDefaultDir contains the directory where the file page1.ht is located. In the emulator it’s WINFS and in the netduino it’s SD. I change it “manually” depending on the environment I’m in. No real need to write code for this. Remember we are running an embedded application, no one will change it!! So things like this one can be written in the marble ![Sourire](/assets/4401.wlEmoticon-smile_2.png)

 Page1.ht contains the following:
 
```javascript
<script langague="Javascript"> 
// You don't need to worry about this 
function findTop(iobj) { 
    ttop = 0; while (iobj) { 
        ttop += iobj.offsetTop; 
        iobj = iobj.offsetParent; 
    } 
    return ttop; 
    } 
function findLeft(iobj) { 
    tleft = 0;
     while (iobj) { 
        tleft += iobj.offsetLeft; 
        iobj = iobj.offsetParent; 
    } 
    return tleft; 
} 
//This is all you need: 
//findTop(doucment.all.MyImage); 
</script> 
<p> 
    Click the lamp to switch on or off 
</p> 
```

As expected, it’s just HTML code ![Sourire](/assets/4401.wlEmoticon-smile_2.png) and if you want to understand what is the javascript in the page doing, read this post. By the way, I don’t like javascript, I truly prefer C/C++, C#, VB or even real Java (I say I prefer Java over javascript but it’s far far away from the 3 others ![Sourire](/assets/4401.wlEmoticon-smile_2.png)).
 
```csharp
//build array of Span 
LegoLight mLegoLight; 
for (int i = 0; i < myLegoLight.Count; i++) { 
    mLegoLight = (LegoLight)myLegoLight[i]; 
    strResp += "<span style='position:absolute;margin-left:" + mLegoLight.PosX +  
        "px; margin-top:" + mLegoLight.PosY + "px;width:26px;height:45px;top:findTop(document.all.MyImage); left:findLeft(document.all.MyImage);'>";
    strResp += "<a href='/" + MyParamPage.pageDefault + ParamStart+ MyParamPage.id +   
        ParamEqual + i + ParamSeparator + MyParamPage.lg + ParamEqual + !mLegoLight.Light + "'>"; 
    strResp = OutPutStream(response, strResp); 
    if (mLegoLight.Light) 
        SendFile(response, strDefaultDir + "\\imgon.ht"); 
    else 
        SendFile(response, strDefaultDir + "\\imgoff.ht"); strResp += "</a></span>"; 
    strResp = OutPutStream(response, strResp); 
}
```

The next part of the code displays as a span over the main image all LegoLight object and depending if the state of the light is on or off send a different file. in one case it’s imgon.ht and in the other imgoff.ht. Both code are very similar:
 
```html
<img border=0 width=26 height=45 src="/WINFS/lampon.png">
<img border=0 width=26 height=45 src="/WINFS/lampoff.png">
```

Of course the /WINFS here reference the emulator path. In the reality it will be /SD as content will be in an SD card in the real word.

And to finish, couple of lines:
 
```csharp
//send rest of the file 
SendFile(response, strDefaultDir + "\\page2.ht"); 
strResp += "</BODY></HTML>"; 
strResp = OutPutStream(response, strResp);
```

They basically send the rest of the page. page2.ht just contains the image:

```html
<img alt="" src="/WINFS/ville.jpg" /></p> 
```

 Again, here I’m using the emulator. In order to make it work, /WINFS need to be change to /SD to work on the Netduino. 

 And you are done, here is a simple example that mix file and dynamic generated code to produce a web page in .NET Microframework ![Sourire](/assets/4401.wlEmoticon-smile_2.png) Enjoy the beauty of the code ![Sourire](/assets/4401.wlEmoticon-smile_2.png)

