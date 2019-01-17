---
layout: post
title:  "Creating an Azure IoT Device Explorer in node.js, express and jade"
date: 2015-11-24 08:25:45 +0100
categories: 
---
After playing a bit with Azure IoT hub and building a [webcam system with a RaspberryPi 2 running Linux]({% post_url 2015-11-13-Azure-IoT-Hub-uploading-a-Webcam-picture-in-an-Azure-blob-with-node.js-on-Windows-and-Linux %}) in my previous article, I’ve decided to continue developing a bit in node.js to build a simple equivalent of the [Device Explorer](https://github.com/Azure/azure-iot-sdks/tree/master/tools/DeviceExplorer) but in node.js. I’m not a node.js expert so there may be more efficient way to write some of the code.

Code is available on GitHub: [https://github.com/Ellerbach/nodejs-webcam-azure-iot/tree/master/DeviceExplorer](https://github.com/Ellerbach/nodejs-webcam-azure-iot/tree/master/DeviceExplorer). The code on GitHub does include more than what is explained in this article.

### Setup the dev environement

I’ll use Windows as my development environment. Visual Studio 2015 can support node.js development and debugging. So first I needed to setup Visual Studio 2015:

* You can get for free Visual Studio Community 2015 [here](https://www.visualstudio.com/en-us/products/visual-studio-community-vs.aspx).  
* Then you need to the free node.js tools for Visual Studio, download them [here](https://www.visualstudio.com/en-us/features/node-js-vs.aspx).  
* And you need of course to install the node.js framework from [here](https://nodejs.org/en/).  Once those 3 steps done, you’re good to go!

### Creating a node.js project in Visual Studio

This is quite straight forward, you have new project type which is node.js. I’ve created a Start Node.js Express 3 Application project. It does come with a simple MVC project containing couple of example pages which makes it easy to learn and understand how all is working.

![image](/assets/6253.image_4E7B973B.png)

Views are using jade. This is a part where I had quite some difficulties to use. I do recommend to read the Language Reference as well as the examples from [http://jade-lang.com/](http://jade-lang.com/). The most important is to keep in mind the indentation must be respected and this is what makes groups and makes all the code logic. When you got that, the rest is quite easy and really nice to use.

It will create a full web site. I do recommend to create TypeScript file and not Javascript. this will allow to have a better support and maintainability over time. Javascript file are generated once compiled. And you can choose which version of Javascript you want to generate.

### Listing devices

The idea is to have a page that will allow to enter the connection key:

![image](/assets/7762.image_1CA61155.png)

Once the key is entered, it will list the devices plus their keys and couple of other info (in real, keys are displayed instead of the blue box):

![image](/assets/2318.image_0ED65C0B.png)

So to build this, we will need to:

* Create a view which is about adding a jade file in the views directory 
* Add a function to handle requests on the page 
* Add a route so the traffic will be correctly redirected to the page  I will first explain how the principle of views and code is working.

#### Adding the view

Right click on Views then Add and New Items…

![image](/assets/7317.image_098A060B.png)

select jade file and create one call devices.jade. Let start by replacing what is generated by this code:

```html
extends layout

block content   
  h2 #{title}   
  h3 #{message}

div.connbox   
  p please enter your connection string like HostName=XXX.azure-devices.net;SharedAccessKeyName=iothubowner;SharedAccessKey=XXX   
  form(name="connection", action="/devices", method="post")   
    input(type="text", name="constr")   
    input(type="submit", value="Connect")   
```

In jade, it will create a page where the #{title} will be replaced by the default text rendering of the title object which will be given to the page. It can be a string or any object. This will allow to manipulate those objects and we will see later how to do that.

The second part will create a form which will post the value of the text box to the page names /devices

#### Adding a function to handle the code

In the routes/index.js file, add this code:

```javascript
export function devices(req: express.Request, res: express.Response) {   
    var cnxstr = req.body['constr'];   
    res.render('devices', { title: 'Devices', message: 'this is the connection string: ‘ + cnxstr });   
};
```

This simple code just find the value of ‘constr’ which has been send by the post form and ask to render the page by sending back the info in the message object. Now, we still don’t have everything as the function has not been declared as a route when the page /devices is called.

#### Adding the route for a page

Find the file app.ts and add the lines after the line “app.get(‘/’, routes.index);”

```javascript
app.get('/devices', routes.devices);   
app.post('/devices', routes.devices);
```

Basically, the function we’ve just wrote is now linked to a get or post request on the /devices page.

All together, if you click F5, go on the /devices pages, you’ll be able to fill the text box, send the data to the page and see what you’ve posted. This is a very basic example but it does allow to understand how express and jade are working together.

#### Requesting Azure IoT Hub devices list in node.js

You’ll need to add the ‘azure-iothub”"’ module.  Using Visual Studio, makes it super easy. Just right click on “npm” in your project then select “Install new npm Packages…”

![image](/assets/2337.image_07A05C30.png)

This will open this window where you can search for the packages.

![image](/assets/4604.image_04FA3130.png)

Here is the code to request all devices present in the IoT hub. It is quite straight forward as the node.js SDK is really nicely done:

```javascript
var iothub = require('azure-iothub');   
var cnxString = '';

export function devices(req: express.Request, res: express.Response) {   
    var cnxstr = req.body['constr'];   
    if ((cnxstr != undefined) || (cnxString)) {   
      if (cnxString) {   
        if ((cnxstr == undefined)||(cnxstr ==''))   
                cnxstr = cnxString;
        }
        var registry = iothub.Registry.fromConnectionString(cnxstr);   
        registry.list(function (err, deviceList) {   
            if (!err) {   
                cnxString = cnxstr;
                res.render('devices', { title: 'Devices', year, message: 'Getting list of devices', devicelist: deviceList });   
            } else   
                res.render('devices', { title: 'Devices', year, message: 'Error getting list of devices', devicelist: null });   
        });   
    } else   
        res.render('devices', { title: 'Devices', year, message: 'Please give a valid connection key', devicelist: null });      
};
```

First part of the code is really here to check if a connection string has already been provided. If yes, it will reuse the connection string. The key 2 lines are:

```javascript
var registry = iothub.Registry.fromConnectionString(cnxstr);   
registry.list(function (err, deviceList) {   
```

this will return in devicelist an array of devices. If no error, then it is passed to the devices view. So very straight forward.

#### Modifying the jade view to render devices list

Go back to the devices.jade file and add the following code:

```html
div.text   
if(devicelist!=null)   
  table   
    thead   
      tr   
        th   
          | DeviceId   
        th   
          | Prim Key   
        th   
          | Sec key   
        th   
          | Last upd   
        th   
          | Status   
        th   
          | Msg waiting   
    tbody   
      each device in devicelist   
        tr   
          td   
            a(href='/devicedetail/' + device.deviceId) #{device.deviceId}   
          td   
            | #{device.authentication.SymmetricKey.primaryKey}   
          td   
            | #{device.authentication.SymmetricKey.secondaryKey}   
          td   
            | #{device.lastActivityTime}   
          td   
            | #{device.status}   
          td   
            | #{device.cloudToDeviceMessageCount}   
  p   
  a(href='/adddevice/') Add a new device
```

This part took me quite a lot of time. The reason is jade and the way indentation is working. It is really super important to respect it and the alignment almost drives the behavior of what will be generated. the good news is that you can add code in the jade file like testing if you have a devicelist object. and do for each in the code. The code will generate a simple table which contains some of the device properties. I’ve created as well a page for details as well as a page to add a new device.

This code will render exactly as in the screen capture from the first part of this article. Now, let see how to generate the detailed page for devices as well as creating a new device.

### Listing devices properties

Similar to the previous part, add a devicedetail jade, here is the code very similar to the previous page to generate the details in a table:

```html
extends layout

block content   
  h2 #{title}   
  p #{message}

div.text   
  if(device!=null)   
    table   
      thead   
        tr   
          th   
            | Details   
          th   
            | Values   
      tbody   
          tr   
            td   
              | Device Id   
            td   
              | #{device.deviceId}   
          tr   
            td   
              | Primary Key   
            td   
              | #{device.authentication.SymmetricKey.primaryKey}   
          tr   
            td   
              | Secondary Key   
            td   
              | #{device.authentication.SymmetricKey.secondaryKey}   
          tr   
            td   
              | Last Activity   
            td   
              | #{device.lastActivityTime}   
          tr   
            td   
              | Generation Id   
            td   
              | #{device.generationId}   
          tr   
            td   
              | Messages waiting   
            td   
              | #{device.cloudToDeviceMessageCount}   
          tr   
            td   
              | etag   
            td   
              | #{device.etag}   
          tr   
            td   
              | Status   
            td   
              | #{device.status}   
          tr   
            td   
              | Status Reason   
            td   
              | #{device.statusReason}   
          tr   
            td   
              | Connection State   
            td   
              | #{device.connectionState}   
          tr   
            td   
              | connectionStateUpdatedTime   
            td   
              | #{device.connectionStateUpdatedTime}   
          tr   
            td   
              | statusUpdatedTime   
            td   
              | #{device.statusUpdatedTime}
```

We’ll need to create a function that will handle the request and return the device object:

```javascript
export function devicedetail(req: express.Request, res: express.Response) {   
    var devId = req.params.deviceId;   
    var strcnx = getHostName(cnxString);   
    if (strcnx == '')   
      res.render('devicedetail', { title: 'Device detail', year, message: 'Error getting device details. Connection string was: ' + cnxString + ' and deviceId: ' + devId });   
    strcnx += ';DeviceId=' + devId;   
    var registry = iothub.Registry.fromConnectionString(cnxString);   
    var msg = 'No device found';   
    registry.get(devId, function (err, device) {   
      if (!err) {   
        strcnx += ';SharedAccessKey=' + device.authentication.SymmetricKey.primaryKey;   
        res.render('devicedetail', { title: 'Device detail', year, message: 'Those are the device details. Connection string: ' + strcnx, device: device });   
      } else   
        res.render('devicedetail', { title: 'Device detail', year, message: 'Error connecting' });   
    });   
};

function getHostName(str)   
{   
    var txtchain = str.split(';');   
    for (var strx in txtchain) {   
      var txtbuck = txtchain[strx].split('=')   
      if (txtbuck[0].toLowerCase() == 'hostname')   
        return txtchain[strx];   
    }   
    return '';   
}
```

As you’ll see in the jade page, the link to the page is /devicedeatil/name_of_a_device. In order to catch it, we’ll need to declare it in the route and it will allow to have it thru the req.params function. I will name it deviceid. so add this line in the app.ts file:

app.get('/devicedetail/:deviceId', routes.devicedetail);

First part of the code is about getting the list of devices and making sure the device exists. then it’s about getting the device and returning it. As sometimes you need the device connection string, this string is built and returned as well. As a result, you’ll get a detailed page like:

![image](/assets/0003.image_742EE153.png)

Those properties are the ones available for every device. The status shows is the device is allow or not to connect. If not, you’ll have a reason (128 bit max) displayed in the Status reason. If you send messages to your device, you’ll see as well if messages are waiting.

### Adding a device to Azure IoT hub

Very similar to the previous part, we’ll just add a jade file adddevice. Here is the code:

```html
extends layout

block content   
  h2 #{title}   
  p #{message}

div.connbox   
  if(deviceId==null)   
     p please enter your device name   
     form(name="adddevice", action="/adddevice", method="post")   
       input(type="text", name="deviceId")   
       input(type="submit", value="Add")
```

Simple code, very similar to the first example. For the main function code, it’s quite easy as well:

```javascript
export function adddevice(req: express.Request, res: express.Response) {   
     var devId = req.params.deviceId;   
     if (devId == undefined) {   
        devId = req.body['deviceId'];   
        if (devId == undefined)   
          res.render('adddevice', { title: 'Add device', year, message: 'Error, no device ID' });   
     }   
     if (cnxString == '')   
        res.render('adddevice', { title: 'Add device', year, message: 'Error, no connection string' });   
     else {   
        var registry = iothub.Registry.fromConnectionString(cnxString);   
        //create a new device   
        var device = new iothub.Device(null);   
        device.deviceId = devId;   
        registry.create(device, function (err, deviceInfo, response) {   
          if (err)   
            res.render('adddevice', { title: 'Add device', year, message: 'Error, creating device' + err.toString() })   
          else   
            if (deviceInfo)   
              res.render('adddevice', { title: 'Add device', year, message: 'Device created ' + JSON.stringify(deviceInfo) });   
            else   
              res.render('adddevice', { title: 'Add device', year, message: 'Unknown error creating device ' + devId });   
      });   
    }    
}
```

All up, first part of the code is just to check if there is a device name either thru get as a param, either thru post. Second part is about adding a device:

```javascript
var registry = iothub.Registry.fromConnectionString(cnxString);   
//create a new device   
var device = new iothub.Device(null);   
device.deviceId = devId;   
registry.create(device, function (err, deviceInfo, response) { 
```

Again, very simple, very straight forward. we’re connecting to the Azure IoT Hub registry, then create an empty device, set the deviceId name and ask for creation. 

Don’t forget to add the route as well. the “?” in “/adddevice/:deviceId?” is to make the param as optional.

```javascript
app.get('/adddevice/:deviceId?', routes.adddevice);   
app.post('/adddevice', routes.adddevice);
```

Once created, if there is no error, the device is sent back. I’m just converting it into a JSON and display it in the message:

![image](/assets/1803.image_752961E1.png)

You can do a very similar code to delete a device. You can as well send a message to the device and monitor the results. I have to say the Azure IoT SDK in node.js is really great and working perfectly. And please note that this website can be deployed on Azure, Windows or Linux or anything else that can run one of the latest node.js version (see restriction in the Azure Iot SDKs on GitHub [here](https://github.com/Azure/azure-iot-sdks))

More examples on [my GitHub](https://github.com/Ellerbach/nodejs-webcam-azure-iot/tree/master/DeviceExplorer)! Enjoy and feedback welcome.
