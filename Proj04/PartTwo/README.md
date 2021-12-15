# DOSP_Proj04 Part II

Please read this cute note => In project of our group, though it was a hard time, the realization of it is by websocket. If there is any possibility, is there a little bonus for this cute group?

## Group Members

1. UFID: 83981600, Email: yingjie.chen@ufl.edu
2. UFID: 84714011, Email: wei.he@ufl.edu

## How to Run

### Windows 10 or Up

1. Environment Setup: 

   ​	.NET SDKs

   ​	.NET runtimes
   
   ​	Various browsers. If it is the chrome, update to the newest version.

2. Decompressing the .RAR file: PartTwo.rar

3. Opening Command Prompt and Going to the folder obtained from the second step

4. Run the command line "dotnet run"

<img width="715" alt="image" src="https://user-images.githubusercontent.com/28448629/146088560-af0ec4a4-cef7-4ac4-8a53-77275154ce3a.png">


5. When the program starts, showing "Now listening on: http://localhost:5000  Now listening on: https://localhost:5001", open one browser and enter in "localhost:5000".

<img width="867" alt="image" src="https://user-images.githubusercontent.com/28448629/146088865-8b4f5081-6f7a-4217-8b7b-a63e95ebfa57.png">


6. Sign in with a signed name and passport, or sign up with new user name and passport.

- Sign in & Sign up

<img width="638" alt="image" src="https://user-images.githubusercontent.com/28448629/146090116-98d1ff9a-920a-4d5d-bf62-31a92522d8ab.png">

- Operations

<img width="653" alt="image" src="https://user-images.githubusercontent.com/28448629/146113561-27b2c433-f00a-4f0f-ac1f-5b095ae1d11a.png">


7. After signning in, can do operations like following, receiving following tweets, retweeting, querying tweets by Hashtag/Mention/Following.


### Result Description

- Followed tweets, querying results will all shown on the webpage in the blank. Details about the results will be described in the section of "experiments results". 

<img width="659" alt="image" src="https://user-images.githubusercontent.com/28448629/146134632-b1455591-5b9b-4bb5-b539-3fe4e3985f08.png">


## Architecture

The project binds the frontend and backend up. As is described in the picture below with what not described, our program uses RPC, Json, websocket, template engine, which are either coded in a single file or contained in different processes as functions.

- RPC: a protocol of sending messages to receiving and reacting. Here it is in a single file. It is a client/server mode. Here it calls functions in another file "RealTimeServer.fs" connecting with Database to complete the function of signing in and signing up in the "AccountPageProcess".

- Json: To send messages between frontend and backend which uses different messages types that if there is not a transportable form, the received messages cannot be processed. So here we use the serialization and deserialization of Json.
   - Serialization: convert the object into a string.
   
   <img width="158" alt="image" src="https://user-images.githubusercontent.com/28448629/146115673-30b7ca49-6c39-45cd-ae47-521fa1e6edec.png">

   - Deserialization: inverse the process of Serialization, that convert the string into the object.
   
   <img width="383" alt="image" src="https://user-images.githubusercontent.com/28448629/146114563-92b6229c-b8be-46ca-a179-92a5ed734728.png">

- Websocket: is used to connect the main ports between websockets servers, and after that the data can be transported on this connection.
 
 <img width="511" alt="image" src="https://user-images.githubusercontent.com/28448629/146115487-2d729560-6556-47a4-98a2-0da22339cecb.png">

 <img width="355" alt="image" src="https://user-images.githubusercontent.com/28448629/146115517-9ce96c7c-f46d-480f-a9a6-f69da0ab0ca4.png">


- template engine: traverse the whole nodes and ask for the recognized labels. Make the engine with the lables.

<img width="473" alt="image" src="https://user-images.githubusercontent.com/28448629/146117813-65c9d00f-1f74-4e62-ac06-205de73f25e2.png">


A user signing in to the system has two labels here: user name + token. 
-  A token is a configuration for a user in the system. When a user login, the server will allocate a token to it. Each operation by the user will contains this token. If server doesn't find a token in the token list, it will clear the cookies.

<img width="542" alt="image" src="https://user-images.githubusercontent.com/28448629/146118281-2a76957d-544e-4a76-ac71-f9883b682bb6.png">

- Attention: If enter "localhost:5000/main" and want to do some operations directly, tha page will jump to the login page. It is because tokens are used here.


<img width="922" alt="image" src="https://user-images.githubusercontent.com/28448629/146102679-fe7f3c16-1ffe-44a6-bdc6-8c49598562ca.png">

Here the data layer we have done in the PART I project is still the same set int the backend. The introduction of this project will be splited into two parts: frontend, backend.

### Frontend

The frontend files are all contained in the folder "templates". It contains five parts, including "AccountPageProcess" which contains processes for frontpage entering and processing receiving. "MainPageProcess" containes processes for corresponding to the operation page after login. "Account" and "Main" are the html file for rendering the page. A  templates which use template engine to substitute labels with required data for this separating purpose to generate webpages with templates.
It needs to clarify that, the "AccountPageProcess" is connected to the RPC server which is 

<img width="158" alt="image" src="https://user-images.githubusercontent.com/28448629/146104608-7209b059-cd38-4a77-accf-edc829753eef.png">

to send the operation message and get the processing method back.

- Account.html
   - In the Account.html file, it defines the login page with "SignIn" and "SignUp". 
- AccountPageProcess.fs
   - Get the sign in and sign up information.
   - Send the information to RPC and get serialized information back.
   - Show the results of operations.
   - Judge if signing in or signing up success
      - Success, sign the status and show "sign in" or "sign up"
      - Not success, set error information to the status. 

<img width="553" alt="image" src="https://user-images.githubusercontent.com/28448629/146139294-73e9a669-52b3-4b22-b921-902574a26d67.png">


- Main.html
   - Defines the page after signing in.
   - Contains blanks for following, retweeting, querying, and shows the automatically delivered followed tweets.
- MainPageProcess.fs
   - Send operating messages to "RealTimeServer" from websocket.
   - An example:
   
   <img width="512" alt="image" src="https://user-images.githubusercontent.com/28448629/146124216-7a74e96e-8b95-499d-b15a-d7784a055d16.png">

   - Get messages from websocket (in fact the real processing part is "RealTimeServer", and the messages are just sent by the websocket) with asychronization. 
   - Show the message on the webpage.
   - Example:
   
   <img width="383" alt="image" src="https://user-images.githubusercontent.com/28448629/146124373-e4e073ac-943c-4af9-9591-4c150823f828.png">


<img width="551" alt="image" src="https://user-images.githubusercontent.com/28448629/146139003-32a2f393-0f47-4b48-9e68-bdfa571bc7b8.png">


- Templates.fs
   - Use template engine to substitute labels with required data.
   - Use template to generate webpages by
   
   <img width="665" alt="image" src="https://user-images.githubusercontent.com/28448629/146132461-cbd050d8-b5bb-423e-9b0c-5f38c2c36643.png">


In these frontend files, there are always some labels as "[<JavaScript>]" before modules.
- To make these codes work in the modules and methods that are labeled with "[<JavaScript>]".
- The files converted from those using F# cannot be used in them directly.


### Backend

In the backend, it contains files as below.

<img width="125" alt="image" src="https://user-images.githubusercontent.com/28448629/146135142-70016418-8d9c-4852-9cdb-b56283b8bcde.png">


- "DTO.fs": defines some transportable objects.
   - C2SMessage: Message object from client to server.
   
   <img width="268" alt="image" src="https://user-images.githubusercontent.com/28448629/146135353-1a29388f-222c-4635-aff3-bfb556fcf745.png">

   - S2CMessage: Message object from server to client.
   
   <img width="328" alt="image" src="https://user-images.githubusercontent.com/28448629/146135415-cc3100c1-ff0f-4271-82f5-3b395736b0c0.png">

   - Others like UsernameToken, FollowInfo, TweetInfo, FollowingNewTweetInfo.
   
   <img width="149" alt="image" src="https://user-images.githubusercontent.com/28448629/146136305-40ede159-56ea-416d-b4f7-69022df05034.png">
   <img width="139" alt="image" src="https://user-images.githubusercontent.com/28448629/146136337-bba1f515-b76f-4f66-8292-8dbb0db3bd51.png">
   <img width="140" alt="image" src="https://user-images.githubusercontent.com/28448629/146136368-33b93d38-6973-45c7-8dd4-2b0a0257e7f5.png">
   <img width="190" alt="image" src="https://user-images.githubusercontent.com/28448629/146136383-5b34dfdf-17f9-4087-b554-4e36b3e9c6f2.png">


- "Entities.fs": defines objects of "Account", "Follow", "Hashtag", "Tweet", "TweetMention", "TweetHashTag"
   - These are the objects that communicates with database.

- "RPCServer.fs": operation on the database based on the information coming from frontend "AccountPageProcess". 
   - SignUpProcess: 
      - set a new account with username and password into Database accountDAO
   - SignInProcess: 
      - get account by username from DatabaseDAO
      - if account doesn't exist, return Json.Serialize(loginSign=false, token="")
      - if account exist, return Json.Serialize(loginSign=true, token=newToken)
      - Call function "addNewUsernameToken()" in RealTimeServer.fs to add this new online user in

- "RealTimeServer.fs": functions of usertoken list or websocket services. 
   - addNewUsernameToken: username - token list
      - if the user doesn't exist searching by username, add username - token pair in
      - if username exist but token doesn't equal, remove the old pair and add in the new pair (which means the user logins in again).
   - checkLogin: check if the login is legal
      - if the username exists and token is exact the current token, return true
      - else, return false
   - buildTweetsString: build tweets content into Json.Serialize
   - Start(): StatefulAgent<S2CMessage, C2SMessage, int>
      - this is the websocket using.
      - Receive the messages from clients
      - communicate with and operate database
      - return the processed message
 
- "Startup.fs": start some services and build the environment
   - Build database
      - find database files, connect to SQLite and create the database.
   - Build webhost
      - create default builder of websocket and build
   
   <img width="479" alt="image" src="https://user-images.githubusercontent.com/28448629/146140249-8bf01231-b990-4d22-8325-b3bc869bb9a6.png">

- "Site.fs": contains frontier templetes that are created. "Account" and "Main"

- "SimpleTwitter.fsproj": contains all files in the project, and includes SQLite and WebSharper services.


## Experiment

### Result

1. Sign up. At the same time, a user can only login on one device or webpage.
   
   -"littleboy"
   
   <img width="531" alt="image" src="https://user-images.githubusercontent.com/28448629/146092124-6ad007a6-00ab-49ff-a5de-230110a28676.png">

   -"binarysearchtree"

   <img width="512" alt="image" src="https://user-images.githubusercontent.com/28448629/146092307-eec8f53d-d70e-4226-92d0-c6721d18c742.png">
   
   - sign up again
   
   <img width="507" alt="image" src="https://user-images.githubusercontent.com/28448629/146198868-345f5b97-8cdb-441c-91bf-09c94972386c.png">


2. Sign in after signing up 
   
   - "littleboy"
   
   <img width="658" alt="image" src="https://user-images.githubusercontent.com/28448629/146140942-70bd1693-c0ed-4020-8349-9e461f059258.png">

   - "binarysearchtree"

   <img width="655" alt="image" src="https://user-images.githubusercontent.com/28448629/146141021-653523e0-7dd4-4881-9d7a-dd45a1c8a1ae.png">

   - wrong username or password
   
   <img width="512" alt="image" src="https://user-images.githubusercontent.com/28448629/146198675-590be1e5-0790-4ac9-941b-757734942ae4.png">
   
   - username that doesn't exist
   
   <img width="506" alt="image" src="https://user-images.githubusercontent.com/28448629/146199185-c887d426-01d4-4331-a9eb-d422381666f3.png">
   
   - lost username or password
   
   <img width="507" alt="image" src="https://user-images.githubusercontent.com/28448629/146201117-cfe7b5e7-9fb2-4e1b-98a2-266765db3cb3.png">

   - go to page "localhost:5000/maint" directly
   
   <img width="680" alt="image" src="https://user-images.githubusercontent.com/28448629/146199343-ae768a9f-8940-4047-8f21-37ba67276ec4.png">

   
3. Follow someone
   
   - "little boy" follows "binarysearchtree"
   
   <img width="653" alt="image" src="https://user-images.githubusercontent.com/28448629/146141126-4c182b38-21f2-43ae-8b35-0024c914543a.png">

   - "little boy" follows "binarysearchtree" again
   
   <img width="676" alt="image" src="https://user-images.githubusercontent.com/28448629/146198530-f7ee0cb5-1c8e-4a33-871a-a4c34f977bd1.png">

   - "ab" follows someone that doesn't exist
   
   <img width="647" alt="image" src="https://user-images.githubusercontent.com/28448629/146199691-ba9ed3b0-a327-4fda-a364-49a1544fff76.png">

   
4. Tweet
   
   - "binarysearchtree" posts a tweet
   
   <img width="656" alt="image" src="https://user-images.githubusercontent.com/28448629/146141235-f5f5cd5c-70be-417d-b8f5-00b3f082ae61.png">

   - "binarysearchtree" regrets and then clears what he has written in
   
   <img width="656" alt="image" src="https://user-images.githubusercontent.com/28448629/146200522-077c5f44-ed0c-472d-bf5d-5a4b71dc6ef4.png">

5. Show the following tweet

   - "littleboy" receives the tweet by "binarysearchtree"
   
   <img width="653" alt="image" src="https://user-images.githubusercontent.com/28448629/146141350-992ebe26-207f-4188-9d12-4c94bf70caf8.png">

6. Retweet
   
   - "littleboy" retweets the tweet "search 2" by "binarysearchtree"
      - Get the tweet ID from about "Real Time Tweet from Following"
   
      <img width="440" alt="image" src="https://user-images.githubusercontent.com/28448629/146141654-f4dd6629-58d7-4348-bd87-cb8b07eed6b8.png">

      - Input the "tweet ID"
      
      <img width="251" alt="image" src="https://user-images.githubusercontent.com/28448629/146141708-0fe45a99-8d32-4ade-a6ef-3e3f0565c6af.png">

      - Input the content of the retweet
      
      <img width="267" alt="image" src="https://user-images.githubusercontent.com/28448629/146141769-26fb6219-e13a-4703-8835-d3072ca7ff72.png">

      - Click "Retweet"
      
      <img width="476" alt="image" src="https://user-images.githubusercontent.com/28448629/146141810-c378f739-3454-4486-8b92-0f9f9f35409d.png">


7. Query
   
   - Query tweets subscribed to:
   
   <img width="634" alt="image" src="https://user-images.githubusercontent.com/28448629/146142184-62dc488e-f17c-427f-86a2-5ce3c18ca215.png">

   - Query a special hashtag:
   
   - Query mentioned tweets:
   
   <img width="655" alt="image" src="https://user-images.githubusercontent.com/28448629/146201664-fd664fb3-3e76-4b09-bf6f-694c1bb4ba6a.png">

   
   - multiple mentions, use ';' to seperate each mention.
   
   
