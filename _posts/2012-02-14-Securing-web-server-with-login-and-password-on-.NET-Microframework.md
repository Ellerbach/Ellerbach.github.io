---
layout: post
title:  "Securing web server with login and password on .NET Microframework"
date: 2012-02-14 20:09:33 +0100
categories: 
---
If you want to expose your [.NET Microframework web server]({% post_url 2011-09-12-Implementing-a-simple-HTTP-server-in-.NET-Microframework %}) on the Internet you better have to make sure it is protected. Even if you keep the URL secret, it will not stay secret ![Sourire](/assets/4401.wlEmoticon-smile_2.png) it’s not a good way to protect it! A better way if you have to expose it is to use a login and a password. Still not the best way but reasonably ok. To add more security, use https on your board. But if you are using a [netduino board]({% post_url 2011-09-09-netduino-board-geek-tool-for-.NET-Microframework %}) like me, the https web server is a big too big and will not fit. So I’ve decided to use login/password. 

the good news is that you already have classes to be able to implement a login and password very easily. First [read this post]({% post_url 2011-09-12-Implementing-a-simple-HTTP-server-in-.NET-Microframework %}) to understand how to implement a web server. In the main function ProcessClientGetRequest, just add at the beginning of the function the following code:
 
```csharp
string strResp = "<HTML><BODY>netduino sprinkler<p>"; 
bool IsSecured = false; 
if (request.Credentials != null) { 
    // Parse and Decode string. 
    Debug.Print("User Name : " + request.Credentials.UserName); 
    Debug.Print("Password : " + request.Credentials.Password); 
    // if the username and password are right, then we are authenticated 
    if (request.Credentials.UserName == MyLogin &&   
        request.Credentials.Password == MyPassword) { 
            IsSecured = true; 
    } 
} 
//if not secured display an error message. And return the function 
if (!IsSecured) { 
    strResp += "<p>Authentication required<p>Invalid login or password"; 
    response.StatusCode = (int)HttpStatusCode.Unauthorized; 
    response.Headers.Add("WWW-Authenticate: Basic realm=\"netduino sprinkler\""); 
    strResp += "</BODY></HTML>"; 
    byte[] messageBody = Encoding.UTF8.GetBytes(strResp); 
    response.ContentType = "text/html"; 
    response.OutputStream.Write(messageBody, 0, messageBody.Length); return; 
}
```

The IsSecured boolean is there to check if the login and password are valid. by default it is set at false. Then the request.Credentials contains the login credentials. If they are not null, you can just check if they are the right ones. UserName contains the user name and guess what, Password contains the password. Not rocket science there ![Sourire](/assets/4401.wlEmoticon-smile_2.png) just compare them with the one you want to use. By default the authentication type is basic. So login and password are send in clear mode. Not the most secure but unfortunately like most of the internet protocol still use for mail like POP and other protocols like that. Again, to get more protection, use https if you can on your board.

.NET Micro framework web server object also support Windows Live authentication. As I’m doing this code in a plane from Moscow to Dubai, quite hard to test it!
 
```csharp
request.Credentials.AuthenticationType = AuthenticationType.Basic; 
request.Credentials.AuthenticationType = AuthenticationType.WindowsLive;
```

Those are the 2 modes. If I have the opmportunity, I’ll test the Windows Live one. If anyone do the test, please let me know!

The rest of the code is pretty straight forward, it’s just about displaying an error message with the HttpStutusCode as Unauthorized. And just return the function before the end. Very efficient to protect all site and all pages. The good news is that you will not have to authenticate to access all pages, it will be carry on from one page to another up to when you’ll close your browser and come back. I do not recommend to store any login/password on any of your pc, or device. Always type them!

If you want to store somewhere the login and password, you can store it on the SD card of your board. If your board is store in a secure location in your house and not accessible, you may not need to crypt the login and password. And anyway if the person has access to your board controlling some equipment, it’s probably too late. But if you need, crypt it on the storing location.

Here is a code example how to read this login and password from a file.
 
```csharp
if (Microsoft.SPOT.Hardware.SystemInfo.IsEmulator) 
    strDefaultDir = "WINFS"; 
else 
    strDefaultDir = "SD"; 
FileStream fileToRead = null; 
try { 
    fileToRead = new FileStream(strDefaultDir + "\\"   
        + strFileProgram, FileMode.Open, FileAccess.Read); 
    long fileLength = fileToRead.Length; 
    Debug.Print("File length " + fileLength); 
    //file length has to be less than 1024 otherwise, it will raise an exception 
    byte[] buf = new byte[fileLength]; 
    string mySetupString = ""; 
    // Reads the data. 
    fileToRead.Read(buf, 0, (int)fileLength); 
    // convert the read into a 
    string mySetupString = new String(Encoding.UTF8.GetChars(buf)); 
    int mps = mySetupString.IndexOf(ParamSeparator); 
    MyLogin = mySetupString.Substring(0, mps); 
    MyPassword = mySetupString.Substring(mps + 1, mySetupString.Length - mps-1); 
    fileToRead.Close(); 
} catch (Exception e) { 
    if (fileToRead != null) { 
        fileToRead.Close(); 
    }
     //throw e; 
     Debug.Print(e.Message); 
}
```

strFileProgram contain the file name in which you ware storing the login and password. And ParamSeparator is a char containing a separator character between the login and password stored. So it’s a character that you will not allow to use in a login or password. So use a character like return. As it can’t be used in a login and password.

The first couple of lines are to identify if we are running in the emulator or not. In the emulator storing access is by default in the WINFS directory. And on the netduino board in SD. Then the code is very similar to what I’ve already shown [in this article]({% post_url 2011-11-04-Read-a-setup-file-in-.NET-Microframework %}). The login and password will have to be stored in a global string of the http server class. As explain in the mentioned article, you can also save those data quite easily using the same file. And you can also build a simple page to allow the user (usually you) to change this login and password. In my case I’m not doing it as I’m the only user. And if I need to change it, I can do it directly on the SD card. Of course, use a long login and a complex and long password. At least 10 character, a majuscule, a miniscule, a numeric and a non alphanumeric character. And you can consider you may be ok. But again, it’s not the most secure solution. So if you have to expose directly your board to the internet and you’ll need to access it thru the web sever, make sure you are not piloting any critical part of your house or resources. 

As always, any feedback welcome.