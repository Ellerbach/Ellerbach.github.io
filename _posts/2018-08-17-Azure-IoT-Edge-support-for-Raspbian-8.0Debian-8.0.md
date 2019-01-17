---
layout: post
title:  "Azure IoT Edge support for Raspbian 8.0/Debian 8.0"
date: 2018-08-17 11:04:53 +0100
categories: 
---
In theory, Azure IoT Edge is working only for the version 9 of Debian. If you’re using a Raspberry Pi, you’ll most likely use a Raspbian version which is based on Debian. So same, you’ll have to be on the version 9 to be able to deploy Azure IoT Edge on your device. But what if you’re running a version 8? And for some reasons, you can’t upgrade to version 9? Well, the good news is that there is a way to make it working. Let’s see how and what is needed. 

## Specificities of Azure IoT Edge

 Azure IoT Edge uses some of the components which are not present in v8 of Debians like libssl 1.0.2. Reason is because IoTEdge is using DTLS which are not present in the previous versions. You can try to upgrade the list of packages, force the version, in the 8 version (jessie), this package do not exist. So the way to get it is from the next version, 9 so stretch.  The libssl can be found here: [https://packages.debian.org/stretch/libssl1.0.2](https://packages.debian.org/stretch/libssl1.0.2) and as you’ll see there is the support for armhf which is the version of the RPI processor. 

## Pre installation before Azure IoT Edge

 Basically, we’ll have to download and install the package. _wget http://ftp.us.debian.org/debian/pool/main/o/openssl1.0/libssl1.0.2_1.0.2l-2+deb9u3_armhf.deb_ _sudo dpkg -i libssl1.0.2_1.0.2l-2+deb9u3_armhf.deb_ _sudo apt-get install -f_ If you get any error message, it may be related to the fact you have a non-compatible version of libssl like the libsll-dev version. In this case, just purge it with _sudo apt-get purge libssl-dev_ Now, you can follow the instructions here: [https://docs.microsoft.com/en-us/azure/iot-edge/how-to-install-iot-edge-linux-arm](https://docs.microsoft.com/en-us/azure/iot-edge/how-to-install-iot-edge-linux-arm) 
 
## Bang, another issue: LISTEN_FDNAMES

 Once you’ve done that, you’ll still have an issue. When looking at the journal _sudo journalctl -u iotedge -f_, the IoTEdge will tell you the environment variable LISTEN_FDNAMES is not setup. The reason is that in the Debian 9, this is not used anymore. The way the service is setup is using another mechanism. So, in short, we’ll have to add it. Reading a bit about it and searching about it, it needs to be added in the _/etc/system/system/multi-user.target.wants/iotedge.service_ Just add the line below in the In the [Service] section Environment=LISTEN_FDNAMES=iotedge.mgmt.socket:iotedge.socket And now, if you look at the journal again, you will see the 2 core containers downloading, you’ll see in your portal that your IoTEdge device will get connected. Just allow some time for the donwloads to happen and you’ll be good to go! Keep in mind that if you’re building your own edge containers, they must be built for Debian 8 and not the 9 version. Otherwise, they won’t start or run correctly. Please note as well, this scenario is not supported. In theory, Azure IoTEdge is not built to run on Debian 8, only 9. So other things can happen.   