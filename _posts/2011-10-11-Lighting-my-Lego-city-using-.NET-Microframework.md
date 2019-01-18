---
layout: post
title:  "Lighting my Lego city using .NET Microframework"
date: 2011-10-11 23:10:00 +0100
author: "Laurent Ellerbach"
thumbnails: "/assets/2011-10-11-thumb.jpg"
---
Now I have created a software to [pilot sprinklers]({% post_url 2011-09-12-Implementing-a-simple-HTTP-server-in-.NET-Microframework %}), I want to be able to pilot my Lego city and light it. I know my sprinkler project is not over as I still need to work on the hardware. I will do it over the winter and try to find someone to help me. This new project looks simpler for me as it will basically only be couple of led. So it can be directly plugged to the [netduino]({% post_url 2011-09-09-netduino-board-geek-tool-for-.NET-Microframework %}) board.

 I have quite a big Lego city as you can see in this picture with 2 trains, a modern city, an old city, an airport, couple of rail stations.

 [![](/assets/3343.ville.jpg)](/assets/3343.ville.jpg)

 For those who may not have guess, I’m a Lego fan, I have more than 650 official Lego sets from 1970+ to now. Not all are build, some are just used for spare parts to build other kind of constructions. And I have also hundreds of kilos of brick at home which allow me to build whatever I want. You can have a look at part of the collection [here](http://www.ellerbach.net/lego).

 Same as for the sprinkler project, the idea is to be able to switch some part on automatically, at night for example, and switch them on and off also manually. In order to reuse the work I’ve done, I will create a similar project with and [http web server]({% post_url 2011-09-12-Implementing-a-simple-HTTP-server-in-.NET-Microframework %}) (like IIS or Apache running on Windows or Linux), dynamic pages (like ASP.NET, php or Java) with parameters. I’ve already develop a web server I will be able to reuse and the management for dynamic input.

 My idea this time is to be able to display a web page with pictures of the city and specific points on the pictures that will represent the light to switch on and off. clicking on them will change the color of the points and switch the light accordingly.

 In a longer term, I will try to pilot also the train lights and the rail switch. That will need a bit more electronic but is very easy to do in term of code. So probably that the code will exist before the electronic as for the sprinkler project. I will try also in this project to use the SD card to store the picture of the city and the points coordinate to be displayed. So a kind of setup file. I’ve already try to play with the SD card but with limited success. I don’t know from where the problem is coming from. I use couple of different cards and I always get errors but never the same. So I think the problem is coming from the hardware.

 So let see where this project will go ![Sourire](/assets/1030.wlEmoticon-smile_2.png)

