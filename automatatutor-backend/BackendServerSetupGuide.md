# automatatutor-backend Server Setup and Deploy

The following section explains how to setup a Windows Server running the AutomataTutor backend and how to deploy the current version via WebDeploy.

## Prerequisites

* On your local machine you need VisualStudio with a working version of the backend.

* You need a Windows Server that is reachable under a given IP (let's say 1.1.1.1) and an open TCP Port (let's say 54321) for the service and another open TCP Port 8172 for "Web Deploy".

One could use Amazon Web Services (AWS). They offer a free 1 year option with limited computing power. 
After account creation login to the EC2 console. There choose the nearest server location and launch a prebuild Windows 2016 Server.
(Make sure to use a t2.micro instance to stay in free tier.) Now you need to change/create a security group that allows inbound TCP requests on the two ports 54321 and 8172.
Connect to the newly created instance (e.g. via the remote desktop file).

## Step 1 (IIS 8 with .NET Framework 4.5)

Follow the steps for "Setting up IIS 8.0 with support for ASP.NET 3.5 and ASP.NET 4.5" in https://docs.microsoft.com/en-us/iis/get-started/whats-new-in-iis-8/iis-80-using-aspnet-35-and-aspnet-45
Make sure to add both .NET 3.5 and .NET 4.6.  
You can stop before "Exploring the IIS 8.0 Installation"

## Step 2 (Web Deploy)

Install the Microsoft Web Platform Installer.
Start it and search for "Web Deploy" and install "Web Deploy 3.5 without bundled SQL support (latest)".  
(Web Deploy 3.6 instead of 3.5 should work too, but wasn't tested...)

## Step 3 (Windows Firewall)

Go to the windows firewall advanced settings and add a new inbound port rule for port 54321 allowing all requests.

## Step 4 (IIS Setup)

Go to the IIS Manager (Start -> Server Manager -> Tools -> Internet Information Services (IIS) Manager).

In the Connections Panel right click "Sites", then "add a Website".

As name put "AutomataTutor".  
Create a location for the physical path (e.g. "C:\inetpub\wwwroot\AutomataTutor").  
In Binding use: "http", "All Unassigned" and port "54321"  
Leave Host Name empty  
Click OK.

Now right click on the newly created website "AutomataTutor" and click "add application".

Alias: "AutomataTutor"  
Application pool: "AutomataTutor"  
Physical path: same as for the website
Click OK.

In the Connections panel click on "Application pools" -> right click on "AutomataTutor" -> "advanced settings".  
There change "Enable 32-Bit Applications" to True.

## Step 5 (Deploy)

(inspired by : https://github.com/ServiceStack/ServiceStack/wiki/Simple-Deployments-to-AWS-with-WebDeploy)

On your local machine, right click on the project "WebServicePDL", then on "publish".  
Create a new IIS Profil for IIS (Web Deploy).  
Server: "1.1.1.1" (or the servers IP adress)  
WebsiteName: "AutomataTutor" (this needs to be exactly the name you used above)  
Username: Administrator  
Password: *****  
Configuration: "Release - ANY CPU"

Now deploy the project.

You should be able to see an overview with the available requests for the backend if you visit  
"1.1.1.1:54321:54321/Service1.asmx".




