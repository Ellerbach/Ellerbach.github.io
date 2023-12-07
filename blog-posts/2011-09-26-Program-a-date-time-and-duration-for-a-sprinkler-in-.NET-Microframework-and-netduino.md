# 2011-09-26 Program a date time and duration for a sprinkler in .NET Microframework and netduino

In my [previous posts](./2011-09-09-netduino-board-geek-tool-for-.NET-Microframework.md), I've explained that I wanted to be able to program sprinklers in my garden day by day thru the Internet when I was not at home to save energy and water. No need to use a sprinkler when it has rained all night but need to use them when it has been very dry and sunny all day and no forecast for rain in the coming days. So I choose to use a [netduio](https://www.netduino.com/netduinoplus/specs.htm), it's a system on chip simple board, using no OS so no Windows, no Linux, no DOS or any anything else than .NET Microframework (in one word and not Micro framework). I use C# to develop on it but it's also possible since recently to use Visual Basic (VB.NET).

 So to summarize, after couple of post, I've created a web server to handle http requests like any Windows, Linux server running IIS, Apache or any other web server and being able to handle request in the URL like for ASP.NET, PHP or Java, [see this post](./2011-09-12-Implementing-a-simple-HTTP-server-in-.NET-Microframework.md). I already manage to [display a calendar](./2011-09-19-Displaying-a-calendar-in-a-web-page-using-.NET-Microframework.md). So now it's time to add programs!

 As for the calendar, I start by decoding the URL, this time URL looks like program.aspx?year=2011;month=9;day=10;hour=21;minute=2;duration=20;spr=0

 There are 7 parameters. I pass to the URL the year, month, day, start hour, start minute and duration of the program on a specific sprinkler. I made it clear in the parameters to be understandable by a human ![Sourire](../assets/4401.wlEmoticon-smile_2.png) In the production version and if I need to get more free space and to improve performance, I'll reduce the length of the parameters name and URL name. I will save footprint but also time processing and bandwidth when generating the URL. As I did program with clean code, I just need to change couple of strings in a single class for the overall code. All is explained in a previous post.

 I have decided (that's the chance to be the developer ![Clignement d&#39;œil](../assets/0728.wlEmoticon-winkingsmile_2.png)) to reset a program when I set the duration to 0. In this case, I just need to have a valid year, month and sprinkler number. So this code will do the job:

```csharp
if (intDuration == 0) { for (int i = 0; i < SprinklerPrograms.Count; i++) { 
    // case the date already exist => update the hour, minute and duration for the given Sprinkler 
    if ((intYear == ((SprinklerProgram)SprinklerPrograms[i]).DateTimeStart.Year) 
        && (intMonth == ((SprinklerProgram)SprinklerPrograms[i]).DateTimeStart.Month) 
        && (intDay == ((SprinklerProgram)SprinklerPrograms[i]).DateTimeStart.Day) 
        && (intSprinklerNumber == ((SprinklerProgram)SprinklerPrograms[i]).SprinklerNumber)) 
        { 
            SprinklerPrograms.RemoveAt(i); strResp += "Deleting Sprinkler " + intSprinklerNumber + " for " + intYear + " "   
+ intMonth + " " + intDay + ". <br>"; 
            strResp += "<a href='/" + MyParamPage.pageSprinkler + "'>Back to main page</a>"; strResp = OutPutStream(response, strResp); 
        } 
    } 
}
```

 As you can see, there a SprinklerPrograms variable. So what is this? It's an array define like this:

```csharp
public static ArrayList SprinklerPrograms = new ArrayList()
```

 Good news with .NET Microframework is that like for the real .NET framework or even Java or PHP, there is a minimum to manage array and lists. This ArrayList is smart enough to take any object in the array, up to 1024 elements (that's the limit of netduino platform). And there is function to add, remove, find specific objects.

```csharp
intYear == ((SprinklerProgram)SprinklerPrograms[i]).DateTimeStart.Year
```

 I do test to see if there is an existing element with the same date (year, month, day) and sprinkler. And If I found one, I just remove it. If nothing if found, nothing will be done and no message display. In a normal usage, this page is called from other page. Of course, as it's just a web page, it can be called by anyone with crappy elements in the URL. It's just a choice as in a normal usage, it should not happen.

 I also cast an element of the ArrayList to a SprinklerProgam class. This class is created to store a single program for a sprinkler.

```csharp
public class SprinklerProgram { 
    private DateTime myDateTimeStart; 
    private TimeSpan myDuration; 
    private int mySprinklerNumber; 
    public SprinklerProgram(DateTime mDT, TimeSpan mTS, int mSN) { 
        myDateTimeStart = mDT; 
        myDuration = mTS; 
        mySprinklerNumber = mSN; 
    } 
    public DateTime DateTimeStart { get { return myDateTimeStart; } set { myDateTimeStart = value; } } 
    public TimeSpan Duration { get { return myDuration; } set { myDuration = value; } } 
    public int SprinklerNumber { get { return mySprinklerNumber; } set { mySprinklerNumber = value; } } 
}
```

 Nothing really complicated there, it's just about storing the start date, time, duration and sprinkler number. And I do test the year, month, day and sprinkler with a casting.

 So next, the idea is to see if the date and time are valid, create a DateTime object and check if it is today or not. If it is today, only part of the time will be displayed as the idea is not to be able to program a past day. Then after couple of test, either there is a program existing for a sprinkler and it just need to be updated, either it has to be created.

```csharp
else if ((intYear > 1900) && (intMonth > 0) && (intMonth < 13) && (intHour >= 0)   
&& (intHour < 24) && (intMinute >= 0) && (intMinute < 60)) { 
    MyDate = new DateTime(intYear, intMonth, intDay, intHour, intMinute, 0); 
    bool TodayIsToday = false; 
    if ((intYear == tmpNow.Year) && (intMonth == tmpNow.Month) && (intDay == tmpNow.Day)) 
        TodayIsToday = true; 
    // Is the program in the future or today! 
    if ((MyDate >= tmpNow) || (TodayIsToday)) { 
        bool updated = false; 
        // is the duration the right one? with an existing sprinkler? 
        if ((intDuration > 0) && (intDuration < 1440) && (intSprinklerNumber >= 0)   
            && (intSprinklerNumber < NUMBER_SPRINKLERS)) 
        { 
            MySpanDuration = new TimeSpan(0, intDuration, 0); 
            // is it a new program for a day a just an update (only 1 program per day available) 
            for (int i = 0; i < SprinklerPrograms.Count; i++) { 
                // case the date already exist => update the hour, minute and duration for the given Sprinkler 
                if ((MyDate.Year == ((SprinklerProgram)SprinklerPrograms[i]).DateTimeStart.Year) 
                    && (MyDate.Month == ((SprinklerProgram)SprinklerPrograms[i]).DateTimeStart.Month) 
                    && (MyDate.Day == ((SprinklerProgram)SprinklerPrograms[i]).DateTimeStart.Day) 
                    && (intSprinklerNumber == ((SprinklerProgram)SprinklerPrograms[i]).SprinklerNumber) 
                    && (updated == false)) { 
                            ((SprinklerProgram)SprinklerPrograms[i]).DateTimeStart = MyDate; 
                            ((SprinklerProgram)SprinklerPrograms[i]).Duration = MySpanDuration;
                             updated = true; 
                             strResp += "Updating Sprinkler " + intSprinklerNumber + " for "   
                            + MyDate.ToString("yyyy MMM d") + " to start at " + MyDate.ToString("HH:mm") + " and duration of "   
                            + MySpanDuration.Minutes + " minutes. <br>"; strResp = OutPutStream(response, strResp); 
                    } 
            } // does not exist, then will need to create it 
            if (updated == false) { 
                SprinklerPrograms.Add(new SprinklerProgram(MyDate, MySpanDuration, intSprinklerNumber)); 
                strResp += "Adding Sprinkler " + intSprinklerNumber + " for "   
                + MyDate.ToString("yyyy MMM d") + " to start at " + MyDate.ToString("HH:mm") + " and duration of "   
                + MySpanDuration.Minutes + " minutes. <br>"; updated = true; strResp = OutPutStream(response, strResp); 
            } 
        } 
        if (updated == false) { 
            //create a timeline to select hour and minutes 
            strResp += "<br>Select your starting time.<br>"; 
            strResp += "<table border=1>"; 
            //in case it's Today, allow programation for the next hour 
            int StartTime = 0; 
            if (TodayIsToday) 
                StartTime = intHour+1; 
            strResp = OutPutStream(response, strResp); 
            for (int i = StartTime; i < 24; i++) { 
                for (int j = 0; j < 2; j++) { 
                    strResp += "<tr><td>"; 
                    DateTime tmpDateTime = new DateTime(intYear, intMonth, intDay, i, j * 30, 0); 
                    strResp += tmpDateTime.ToString("HH:mm"); 
                    strResp += "</td><td>"; 
                    strResp += "<a href='" + MyParamPage.pageProgram + ParamStart   
                    + MyParamPage.year + ParamEqual + tmpDateTime.Year + ParamSeparator + MyParamPage.month + ParamEqual   
                    + tmpDateTime.Month + ParamSeparator + MyParamPage.day + ParamEqual + tmpDateTime.Day + ParamSeparator   
                    + MyParamPage.hour + ParamEqual + i + ParamSeparator + MyParamPage.minute + ParamEqual + j * 15   
                    + ParamSeparator + MyParamPage.duration + ParamEqual + "20" + ParamSeparator + MyParamPage.spr   
                    + ParamEqual + intSprinklerNumber + "'>20 minutes</a>"; 
                    strResp += "</td>"; 
                    strResp = OutPutStream(response, strResp); 
                    strResp = "</tr>"; 
                } 
            } 
            strResp += "</table>"; 
        } else { 
            // something has been updated so redirect to the main page 
            strResp += "<a href='/" + MyParamPage.pageSprinkler + "'>Back to main page<a><br>"; 
            strResp += "<a href='/" + MyParamPage.pageCalendar + ParamStart + MyParamPage.year   
            + ParamEqual + intYear + ParamSeparator + MyParamPage.month + ParamEqual + intMonth + ParamSeparator   
            + MyParamPage.spr + ParamEqual + intSprinklerNumber + "'>Proram this month</a>"; 
            strResp = OutPutStream(response, strResp); 
        } 
    } else { 
        strResp += "Date must be in the future"; 
    } 
} 
```

 In the case, a new program has to be added, the code is really simple as we are using an ArrayList:

```csharp
 SprinklerPrograms.Add(new SprinklerProgram(MyDate, MySpanDuration, intSprinklerNumber));
```

 Just use the Add member to add a new object. As explain, the only object that are added are SprinklerProgam. In the case, it has to be updated, it just search the list, compare the dates and update the information in the object for hour, minute and duration.

 And finally, if nothing has been update, it display a simple table containing the start hour and the duration. Here by default, it's 20 minutes but it can of course be different. It's hard coded here as an example. Best way is to use a constant of a variable that can be set in a different page.

 In case an update or an add has been done, it displays the ability to return to the main page or program another day. The smart thing with this code is that this page just point on itself. I mean by this that this page either display the list of hour and duration or either add, remove or update the information regarding a program. So it is quite efficient as many tests has already be done and are common anyway.

 And last but not least, when a date is not correct, or a time or anything else, it just display a gentle and simple message telling something is wrong. No real benefit here to do more detailed as it should not be displayed when clicking on the normal link. It may only happen when URL are type like this or called from another code or generated by another code.

 Now we have programs for sprinkler the question is how we will be able to launch those programs! See the answer in the next post ![Sourire](../assets/4401.wlEmoticon-smile_2.png) and enjoy .NET Microframework! As for the last post, my code is far to be perfect, I'm just a marketing director writing code! Code is life ![Sourire](../assets/4401.wlEmoticon-smile_2.png)
