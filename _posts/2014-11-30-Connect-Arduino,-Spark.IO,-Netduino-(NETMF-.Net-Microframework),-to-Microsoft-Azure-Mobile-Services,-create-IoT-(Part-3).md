---
layout: post
title:  "Connect Arduino, Spark.IO, Netduino (NETMF .Net Microframework), to Microsoft Azure Mobile Services, create IoT (Part 3)"
date: 2014-11-30 02:14:23 +0100
categories: 
author: "Laurent Ellerbach"
thumbnails: "/assets/2014-11-30-thumb.jpg"
---
In the 2 previous articles, we’ve created a local network of objects which communicate sensor information to a more smarter object which will post the information somewhere in the Cloud. Idea is to create a full Internet of Things (IoT) infrastructure. Those sensors are temperature, humidity (air and soil), wind direction as well as rain. Some have been just reused from Oregon Scientific and some have been developed from scratch using simple [Arduino](http://www.arduino.cc) or equivalent like [Spark.IO](http://spark.io). 

It is now time to look at how to post the sensor information in the Cloud. There are multiple offers to post data in the Cloud. I prefer the Microsoft Azure offer for multiple reasons. First, it’s free for a low usage like my hobbyist usage. **Yes, it’s just free. Yes, really free**. I will use for this the Azure Mobile Services as well as an Azure SQL database for the storage. The Azure SQL database is free up to 1Gb of data. And in one of the next article, we’ll see how to consume those data, **also for free in an Azure Web site**. 

There are also good technical reasons to use Azure: first it’s totally open, easy to access, based on the Internet protocols. All what you need is simply an IP stack and sockets. It can work on a low speed connection like GPRS or high speed like WiFi, using HTTPS or HTTP. So it’s very flexible and very easy to use. Let go deeper in the code. As a summary, Azure Mobile Services are:

* Fully REST API, can be access HTTP or HTTPS, in our case, as the processors are very low end, it will be HTTP only  
* Dynamic table auto setup, no need to be SQL guru  
* Access thru application key, this is a simple but easy and simple security mechanism  
* Can customize insert, update and all other functions, I’ll use this to simplify a bit the data returned by Azure  
* Can create custom API, I’ll use this feature for the last part to connect my Sprinkler to the Azure data   First step is to open an Azure subscription: [http://azure.microsoft.com/pricing/free-trial/](http://azure.microsoft.com/pricing/free-trial/). Your credit card will be required but won’t be used if you decide to stay with free offer. The credit card is used for verification mainly. You’ll get 30 days also with real $$ to use for free if you want to test other part of Azure which I of course encourage you to do.

First step is to create a Mobile Service. For this, just follow the excellent step by step you’ll find here. [http://azure.microsoft.com/en-us/documentation/services/mobile-services/](http://azure.microsoft.com/en-us/documentation/services/mobile-services/). It’s just 3 easy steps and you’re done for the server side.

![image](/assets/6735.image_2.png)

Now the service is creates, to access it, it’s super simple: connect to the port 80 (stand HTTP port) of the Azure Mobile Service you’ve just created. And send the following data:

```
POST /tables/weather/ HTTP/1.1
X-ZUMO-APPLICATION: 123456789abcdef123456789abcdef12
Host: nomduservice.azure-mobile.net
Content-Length: 88
Connection: close

{"sensorID":22, "channel":5, "instSpeed":12,"averSpeed":5,"direction":2,"batterylife":90}
```

The server will return the following text:

```
HTTP/1.1 201 Created
Cache-Control: no-cache
Content-Length: 133
Content-Type: application/json
Location: https://nomduservice.azure-mobile.net/tables/weather//931CFDDE-AB7F-4480-BA28-F1D5C611398B
Server: Microsoft-IIS/8.0
x-zumo-version: Zumo.master.0.1.6.3803.Runtime
X-Powered-By: ASP.NET
Set-Cookie: ARRAffinity=da4a9f7437a690e3c1a799d3a6c3ddf3ee0cbb9f5a67008d3c919f0149f34ee3;Path=/;Domain= nomduservice.azure-mobile.net
Date: Sun, 31 Aug 2014 15:40:12 GMT
Connection: close

{"sensorID":22,"channel":5,"instSpeed":12,"averSpeed":5,"direction":2,"batterylife":90,"id":"931CFDDE-AB7F-4480-BA28-F1D5C611398B"} 
```

yes, it’s HTTP, so it’s just simple text. What we send is a POST request on a specific table, here /tables/weather/ and we send information in a JSON format. See JSON as a simplified XML still readable by a human ![Sourire](/assets/8420.wlEmoticon-smile_2.png) So basically a bit less text to send with still a structured way.

What we get in return from the server is a standard header containing quite lots of information as well as the data and the unique ID returned. Wait, a unique ID? Yes, it’s part of what is generated automatically when you are using the Azure Mobile Services. All the database is created for you, all the mechanism to generate the data in the database is totally hidden for you. So clearly no need to be the king of SQL. It’s fully transparent. And yes, you can personalize, you can customize the received data, you can do check and all this. We'll have a look later on how to simply do it.

OK, time to see code. Here is the full Arduino code necessary to post the example above:

```Csharp
TCPClient client;
byte AzureServer[] = { 12, 34, 56, 78 };

String writeJsonWind(struct wind wd) {
    // Create a simple JSON;
    String datastring = "{\"sensorID\":";
    datastring += String(wd.ID);
    datastring += ",\"channel\":";
    datastring += String(wd.channel);
    datastring += ",\"instSpeed\":";
    datastring += String(wd.instantSpeed);
    datastring += ",\"averSpeed\":";
    datastring += String(wd.averageSpeed);
    datastring += ",\"direction\":";
    datastring += String(wd.direction);
    datastring += ",\"batterylife\":";
    datastring += String(wd.bat);
    datastring += "}";
    return (datastring);
}

void sendData(String thisData) {
    // create a connection to port 80 on the server
    // IP is your Mobile Services address
    if (client.connect(AzureServer, 80)) 
    {
        //Serial.println("Connected to Azure Server");
        // create the REST request using POST
        // Nomdelatable is name of the table
        client.print("POST /tables/weather/");
        client.println(" HTTP/1.1");
        // use the application key
        client.println("X-ZUMO-APPLICATION: 123456789abcdef123456789abcdef12");
        // host name is name of your Azure Mobile Service
        client.println("Host: nomdumobileservice.azure-mobile.net");
        client.print("Content-Length: ");
        client.println(thisData.length());
        client.println("Connection: close");
        client.println();
        // and finally data!
        client.println(thisData);
    }
    else {  // in case of error, stop connection
        client.stop();
}  }
// Sending data is simple, create a JSON, and send it on port 80!
String dataString = writeJsonWind(myWind);
sendData(dataString);
```

That’s it? Yes, that’s it. Nothing more is needed. As I wrote before, you need an IP stack and sockets. You have them with the TCPClient object. I’m using a Spark.IO which is based on the same kind of processor you find in normal Arduino but with a WiFi chip on top. See previous articles for more information.

Rest of the code is quite straight forward, the writeJsonWind function create the Json string. sendData is first connecting to the Azure Mobile Services server on port 80. Then a socket connection is created, it write in the socket the header and then the Json data. And that’s all! The data will be stored automatically in the SQL Azure database and you’ll be able to access them.

It’s possible to personalize and create your own Azure Mobile Services API. You can do it either in Javascript directly in the Azure management console or in .NET using the excellent and free Visual Studio, either the Express version and installing the Azure SDK, either the [Visual Studio Community](http://www.visualstudio.com/products/visual-studio-community-vs) edition. As an example, I’ll use Javascript here. The idea is to reduce number of data sent back to the client. No need to send the generated data, it won’t be use. The header will be largely enough.

```javascript
function insert(item, user, request) {
    request.execute({
        success: function(results) {
            request.respond(statusCodes.OK);  },
        error: function(results) {
            request.respond(statusCodes.BAD_REQUEST); }  
    }); }
```

This is a simple modification which return OK if the data has been successfully entered into the database or bad request if not. You can of course do more. I encourage you to follow the project my friend Mario Vernari is doing [here](https://highfieldtales.wordpress.com/2014/11/07/azure-veneziano-part-1/) and [here](https://highfieldtales.wordpress.com/2014/11/16/azure-veneziano-part-2/). Lots of great and cool stuff too with example of personalized API.

As always, you can raise the question: what about security?

It’s a critical part of Mobile Services. The default access is HTTPS which offers a reasonable level of security with the application key. Here, we are using very cheap processors which are not powerful enough to run HTTPS, so we are using basic HPPT. Of course, for a professional project or a project which require more sensible data, we’ll clearly have to use more robust processors. On top, you can use Azure Directory federation and make user/device authentication. When you’ll have to manage thousands of devices, when you’ll have them randomly in the wild and not physically secured, you’ll be happy to use those kind of mechanism to exclude specific devices. You have on top an easy federation with Microsoft ID, Google, Facebook and Twitter. And of course the Azure SQL database is secured with login/pwd, you can control which server/PC/device can have access by IP as well as user/pwd.

What are the other way to post data in Azure?  

There are other ways of course. You can access directly the SQL database but it’s not the easiest method to manage authentication, validate data. You better want to use the [Azure Event Hubs](http://azure.microsoft.com/services/event-hubs/) to connect millions of devices across platforms and which allow the support for AMQP and HTTP. It does have native client libraries also exist for popular platforms

You can also use framework like [Intelligent Systems Service](http://www.microsoft.com/windowsembedded/en-us/intelligent-systems-service.aspx), more oriented to consume the data. It’s based on Azure, provide additional tools for analyze and data consumption. 

So all up, we’ve seen how to post data from a 1$ chip with access to an IP stack and Socket to Azure and store them in a SQL Azure database. It is simple, straight forward and flexible solution. It’s free for low usage so it makes it the best solution for simple projects and hobbyists.

