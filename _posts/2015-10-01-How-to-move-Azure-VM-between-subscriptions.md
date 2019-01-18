---
layout: post
title:  "How to move Azure VM between subscriptions"
date: 2015-10-01 07:40:17 +0100
categories: 
author: "Laurent Ellerbach"
thumbnails: "/assets/2015-10-01-thumb.jpg"
---
I recently needed to move an Azure Virtual Machine from one subscription to another one. I read a LOT on how to do that and it looks super complicated. At the end of the day, I found an easy 3 steps way to make it, so sharing on this blog ![Sourire](/assets/4401.wlEmoticon-smile_2.png)

# Step 1: Move the Azure VM from one Blob storage to another one

In Microsoft Azure, when you have a blob  storage, it can be attached only to 1 subscription. You can of course have multiple storage attached into a subscription. So first step is to move the VHD which is used by the VM.

For this, I used the excellent CloudBerry Explorer for Azure which you can download for free [here](http://www.cloudberrylab.com/free-microsoft-azure-explorer.aspx). After installation, just register for free and you’re good to go. 

You’ll need to add your 2 blob storage, the one you want to move the VHD from and the one you want to move the VHD to.

![image](/assets/0456.image_79A71CD9.png)

To find the name of the storage and the key, just go into the Azure management console and select Manage Access Keys, you’ll get the info you need to setup both accounts.

![image](/assets/7607.image_7DB3FB5C.png)

Once setup, you can now have a view like this:

![image](/assets/7711.image_584C989F.png)

Stop your VM and you’re good to copy/paste your VM from one storage to another.

# Step 2: Create a Disk from VHD

In the management console, go to Virtual Machine then Disks

![image](/assets/2388.image_72AF1C67.png)

then Create

![image](/assets/0250.image_0251B82A.png)

fill a name, select the VHD from the storage you just moved your VHD file to.

![image](/assets/2086.image_5D53F7B0.png)

# Step 3: Create the VM from the Disk

Go to the Virtual Machine instances

![image](/assets/6648.image_5AADCCB0.png)

then create a New

![image](/assets/0407.image_1F5CF7A3.png)

select From Gallery

![image](/assets/8360.image_0588C174.png)

and go to Disks to select the disk you just created.

![image](/assets/0383.image_5C148033.png)

And you’re good to go to run your VM!

