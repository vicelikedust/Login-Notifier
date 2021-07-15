# Login-Notifier
Sends an Email with a Webcam photo and multiple screenshots when a User logins in or unlocks a Windows Session

I wanted to have a tool that would notify me when my work computer had been logged into or unlocked and take a picture with the webcam as well as multiple screenshots so I tried to make my own!

Upon first run the application will register itself in the Windows Task Scheduler with 2 triggers <br/>
  * 1.) At Log On
  * 2.) On Workstation Unlock
<br />

Modify the creds.json with your details and place it in the application root.

