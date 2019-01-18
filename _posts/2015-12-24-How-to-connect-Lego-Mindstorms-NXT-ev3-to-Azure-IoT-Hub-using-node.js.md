---
layout: post
title:  "How to connect Lego Mindstorms NXT ev3 to Azure IoT Hub using node.js"
date: 2015-12-24 08:22:35 +0100
categories: 
author: "Laurent Ellerbach"
thumbnails: "/assets/2015-12-24-thumb.jpg"
---
Recently, I’ve played with node.js and Azure IoT Hub. You can see my previous blog posts [here]({% post_url 2015-11-13-Azure-IoT-Hub-uploading-a-Webcam-picture-in-an-Azure-blob-with-node.js-on-Windows-and-Linux %}), [here]({% post_url 2015-11-24-Creating-an-Azure-IoT-Device-Explorer-in-node.js,-express-and-jade %}) and [here]({% post_url 2015-12-01-How-to-deploy-a-node.js-site-into-Azure-Web-App-to-create-a-Website %}). And as I’m a huge fan of Lego, I’ve decided to connected my [Lego Mindstorms ev3](http://www.lego.com/en-us/mindstorms/) (the new version of NXT) to Azure IoT Hub. Well, at the end of the day, the ev3 is just a 32-bit ARM9 processor, Texas Instrument AM1808 cadenced at 300MHz. The main OS is Linux and it does have 1 USB port and 1 SD card reader. It does allow to boot on an SD card another OS so you don’t have to flash the main one. and on the USB port, you can plus a wifi dongle.

# Setup the Mindstorms ev3

I already used the excellent [monobrick](http://www.monobrick.dk/) to run C# code on the brick and it was working perfectly. Now my challenge was to run node.js and connect it to Azure IoT Hub. So I looked quickly at various available images and found quickly the [ev3dev](http://www.ev3dev.org/) one.

So I’ flashed a 4Gb SD card and booted on it. Just follow the steps on the ev3dev site to flash the SD card. I’m using a very cheap wireless dongle, an [Edimax](http://www.amazon.com/Edimax-EW-7811Un-150Mbps-Raspberry-Supports/dp/B003MTTJOY/ref=sr_1_1?ie=UTF8&amp;qid=1450969491&amp;sr=8-1&amp;keywords=edimax). It does cost less than 10$/€ and it’s very small, so easy to add to the brick. It does connect at 150Mb max but you really don’t need to have more on the brick!

Time to boot the brick using ev3dev. I’m using an external power supply so I’m not consuming batteries and I do recommend to do it while running all those tests. It is very convenient during the development and test phase. Later you can of course run on batteries.

![WP_20151224_16_08_03_Pro](/assets/5672.WP_20151224_16_08_03_Pro_3A778E5E.jpg)![WP_20151224_16_08_08_Pro](/assets/8130.WP_20151224_16_08_08_Pro_6DD5E26B.jpg)

Once you’ve booted, connect to the wifi, you can as well use a wired dongle if you prefer. Careful as you only have 2 minutes to enter your key, so you better have to speed up to enter it ![Sourire](/assets/4401.wlEmoticon-smile_2.png) I had to redo it 3 times the first time as I have a quite long key and it’s not that easy to enter using the buttons and screen from the ev3.

Once connected to wifi, the IP address will display on the screen. Time to connect to the brick. I’m using PuTTY to connect to the brick.

```
login as: root   
root@192.168.1.20's password:   
             _____     _   
   _____   _|___ /  __| | _____   __   
  / _ \ \ / / |_ \ / _` |/ _ \ \ / /   
 |  __/\ V / ___) | (_| |  __/\ V /   
  \___| \_/ |____/ \__,_|\___| \_/

Debian jessie on LEGO MINDSTORMS EV3!

The programs included with the Debian GNU/Linux system are free software;   
the exact distribution terms for each program are described in the   
individual files in /usr/share/doc/*/copyright.

Debian GNU/Linux comes with ABSOLUTELY NO WARRANTY, to the extent   
permitted by applicable law.   
Last login: Wed Dec 23 18:49:39 2015 from pc10.home   
root@ev3dev:~# 
```

Follow the instruction of the e3vdev website to update the brick. It will take some time to update and upgrade all packages.

At the end of the day, when running a node –v, you’ll get v0.10.29 and npm is 1.4.21. And it’s the best you can get on this armel distribution so far.

# Setup the development environment

I’ve decided to go for Visual Studio. You can get a free version of Visual Studio Community [here](https://www.visualstudio.com/en-us/products/visual-studio-community-vs.aspx). I’ve installed as well the needed tools as explained in my blog post [here]({% post_url 2015-11-13-Azure-IoT-Hub-uploading-a-Webcam-picture-in-an-Azure-blob-with-node.js-on-Windows-and-Linux %}). If you prefer you can go as well for Visual Studio Code which you’ll find [here](https://code.visualstudio.com/). It does work perfectly as well.

Once all ready, create a new node.js project, I went for the simple web server one. Again, follow the steps from my previous article to get it all setup. I’m of course using TypeScript as it makes it easy to maintain the code. It does generate automatically at build time the javascript files.

Add the azure-iot-device and ev3dev packages. 

![image](/assets/2543.image_364A6441.png)

Up to this point, it was quite straight forward and I didn’t had any issue. The next steps are where I started to face issues.


# Deploy the solution on the brick

To deploy the solution, it’s just about creating a directory and copying the needed files on it. This time I used WinSCP for this. I’ve created a nodeNXT directory in /home and deployed the files there. Just copy the generated server.js and package.json file. 

![image](/assets/0447.image_73047D82.png)

On the device, in the /home/nodeNXT directory, run an “npm install” command. It will install the missing packages. Be patient, it does take a while. And it’s where the problems starts. 

As the needed version of node.js for the Azure IoT SDK is 0.12, and as the system can’t get more recent build than 0.10.29, some of the packages do not get deploy correctly. The main faulty one is the websocket one. 

If you try to create in your code a “var device = require('azure-iot-device');” you’ll get an error message on the device. I have to say I didn’t had any idea on how to get thru this. So I’ve asked Pierre Cauchois who is working in the Azure IoT team for advices. And he told me to remove the websocket part and just use classes which I’ll need. In my case I just needed the http way to publish in Azure IoT Hub.

For this, you’ll need to edit on the device “_./node_modules/azure-iot-device/device.js_”, in the last part of the file, just keep this:

```javascript
var common = require('azure-iot-common');

module.exports = {   
    Client: require('./lib/client.js'),   
    ConnectionString: require('./lib/connection_string.js'),   
    Http: require('./lib/http.js'),   
    Message: common.Message,   
    SharedAccessSignature: require('./lib/shared_access_signature.js'),   
};
```

And you’ll need as well to edit the file on the device  “./node_modules/azure-iot-device/lib/http_receiver.js”, comment the following lines:

```javascript
//var util = require('util');
//util.inherits(HttpReceiver, EventEmitter);
```

It looks like there is an issue as well in the util package. For what I’l do later, looks like those 2 lines have no issue. My goal is just to send data to Azure IoT Hub from the NXT. I haven’t tested to receive data from the Azure IoT Hub, so this may have an impact.

I’ve spend a lot of time fixing this issue due to the outdated version of node.js running on the brick but if you do this, you’ll be able to upload data in the Azure IoT Hub.

I found another issue, not related to the Azure IoT SDK but to the ev3dev version. The version deployed thru npm is the 0.9.2 and is quite basic. You can find a new version on [github which is 0.9.3](https://github.com/WasabiFan/ev3dev-lang-js). So download and install this version in your node modules, add it as a project in Visual Studio, and use this one. There is a bug in the index.ts file. Some references are missing for sensors.

In the sensor.ts file, make sure you reference all sensors from the sensor.ts file:

```javascript
// Sensors   
export var Sensor = sensors.Sensor;   
export var I2CSensor = sensors.I2CSensor;   
export var TouchSensor = sensors.TouchSensor;   
export var ColorSensor = sensors.ColorSensor;   
export var UltrasonicSensor = sensors.UltrasonicSensor;   
export var GyroSensor = sensors.GyroSensor;   
export var InfraredSensor = sensors.InfraredSensor;   
export var SoundSensor = sensors.SoundSensor;   
export var LightSensor = sensors.LightSensor;
```

Note that the code is coming in TypeScript, so you’ll need to compile it and deploy it manually to the device. I did it in a very basic way, once compiled, I’ve just copied all files from the directory. In theory, you just need the js ones and the package.json and the scprits folder.

![image](/assets/7077.image_081AAB4B.png)

OK, now we’re really done on the device side ![Sourire](/assets/4401.wlEmoticon-smile_2.png) Good because it takes me a lot of time to figure out all those issues.   

# Some code to upload data

Now here is some code to play with. This is a very basic code but shows all what you need to get data from sensors and upload them in Azure IoT Hub:

```javascript
import http = require('http');   
var url = require('url');   
var ev3dev = require('./node_modules/ev3dev-lang/index.js');   
var device = require('azure-iot-device');   
var connectionString = 'HostName=XXX.azure-devices.net;DeviceId=NXT;SharedAccessKey=XXXXXXXXXXXX';

var port = process.env.port || 1337   
http.createServer(function (req, res) {   
    var request = url.parse(req.url, true);   
    var action = request.pathname;   
    var tablee = [];   
    var i = 0;

if (action == '/battery') {   
    var battery = new ev3dev.PowerSupply();   
    var str = '';

    if (battery.connected) {   
        str += '  Technology: ' + battery.technology + '\n';   
        str += '  Type: ' + battery.type + '\n';   
        str += '  Current (microamps): ' + battery.measuredCurrent + '\n';   
        str += '  Current (amps): ' + battery.currentAmps + '\n';   
        str += '  Voltage (microvolts): ' + battery.measuredVoltage + '\n';   
        str += '  Voltage (volts): ' + battery.voltageVolts + '\n';   
        str += '  Max voltage (microvolts): ' + battery.maxVoltage + '\n';   
        str += '  Min voltage (microvolts): ' + battery.minVoltage + '\n';   
        tablee[i] = JSON.stringify({   
            Sensor: 'PowerSupply',   
            technology: battery.technology,   
            type: battery.type,   
            measuredCurrent: battery.measuredCurrent,   
            currentAmps: battery.currentAmps,   
            measuredVoltage: battery.measuredVoltage,   
            voltageVolts: battery.voltageVolts,   
            maxVoltage: battery.maxVoltage,   
            minVoltage: battery.minVoltage   
        });   
        i++;   
    }   
    else {   
        str = '  Battery not connected!';   
        tablee[i] = JSON.stringify({   
            Sensor: 'PowerSupply',   
            error: 'not connected'   
        });   
        i++   
    }   
    sendmsg(JSON.stringify({   
        tablee   
    }));   
    res.writeHead(200, { 'Content-Type': 'text/plain' });   
    res.end(str + JSON.stringify(battery));   
} else if (action == '/sensor') {   
    var touchSensor = new ev3dev.TouchSensor();   
    var colorsensor = new ev3dev.ColorSensor();   
    var UltrasonicSensor = new ev3dev.LightSensor();   
    var InfraredSensor = new ev3dev.InfraredSensor();   
    var SoundSensor = new ev3dev.SoundSensor();   
    var str = '';

    if (touchSensor.connected) {   
        str += 'touch pressed: ' + touchSensor.isPressed + '\n';   
        tablee[i] = JSON.stringify({   
            Sensor: 'TouchSensor',   
                isPressed: touchSensor.isPressed   
        });   
        i++;   
    }   
    if (colorsensor.connected) {   
        str += 'color sensor: \n   reflectedLightIntensity: ' + colorsensor.reflectedLightIntensity + '\n';   
        str += '   ambientLightIntensity: ' + colorsensor.ambientLightIntensity + '\n';   
        str += '   color: ' + colorsensor.color + '\n';   
        str += '   red: ' + colorsensor.red + '\n';   
        str += '   green: ' + colorsensor.green + '\n';   
        str += '   blue: ' + colorsensor.blue + '\n';   
        tablee[i] = JSON.stringify({   
            Sensor: 'ColorSensor',   
            reflectedLightIntensity: colorsensor.reflectedLightIntensity,   
            ambientLightIntensity: colorsensor.ambientLightIntensity,   
            color: colorsensor.color,   
            red: colorsensor.red,   
            green: colorsensor.green,   
            blue: colorsensor.blue   
        });   
        i++;   
    }   
    if (UltrasonicSensor.connected) {   
        str += 'UltrasonicSensor: \n   distanceCentimeters' + UltrasonicSensor.distanceCentimeters + '\n';   
        str += '   otherSensorPresent: ' + UltrasonicSensor.otherSensorPresent + '\n';   
        tablee[i] = JSON.stringify({   
            Sensor: 'UltrasonicSensor',   
            distanceCentimeters: UltrasonicSensor.reflectedLightIntensity,   
            otherSensorPresent: UltrasonicSensor.ambientLightIntensity   
            });   
        i++;   
    }   
    if (InfraredSensor.connected) {   
        str += 'InfraredSensor: \n   proximity: ' + InfraredSensor.proximity + '\n';   
        tablee[i] = JSON.stringify({   
            Sensor: 'InfraredSensor',   
            proximity: InfraredSensor.proximity   
        });   
        i++;   
    }   
    if (SoundSensor.connected) {   
        str += 'SoundSensor: \n   soundPressure: ' + SoundSensor.soundPressure + '\n';   
        str += '   soundPressureLow: ' + SoundSensor.soundPressureLow + '\n';   
        tablee[i] = JSON.stringify({   
            Sensor: 'SoundSensor',   
            soundPressure: SoundSensor.soundPressure,   
            soundPressureLow: SoundSensor.soundPressureLow   
        });   
        i++;   
    }   
    sendmsg(JSON.stringify({   
        tablee   
    }));

    res.writeHead(200, { 'Content-Type': 'text/plain' });   
    res.end(str);   
    } else {   
            res.writeHead(200, { 'Content-Type': 'text/plain' });   
           res.end('Try /battery /sensor \n');   
    }   
}).listen(port);

function sendmsg(data) {   
     var client = device.Client.fromConnectionString(connectionString);   
     var message = new device.Message(data);   
     message.properties.add('NXTsensors', 'sensorData');   
     console.log("Sending message: " + message.getData());   
     client.sendEvent(message, printResultFor('send'));   
}

function printResultFor(op) {   
     return function printResult(err, res) {   
         if (err) console.log(op + ' error: ' + err.toString());   
         if (res && (res.statusCode !== 204)) console.log(op + ' status: ' + res.statusCode + ' ' + res.statusMessage);       
     };   
} 
```

The code is quite simple, I do return, as text, in the page, the sensors and their states. At the same time I’m building a JSON table containing the sensors data. 

The function sendmsg is sending the data to Azure IoT Hub. This part of the code coming from the sample code you can find in the excellent SDK (it’s the simple http one). In the code, replace the XXX in the connection string by your IoµT Hub, make sure the name of your device is NXT or replace NXT by the name of device you want to use. Don’t forget to create a device before if needed. And of course, the primary key of the device.

Now, compile everything, deploy the server.js file to the device, run “node server.js” and wait 30 seconds. In a browser, you can now access the NXT brick on port 1337 like [http://ipaddress:1337/](http://ipaddress:1337/) (replace ipaddress by the IP address of your ev3). It will return: Try /battery /sensor

the /sensor returns in my case:

![image](/assets/3531.image_26F6408B.png)

/battery returns:

![image](/assets/0312.image_3CDFB2DB.png)

On the device side, you can check that the data has been sent:

![image](/assets/5314.image_4E553515.png)

What about Azure IoT Hub? Well, just use the Device Explorer (see my [previous article]({% post_url 2015-11-13-Azure-IoT-Hub-uploading-a-Webcam-picture-in-an-Azure-blob-with-node.js-on-Windows-and-Linux %}) on where to get it and how to use it). Run the Device Explorer and connect it to your IoT Hub before trying /sensor and /battery.

![image](/assets/0160.image_216262EB.png)

And voilà ![Sourire](/assets/4401.wlEmoticon-smile_2.png) we made it ![Sourire](/assets/4401.wlEmoticon-smile_2.png) The Lego Mindstorms ev3 is now connected to Azure IoT Hub using node.js and able to send data to Azure. 

Bottom line: when you have sources of and SDK, don’t be afraid to adapt, change and modify what you need for your own platform. I did it with the Azure IoT SDK because my node.js version was too old to fully run it. And I did it with the ev3dev package as it was outdated and there were couple of bugs in it.

As always, feedbacks welcome!