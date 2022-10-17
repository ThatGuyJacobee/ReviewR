# ReviewR
This was my A Level Computer Science Project. First time using C# as well as Microsoft's UWP platform for app development. Received an A* grade for my NEA project.
The app consists of two main modules, the recommendation and review systems which were the main target during the application development. 

Note: Any API tokens have been reset and removed. 

The main app can be downloaded from: [Final Build](https://github.com/ThatGuyJacobee/ReviewR/releases/tag/v1.0-final). The app will work as long as I keep the database running (and I most likely will for the long-term). Otherwise, you can also self-host it and change it to how you like! I've decided to open source it just in-case anyone wants to use it and to preserve code :)

---
# Features
- Email/Password & Google Account OAuth2 Sign-in.
- Review up/down-voting.
- Game recommendation algorithm which suggests games that are likely to match your likings.
- Use of IGDB game database system (which is constantly updated with new games).
- User profiles which display a range of information including current reviews.
- Game Hubs which display key information about a game including socials etc.

---
# Installation
1) Download the .zip release folder and unzip it.
2) Install the certificate that's within the folder: Open the certificate -> Press "Install certificate..." -> Select "Local Machine" as the store location -> Accept the UAC prompt -> Select "Place all certificate in the following store" and press "Browse..." -> Pick "Trusted Root Certificate Authorities" from the menu and press "OK" -> Press "Next" and finally "Finish".
3) Lastly, run the .msixbundle file and press "Install". The app will then install and open. That's all, enjoy!


---
# Self-hosting
This repo is now open-source for anyone to use. If you wish to host your own database, including tokens etc. to self-host the app or reuse parts of it, you must configure a few things. Keep in mind of course this is for your own entire use, meaning data won't be synced with my database so it's fresh hosting for you.

1) Head over to your App.xaml.cs and edit line 32 with your own database connection string following the format which is uncommented.
2) In the file, also change line 53 with your own IGDB token which can be taken from: https://api-docs.igdb.com/#account-creation.
3) Once again in that file, change line 66 and 71 following the uncommented format, with your client id and secret.
4) Finally, head over to LoginPage.xaml.cs and change line 226 with your own clientID which can be taken from: https://console.cloud.google.com/ 
You MUST create an OAuth 2.0 Client ID with the Type set to "iOS" (limitation of UWP) and you MUST set the Bundle ID to "reviewr.oauth2". Then copy the link it provides into the field.
5) Finally, compile the code in Visual Studio (or test first in the built-in VS editor) and install the app and it will work fine.

---
# License
Do not attempt to use this as your work for your A Level coursework (or you will probably face disqualification) as it's already been graded!

[Apache © ThatGuyJacobee](./LICENSE.md)
