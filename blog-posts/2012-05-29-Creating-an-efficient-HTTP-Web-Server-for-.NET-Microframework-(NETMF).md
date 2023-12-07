# 2012-05-29 Creating an efficient HTTP Web Server for .NET Microframework (NETMF)

That's not the first post I'm doing on incorporating a Web Server in .NET Microframework (NETMF). In some of [my previous posts](./2011-12-07-Creating-dynamically-a-web-page-using-.NET-Micro-framework.md), I've explain how to do it using the existing .NET classes for this. And it is working very well!

 The main concerns I have is that I'm using a [netduino](./2011-09-09-netduino-board-geek-tool-for-.NET-Microframework.md) board. This board is great, I love it. But there is a very limited amount of memory available and limited amount of space to store the programs. Total storage is 64Kb and what is left when the code is in is about 48K… So very little amount of memory. And the http .NET classes are great and use stream but they are intense in terms of memory and also the main http class is huge…

 So I had to find a solution and it was to redevelop a web server like IIS or Apache. OK, I can't compare ![Sourire](../assets/4401.wlEmoticon-smile_2.png) I just want a web server which handle GET answers and respond a simple web page. The other challenge I have is to be able to reuse the code I4ve already written for my Sprinkler, to pilot my Lego city and my Lego infrared receiver like my Lego trains…

 So I was searching for code to reuse on the Internet and found some code. So I did a mix of existing code and spend some time testing various solutions ![Sourire](../assets/4401.wlEmoticon-smile_2.png) Most of the existing code is not really robust. It does fail if there is a network problem, if 2 requests arrive at the same time, etc. I'm not saying my code is perfect but it is working and working well for the last month with no problem at all.

 A web server is simple, it's just a connection on a socket and a protocol of communication which is HTTP. It is also very simple as it is text based. What is interesting is to see all what you can do with such a simple protocol and such a simple markup language like HTML and some javascript.

 OK, so let start with what is necessary: a thread that will run all the time and handle socket requests. So we need also a socket. And a way to stop the thread.

```csharp
private bool cancel = false; private Thread serverThread = null; 

public WebServer(int port, int timeout) { 
    this.Timeout = timeout; 
    this.Port = port; 
    this.serverThread = new Thread(StartServer); 
    Debug.Print("Web server started on port " + port.ToString()); 
} 
```

As you can see, it is quite simple, the WebServer object is initialize with a specific port and a timeout. By default, the http port is 80 but it can be anything. There is no limitation. And as it's easy to implement, let make the code generic enough to be able to be use with different ports. And a new Thread is created to point on function StartServer. I will detail it later. I will explain also why we need a timeout later.

 Now we have this object initialize, let start the Webserver:

```csharp
public bool Start() { 
    bool bStarted = true; 
    // start server 
    try { 
        cancel = false; 
        serverThread.Start(); 
        Debug.Print("Started server in thread " + serverThread.GetHashCode().ToString()); 
    } catch { 
        //if there is a problem, maybe due to the fact we did not wait engouth 
        cancel = true; 
        bStarted = false; 
    } 
    return bStarted; 
} 
```

That is where the fun being! We start listening and initialize a variable we will use later to stop the server if needed. The catch can contain something to retry to start, here, it just return if it is started or not. At this stage, it should work with no problem as it is only a thread starting. But who knows ![Sourire](../assets/4401.wlEmoticon-smile_2.png)

```csharp
private void StartServer() { 
    using (Socket server = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp)) { 
        //set a receive Timeout to avoid too long connection server.
        ReceiveTimeout = this.Timeout; server.Bind(new IPEndPoint(IPAddress.Any, this.Port)); 
        server.Listen(int.MaxValue); 
        while (!cancel) { 
            try { 
                using (Socket connection = server.Accept()) { 
                    if (connection.Poll(-1, SelectMode.SelectRead)) {
                         // Create buffer and receive raw bytes. 
                         byte[] bytes = new byte[connection.Available]; 
                         int count = connection.Receive(bytes); 
                         Debug.Print("Request received from " 
                            + connection.RemoteEndPoint.ToString() + " at " + DateTime.Now.ToString("dd MMM yyyy HH:mm:ss")); 
                        //stup some time for send timeout as 10s. 
                        //necessary to avoid any problem when multiple requests are done the same time. 
                        connection.SendTimeout = this.Timeout; ; 
                        // Convert to string, will include HTTP headers. 
                        string rawData = new string(Encoding.UTF8.GetChars(bytes)); 
                        string mURI; 
                        // Remove GET + Space 
                        // pull out uri and remove the first 
                        if (rawData.Length > 5) { 
                            int uriStart = rawData.IndexOf(' ') + 2; 
                            mURI = rawData.Substring(uriStart, rawData.IndexOf(' ', uriStart) - uriStart); 
                        } else 
                            mURI = ""; 
                        // return a simple header 
                        string header = "HTTP/1.1 200 OK\r\nContent-Type: text/html; charset=utf-8\r\nConnection: close\r\n\r\n"; 
                        connection.Send(Encoding.UTF8.GetBytes(header), header.Length, SocketFlags.None); 
                        if (CommandReceived != null) 
                            CommandReceived(this, new WebServerEventArgs(connection, mURI)); 
                    } 
                } 
            } catch (Exception e) { 
                //this may be due to a bad IP address 
                Debug.Print(e.Message); 
            } 
        } 
    } 
} 
```

This function will run all the time in a thread. It's in an infinite loop which can be break by the cancel variable. First, we need to initialize the Socket. We will use IPv4 with a stream and the TCP protocol. No timeout to receive the request. The, you'll have to bind this socket to a physical IP address. In our case, we will use all IP address on the port initialized before. Any IP address mean all addresses and in our case only 1 IP address as we do have only 1 Ethernet interface. We are using '"using" to make sure the server Socket will be closed and cleaned properly after usage.

The way it is working is not too complicated. Remember that we've open a Socket named Server, setup it to listen to port 80. This is running in a separate thread in this thread. So in order to analyze the information returned when a connection is accepted (so when a Browser ask for a page), we need to create another Socket pointing to the same Socket, here "using (Socket connection = server.Accept())". In this case "using" allow the code to clean in the "proper way" when the thread will be finished or then the loop end or when it goes back to the initial loop. It's thread in thread and if you don't close things correctly, it can quickly let lots of objects in the memory, objects which will be seen as alive by the garbage collector.

When there are bytes ready to read with connection.Poll, we just read them. The request is transformed into a string. An http request look like "GET /folder/name.ext?param1=foo&param2=bar HTTP/1.1". Areal life example looks more like this: "GET /folder/name.ext?param1=foo&param2=bar HTTP/1.1\r\nAccept: text/html, application/xhtml+xml, */*\r\nAccept-Language: fr-FR,fr;q=0.8,en-US;q=0.5,en;q=0.3\r\nUser-Agent: Mozilla/5.0 (compatible; MSIE 10.0; Windows NT 6.2; WOW64; Trident/6.0)\r\nAccept-Encoding: gzip, deflate, peerdist\r\nHost: localhost:81\r\nConnection: Keep-Alive\r\nX-P2P-PeerDist: Version=1.1\r\nX-P2P-PeerDistEx: MinContentInformation=1.0, MaxContentInformation=1.0\r\n\r\n"

For a normal, full web server like IIS or Apache, you'll analyze all those parameters, and there are lots, see the W3C protocol [here](https://www.w3.org/Protocols/rfc2616/rfc2616-sec14.html). For our usage, the only thing that interest us is the full URL. And it is located between the 2 first spaces. And we will extract the URL, remove the first ‘/' as I will not use it in the rest of the code.

Now, the next step is to start answering the request. When someone ask you something, it's polite to answer ![Sourire](../assets/4401.wlEmoticon-smile_2.png) Like for the request, the response need to have couple of header information. And as my usage is extremely simple, I will always consider that it is OK, I'm only delivering text content and that the connection can be closed. By the way, whatever you put there, HTTP is a disconnected protocol, so you should never consider that you are always connected! It's an error and can drive you to very bad behaviors.

connection.Send return the first part of the message and then I call an event to tell the creator of the WebServer object that something happened. I send of course the connection object so that the caller will be able to create an HTML page and answer and also the URL so that it can analyze it.

Last but not least, the try and catch is extremely important. With Sockets a problem can quickly arrive due to a network problem. And I've seen it happening on the netduino for no reason. Just capturing the problem and not doing anything makes the web server working for months! Even if you loose the network, the catch will capture the problem and the server will continue to work up to the point the network will work again. The other reason to use it is because of the timeout. If something happen between the client and our webserver, after the timeout, you'll get in this catch and you'll start a new socket and the process will go back to something normal. It can happen and happened to me with very long HTML pages I was generating. When I was interrupting the creation and ask for a new page, the socket went into a kind of infinite loop waiting for a request. There should be a smart way to check is something goes well or not but it's an easy way.

```csharp
public delegate void GetRequestHandler(object obj, WebServerEventArgs e); 
public class WebServerEventArgs: EventArgs { 
    public WebServerEventArgs(Socket mresponse, string mrawURL) { 
        this.response = mresponse; 
        this.rawURL = mrawURL; 
    } 
    public Socket response { get; protected set; } 
    public string rawURL { get; protected set; } } 
    public event GetRequestHandler CommandReceived; 
}
```

Right after the header is sent back, an event is raised. The arguments are simple here, we do send the Socket object and the URL. If you want to enrich the web server, you can add other elements like the header element rather than sending them right away, the browser requesting the page, the IP address or whatever you want! Again, simple and efficient there.

Last but not least if you need to stop the Server, you'll need a function to this and also to clean the code at the end:

```csharp
private bool Restart() { 
    Stop(); 
    return Start(); 
} 
public void Stop() { cancel = true; Thread.Sleep(100);
 serverThread.Suspend(); Debug.Print("Stoped server in thread "); } 
public void Dispose() { 
    Dispose(true);
     GC.SuppressFinalize(this); 
} protected virtual void Dispose(bool disposing) { 
    if (disposing) { 
        serverThread = null; 
    } 
} 
```

Nothing too complex there, it's just about pausing the thread (remember, there are other tread attached in it), closing the other thread leaving in the Server object and cleaning everything. I hope it's the good code to let it clean ![Sourire](../assets/4401.wlEmoticon-smile_2.png) But at the end of the day, my only interest is to let this server running all the time. So I don not really care if it will stop correctly!

Now, to use the server, easy:

```csharp
private static WebServer server; 
// Start the HTTP Server 
WebServer server = new WebServer(80, 10000); 
server.CommandReceived += new WebServer.GetRequestHandler(ProcessClientGetRequest); 
// Start the 
server. server.Start(); 
```

Declare a static WebServer if you want it to be unique. Technically, you can have multiple servers running on different port. In my case, no need for this. Then, it's about creating the object, adding an event and starting the server!

```csharp
private static void ProcessClientGetRequest(object obj, WebServer.WebServerEventArgs e) 
```

And you are ready to do some treatment into this function. To return part of the answer, just use e.response.Send as for the header part and you're done!

To simplify the process, as it's a function which you'll have to call often, I've created a function to do this:

```csharp
public static string OutPutStream(Socket response, string strResponse) { 
    byte[] messageBody = Encoding.UTF8.GetBytes(strResponse); 
    response.Send(messageBody, 0, messageBody.Length, SocketFlags.None); 
    //allow time to physically send the bits 
    Thread.Sleep(10); return ""; 
} 
```

This can be a function you add in your main code or can add in the Web Server code.

Now you have a fully functional simple Web Server. You can [read the previous article](./2011-09-12-Implementing-a-simple-HTTP-server-in-.NET-Microframework.md) on how to handle parameters and analyzing them. The code to manage the parameter is now in the WebServer class.

I'll post the code in CodePlex so anyone will be able to use it as a helper.

Enjoy this code ![Sourire](../assets/4401.wlEmoticon-smile_2.png) Again, I'm just a marketing director doing some code. And it's the code running in my sprinkler management system for the last month without any problem!
