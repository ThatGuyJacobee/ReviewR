# ReviewR
This was my A Level Computer Science Project. First time using C# as well as Microsoft's UWP platform for app development. Received an A* grade for my NEA project.
The app consists of two main modules, the recommendation and review systems which were the main target during the application development. 

Note: Any API tokens have been reset and removed. 

Keep in mind the application won't function as the DB and tokens have been deprecated by me (follow config below if you would like to get it to work yourself, albeit without any data). This is more just to preserve the code and open source it if anyone wants to use anything :)

---
# Features
- Email/Password & Google Account OAuth2 Sign-in.
- Review up/down-voting.
- Game recommendation algorithm which suggests games that are likely to match your likings.
- Use of IGDB game database system (which is constantly updated with new games).
- User profiles which display a range of information including current reviews.
- Game Hubs which display key information about a game including socials etc.

---
# Configuration
There are a few changes you would need to do if you want the app to work for yourself (since I deprecated my database and reset tokens).

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
