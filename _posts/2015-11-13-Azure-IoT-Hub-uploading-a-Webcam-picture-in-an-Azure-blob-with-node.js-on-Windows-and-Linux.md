---
layout: post
title:  "Azure IoT Hub uploading a Webcam picture in an Azure blob with node.js on Windows and Linux"
date: 2015-11-13 08:07:00 +0100
categories: 
author: "Laurent Ellerbach"
thumbnails: "/assets/2015-11-13-thumb.jpg"
---
In my garden, I have couple of sensors and a greenhouse. In order to play with different technologies I’m using a RaspberryPI v1 (RPI) under Linux with an Atmel328 (same as in Arduino) for analogic data. And I wanted to test the new Azure IoT Hub. As I didn’t know anything on node.js, I‘ve decided to go for node.js as well using this RPI under Linux. So many new technologies for me ![Sourire](/assets/4401.wlEmoticon-smile_2.png) As I just wrote, I’m not a node.js king neither a Linux expert, just a beginner but I want to share my experience. And there may be best ways to code all this, so feedbacks welcome.

## Setup the dev environement

I’ll use Windows as my development environnement. Visual Studio 2015 can support node.js development and debugging. So first I needed to setup Visual Studio 2015. You can get for free Visual Studio Community 2015 [here](https://www.visualstudio.com/en-us/products/visual-studio-community-vs.aspx). Then you need to the free node.js tools for Visual Studio, download them [here](https://www.visualstudio.com/en-us/features/node-js-vs.aspx). And you need of course to install the node.js framework from [here](https://nodejs.org/en/).

And you’re all done for the dev side! so fast, so quick on the Windows side!

## Setup the RaspberryPI

I have to say, this part was a bit more difficult for me. First I’m not a Linux guy, second, it’s not that easy to install node.js when you don’t really know what you need. So here are the steps to install the latest version. I have to say I’ve spend quite a lot of time trying to install the right version. The one by default in the raspian repository is not the latest version. Finally, after some time I found the way to properly install it from the Azure IoT SDK github page [here](https://github.com/nodejs/node-v0.x-archive/wiki/Installing-Node.js-via-package-manager). The version I’m running on the RPI is a Debian (wheezy). So if you’re running the Jessie or Wheezy version, to get node.js installed correctly, here are the steps:

```bash
sudo apt-get install curlsudo curl --silent --location https://deb.nodesource.com/setup_0.12 | bash –sudo apt-get install --yes nodejs
```
Most likely because of previous/bad versions installed before it didn’t worked right away. After a reboot, it did finally (who said, no reboot is necessary in Linux? ![Sourire](/assets/4401.wlEmoticon-smile_2.png)). This will install as well “npm” which will allow later to download some needed module packages.

## Creating a node.js project in Visual Studio

this is quite straight forward, you have new project type which is node.js. I’ve created a Web project (this will allow easily to test couple of functions)

![image](/assets/5023.image_61A81B28.png)

It will create a server.ts file. I do recommend to create TypeScript file and not Javascript. this will allow to have a better support and maintainability over time. Javascript file are generated once compiled. And you can choose which version of Javascript you want to generate.

You can hit F5 and run your code ![Sourire](/assets/4401.wlEmoticon-smile_2.png) by default, it will create a project with a simple web server, you can try to debug, put break point. Quite magic ![Sourire](/assets/4401.wlEmoticon-smile_2.png)

## How to run all this on Raspberry?

the only 2 files you’ll need are the “server.js” and the “package.json” one. this second one contains the dependencies with other modules. So far, no real dependencies. It’s needed as well if you want to publish and share back your code as a package. So you’ll make sure that you’ll get all the right dependencies for your package and that it will always work.

to lunch the server, just type:

```bash
node server.js
```

The server will run as long as you close the console or stop it. Access it thru a browser with [http://ipaddress:port/](http://ipaddressport) the ipaddress is the ip address of the RPI and the port, the port you’ve chosen in your code.

If at this point it does not work, then it’s maybe because you have a node.js problem. Try to type “node –v”, this is supposed to give you the installed version. The one I’ve installed is v0.12.7

## Installing packages on dev environment

With node.js you’ll need to install couple of packages. Both on the dev environment and on the production environment.

Using Visual Studio, makes it super easy. Just right click on “npm” in your project then select “Install new npm Packages…”

![image]((/assets/2337.image_07A05C30.png)

This will open this window where you can search for the packages.

![image](/assets/4604.image_04FA3130.png)

We’ll need the following ones: “azure” and “azure-iot-device”. So install them. this will install all the packages and dependencies needed. It makes it very easy to see what are the available versions, if a package is already installed locally.

## Installing packages on the RPI

it’s about the same except it’s in command line. Thanks to the npm utility in Visual Studio, I know the packages I’ll need to install ![Sourire](/assets/4401.wlEmoticon-smile_2.png) 

```bash
sudo npm install azuresudo npm install azure-iot-device
```

Note: if you need a specific version, you can install it by giving the version like: sudo npm install azure-iot-device@1.0.0-preview5


## Connecting a device to Azure IoT Hub

The first thing to do is to create an Azure IoT Hub. Step by step [here](https://azure.microsoft.com/en-us/documentation/articles/iot-hub-csharp-csharp-getstarted/). This is great step by step, very easy. I do recommend to do all the tutorial and create all the codes in C#. This will allow you to understand how Azure IoT Hub is working. You’ll need this to continue this article.

The most important is to create at least 1 device identity. If you don’t, you will not be able to access the IoT Hub, send and receive messages.

The other tool I recommend you to use is part of the Azure IoT SDK and named [DeviceExplorer](https://github.com/Azure/azure-iot-sdks/tree/master/tools/DeviceExplorer). Follow the tutorial to create a device, launch this tool, put the connection string from your Azure IoT Hub in it (connection string look like: HostName=youriothub.azure-devices.net;SharedAccessKeyName=iothubowner;SharedAccessKey=thekey), and you should be able to see the device:

![image](/assets/2500.image_7AC896C2.png)

From here, you’ll need for later the device ConnectionString. this connection string is different from the main hub connection string! it is formed the same way as the previous one but is contains DeviceId (here calle LaurelleRPI) which is the name of your device instead of SharedAccessKeyName.

```
HostName=youriothub.azure-devices.net;DeviceId=LaurelleRPI;SharedAccessKey=thedevicekey
```

In the code, you’ll need this to get connected to the IoT Hub:

```javascript
var device = require('azure-iot-device');  
// must match the deviceID in the connection string   
var connectionstring = 'HostName=youriothub.azure-devices.net;DeviceId=LaurelleRPI;SharedAccessKey=thedevicekeyI';   
var iotHubClient = new device.Client(connectionstring, new device.Https());
```

And yes, that’s all what you need! How to test it? We can modify a bit the web server code to have a function which will send data to the IoT Hub. We will check with the Device Explorer if data arrive. So add this to the previous code and change the server function.

```javascript
var url = require('url');  
var deviceID = 'LaurelleRPI';  
var port = process.env.port || 1337

http.createServer(function (req, res) {  
    var request = url.parse(req.url, true);  
    var action = request.pathname;

if (action == '/senddata') {  
    var payload = '{\"deviceid\":\"' + deviceID + '\",\"wind\":42 }';  
    var message = new device.Message(payload);  
    iotHubClient.sendEvent(message, function (err, res) {  
    if (!err) {  
        if (res && (res.statusCode !== 204)) 
            console.log('send status: ' + res.statusCode + ' ' + res.statusMessage);  
    }  
    else  
        console.log('no data send, error ' + err.toString());  
    });  
    res.end('data sent');  
    } else {  
    res.writeHead(200, { 'Content-Type': 'text/plain' });  
    res.end('Linux RPI working perfectly, try /status /postimage /senddata and /image.jpg \n');  
    }

}).listen(port);
```

Run the code and access to the [http://ipaddress:1337/senddata](http://ipaddress:1337/senddata)

this will post data to the Azure IoT Hub and you’ll see them in the Device Explorer, You’ll need to monitor the device you’ve just created and you’ll use in your node.js code. At this point, you should see this:

![image](/assets/1348.image_65D9A500.png)

if not, you’ll most likely get an error message. Most of the error I got was because I made a mistake in the connection string or in my Typescript.

Now, the next step is to be able to receive messages from the IoT Hub. In order to see if we have received a message or not, we’ll need to listen all the time to check is a message is arrived. The function bellow check is a message is arrived and place it into messageFromIoTHub

```javascript
var messageFromIoTHub = "";

function pushtoblob()  
{ //do nothing 
}

function isMessage()  
{  
    iotHubClient.receive(function (err, res, msg) {  
        if (!err && res.statusCode !== 204) {  
            console.log('Received data: ' + msg.getData());  
            // process the request  
            messageFromIoTHub = msg.getData();  
            if (messageFromIoTHub === "picture")   
            pushtoblob();  
            iotHubClient.complete(msg, function (err, res) {  
                if (err) console.log('complete error: ' + err.toString());  
                if (res && (res.statusCode !== 204)) console.log('complete status: ' + res.statusCode + ' ' + res.statusMessage);       
            });  
            return true;  
        } else if (err) {  
            console.log('receive error: ' + err.toString());  
        }  
    });  
    return false;  
}

var isWaiting = false;  
function waitForMessages() {  
    isWaiting = true;  
    isMessage()  
    isWaiting = false;  
}

// Start messages listener  
setInterval(function () { if (!isWaiting) waitForMessages(); }, 1000);
```

The hub will be checked every second. Up to this point, you can test the code. Place a break point on the line “messageFromIoTHub = msg.getData();” and run the code.

From Device Explorer, send the message “picture”, if all geos well, you’ll receive back a feedback.

![image](/assets/1323.image_10B499C4.png)

In terms of code, it’s quite straight forward, the Azure SDK IoT is very well done and easy to use. Most of the function to access the IoT hub are super well designed and only 1 or 2 functions are needed.

## Uploading the picture to an Azure Blob storage

As for the previous part, you’ll need a storage setup. Follow the very well done step by step [here](https://azure.microsoft.com/en-us/documentation/articles/storage-create-storage-account/). You’ll need to go thru to continue this code.

Blob storage are very easy as well to access in node.js. They can get access like http://_mystorageaccount_.blob.core.windows.net/_mycontainer_/_myblob_.

In terms of code, as for the IoT Hub, you need the connection string and create the access to the blob service.

```javascript
var azure = require('azure');

//need to change AccountName and AccountKey  
var connectionblob = 'DefaultEndpointsProtocol=https;AccountName=mystorageaccount;AccountKey=thelongkey';  
var blobSvc = azure.createBlobService(connectionblob);
```

From the previous code, replace

```javascript
function pushtoblob()  
{  
    blobSvc.createContainerIfNotExists('webcam', function (error, result, response) {  
    if (!error) {  
        // Container exists  
        if (result == true)  
            console.log('blob created');  
        else  
            console.log('blob existing');  
    }  
    });  
    //create the picture named image.jpg  
    //webcam.run();     
    // this will upload the picture named image.jpg which needs to be in the same directory as the js file  
    blobSvc.createBlockBlobFromLocalFile('webcam', 'picture', 'image.jpg', function (error, result, response) {  
        if (!error) {  
        // file uploaded  
            console.log('file uploaded :-) ');  
        } else {  
            console.log('error uploading picture');  
        }  
    });  
}
```

the code is very straight forward as well. I do create a blob called “webcam” and upload an existing picture from the disk called “image.jpg”. We’ll see right after how to generate this picture from the webcam. In the meantime, make sure you add a picture called “image.jpg” in the same directory as your server.js file.

You can as well add the following code into the ‘'”http.createServer” function, after the “action =”.

```javascript
if (action == '/image.jpg') {  
    //webcam.run();  
    var img = fs.readFileSync('./image.jpg');  
    res.writeHead(200, { 'Content-Type': 'image/jpeg' });  
    res.end(img, 'binary');  
    }
```

Hit F5 in VS and test the code by accessing [http://ipaddress:1337/image.jpg](http://ipaddress:1337/image.jpg)

You should be able to see the picture you’ve placed in the folder. now, go back to the Device Explorer, and send again the “picture” message. You will see a confirmation in the Device Explorer that the message has been received.

So how to check if the blob picture has been uploaded correctly?

One of the way is to check in the Azure portal is the container has been created. And you can change the properties to make it public for example, so you can directly from the browser check your image. You’ll access it like [https://yourblobstorage.blob.core.windows.net/webcam/picture](https://yourblobstorage.blob.core.windows.net/webcam/picture)

![image](/assets/0456.image_508110FA.png)

You can use as well the excellent CloudBerry Explorer for Azure which you can download from [here](http://www.cloudberrylab.com/free-microsoft-azure-explorer.aspx). Once setup (see [my previous post]({% post_url 2015-10-01-How-to-move-Azure-VM-between-subscriptions %}) where I show how to move one Azure VM from one subscription to another to have more info on how to setup this tool). If all went correctly, you should be able to see the “webcam” container created

![image](/assets/7723.image_6B4D3706.png)

if you double click in it, you’ll see the picture blob:

![image](/assets/3730.image_6FC6487E.png)

and if you double click on the “picture” blob, you can open it and download your picture.

## Saving a picture from the webcam

I have to say, I did quite a lot of research to see if there is an efficient way to do this on a RPI. And there are not that many nice, easy and straight forward way to do it. First, not all webcam are supported. I had to check couple of webcams to find an old Lifecam-VX-6000 supported.

I finally went for some command line tool that generate images from the webcam entry (/dev/video0 for the default webcam). I’ve installed fswebcam

```bash
sudo apt-get install fswebcam
```

To create an image, just run “fswebcam image.jpg” and it will generate an image. Worked quite well on my Raspberry.

Then I’ve created in my project a webcam.ts file and put the following code:

```javascript
'use strict'

var child = require('child_process');

module.exports = {  
    run: function () {  
        child.execSync('fswebcam image.jpg', function (err, stdout, stderr) {  
            if (err !== null) {  
                console.log('error ' + err);  
            } else  
            {  
                console.log('image saved');  
            }  
         });    
     }  
};
```

and in the main server.js file:

var webcam = require('./webcam');

You can un comment the 2 lines you’ll find with this “//webcam.run();” This code just create the image from the webcam. Now, if you test this code on Windows, it will not create the image as the command “fswebcam” does not exist. But it will work on the RPI where you’ve install the command. Note that you need to use execSync to create the external process. It will wait for the external process to finish before continuing. If you're using the normal exec, the picture won't have time to be saved before the next steps.

## Deployment on the Raspberry and all together

You’ll need to deploy only the 3 files: “server.js”, “webcam.js” and “package.json”. Use the method you want (I personnaly use the old smb way, so installing and setting up smb…). I have to say I like as well to access my Raspberry with Remote Desktop. Just install “xrdp”  and you’re good to go for this ![Sourire](/assets/4401.wlEmoticon-smile_2.png)

And voilà, you can get it from your blob storage:

![blob](/assets/6866.image.jpg)

For sure all this can be improved, especially with the webcam part.

I've placed the webcam on the RPI which is in the garden and this is the live picture (picture only refresh when it is asked to):

![image live](https://portalvhdskb2vtjmyg3mg.blob.core.windows.net/webcam/picture)

## Conclusion

Visual Studio 2015 + node.js tools + Azure IoT Hub + Azure Storage = happiness

So easy to develop, so easy to debug, so easy to find the right module with the npm tools in VS. All the Azure resources are really easy and simple to use with node.js.

The great news is that my solution is working the same way under Windows and Linux. Only difference is about the webcam part but I really didn’t need it on the Windows side.

I’ve learned a lot in this project and will for sure continue it!



