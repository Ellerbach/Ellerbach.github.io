---
layout: post
title:  "Creating and launching timer in .NET Microframework"
date: 2011-10-06 02:49:25 +0100
---
In previous posts, I had the occasion to show how to [implement a web server using HTTP]({% post_url 2011-09-12-Implementing-a-simple-HTTP-server-in-.NET-Microframework %}) and handling web request (in my example GET requests) with parameters URL like in a real Windows or Linux server running Internet Information Server (IIS) or Apache with a generated HTML page like for ASP.NET, PHP or Java. Of course in couple of Kilo bits of memory, you just can’t do the same as IIS or Apache. Of course, security is very limited, capacity to cache non existent and many functions does just not exist! But at least you can do what you want and you just can focus on creating web page by hands with real C# code using the standard HTML language ![Sourire](/assets/6685.wlEmoticon-smile_2.png) for those of my age who have started to write web pages in 1993 or so remember that it’s not very complicated and notepad was your best friend. Youngest one using tools like Visual Studio, Eclipse or others just don’t know ![Clignement d&#39;œil](/assets/0334.wlEmoticon-winkingsmile_2.png) Ok, I’m probably a bit polemic there but they may be less comfortable doing it than people of my age or people who developed couple of ISAPI filters.

 So read first this post on the [web server implementation]({% post_url 2011-09-12-Implementing-a-simple-HTTP-server-in-.NET-Microframework %}), then how to [setup the time and date]({% post_url 2011-09-13-Setup-a-time-and-date-using-.NET-Microframework %}), this one on [how to generate a calendar]({% post_url 2011-09-19-Displaying-a-calendar-in-a-web-page-using-.NET-Microframework %}) then this one on [how to create programs]({% post_url 2011-09-26-Program-a-date-time-and-duration-for-a-sprinkler-in-.NET-Microframework-and-netduino %}). All done? Good! now, we will create couple of timers to launch the sprinklers. the good news is that in .NET Microframework, there is all what you need to do that. the only thing you have to do is to add a line like that in your initialization phase. I’ve added it in the StartHttpServer function:

 
```csharp
Timer MyTimer = new Timer(new TimerCallback(ClockTimer_Tick), null, 30000, 30000);
```
 So we have created a Time object that will call a function ClockTimer_Tick every 30 seconds in 30 seconds. The ClockTimer_Tick function looks like:

 
```csharp
static void ClockTimer_Tick(object sender) {
     DateTime now = DateTime.Now; 
     Debug.Print(now.ToString("MM/dd/yyyy hh:mm:ss")); 
    //do we have a Sprinkler to open? 
    for (int i = 0; i < SprinklerPrograms.Count; i++) { 
        SprinklerProgram MySpr = (SprinklerProgram)SprinklerPrograms[i]; 
        if ((now.Year == MySpr.DateTimeStart.Year) 
            && (now.Month == MySpr.DateTimeStart.Month) 
            && (now.Day == MySpr.DateTimeStart.Day) 
            && (now.Hour == MySpr.DateTimeStart.Hour) 
            && (now.Minute >= MySpr.DateTimeStart.Minute)) { 
                // this is the time to open a sprinkler 
                Debug.Print("Sprinkling " + i + " date time " + now.ToString("MM/dd/yyyy hh:mm:ss"));  
                Springlers[MySpr.SprinklerNumber].Manual = false; 
                Springlers[MySpr.SprinklerNumber].Open = true; 
                // it will close all sprinkler in the desired time of sprinkling. Timer will be called only once. 
                Timer MyTime = new Timer(new TimerCallback(ClockStopSprinkler), null, (int)MySpr.Duration.Ticks/10000, 0); 
                SprinklerPrograms.RemoveAt(i); 
        } 
    } 
} 
```

 It starts by creating a DateTime object that take the actual date time. It will be used to see if there are programs to launch. The loop just go thru all the programs. Programs are contained into an ArrayList SprinklerPrograms and contains objects that are SprinklerProgram. All this is describe in this post.

 So what it does is just testing if the program is the right year, right month, right day, right hour and if the minute to start is pasted. As the function is called every 30 seconds, a sprinkler will be started during the minute it was planned. To open the sprinkler, the Sprinkler function Open from the Sprinkler class is called. The code for the class looks like:

 
```csharp
static private OutputPort led = new OutputPort(Pins.ONBOARD_LED, false);
public class Sprinkler { 
    private bool MySpringlerisOpen = false; 
    private int MySprinklerNumber; 
    private bool MyManual = false; 
    public Sprinkler(int SprNum) { 
        MySprinklerNumber = SprNum; 
        //need hardware here 
    } 
    // open or close a sprinkler 
    public bool Open { 
        get { return MySpringlerisOpen; } 
        set { MySpringlerisOpen = value; 
        //do harware here 
        if (MySpringlerisOpen) 
            led.Write(true); 
        else 
            led.Write(false); 
        } 
    } 
    public bool Manual { get { return MyManual; } set { MyManual = value; } } 
    //read only property 
    public int SprinklerNumber { get { return MySprinklerNumber; } } 
}
```
 This class does not include the real hardware work. It just light on the embedded led when a sprinkler is open and switch it of when closed. As there are multiple sprinklers sharing the same internal led, the led object as to be declare as a static object in the main class to be kept alive and shared by all the sprinklers. 

 Here come the specific of the [netduino]({% post_url 2011-09-09-netduino-board-geek-tool-for-.NET-Microframework %}). The internal led is an OutputPort and use a specific port. It can be found in the definition of the enum:

 
```csharp
public const Cpu.Pin ONBOARD_LED = 55;
```
 The sprinkler class contains the number of the sprinkler, if it is open and if it is a manual open or an automatic opening. The only way to create a sprinkler class is to give it a number. It is done as behind there will be some hardware initialization and all objects will need to be unique. When I’ll have done the hardware work, I’ll come back to post the necessary code ![Clignement d&#39;œil](/assets/0334.wlEmoticon-winkingsmile_2.png)

 So back to the launch of the program, the code will set the opening to manual and open it in the sprinkler:
 
```csharp
Springlers[MySpr.SprinklerNumber].Manual = false; 
Springlers[MySpr.SprinklerNumber].Open = true; 
// it will close all sprinkler in the desired time of sprinkling. Timer will be called only once. 
Timer MyTime = new Timer(new TimerCallback(ClockStopSprinkler), null, (int)MySpr.Duration.Ticks/10000, 0); 
SprinklerPrograms.RemoveAt(i);
```

 Then it will create a new timer that will be called after the duration specified to sprinkle. The Ticks member return a number of ticks. 10000 ticks represent 1 second. So the ClockStopSprinkler will be called after the right duration and only 1 time. The function is quite simple, it just reset all sprinklers to close. I’ve decided to do this as I feel it is much more secure. I just can’t run anyway 2 sprinklers at the same time as I don’t have enough water pressure to run more than once a time.

 
```csharp
static void ClockStopSprinkler(object sender) { 
    Debug.Print("Stop sprinkling " + DateTime.Now.ToString("MM/dd/yyyy hh:mm:ss")); 
    //close all sprinklers if automatic mode 
    for (int i = 0; i < NUMBER_SPRINKLERS; i++) { 
        if (Springlers[i].Manual == false) 
            Springlers[i].Open = false; 
    } 
}
```

 If the mode is manual, no reason to stop a sprinkler. The manual mode can be run in another page. I’ll describe this into a future post.

 So quite easy code here to open and close the sprinklers when needed. Just use simple timers! I hope you’ve also enjoyed this post. Coding makes you feel better ![Sourire](/assets/6685.wlEmoticon-smile_2.png) and that’s just a marketing guy telling you this ![Clignement d&#39;œil](/assets/0334.wlEmoticon-winkingsmile_2.png)

