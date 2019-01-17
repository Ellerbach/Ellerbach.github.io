---
layout: post
title:  "Displaying a calendar in a web page using .NET Microframework"
date: 2011-09-19 22:08:53 +0100
---
As I want to program sprinklers I need to be able to select a date. [In my previous posts]({% post_url 2011-09-12-Implementing-a-simple-HTTP-server-in-.NET-Microframework %}), I've already explained how I've setup a Web Server using .NET Microframework, my netduino with Visual Studio using C# and all the magic of code ![Rire](/assets/0842.wlEmoticon-openmouthedsmile_2.png). My implementation allow me to pass parameters in a URL. And I want to create a page to display a calendar. Using PHP, Java or ASP.NET is so easy as you don't have to do anything, just call a date time picker class, object, widget or whatever extension it can be. Here, there is just nothing ![Triste](/assets/7245.wlEmoticon-sadsmile_2.png)

So all is to be done manually. and it's not very easy as we will need to determine the number of day in a month, display them in a nice table, add links on the days to call another page. So quite a bit of work. And also the need to go back in the past HTML docs... Well, lets start somewhere ![Sourire](/assets/4401.wlEmoticon-smile_2.png) I choose to start with a function to return the number of days in a month. I did not find it in the framework.

```csharp
static int NumberDaysPerMonth(int Month, int Year)
        {
            if ((Month <= 0) || (Month >= 13))
                return 0;
            if ((Year % 4 == 0 && Year % 100 != 0) || Year % 400 == 0)
            {
                int[] NbDays = new int[] {31, 29, 31, 30, 31, 30, 31, 31, 30, 31, 30, 31};
                return NbDays[Month-1];
            } else {
                int[] NbDays = new int[] {31, 28, 31, 30, 31, 30, 31, 31, 30, 31, 30, 31};
                return NbDays[Month-1];
        }
```

Basically there are year with month of February that change every 4 years, with the exception of century years and every 400 years. In this case, you have 29 days, in all other cases only 28\. The rest of the months stay constant. To be very efficient, I've just created tables of 12 elements representing each month, and I return the number of days. I'll be very proud if my netduino board will be still used to pilot my sprinklers in the next century... but anyway, it's always better to have as clean code as possible!

[In my previous post]({% post_url 2011-09-12-Implementing-a-simple-HTTP-server-in-.NET-Microframework %}), you'll find the first part of the code for this function. The first part analyze the URL and get the year and month of the sprinkler to program.

So here is the second part of the code. Pretty long but I'll explain all parts later on:

```csharp
    string strResp = "<HTML><BODY>netduino sprinkler<p>";

    // Print requested verb, URL and version.. Adds information from the request. strResp += "HTTP Method: " + request.HttpMethod + "<br> Requested URL: \"" + request.RawUrl +
        "<br> HTTP Version: " + request.ProtocolVersion + "\"<p>";
    response.ContentType = "text/html";
    response.StatusCode = (int)HttpStatusCode.OK;
    if ((intMonth > 0) && (intMonth<13) && (intYear > 2009) && (intYear <2200))
    {
        //are we in the future? DateTime tmpDT = new DateTime(intYear, intMonth, 1);
        DateTime tmpNow = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
        if (tmpDT >= tmpNow)
        {

            for (int i = 0; i < NUMBER_SPRINKLERS; i++)
                if (i != intSprinkler)
                    strResp += "Calendar for <a href='" + MyParamPage.pageCalendar + ParamStart + MyParamPage.year + ParamEqual + intYear + ParamSeparator + MyParamPage.month + ParamEqual + intMonth + ParamSeparator + MyParamPage.spr + ParamEqual + i + "'>sprinkler " + i + "</a><br>";
            strResp = OutPutStream(response, strResp); 
            strResp += "Month: " + intMonth + "<br>";
            strResp += "Year: " + intYear + "<br>";
            // Display some previous and next.
            // is it the first month? (case 1rst of January of the year to program but in the future year so month =12 and 1 less year) if ((intMonth == 1) && (intYear > DateTime.Now.Year))
                strResp += "<a href='" + MyParamPage.pageCalendar + ParamStart + MyParamPage.year + ParamEqual + (intYear - 1) + ParamSeparator + MyParamPage.month + ParamEqual + "12" + ParamSeparator + MyParamPage.spr + ParamEqual + intSprinkler + "'>Previous month</a>&nbsp&nbsp&nbsp";
            else if ((intMonth > DateTime.Now.Month) && (intYear == DateTime.Now.Year)) // (other cases strResp += "<a href='" + MyParamPage.pageCalendar + ParamStart + MyParamPage.year + ParamEqual + intYear + ParamSeparator + MyParamPage.month + ParamEqual + (intMonth - 1) + ParamSeparator + MyParamPage.spr + ParamEqual + intSprinkler + "'>Previous month</a>&nbsp&nbsp&nbsp";
            else if(intYear > DateTime.Now.Year)
                strResp += "<a href='" + MyParamPage.pageCalendar + ParamStart + MyParamPage.year + ParamEqual + intYear + ParamSeparator + MyParamPage.month + ParamEqual + (intMonth - 1) + ParamSeparator + MyParamPage.spr + ParamEqual + intSprinkler + "'>Previous month</a>&nbsp&nbsp&nbsp";
            // next month //case december strResp = OutPutStream(response, strResp); 
            if (intMonth == 12)
                strResp += "<a href='" + MyParamPage.pageCalendar + ParamStart + MyParamPage.year + ParamEqual + (intYear + 1) + ParamSeparator + MyParamPage.month + ParamEqual + "1" + ParamSeparator + MyParamPage.spr + ParamEqual + intSprinkler + "'>Next month</a>";
            else // (other cases strResp += "<a href='" + MyParamPage.pageCalendar + ParamStart + MyParamPage.year + ParamEqual + intYear + ParamSeparator + MyParamPage.month + ParamEqual + (intMonth + 1) + ParamSeparator + MyParamPage.spr + ParamEqual + intSprinkler + "'>Next month</a>";
            // display and build a calendar :) strResp += "<p>";
            strResp += "<table BORDER='1'><tr>";
            for (int i = 0; i < days.Length; i++)
                strResp += "<td>" + days[i] + "</td>";
            strResp += "</tr><tr>";
            int NbDays = NumberDaysPerMonth(intMonth, intYear);
            DateTime dt = new DateTime(intYear, intMonth, 1);
            for (int i = 0; i < (int)dt.DayOfWeek; i++)
                strResp += "<td></td>";
            strResp = OutPutStream(response, strResp); 
            for (int i = 1; i <= NbDays; i++)
            {
                if ((intMonth == DateTime.Now.Month) && (intYear == DateTime.Now.Year) && (i < DateTime.Now.Day))
                { // don't add a link to program a past day strResp += "<td>" + i + "</td>";
                }
                else {
                    strResp += "<td><a href='" + MyParamPage.pageProgram + ParamStart + MyParamPage.year + ParamEqual + intYear + ParamSeparator + MyParamPage.month + ParamEqual + intMonth + ParamSeparator + MyParamPage.day + ParamEqual + i + ParamSeparator + MyParamPage.hour + ParamEqual + DateTime.Now.Hour + ParamSeparator + MyParamPage.minute + ParamEqual + DateTime.Now.Minute + ParamSeparator + MyParamPage.spr + ParamEqual + intSprinkler + "'>" + i + "</a></td>";
                }
                if ((i + (int)dt.DayOfWeek) % 7 == 0)
                    strResp += "</tr><tr>";
                strResp = OutPutStream(response, strResp);
            }
            strResp += "</tr></table>";
        }
        else {
            strResp += "Not in the future, please select a valid month and year, <a href='calendar.aspx?Year=" + DateTime.Now.Year + ";Month=" + DateTime.Now.Month + ";Spr=" + intSprinkler + "'>click here</a> to go to the actual month";
        }
    }
    strResp += "</BODY></HTML>";
    strResp = OutPutStream(response, strResp);
}
```

The first line of code are here to create the HTML page, I use it also to show the URL and the parameters as a debugging point of view. No real rocket science there.

Then I test if the date is in the future. I setup a maximum date for 2200\. I think I can live perfectly up to this date ![Clignement d'oil](/assets/0728.wlEmoticon-winkingsmile_2.png) and if needed, at this time, I'll modify the code again ![Tire la langue](/assets/3036.wlEmoticon-smilewithtongueout_2.png). That said, this validation is not enough. We must know if we are in a future month or the actual or not.

//are we in the future? DateTime tmpDT = new DateTime(intYear, intMonth, 1);
DateTime tmpNow = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
if (tmpDT >= tmpNow)

Those 3 lines of code will give us the answer. If we are not in the future, no need to program a sprinkler. It's quite necessary to do as the page can be called by almost anyone with almost any parameter. And the number of element is a table is limited to 1024\. So no need to use unnecessary space in the memory. Embedded is about optimization. every single line of code has to have a usage. Otherwise, just remove it. If you can do something in 5 lines of code rather than 10, choose the 5 lines! And if 7 is the best compromise for speed/memory print, use the 7 lines. Resources are limited, don't forget it. There are so limited that the strResp string value is limited to 1024 characters. And 1024 characters in HTML is reach very soon!

I have to admit the first page I did I totally forget this limit. And guess what, first run with couple of HML worked perfectly and then it just raise an out of memory exception. And at this time, I just realize string were limited... That's why in the code, you'll find the following function and lines:

```Csharp
private static string OutPutStream(HttpListenerResponse response, string strResponse)
        {
            byte[] messageBody = Encoding.UTF8.GetBytes(strResponse);
            response.OutputStream.Write(messageBody, 0, messageBody.Length);                 
            return "";
        }

and the call is simple:

strResp = OutPutStream(response, strResp);
```

All what this function does is emptying the string and put it into the output stream to be displayed in the browser. As we are building our own page, it's quite easy to know when the string will be close to 1024 characters and empty it. So you'll see this line of code very regularly.

The function return an empty string which I remap to the strResp string. All what the function does is convert the string into an array of bytes, and put it into the output stream and initialize again strResp to an empty string.

I have N sprinklers to program and want to be able to change from a sprinkler to another. So the following line of code will create the right links:

```Csharp
for (int i = 0; i < NUMBER_SPRINKLERS; i++)
                        if (i != intSprinkler)
                            strResp += "Calendar for <a href='" + MyParamPage.pageCalendar 

+ ParamStart + MyParamPage.year + ParamEqual + intYear + ParamSeparator + MyParamPage.month 

+ ParamEqual + intMonth + ParamSeparator + MyParamPage.spr + ParamEqual + i + "'>sprinkler " + i + "</a><br>";
```

As you can see, the URL is build based on a class that returns string and couple of chars. All that is explain in a previous post.

intYear and intMonth has been decoded from a URL like *calendar.aspx?year=2011;month=9;spr=0*. And the code generate URL like this ![Sourire](/assets/4401.wlEmoticon-smile_2.png)

Now, I want to add a *Previous* and *Next* month in my page. Again, usually using automatic component in a high level PHP, Java or ASP.NET on a Windows or Linux box is very easy. Everything is done automatically with high level framework. Here you have to do all the cases by hand. There is the case of the first month (January) where you'll have to decrease for 1 year and change the month to 12 (December) to go on the previous month. There is the case of December where it's the opposite to go on the next month. And as I want to allow only future current and future month, I need to make sure, I will not display any previous when it's in the past.

```Csharp
// Display some previous and next.
// is it the first month? (case 1rst of January of the year to program but in the future year so month =12 and 1 less year) if ((intMonth == 1) && (intYear > DateTime.Now.Year))
    strResp += "<a href='" + MyParamPage.pageCalendar + ParamStart + MyParamPage.year + ParamEqual + (intYear - 1) + ParamSeparator + MyParamPage.month + ParamEqual + "12" + ParamSeparator + MyParamPage.spr + ParamEqual + intSprinkler + "'>Previous month</a>&nbsp&nbsp&nbsp";
else if ((intMonth > DateTime.Now.Month) && (intYear == DateTime.Now.Year)) // (other cases strResp += "<a href='" + MyParamPage.pageCalendar + ParamStart + MyParamPage.year + ParamEqual   
+ intYear + ParamSeparator + MyParamPage.month + ParamEqual + (intMonth - 1) + ParamSeparator   
+ MyParamPage.spr + ParamEqual + intSprinkler + "'>Previous month</a>&nbsp&nbsp&nbsp";
else if(intYear > DateTime.Now.Year)
    strResp += "<a href='" + MyParamPage.pageCalendar + ParamStart + MyParamPage.year + ParamEqual   
+ intYear + ParamSeparator + MyParamPage.month + ParamEqual + (intMonth - 1) + ParamSeparator   
+ MyParamPage.spr + ParamEqual + intSprinkler + "'>Previous month</a>&nbsp&nbsp&nbsp";
// next month //case december strResp = OutPutStream(response, strResp); 
if (intMonth == 12)
    strResp += "<a href='" + MyParamPage.pageCalendar + ParamStart + MyParamPage.year + ParamEqual   
+ (intYear + 1) + ParamSeparator + MyParamPage.month + ParamEqual + "1" + ParamSeparator   
+ MyParamPage.spr + ParamEqual + intSprinkler + "'>Next month</a>";
else // (other cases strResp += "<a href='" + MyParamPage.pageCalendar + ParamStart + MyParamPage.year + ParamEqual   
+ intYear + ParamSeparator + MyParamPage.month + ParamEqual + (intMonth + 1) + ParamSeparator   
+ MyParamPage.spr + ParamEqual + intSprinkler + "'>Next month</a>";
```

OK, we do have our *Previous* and *Next* links now ![Sourire](/assets/4401.wlEmoticon-smile_2.png). Not rocket science code but efficient and easy to write. Let go for the calendar itself!

```csharp
// display and build a calendar :) strResp += "<p>";
strResp += "<table BORDER='1'><tr>";
for (int i = 0; i < days.Length; i++)
    strResp += "<td>" + days[i] + "</td>";
strResp += "</tr><tr>";
int NbDays = NumberDaysPerMonth(intMonth, intYear);
DateTime dt = new DateTime(intYear, intMonth, 1);
for (int i = 0; i < (int)dt.DayOfWeek; i++)
    strResp += "<td></td>";
strResp = OutPutStream(response, strResp); 
for (int i = 1; i <= NbDays; i++)
{
    if ((intMonth == DateTime.Now.Month) && (intYear == DateTime.Now.Year) && (i < DateTime.Now.Day))
    { // don't add a link to program a past day strResp += "<td>" + i + "</td>";
    }
    else {
        strResp += "<td><a href='" + MyParamPage.pageProgram + ParamStart + MyParamPage.year   
+ ParamEqual + intYear + ParamSeparator + MyParamPage.month + ParamEqual + intMonth + ParamSeparator   
+ MyParamPage.day + ParamEqual + i + ParamSeparator + MyParamPage.hour + ParamEqual + DateTime.Now.Hour   
+ ParamSeparator + MyParamPage.minute + ParamEqual + DateTime.Now.Minute + ParamSeparator   
+ MyParamPage.spr + ParamEqual + intSprinkler + "'>" + i + "</a></td>";
    }
    if ((i + (int)dt.DayOfWeek) % 7 == 0)
        strResp += "</tr><tr>";
    strResp = OutPutStream(response, strResp);
}
strResp += "</tr></table>";

In order to display a month, we will need 7 columns and as many row as necessary. We will only display the number of the day where it has to be. So the code starts with creating a table with a simple border.

strResp += "<table BORDER='1'><tr>";
for (int i = 0; i < days.Length; i++)
    strResp += "<td>" + days[i] + "</td>";
strResp += "</tr><tr>";

The days table is the following:

static string[] days = new string[] { "Sunday", "Monday", "Tuesday", "Wednesday", "Thursday"  
, "Friday", "Saturday" };
```

Of course, you can get this from resources, make it localizable, etc. In my case, English will perfectly work for me and I need to use a minimum space in the netduino ![Sourire](/assets/4401.wlEmoticon-smile_2.png) The DateTime class has an i