# 2011-09-13 Setup a time and date using .NET Microframework

In theory, .NET Microframework implement a class to get the time from a time server. It never worked for me using my [netduino](https://www.netduino.com/netduinoplus/specs.htm) board. You'll find more info on this board in my [previous post](./2011-09-12-Implementing-a-simple-HTTP-server-in-.NET-Microframework.md). And also I've implemented a Web Server with the possibility to decrypt a parameter URL.

 netduino has no BIOS and no way when it's off to keep a date and time. So each time you boot it, you have to setup the time and date yourself. Normally in order to do that, you can use the TimeService class.It looks like it's not implemented on this board. [The forum is very active and very useful](https://forums.netduino.com/index.php?/topic/425-using-timeservice/page__p__3102__hl__timeservice__fromsearch__1#entry3102). So as I needed the right time on my board and not in a very precise way (second and even minutes were ok), I get the idea of requesting a web page on my server that will return a date and time value.

 In terms of code on an IIS server, it's very very very very simple:

```html
<%@ Page Title="Home Page" Language="C#" CodeBehind="Default.aspx.cs" Inherits="DateHeure._Default" %><html><head></head>  
<body><% Response.Write(DateTime.Now.ToString("u")); %></body></html> 
```

 Nothing else is needed! the `u` formatting return a date time like `2011/09/15 15:20:30Z`. So the return code from the server will be: `<html><head></head><body>2011/09/15 15:20:30Z</body></html>` On the client side on the netduino board, I needed to have an HTTP client and then decrypt the date and time. I found the HTTP Client in the sample and just simply it. So I've created a function that take a URL to request the page and return a date.

```c#
public static DateTime ReadDateTime(string url) { 
    // Create an HTTP Web request. 
    HttpWebRequest request = HttpWebRequest.Create(url) as HttpWebRequest; 
    // Set request.KeepAlive to use a persistent connection. 
    request.KeepAlive = true; 
    // Get a response from the server. 
    WebResponse resp = null; 
    DateTime MyDateTime = DateTime.Now; 
    try { 
        resp = request.GetResponse(); 
    } catch (Exception e) { 
        Debug.Print("Exception in HttpWebRequest.GetResponse(): " + e.ToString()); 
    } // Get the network response stream to read the page data. 
    if (resp != null) { 
        Stream respStream = resp.GetResponseStream(); 
        string page = null; 
        byte[] byteData = new byte[1024]; 
        char[] charData = new char[1024]; 
        int bytesRead = 0; 
        Decoder UTF8decoder = System.Text.Encoding.UTF8.GetDecoder(); 
        int totalBytes = 0; 
        // allow 5 seconds for reading the stream 
        respStream.ReadTimeout = 5000; 
        // If we know the content length, read exactly that amount of 
        // data; otherwise, read until there is nothing left to read. 
        if (resp.ContentLength != -1) { 
            Thread.Sleep(500); bytesRead = respStream.Read(byteData, 0, byteData.Length); 
            if (bytesRead == 0) { 
                return MyDateTime; 
            } 
            // Convert from bytes to chars, and add to the page 
            // string. 
            int byteUsed, charUsed; 
            bool completed = false; 
            totalBytes += bytesRead; 
            UTF8decoder.Convert(byteData, 0, bytesRead, charData, 0, bytesRead, true, out byteUsed, out charUsed, out completed); 
            page = page + new String(charData, 0, charUsed); 
            // Display the page download status. 
            Debug.Print("Bytes Read Now: " + bytesRead + " Total: " + totalBytes); 
            page = new String( System.Text.Encoding.UTF8.GetChars(byteData)); 
        } 
        // Display the page results. 
        Debug.Print(page); 
        // Close the response stream. For Keep-Alive streams, the 
        // stream will remain open and will be pushed into the unused 
        // stream list. 
        resp.Close(); 
        if (page.Length > 0) { 
            int start = page.IndexOf("<body>"); 
            int end = page.IndexOf("</body>"); 
            if ((start >= 0) && (end >= 0)) { 
                String strDateHeure = page.Substring(start + 6, end - start - 7); 
                if (strDateHeure.Length > 0) { 
                    int year = -1; 
                    int month = -1; 
                    int day = -1; 
                    int hour = -1; 
                    int minute = -1; 
                    Convert.ToInt(strDateHeure.Substring(0, 4), out year); 
                    Convert.ToInt(strDateHeure.Substring(5, 2), out month); 
                    Convert.ToInt(strDateHeure.Substring(8, 2), out day); 
                    Convert.ToInt(strDateHeure.Substring(11, 2), out hour); 
                    Convert.ToInt(strDateHeure.Substring(14, 2), out minute); 
                    if ((year != -1) && (month != -1) && (day != -1) && (hour != -1) && (minute != -1)) 
                        MyDateTime = new DateTime(year, month, day, hour, minute, 0); 
                } 
            } 
        } 
    } 
    return MyDateTime; 
}
```

 First part of the code is the one form the sample. I've just modify the size of the buffer to fit with the max size in the netduino. Second part if the analyze of the return page. It does just keep what is between the `<body>` and `</body>` tags, remove the Z and basically convert the string to int using the convert class [I've developed and expose in my previous post](./2011-09-12-Implementing-a-simple-HTTP-server-in-.NET-Microframework.md). I did not convert the second as I don't need to be very precise. And of course, I'm checking if values are correct. I should have check also a range for year, month, day, hours and minutes. I did not and if something goes wrong I may get an exception when creating the DateTime class. I take the risk here. Now, I initialize the date and time when starting the HTTP server like this:

```c#
DateTime TodayIs = new DateTime(2011, 8, 14, 10, 0, 0); 
Utility.SetLocalTime(TodayIs); 
Debug.Print(TodayIs.ToString()); 
TodayIs = ReadDateTime(MyTimeServer); 
Utility.SetLocalTime(TodayIs); 
Debug.Print(TodayIs.ToString());
```

 MyTimeServer is a string with the URL of my date time page. Be careful as you'll need a full URL including the name of the page. In my case the URL is [https://www.ellerbach.net/DateHeure/default.aspx](https://www.ellerbach.net/DateHeure/default.aspx).

 By following the debug trace, you'll see the date and time changing.

 It took about 1 hour to create this function and implement it in real ![Sourire](../assets/4401.wlEmoticon-smile_2.png) I love this very cool netduino and .NET Microframework. The marketing guy doing development. ![Sourire](../assets/4401.wlEmoticon-smile_2.png)
