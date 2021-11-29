# DOSP_Proj04

## Group Members

1. UFID: 83981600, Email: yingjie.chen@ufl.edu
2. UFID: 84714011, Email: wei.he@ufl.edu

## How to Run

### Windows 10

1. Environment Setup: 

   ​	.NET SDKs

   ​	.NET runtimes

2. Decompressing the .RAR file: PartOne.rar

3. Opening Command Prompt and Going to the folder obtained from the second step

4. Run the command line "dotnet run [numberOfClients]"

![415612026495964824](https://user-images.githubusercontent.com/28448629/140237180-54ef7eea-69b9-4937-8cf2-0ec9d1d5fd02.png)


5. Wait and observe the result 


### Result Description

- The results of subscribers, querying and others are printed on the screen in the termination. Details about the results will be described in the section of "experiments results". 

<img width="872" alt="image" src="https://user-images.githubusercontent.com/28448629/143763354-9740d1f8-f76d-4af1-9972-d540130292a1.png">


- A file of statistic data of number of subscribers and tweets amounts in folder "output" is newly created, named "statistic.txt" each time restarting the programming. 

<img width="467" alt="image" src="https://user-images.githubusercontent.com/28448629/143749845-f55efb96-ed65-4b10-bf29-bdf5e33ec250.png">


   - In this txt file, one line of the output result shows like:

<img width="562" alt="image" src="https://user-images.githubusercontent.com/28448629/143752091-3efa1053-5fc5-490a-89b9-6a8782714f8a.png">

   - ID number is listed in the left, followed with the user names, number of followers, and tweet posting rates.
   - There only screen shotting a counting of the first 20 users. The other users are listed below but not screen shotting in.



## Architecture

The project uses three-tier architecture to build the twitter web simulation, which contains presentation tier, logic tier and data tier. 
- presentation tier: web server, analyzing protocols
- logic tier: mongrel rails server, to abate the work load
- data tier: mysql of users data. Here we use SQLite imbedded in the program for convenience and speed consideration.
Here we combined the presentation and logic tiers to one tier, so that the whole program has two part: 1. data tier, using SQLite. 2. presentation and logic tier, including engine which processes user register and query, and user actors.

<img width="412" alt="image" src="https://user-images.githubusercontent.com/28448629/143783350-c28906f6-753b-4631-95ff-098e80d1bb15.png">

As the subject of the project has two parts, users and tweets, we define 
- user with their ID(number between 1 and the maximum of user amount), name(a random string)
- tweet ID(the sequence number of one piece of tweeet),  and retweet ID of one tweet(-1 for not been retweeted, with positive integers representing retweeted ID)
- hashtag(topic of a tweet, which can be created by a random user)

### Data Tier

In the data tier, we decided six relationships tables in SQLite to define the user actions, which is defined in the path "resources\\create_table.sql". Their basic CUID operations of the tables are defined in the namespace DAO, which includes "AccountDAO.fs", "FollowDAO.fs", "HashtagDAO.fs", "TweetDAO.fs", "TweetMentionDAO.fs", "TweetHashtagDAO.fs". Each time running the program, the programmed databade file "tweet_sys_db" is newly created. 
- Account: User ID, User name
- Follow: User name, Follower name
- Hashtag: User ID, Topic, Creator(User name)
- Tweet: User ID, Creator(User name), Topic(Hashtag), Retweet ID
- Tweet_Mention: Tweet ID, User name
- Tweet_Hashtag: Tweet ID, HashtagID

<img width="538" alt="image" src="https://user-images.githubusercontent.com/28448629/143786338-00bd2be0-8dd1-4c61-aec1-ffcde3a4f3ec.png">


### Presentation & Logic Tier

Above the data tier, the defination of user actions and corresponding behaviors of the engine are described in this layer.

#### Frame & message types
It contains the message type in folder "Msgs", which contains "TweetEngineMsgs.fs" to define message types in Engine, "ClientMsgs.fs" to define the message type in clients actors, and "RandomControllerMsgs.fs" to define message types in the random controller.

- "TweetEngineMsgs.fs": Below only list some important message information types here
   - RegisterInfo: User name
   - RegisterSuccessInfo
   - LoginInfo / LogoutInfo: User name
   - SubscribeInfo: Follow, Follower
   - PostTweetInfo: User name, Content(a random string), Number of mentions, Number of existing hashtags, Hashtags, Retweet flag.
   - QueryFollowInfo: Follower
   - QueryMentionInfo: User name
   - QueryHashtagInfo: User name, Hashtag

- "ClientMsgs.fs": Below only list some important message information types here
   - SimpleTweetDTO(gives the tweet and retweet ID): TweetID, RETWEETID
   - TweetDTO: TweetID, Creator(User), Content, Mentions, Hashtags, RetweetID
   - QueryFollowResult / QueryMentionResults: TWEETS(List<SimpleTweetDTO>)
   - PostTweetOperation: RetweetFlag
   - DeliverTweetOperation: TweetDTO
   - DeliverTweetsOperations: TWEETS(List<Tweet>)

- "RandomControllerMsgs.fs"
   - StatisticsStatusEntiy: User name, User ID, Number of Follower, Post Rate
   - StatisticsStatusResult: CLIENTS_STATUS(List<StatisticsStatusEntity>)

#### Actors: Engine, Clients & RandomController

In folder "Actors", it contains actor operations of EngineActor, of ClientAcotor and of RandomControllerActor in "TweetEngineActor.fs", "ClientActor.fs", "RandomControllerActor.fs". There is also a Printer Actor, which will not be introduced formally.

- Engine Actor: Connecting with SQLite to acquire data and send data to clients.
   - When receiving a "RegisterInfo" message, insert this new Info into Table ACCOUNT, then send register success info. 
   - When receiving a "Login / Logout" message, print the login status message.
   - When receiving a "SubscribeInfo" message, insert the follow and follower message into Table FOLLOW and print the message.
   - When receiving a "PostTweetInfo" message, select random clients as equal number as mention numbers and add these clients name into mentions list. Add existing hashtags into the list hashtags. Insert this new tweet into TABLE TWEET and get the tweet ID from the table. Add the mentions list to TABLE TWEET_MENTION. Add hashtags list into TABLE HASHTAG(if not exists, create new hashtag; otherwise insert it into an existing hashtag). If it's a retweet tweet, update the retweetID in the TABLE TWEET. Finally, get  followers and deliver the tweet to them. Print the message.
   <img width="599" alt="image" src="https://user-images.githubusercontent.com/28448629/143789292-2f3c9f0a-dc18-4778-a45b-b56cd80446ef.png">
   
   - When receiving a "QueryFollowInfo" message, query follows from FOLLOW. Then query Tweets of FOLLOWS from TWEET. Change the tweets form and send them to the querying follower.
   - When receiving a "QueryMentionInfo" message, query tweetIDs from TWEET_MENTION by user name. Then gets tweets from TWEET. Change the form and send them to the querying client.
   - When receiving a "QueryHashtagInfo" message, query hashtag from HASHTAG. Then query tweets by the hashtag from TWEET. Change the form and send them to the querying Client.
   - When receiving a "StopSImulationInfo", do clients stop simulation.
   - When receiving a "StatisticsStatus" message, do the statistic work(querying number of clients and tweets and traverse accounts in ACCOUNT. Write the results into "statistics.txt".

- Client Actor: Is controlled by a flag variant "login". "login = true", all the operations can implement; else the clients will do no actions.
   - When receiving "LoginOperation" or "LogoutOperation", set "login" to be "true" or "false".
   - When receiving "RegisterOperation", send RegisterInfo with name to the tweetEngine.
   - When receiving "PostTweetOperation", if "login = true", set numberOfMentions = -1. If there are more than 11 clients, give a random mentioned number in range [0, 9] to numberOfMention; else give a random mentioned number in range [0, numberOfRegister]. Give a random number of new hashtag between [0, 4], and a number of existinghashtags between [0, 4]. Assign a random string as content. Assign a string within length of 20 as a new hashtag and add new hashtags to HASHTAG. Send the posting tweet message to Engine.
   <img width="370" alt="image" src="https://user-images.githubusercontent.com/28448629/143793167-1835e9f5-42c2-4759-afef-1fa63f4999ba.png">

   - When receiving "DeliverTweetOperation", if login is true, print user name and it gets a new tweet.
   - When receiving "QueryFollowOperation", if login is true, send the QueryFollowInfo with user name to Engine. When receiving "QueryFollowResult", print the querying follows' tweets result.
   - When receiving "QueryMentionOperation", if login is true, send the QueryMentionInfo with user name to Engine. When receiving "QueryMentionResult", print the querying mentioned tweets result.
   - When receiving "QueryHashtagOperation", if login is true, send the QueryMentionInfo with user name (and hashtag) to Engine. When receiving "QueryHashtagResult", print the querying tweets result with the hashtag.
   - When receiving "SimulationOperation", simulate possible operations by a actual user with asynchronization. When it's "logout", change the status to "login = true". Otherwise, if "login = true", do "logout", "query" and so on. Give a random number between [0,6] to define the operations.
      - 0 : do logout operation. Possibility = 1/7
      - 1 || 2 : do QueryFollowOperation. Possibility = 2/7
      - 3 || 4 : do QueryMentionOperation. Possibility = 2/7
      - 5 || 6 : do QueryHashtagOperation. Possibility = 2/7
      - After doing the operation. Sleep for 1ms to release the occupation of the thread and give the other actors chances to operate, avoiding one actor occupies one thread for much time.
   - When receiving "StopSimulationOperation", simulationWork will be set to be "false". Print "user name  stop simulation" and send "StopSimulationInfo" to Engine.

- Random Controller Actor: do tests to give a simulating enviroments of random controller for all users. 
   - Get stabilize message and stabilize
   - When receiving "RegisterTest", give all client in clients list the RegisterOperation.
   - When receiving "LoginLogoutTest", doing the belowing operations with asynchronization: 
      - give a random number in range [1, 9]. If the number between [1, 7], change the "Login" and "Logout" status. Possibility = 7/9
      - sleep for 5000ms after a loop of operation for all clients.
   - When receiving "ClientPostTest", choose a random client starA and a random client starB. For all clients do SubscribeOperation to starA. StarA do PostTweetOperation with false while starB with true.
   - When receiving "QueryTest", for starA and starB do SubscribeOperations for all clients. All clients will start a new QueryFollowOperation.
   - When receiving "StartSimulationWithZipf", 
      - assign all other users to subscribe the first user
      - assign 1/2 users to subscribe the second user
      - assign 1/3 users to subscribe the third user
      - ......
      - For each clients do SimulationOperation. After the operations, sleep for 500 ms.The sleep time longer, the tweet or operation numbers larger.
      - Stop the simulation.
    - When receiving "StatisticsStatusResult", output the result to file "output/statistics.txt". When doing this, all operations have stopped for 500ms.
   
- Print Actor:
  - Print the actor path.  
 
<img width="481" alt="image" src="https://user-images.githubusercontent.com/28448629/143792669-e08cf280-9ecc-4a91-a9b3-3b2756b06709.png">


#### Functionalities

1. Register Account

   
2. Send tweet

   
3. Subscribe to user's tweets

   
4. Re-tweets

   
5. Allow querying tweets

   
6. If the user is connected, deliver the above types of tweets live (without querying)
   
   -There are two methods to realize the delivering without querying.
      - One is when a user logins, the Engine finds and sends the new tweets to this user automatically. However, this method contains additional steps of database operations which consumes much more time.
   <img width="358" alt="image" src="https://user-images.githubusercontent.com/28448629/143793605-10623910-1715-4896-8fbe-b3639bfb0ec4.png">

      - The other is that delivers the tweets to all its followers. When the follower is connected, the tweet received in mailbox will be printed without querying. However, if the user is not online, the client actor will do nothing even if the tweets has been sent to its mailbox. This method is realized by adding in a control variant "Login".
   - Considering the convenience and speed of the algorithm, we here choose the second method in our codes.



## Experiment

### Result

1. Since we use SQLite in our experiments, running speed of the program and capacibility of users number are supposed to be very high. Here we tested a scale of 5000 users about which it consumes some time to set the subscribers and tweets and print all the results on the powershell. There is no limitaton of the scale therotically. When running on one machine, the limitation is bound to the machine performance and time. And if this project can be implemented on different machines, there will be no upper limitation on the number therotically.

<img width="872" alt="image" src="https://user-images.githubusercontent.com/28448629/143773599-ec1991d8-ef8d-4442-9aa4-5549c2fde95d.png">

2. There are also simulations of login in and login out of users. The login in informatioin of one user is printed on the screen.
-login in:
<img width="452" alt="image" src="https://user-images.githubusercontent.com/28448629/143777089-b7f7ade7-a1d9-4d50-a0ec-ac5fb64d051d.png">

<img width="467" alt="image" src="https://user-images.githubusercontent.com/28448629/143777096-803e5883-604d-48a7-9ba7-0f5177f64946.png">

-login out:
<img width="437" alt="image" src="https://user-images.githubusercontent.com/28448629/143785149-5a81ddf3-20e9-4ec3-bccb-9eade717c444.png">

<img width="457" alt="image" src="https://user-images.githubusercontent.com/28448629/143785168-593849ec-0259-4599-b549-028da4fdb522.png">

3. If a user queries one hashtag, mentioned ID, or subscribed tweet, the results will be printed on the screen. The form { 30(5), 18 } means that there are two results corresponding to the query, one is the post tweet 30 which is a retweet of tweet 5, the other is tweet 18. "[akka://TweetSimulator/user/NfSWWTUVYv4h62F]" at the beginning shows the querying operation by user who.
- Query tweets subscribed to:
<img width="730" alt="image" src="https://user-images.githubusercontent.com/28448629/143774584-a29036b8-0d6e-43d0-b122-64f6ad5c04f0.png">

<img width="857" alt="image" src="https://user-images.githubusercontent.com/28448629/143776999-67bb5073-aa8b-4928-a7e1-83da1ea12408.png">

- Query a special hashtag:
<img width="743" alt="image" src="https://user-images.githubusercontent.com/28448629/143774554-c38a91e8-b2ca-4ef5-b5b3-678be42b4ecf.png">

<img width="743" alt="image" src="https://user-images.githubusercontent.com/28448629/143776972-bf953c59-a976-4d3c-b218-8464b49e24bd.png">

- Query mentioned tweets:
<img width="555" alt="image" src="https://user-images.githubusercontent.com/28448629/143774638-30f14b64-9340-4dcb-bb2d-02687b5c27cb.png">

<img width="585" alt="image" src="https://user-images.githubusercontent.com/28448629/143777019-82a6536b-dba7-48ce-9d3f-f935e432bfd7.png">

4. A Zipf distribution on the number of subscribers and more subscribers, more tweets and retweets. The count of number of subscribers and tweet proportion of all users are listed in the path "output\\statistics.txt". One line in the file gives the user ID, user name, number of followers and how much proportion his number of tweets occupies in the total tweets.

<img width="571" alt="image" src="https://user-images.githubusercontent.com/28448629/143775072-6d4480f5-38f0-48e3-b632-ccc0b2a82245.png">
