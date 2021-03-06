# DOSP_Proj04 Part I

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

<img width="868" alt="image" src="https://user-images.githubusercontent.com/28448629/143818708-e2790947-63cb-4af0-a855-86fb447de60f.png">


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
- presentation tier: output and show the processing results.
- logic tier: defines some logical processes.
- data tier: among plenty of imbeded database, we use SQLite here to store the user and tweets data, for convenience and speed consideration.

Here we combine the presentation and logic tiers to one tier, so that the whole program has two parts: 1. data tier, using SQLite. 2. presentation and logic tier, including engine which processes user register and query, and user actors.

<img width="412" alt="image" src="https://user-images.githubusercontent.com/28448629/143783350-c28906f6-753b-4631-95ff-098e80d1bb15.png">

As the subject of the project has two parts, users and tweets, we define 
- user with their ID(number between 1 and the maximum of user amount), name(a random string)
- tweet ID(the sequence number of one piece of tweeet),  and retweet ID of one tweet(-1 for not been retweeted, with positive integers representing retweeted ID)
- hashtag(topic of a tweet, which can be created by a random user)

### Data Tier

In the data tier, we decided six relationships tables in SQLite to define the user actions, which is defined in the path "resources\\create_table.sql". Their basic CRUD operations of the tables are defined in the namespace DAO, which includes "AccountDAO.fs", "FollowDAO.fs", "HashtagDAO.fs", "TweetDAO.fs", "TweetMentionDAO.fs", "TweetHashtagDAO.fs". Each time running the program, the programmed databade file "tweet_sys_db" is newly created. 
- Account: User ID, User name
- Follow: User name, Follower name
- Hashtag: User ID, Topic, Creator(User name)
- Tweet: User ID, Creator(User name), Topic(Hashtag), Retweet ID
- Tweet_Mention: Tweet ID, User name
- Tweet_Hashtag: Tweet ID, HashtagID

In the normal database project, there are always foreign keys between table so that the tables can be related together. However, consider that in the further programming there may be deletions or modifications of current tables, we didn't add foreign keys in our database tables. But relations among tables do exist as below.
- ACCOUNT -- NAME := TWEET -- CREATOR; HASHTAG -- CREATOR; FOLLOW -- NAME ; TWEET_MENTION -- NAME.
- TWEET -- ID := TWEET_HASHTAG -- TWEETID.
- TWEET_HASHTAG -- HASHTAGID := HASHTAG -- ID.

<img width="482" alt="image" src="https://user-images.githubusercontent.com/28448629/143825496-e0f2463e-199b-486f-9de3-b9ff657dc721.png">


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
   - When receiving a "QueryHashtagInfo" message, query hashtag from HASHTAG. Using the hashtag to query tweetID from TWEET_HASHTAG. Then query tweets by the tweetID from TWEET. Change the form and send them to the querying Client.
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
   - When receiving "SimulationOperation", simulate possible operations by a actual user with asynchronization. When it's "logout", change the status to "login = true". Otherwise, if "login = true", do "logout", "post tweet", "query" and so on. 
      - Give a random number between [0, UserID-1]. If the random number equals to 0, the user will post a tweet. Possibility of posting a tweet = 1/UserID.
      - If the user is sure to post a new tweet, a new random number between [0, 1] will be decided with '0' representing it's a new tweet and '1' representing it's a re-tweet. Possibility of "it's a retweet" = 1/2.
      - Give a random number between [0,6] to define the operations except post a new tweet.
      - 0 : do logout operation. Possibility = 1/7
      - 1 || 2 : do QueryFollowOperation. Possibility = 2/7
      - 3 || 4 : do QueryMentionOperation. Possibility = 2/7
      - 5 || 6 : do QueryHashtagOperation. Possibility = 2/7
      - After doing the operation. Sleep for 1ms to release the occupation of the thread and give the other actors chances to operate, avoiding one actor occupies one thread for much time.
   - When receiving "StopSimulationOperation", simulationWork will be set to be "false". Print "user name  stop simulation" and send "StopSimulationInfo" to Engine.

- Random Controller Actor: do tests to give a simulating enviroments of random controller for all users. 
   - When receiving "StartSimulationWithZipf", 
      - assign all other users to subscribe the first user
      - assign 1/2 users to subscribe the second user
      - assign 1/3 users to subscribe the third user
      - ......
      - For each clients do SimulationOperation. After the operations, the controller waits for 500 ms, to let the clients do their operations automatically for 500ms. In this 500ms, there will be enough instances of tweets, subscribes, login/logout and so on to be observed (for testing convenience, we set the waiting time to be 500ms. It can be set longer to get more experiment data as you would like). The waiting time of the controller longer, the tweets and operation numbers larger.
      - Stop the simulation.
    - When receiving "StatisticsStatusResult", output the result to file "output/statistics.txt". Before doing this, all clients have run for 500ms (you can set this running time as you like) and then stopped.
   - In Random Controller Actor there are system tests, which are not the project functionality realization. These tests are listed below with some clarification. 
   - When receiving "RegisterTest", give all client in clients list the RegisterOperation.
   - When receiving "LoginLogoutTest", doing the belowing operations with asynchronization: 
      - give a random number in range [1, 9]. If the number between [1, 7], change the "Login" and "Logout" status. Possibility = 7/9
      - sleep for 5000ms after a loop of operation for all clients.
   - When receiving "ClientPostTest", choose a random client starA and a random client starB. For all clients do SubscribeOperation to starA. StarA do PostTweetOperation with false while starB with true.
   - When receiving "QueryTest", for starA and starB do SubscribeOperations for all clients. All clients will start a new QueryFollowOperation.
   
- Print Actor:
  - Print the actor path.  
 
<img width="481" alt="image" src="https://user-images.githubusercontent.com/28448629/143792669-e08cf280-9ecc-4a91-a9b3-3b2756b06709.png">


#### Functionalities

1. Register Account

   <img width="362" alt="image" src="https://user-images.githubusercontent.com/28448629/143794033-4df3b930-5c44-44e5-9cbc-cbbbfd902189.png">

2. Send tweet

   <img width="527" alt="image" src="https://user-images.githubusercontent.com/28448629/143795012-91231759-b691-4c3b-8b38-4de36d9a05bc.png">

3. Subscribe to user's tweets

   <img width="382" alt="image" src="https://user-images.githubusercontent.com/28448629/143794203-1ebe55e3-bc50-4809-b3ae-0149564014aa.png">

4. Re-tweets

   <img width="527" alt="image" src="https://user-images.githubusercontent.com/28448629/143794990-ac77c32f-7f99-4e24-93a8-33034c976520.png">
   
   - The re-tweets process is built in sending tweets, so that its process diagram is the same with send tweet with re-tweet being a part in the whole processing. 

5. Allow querying tweets

   - Query tweets subscribed to.
   <img width="402" alt="image" src="https://user-images.githubusercontent.com/28448629/143795665-c337b1a0-a5ef-41f1-8062-41597faed046.png">

   - Query tweets with specific hashtags
   <img width="413" alt="image" src="https://user-images.githubusercontent.com/28448629/143796225-79d27410-0c2e-495d-ad91-49585818d502.png">

   - Query tweets the user is mentioned
   <img width="414" alt="image" src="https://user-images.githubusercontent.com/28448629/143796529-08a86216-72fb-4e07-8a12-b561a8889671.png">

6. If the user is connected, deliver the above types of tweets live (without querying)
   
   -There are two methods to realize the delivering without querying.
      - One is to set an online user table in the system. If a user sends a new tweet, Engine will query the user's all followers to intersect with that online user table. Basing on the result, Engine then send the new post tweet to the online followers. However, this method contains additional steps of database operations which consumes much more time.
   <img width="358" alt="image" src="https://user-images.githubusercontent.com/28448629/143793605-10623910-1715-4896-8fbe-b3639bfb0ec4.png">

      - The other is that delivers the tweets to all its followers. When the follower is connected, the tweet received in mailbox will be printed without querying. However, if the user is not online, the client actor will do nothing even if the tweets has been sent to its mailbox. This method is realized by adding in a control variant "Login".
   - Considering the convenience and speed of the algorithm, we here choose the second method in our codes.



## Experiment

### Result

1. Since we use SQLite in our experiments, running speed of the program and capacibility of users number are supposed to be very high. Here we tested a scale of 5000 and 10000 users about which it consumes some time to set the subscribers and tweets and print all the results on the powershell. There is no limitaton of the scale therotically. When running on one machine, the limitation is bound to the machine performance and time. And if this project can be implemented on different machines, there will be no upper limitation on the number therotically.

<img width="872" alt="image" src="https://user-images.githubusercontent.com/28448629/143773599-ec1991d8-ef8d-4442-9aa4-5549c2fde95d.png">

<img width="868" alt="image" src="https://user-images.githubusercontent.com/28448629/143821995-58316f56-e067-44c8-abec-697a2ab558f6.png">

2. There are also simulations of login in and login out of users. The login in informatioin of one user is printed on the screen. 
   
-login:
   
<img width="452" alt="image" src="https://user-images.githubusercontent.com/28448629/143777089-b7f7ade7-a1d9-4d50-a0ec-ac5fb64d051d.png">

<img width="467" alt="image" src="https://user-images.githubusercontent.com/28448629/143777096-803e5883-604d-48a7-9ba7-0f5177f64946.png">
   
   -logout:
   
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

<img width="590" alt="image" src="https://user-images.githubusercontent.com/28448629/143822056-3b1507c6-0595-43ab-8b53-2568aef4eb0f.png">

   - Attention 1: there are some post rates = 0 instances. The rate equaling to 0 is not abnormal, since these users has a small number of subscribers, and each user has only a rate of 1/UserID(in system a higher ID number means a lower subscribers number relating to Zipf distribution) to pose a tweet. Therefore, users with relatively small amount of subscribers may have higher chances to get his post rate = 0%.
   - Attention 2: In normal situation, the user with most subscribers will have the UserID = 1 in ACCOUNT. However, as actors sends register messages to Engine with asychronization, the UserID in TABLE ACCOUNT may not show to be 1 here. Actually, in the system each clients has an individual variant "index" to record their own order, so that though the print order seems disordered, the actual calculation of subscribers number and post rate is decided by each "index" of the client, which means that UserID in "statistics.txt" is just an ID number while the real computation of Zipf distribution uses "index" of each client actor.
