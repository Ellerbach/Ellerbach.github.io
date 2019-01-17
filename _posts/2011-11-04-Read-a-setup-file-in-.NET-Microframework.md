---
layout: post
title:  "Read a setup file in .NET Microframework"
date: 2011-11-04 00:20:50 +0100
categories: 
---
In my previous post, I’ve explain how to read the content of a file in .NET Microframework. The idea now is to read a setup file. I want to store the position of the lamp icon I’d like to display on my Lego city map. More information on what [I want to do in this post]({% post_url 2011-10-11-Lighting-my-Lego-city-using-.NET-Microframework %}).

 First I’d like to explain the final structure of object I want to setup with my file. I have created a class which will contain a name, a position, the information if the light is on or not. The class is very simple, here is the code:

 
```csharp
public class LegoLight { 
    private string myName = ""; 
    private int myPosX = 0; 
    private int myPosY = 0; 
    private byte myNetwork = 0; 
    private bool myLight = false; 
    public string Name { get { return myName; } set { myName = value; } } 
    public int PosX { get { return myPosX; } set { myPosX = value; } } 
    public int PosY { get { return myPosY; } set { myPosY = value; } } 
    public byte Network { get { return myNetwork; } set { myNetwork = value; } } 
    public bool Light { get { return myLight; } set { myLight = value; } } 
}
```
 Then I create an Array to store all the LegoLight objects. The good news with array is that you can easily add, remove, search in the array. The main problem is that the object you are adding are just objects. so you need a cast to force the kind of object you want to play with. But as you are the developer, you know what you’ve put in your array ![Sourire](/assets/4401.wlEmoticon-smile_2.png) or if you don’t know, just guess ![Tire la langue](/assets/3036.wlEmoticon-smilewithtongueout_2.png)

 
```csharp
static ArrayList myLegoLight = new ArrayList();
```
I decided to create a simple setup file with the name, network and both position saved in the file. The concept of network is to be able to display all lights that are part of a network at the same time. The position is the position in pixel on the main image of the led to display. The name will be able to be displayed in the alt attribute of the file but also to be able to display the information on a regular page and not on the map. And I don’t save the state of the light, by default, it will be off.

There is no serialization implemented in netduino so to store a save and read a file, you’ll have to do it manually. You have basically 2 options. First is to save the binary state of your object. That’s approximately what the serialize function is doing and the second is to do like the old ini files readable by a human. I will go for the second option just because I’m a human and it will be easiest for me to create the setup file. I can do it easily with notepad. 

So I have decided to use the following format: name(string);network(number in a string);PosX(number in a string); PosY(number in a string)\r

As an example, it gives for one LegoLight object: mairie;1;158;59   
and for an array: 

mairie;1;158;59   
station;1;208;300   
train;1;10;10   
rue;1;700;550   

Now the challenge is to read a file like this. The code I wrote is the following (ParamSeparator = ‘;’, strDefaultDir and strFileSetup contains the path of the file and the name):

```csharp
LegoLight mLegoLight = new LegoLight(); 
FileStream fileToRead = null; 
try { 
    fileToRead = new FileStream(strDefaultDir+"\\"+strFileSetup, FileMode.Open, FileAccess.Read); 
    long fileLength = fileToRead.Length; 
    // Send HTTP headers. Content lenght is ser 
    Debug.Print("File length " + fileLength); 
    // Now loops sending all the data. 
    //file length has to be less than 1024 otherwise, it will raise an exception 
    byte[] buf = new byte[fileLength]; string mySetupString=""; 
    // Reads the data. 
    fileToRead.Read(buf, 0, (int)fileLength); 
    // convert the read into a string 
    mySetupString = new String(Encoding.UTF8.GetChars(buf)); 
    //find "\r" 
    int i = mySetupString.IndexOf("\r"); 
    string mySubstring = ""; 
    string[] myParam; int j=0; 
    while ((i < mySetupString.Length) && (i != -1)) { 
        //split the substring in 3 
        mySubstring = mySetupString.Substring(j, i - j); 
        myParam = mySubstring.Split(ParamSeparator); 
        mLegoLight = new LegoLight(); 
        mLegoLight.Name = myParam[0]; 
        int myint = 0; Convert.ToInt(myParam[1], out myint); 
        mLegoLight.Network = (byte)myint; Convert.ToInt(myParam[2], out myint); 
        mLegoLight.PosX = myint; Convert.ToInt(myParam[3], out myint); 
        mLegoLight.PosY = myint; myLegoLight.Add(mLegoLight); 
        //next string 
        j = i+1; 
        if (j < mySetupString.Length) 
            i = mySetupString.IndexOf("\r", j); 
        else 
            i = -1; 
    } 
    fileToRead.Close(); 
} catch (Exception e) { 
    if (fileToRead != null) { 
        fileToRead.Close(); 
    } 
    throw e; 
    }
```

 First, it create the LegoLight array and a file stream. Then, in a try catch, it starts to open and read the content of the file. See the previous post for most details on how to read a file on a SD. Please note here that the file as to be smaller than the max size of the buffer. For the netduino it is 1024. So the total length of the setup file has to be smaller than 1K. 1 LegoLight is about 35o so it allow to store about 30 objects. Which is quite enough for my needs. I did not test it in my code. I’m the only user of the solution and if I distribute it, I’ll have to do couple of additional check. But as we are in a try catch section, the code will just raise an exception and it will continue to work anyway.

 The second part is more interesting as it is where the parameters will be read and converted. First the buffer is converted into a string. the mySetupString now contains strings that’s has to be analyzed. I had 2 ways to do it there. The first one was to split the string using mySubstring.Split function with the "’\r’ and then for each object in the string, split it again finding the ‘;’. I did it for the first part in the “old way” with a simple while loop and finding the next occurrence of the ‘\r’. That is basically what the Split function is doing anyway to produce the array of strings. 
 
```csharp
 mySetupString = new String(Encoding.UTF8.GetChars(buf)); 
//find "\r" 
int i = mySetupString.IndexOf("\r"); 
string mySubstring = ""; 
string[] myParam; int j=0; 
while ((i < mySetupString.Length) && (i != -1)) { 
    //split the substring in 3 
    mySubstring = mySetupString.Substring(j, i - j); 
    myParam = mySubstring.Split(ParamSeparator); 
    mLegoLight = new LegoLight(); 
    mLegoLight.Name = myParam[0]; 
    int myint = 0; Convert.ToInt(myParam[1], out myint); 
    mLegoLight.Network = (byte)myint; Convert.ToInt(myParam[2], out myint); 
    mLegoLight.PosX = myint; Convert.ToInt(myParam[3], out myint); 
    mLegoLight.PosY = myint; myLegoLight.Add(mLegoLight); 
    //next string 
    j = i+1; 
    if (j < mySetupString.Length) 
        i = mySetupString.IndexOf("\r", j); 
    else 
        i = -1; 
} 
```
 When a line of parameters like “station;1;208;300” is in the myParam string array, the first param contains “station”, the second one “1”, the third one “208” and the last one “300”. As the numbers are in a string format, they need to be converted into real numeric numbers. In a previous post, I’ve created couple of functions that allow an easy conversion, that’s the job of the Convert.ToInt function. The LegoLight object is then added to the array.

 This is an easy and cheap way in terms of memory print to read a setup file and restore the state of objects. For biggest objects or file, you’ll may have to store the binary representation of the data and read by chunk. that should be as efficient but use less memory print. And ob course, avoid XML format they are too big and too costly in foot print. Remember, you have very limited resources, you need to tune them and make sure you are using them very carefully.

 If you want to save your parameters, here is the code (in the previous code, ParamSeparator = ‘;’). Nothing really complicated, it just create a string with all the parameters, add the separators and finally saved them in a file.

 
```csharp
string strSer = ""; 
LegoLight mLegoLight; 
for(int i=0; i<myLegoLight.Count; i++) { 
    mLegoLight=(LegoLight)myLegoLight[i]; 
    strSer += mLegoLight.Name + ";"   
        + mLegoLight.Network.ToString() + ";"   
        + mLegoLight.PosX.ToString() + ";"   
        + mLegoLight.PosY.ToString() + '\r'; 
} 
File.WriteAllBytes(strDefaultDir + "\\" + strFileSetup, Encoding.UTF8.GetBytes(strSer));
```
 If you do it in your code, don’t forget the try catch section and make sure you do not exceed the capacity of the memory for string objects. Remember it is 1024 char in netduino. So the build string has to be smallest. If not, you’ll have to write the file by small write up to the end. You’ll need to open the file before the loop, and write each stream in the file each time and finally close the file. And again, don’t forget the try catch. You never know what can happen. And as you are designing an embedded system, it has to work and just work!

 So I hope I’ve shown you the basic method to read a setup file and save it. Nothing really complicated there. I’m just a marketing director writing code mainly in planes and testing back home ![Sourire](/assets/4401.wlEmoticon-smile_2.png)

