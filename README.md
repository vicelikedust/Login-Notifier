# Login-Notifier
Sends an Email and Webcam photo when a User logins in or unlocks a Windows Session

I wanted to have a tool that would email me when my work computer had been logged into or unlocked and take a picture with the webcam so I tried to make my own!

Upon first run the application will register itself in the Windows Task Scheduler with 2 triggers <br/>
  * 1.) At Log On
  * 2.) On Workstation Unlock
<br />

Modify the creds.json with your details and place it in the application root.

