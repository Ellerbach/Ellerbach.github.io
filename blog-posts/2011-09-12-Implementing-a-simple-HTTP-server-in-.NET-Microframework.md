# 2011-09-12 Implementing a simple HTTP server in .NET Microframework

To follow [my previous posts](./2011-09-09-netduino-board-geek-tool-for-.NET-Microframework.md), I've started to implement a simple HTTP server in my [netduino](https://www.netduino.com/netduinoplus/specs.htm) using .NET Microfamework. I have to admit it was quite easy as there is a HTTP Server example in the samples. I was very impressed that with no OS so no Windows, no Linux, no Mac or any DOS or whatever OS, you can do high level code, create a Web Server in about 2 hours. I haven't seen something comparable using Java, native C or other languages. There may be ways but here, all the framework is designed to be able to handle any kind of networking, there is even multithreading. I'll use multithreading in a future post to show how to handle periodic calls.

 As I would not use all the functions, I've started to simplify the code and kept only what I needed.

 So I just kept couple of functions:

* public static void StartHttpServer()  
* internal static void RunServer(string prefix)  
* static private string GetPathFromURL(string URL)  
* private static void ProcessClientGetRequest(HttpListenerContext context)  
* static void SendFileOverHTTP(HttpListenerResponse response, string strFilePath)  
* class PrefixKeeper   I will not used any post method, so I removed it from the code. I removed also the certificate part because I won't use HTTPS and removed the secure part as I won't also use it. It needs to clean a bit the code but nothing really too complicated.

 I wanted to use parameter in the URL. So have requests like [http://siteweb/page.ext?param1=value1;param2=value2](http://siteweb/page.ext?param1=value1;param2=value2)

 In order to do that, I've created a very simple class which associate on parameter and one value. Nothing really to comment here, it's a very classic class. I let the set and get properties in this class as it may be use in read and write. Both parameters and values are string.

```c#
public class Param { 
    private string myName = ""; 
    private string myValue = ""; 
    public string Name { get { return myName; } set { myName = value; } } 
    public string Value { get { return myValue; } set { myValue = value; } } 
    public Param() { myName = ""; myValue = ""; } 
}
```

 In the sample, there is nothing to decrypt such a URL. So I'll need to function to decrypt the URL and return a table with parameters and the associated values.

 As I want to have clean code (or as clean as possible ![Sourire](../assets/4401.wlEmoticon-smile_2.png)), I've defined 3 chars constant for the various separators.

 ```c#
const char ParamSeparator = ';'; 
const char ParamStart = '?'; 
const char ParamEqual = '=';

private static Param[] decryptParam(String Parameters) {
     Param[] retParams = null; 
     int i = Parameters.IndexOf(ParamStart); 
     int j = i; int k; 
     if ( i> 0) { 
         //look at the number of = and ; 
        while ((i < Parameters.Length) || (i == -1)) { 
            j = Parameters.IndexOf(ParamEqual, i); 
            if (j > i) {
                //first param! 
                if (retParams == null) { 
                    retParams = new Param[1]; 
                    retParams[0] = new Param(); 
                } 
                else { 
                    Param[] rettempParams = new Param[retParams.Length+1]; 
                    retParams.CopyTo(rettempParams, 0); 
                    rettempParams[rettempParams.Length-1] = new Param(); 
                    retParams = new Param[rettempParams.Length]; 
                    rettempParams.CopyTo(retParams, 0); 
                } 
                k = Parameters.IndexOf(ParamSeparator, j); 
                retParams[retParams.Length - 1].Name = Parameters.Substring(i + 1, j - i - 1); 
                //case'est la fin et il n'y a rien 
                if (k == j) { 
                    retParams[retParams.Length - 1].Value = ""; 
                } // cas normal 
                else if (k > j) { 
                    retParams[retParams.Length - 1].Value = Parameters.Substring(j + 1, k - j - 1); 
                } //c'est la fin 
                else { 
                    retParams[retParams.Length - 1].Value = Parameters.Substring(j + 1, Parameters.Length - j - 1); 
                } 
                if (k > 0) 
                    i = Parameters.IndexOf(ParamSeparator, k); 
                else 
                    i = Parameters.Length; 
                } 
            } 
        } 
    return retParams; 
    }
```

 The code here is not very complex. It looks first at the start parameter. Here, it's the question mark (? ParamStart) then it finds the separator equal mark (= ParamEqual) and it finished by the separator (; ParamSeparator). Couple of cases if we are at the end for example as there is usually no separator.

 I'm not sure it's the smartes way to do it but it's working, it's pretty robust and it's been working with many various URL and cases. I've chosen to return a table as it's easy to implement and easy to use. It's pretty simple in .NET Microframework.

 You just has to be careful as depending on the platform you are using the size of the tables are limited. In the case of netduino, it's a maximum 1024 elements. That's also the limit size for strings. No impact here as we can consider (never consider anything in code ![Sourire](../assets/4401.wlEmoticon-smile_2.png)) that a URL will be less than 1024 parameters as the string used will of course of a maximum of 1024 characters.

 In order to have clean code, I've also define couple of parameters and page names in a read only class.

```c#
public class ParamPage { 
    public string year { get { return "year"; } } 
    public string month { get { return "month"; } } 
    public string day { get { return "day"; } } 
    public string hour { get { return "hour"; } } 
    public string minute { get { return "minute"; } } 
    public string duration { get { return "duration"; } } 
    public string spr { get { return "spr"; } } 
    public string pageProgram { get { return "program.aspx"; } } 
    public string pageListPrgm { get { return "listprg.aspx"; } } 
    public string pageCalendar { get { return "calendar.aspx"; } } 
    public string pageSprinkler { get { return "sprinkler.aspx"; } } }
```

 It defines all my parameters, name of the pages I'll use. So in code, I'll use this class to make sure I'll always use the right element and don't mix up my parameters. It will also be the same name use for all pages when needed.

 As you've seen, the decrypt parameter function return a string value. In your code, you may want to convert it to int, float, bool or other numeric values. .NET Microframework does not provide any convert class as the full framework. So you'll to do it by hand ![Sourire](../assets/4401.wlEmoticon-smile_2.png) Not quite hard and you'll be able to keep this class for other development.

```c#
public class Convert { 
    public static bool ToFloat(string s, out float result) { 
        bool success = true; int decimalDiv = 0; 
        result = 0; 
        try { 
            for (int i = 0; i < s.Length; i++) { 
                if (s[i] == '.' && decimalDiv == 0) 
                    decimalDiv = 1;
                else if (s[i] < '0' || s[i] > '9') 
                    success = false; 
                else { 
                    result = result * 10; decimalDiv = decimalDiv * 10; 
                    result += (int)(s[i] - '0'); 
                } 
            } 
            result = (float)result / decimalDiv; 
        } catch { 
            success = false; 
        } 
        return success; 
    } 
    public static bool ToInt(string s, out int result) { 
        bool success = true; 
        result = 0; 
        try { 
            for (int i = 0; i < s.Length; i++) { 
                result = result * 10; result += (int)(s[i] - '0'); 
            } 
        } catch { 
            success = false; 
        } return success; 
    } 
    public static bool ToBool(string s, out bool result) { 
        bool success = true; 
        result = false; 
        try { 
            if ((s == "1") || (s.ToLower() == "true")) 
                result = true; 
        } catch { 
            success = false; 
        } 
        return success; 
    } 
}
```

 The code is very simple too. I've prefer to handle in the class a try catch and return a success value. I feel more comfortable coding like that but if you want to make it more simpler and faster, just don't use a try catch and handle it in a higher level. The conversion for int will return funky numbers if you don't have only decimal in the string and same for float. I'm not looking first if all are numeric if the min and max values are correct. It's just fast and easy way to convert.

 For the bool conversion, I've decided to only validate the true which can be represented as 1 or true. Anything else will just be false. So here false is anything but true.

 Now to use the decrypt function and associate couple of values, here is a complete example. with the request.RawUrl you can get the URL so a string like calendar.aspx?year=2011;month=9;spr=0

```c#
HttpListenerRequest request = context.Request; 
HttpListenerResponse response = context.Response; // decode params 
string strParam = request.RawUrl; 
ParamPage MyParamPage = new ParamPage(); 
int intMonth = -1; 
int intYear = -1; 
int intSprinkler = -1; 
Param[] Params = decryptParam(strParam);
```

 Here you'll get Params as a table of 3 elements. First couple will be year and 2011, second month and 9 and third spr and 0. All as strings. Now you'll have to convert those string into values and then use them in your code.

 ```c#
if (Params !=null) 
    for (int i = 0; i < Params.Length; i++) { 
        //on cherche le paramètre strMonth 
        int j = Params[i].Name.ToLower().IndexOf(MyParamPage.month); 
        if (j == 0) { 
            Convert.ToInt(Params[i].Value, out intMonth); 
        } 
        j = Params[i].Name.ToLower().IndexOf(MyParamPage.year); 
        if (j == 0) { 
            Convert.ToInt(Params[i].Value, out intYear); 
        } 
        j = Params[i].Name.ToLower().IndexOf(MyParamPage.spr); 
        if (j == 0) { 
            Convert.ToInt(Params[i].Value, out intSprinkler); 
        } 
    }
```

At the end, you'll get intMonth = 9, intYear = 2011 and intSprinkler = 0.

Last but not least, to call a function with a name and parameters you'll need to do it in the private static void ProcessClientGetRequest(HttpListenerContext context) function from the sample.

```c#
HttpListenerRequest request = context.Request; 
HttpListenerResponse response = context.Response; 
ParamPage MyParamPage = new ParamPage(); 
string strFilePath = GetPathFromURL(request.RawUrl); 
string strCalendar = "\\" + MyParamPage.pageCalendar;
```

```c#
// if page calendar.aspx 
if (strFilePath.Length >= strCalendar.Length) { 
    if (strFilePath.Substring(0, strCalendar.Length).ToLower() == strCalendar) { 
        ProcessCalendar(context); 
        return; 
    } 
}
```

 So basically, the idea is to see if the name of the page is the one used at the beginning of the URL string. If yes, it calls a specific function and gives it the context object. From there, you can use the previous code to decode your URL and analyze your parameters.

 So to summarize, with couple of more functions, you are now able to pass parameters un a URL and decrypt them. For me, that's what I wanted to do as it's very easy to use to generate URL but also call specific URL in code. If you have better way to do all of this, just let me know, I'm just a marketing guy doing code ![Sourire](../assets/4401.wlEmoticon-smile_2.png)
