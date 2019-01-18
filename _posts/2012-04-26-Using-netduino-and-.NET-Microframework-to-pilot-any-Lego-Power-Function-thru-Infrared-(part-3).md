---
layout: post
title:  "Using netduino and .NET Microframework to pilot any Lego Power Function thru Infrared (part 3)"
date: 2012-04-26 10:39:36 +0100
categories: 
author: "Laurent Ellerbach"
thumbnails: "/assets/2012-04-26-thumb.jpg"
---
In the [previous post]({% post_url 2012-04-07-Using-netduino-and-.NET-Microframework-to-pilot-any-Lego-Power-Function-thru-Infrared-(part-1) %}), I’ve explain how to create a class and the hardware that will be able to pilot any Lego Power Function. In this article, I will explain how to create a web server (like IIS or Apache but much much much more simpler) and create a simple way to call the class. 

I’ve already explained how to [create such a web server]({% post_url 2011-09-12-Implementing-a-simple-HTTP-server-in-.NET-Microframework %}) into one of my previous article. So Il will use the same idea. I will only explain the implementation of the specific part to call the Combo Mode function as I already take it in [the past articles]({% post_url 2012-04-17-Using-netduino-and-.NET-Microframework-to-pilot-any-Lego-Power-Function-thru-Infrared-(part-2) %}).

The function is part of the LegoInfrared class and looks like

```csharp
public bool ComboMode(LegoSpeed blue_speed, LegoSpeed red_speed, LegoChannel channel) 
```
LegoSpeed and LegoChannel are both enums.

The simple view of the HTTPServer Class is the following:

```csharp
public static class MyHttpServer { 
    const int BUFFER_SIZE = 1024; 
    // Strings to be used for the page names 
    const string pageDefault = "default.aspx"; 
    const string pageCombo = "combo.aspx"; 
    // Strings to be used for the param names 
    const string paramComboBlue = "bl"; 
    const string paramComboRed = "rd"; 
    const string paramChannel = "ch"; 
    // Strings to be used for separators and returns 
    const char ParamSeparator = '&'; 
    const char ParamStart = '?'; const 
    char ParamEqual = '='; 
    const string strOK = "OK"; 
    const string strProblem = "Problem"; 
    // Class to be used to find the parameters 
    public class Param  
    // Create a Lego Infrared object 
    private static LegoInfrared myLego = new LegoInfrared(); 
    public static void StartHTTPServer() { 
        // Wait for DHCP (on LWIP devices) 
        while (true) { 
            IPAddress ip = IPAddress.GetDefaultLocalAddress(); 
            if (ip != IPAddress.Any) { 
                Debug.Print(ip.ToString()); 
                break; 
            } 
            Thread.Sleep(1000); 
        }  
        // Starts http server in another thread. 
        Thread httpThread = new Thread((new PrefixKeeper("http")).RunServerDelegate); 
        httpThread.Start(); 
        // Everything is started, waiting for commands :-) 
        Debug.Print("Everything is started, waiting for command"); 
    } 
}
```

There are of course couple of other functions part of the class but I make it simple here. I created couple of const to be able to handle the name of pages and as explained in past articles, I will use URL like combo.aspx?bl=8&rd=2&ch=1

Here by convention, bl will contains the value of the blue speed, rd, the red one and ch the Channel. the combo.aspx is the page name to call the ComboMode function. And yes, I do not need to use any extention, I can use just combo? or cb? or whatever I want. But I feel it’s cool to see .aspx as an extension in a board with only couple of kb of memory ![Sourire](/assets/4401.wlEmoticon-smile_2.png)

Now let have a look at the ProcessClientGetRequest function which will allow us to switch from one page to another.

```csharp
private static void ProcessClientGetRequest(HttpListenerContext context) {  
    HttpListenerRequest request = context.Request; 
    HttpListenerResponse response = context.Response; 
    string strFilePath = request.RawUrl; 
    // Switch to the right page  
    // Page Combo 
    if (strFilePath.Length > pageCombo.Length) { 
        if (strFilePath.Substring(1, pageCombo.Length).ToLower() == pageCombo) { 
            ProcessCombo(context); 
            return; 
        } 
    } 
    // Brunch other pages + Start HTML document 
} 
```

This function is called when a GET request is done to the Web Server. The request.RawURL return the name of the URL with all the parameters. So we just need to compare the name of the page with the first part of the URL. All URL start with /, so we start at position 1. If it does match, then, go to the ProcessCombo function. Otherwise, continue and do switches like that for all needed pages. And finally, you can build your own code. And that is what we will do later. Yes, we will generate a bit of HTML ![Sourire](/assets/4401.wlEmoticon-smile_2.png)

```csharp
private static void ProcessCombo(HttpListenerContext context) { 
    HttpListenerRequest request = context.Request; 
    HttpListenerResponse response = context.Response; 
    string strParam = request.RawUrl; 
    string strResp = ""; 
    if (DecryptCombo(strParam)) 
        strResp = strOK; 
    else 
        strResp = strProblem; 
    OutPutStream(response, strResp);
} 
```

This function is very simple and just do a branch depending on the result of the DecryptCombo function. If it is successful, then it will return OK, if not, it will return Problem.

```csharp
private static bool DecryptCombo(string StrDecrypt) { 
    // decode params 
    Param[] Params = decryptParam(StrDecrypt); 
    int mChannel = -1; 
    int mComboBlue = -1; 
    int mComboRed = -1; 
    bool isvalid = true; 
    if (Params != null) { 
        for (int i = 0; i < Params.Length; i++) { 
            //on cherche le paramètre strMonth 
            int j = Params[i].Name.ToLower().IndexOf(paramChannel); 
            if (j == 0) { 
                mChannel = Convert.ToInt32(Params[i].Value); 
                if (!((mChannel >= (int)LegoInfrared.LegoChannel.CH1)  
                    && (mChannel <= (int)LegoInfrared.LegoChannel.CH4))) 
                    isvalid = false; 
            } 
            j = Params[i].Name.ToLower().IndexOf(paramComboBlue); 
            if (j == 0) { 
                mComboBlue = Convert.ToInt32(Params[i].Value); 
                if (!((mComboBlue == (int)LegoInfrared.LegoSpeed.BLUE_BRK)  
                    || (mComboBlue == (int)LegoInfrared.LegoSpeed.BLUE_FLT) 
                    || (mComboBlue == (int)LegoInfrared.LegoSpeed.BLUE_FWD)  
                    || (mComboBlue == (int)LegoInfrared.LegoSpeed.BLUE_REV))) 
                    isvalid = false; 
            } 
            j = Params[i].Name.ToLower().IndexOf(paramComboRed); 
            if (j == 0) { 
                mComboRed = Convert.ToInt32(Params[i].Value); 
                if (!((mComboRed >= (int)LegoInfrared.LegoSpeed.RED_FLT)  
                    && (mComboRed <= (int)LegoInfrared.LegoSpeed.RED_BRK))) 
                    isvalid = false; 
            } 
        } 
    } 
    // check if all params are correct 
    if ((isvalid) && (mComboRed != -1) 
        && (mChannel != -1) 
        && (mComboBlue != -1)) { 
        if (!Microsoft.SPOT.Hardware.SystemInfo.IsEmulator) 
            isvalid = myLego.ComboMode((LegoInfrared.LegoSpeed)mComboBlue,  
                (LegoInfrared.LegoSpeed)mComboRed, (LegoInfrared.LegoChannel)mChannel); 
        else 
            Debug.Print("Sent Combo blue " + mComboBlue.ToString()  
                + " red " + mComboRed.ToString() + " channel " 
                + mChannel.ToString()); 
        } 
        else 
            isvalid = false; 
        return isvalid; 
} 
```

I’ve been thru this kind of explanation couple of times in the [previous examples]({% post_url 2011-09-12-Implementing-a-simple-HTTP-server-in-.NET-Microframework %}). The code is quite simple, it first call a decrypt function that will return a table containing the parameter name and the value, both as string. More info here. If I take the previous example combo.aspx?bl=8&rd=2&ch=1, it will return 3 params with the pair (bl, 8), (rd, 2), (ch, 1). The idea is to convert and validate those parameters. When one is found, it is converted to the right type and compare to the enums values to check is everything is correct.

If all parameters are converted correctly and the values are valid, then the ComboMode function is called. If the function is successful, then the value true is return form the function and see the previous section, it will return OK. If not, Problem will be returned.

As you can see, I check if I’m in the emulator or in a real board. If I’m in the emulator, I will not physically send the signal as it is just an emulator ![Sourire](/assets/4401.wlEmoticon-smile_2.png) That allow me to do development in the planes and test the code. Of course, back home, I test them for real, just in case ![Sourire](/assets/4401.wlEmoticon-smile_2.png)

So we now have a way to call our ComboMode function thru a simple URL. I can also create a simple UI to be able to send commands very simply thru a web page. For this, I’ll need to do some HTML. And I have to admit, I don’t really like HTML… The idea of this simple UI is to be able to have dropdown box to select the speed for both the Blue and the Red output but also select the channel. At the end, the UI will looks like this:

![image](/assets/5736.image_2.png)

The HTML code to do it is the following for the ComboMode:

```html
<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd"> 
<html xmlns="http://www.w3.org/1999/xhtml"> 
<head><title></title></head><body> 
<form method="get" action="combo.aspx" target="_blank">
<p>Combo Mode<br /> 
Speed Red<select id="RedSpeed" name="rd"> 
<option label='RED_FLT'>0</option> 
<option label='RED_FWD'>1</option> 
<option label='RED_REV'>2</option> 
<option label='RED_BRK'>3</option> 
</select> Speed Blue<select id="BlueSpeed" name="bl"> 
<option label='BLUE_FLT'>0</option> 
<option label='BLUE_FWD'>4</option> 
<option label='BLUE_REV'>8</option> 
<option label='BLUE_BRK'>12</option> 
</select> Channel<select id="Channel" name="ch"> 
<option label='CH1'>0</option> 
<option label='CH2'>1</option> 
<option label='CH3'>2</option> 
<option label='CH4'>3</option> 
</select>&nbsp;&nbsp;&nbsp;&nbsp;&nbsp; 
<input id="Submit1" type="submit" value="Send" />
</p></form> </body> </html> 
```

Nothing rocket science, just a bit of HTML. Please note that the form will be send in get mode so with a URL exactly like the one we want. the target=”_blank” attibute of form is to open it in a new window when the submit button will be clicked. I prefer this as it allow to keep the main windows open and change easily one of the value without changing everything. 

The label attibute of the option tag in a select allow you to put the name you want in the combo list. The value inside the option tag is what will be send. So you can display a friendly name instead of a “stupid” number. 

In terms of code, here is what I have done. This code is part of the ProcessClientGetRequest function:

```csharp
// Start HTML document 
string strResp = "<!DOCTYPE html PUBLIC \"-//W3C//DTD XHTML 1.0 Transitional//EN\" \"http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd\">"; 
strResp += "<html xmlns=\"http://www.w3.org/1999/xhtml\"><head><title></title></head><body>"; 
// first form is for Combo mode 
strResp += "<form method=\"get\" action=\"combo.aspx\" target=\"_blank\">  
<p>Combo Mode<br />Speed Red<select id=\"RedSpeed\" name=\"rd\">"; 
strResp += "<option label='RED_FLT'>0</option><option label='RED_FWD'>1</option>  
    <option label='RED_REV'>2</option><option label='RED_BRK'>3</option>"; 
strResp += "</select> Speed Blue<select id=\"BlueSpeed\" name=\"bl\">  
    <option label='BLUE_FLT'>0</option><option label='BLUE_FWD'>4</option>  
    <option label='BLUE_REV'>8</option><option label='BLUE_BRK'>12</option>"; 
strResp += "</select> Channel<select id=\"Channel\" name=\"ch\">  
    <option label='CH1'>0</option><option label='CH2'>1</option>  
    <option label='CH3'>2</option><option label='CH4'>3</option>"; 
strResp += "</select>&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;  
    <input id=\"Submit1\" type=\"submit\" value=\"Send\" /></p></form>"; 
strResp = OutPutStream(response, strResp); 
```

I just create the form as it should be, very simply. Filling a string and outputting the string to the response object. Be careful as in netduino, the maximum size of any object, so including strings is 1024. The strResp object can’t contain more than 1024 caracters, so it is necessary to empty it. The way I do it is thru the OutPutStream function which output the string value into the response stream. I’ve already explained it in a past article.

So here is it for this part. I’ve explained how to be able to create a set of Web API, very simple to control this infrared emitter. This principle can be apply to anything! I have couple of ideas for the next step. I can implement a system with sensors (can be ILS for examples) and signals (green/red lights) and switch management. But I’m not there yet! Any feedback and ideas welcome.

