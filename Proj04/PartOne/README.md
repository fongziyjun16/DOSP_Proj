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

- Engine Actor:
  - Print the addresses of the sender and its successor.
  - When the searching is down, be called and print out the result massage

- Client Actor: 
  - Destination actor of the messages, with messages processing
  - To verify the correctness of structure of the chord, by "CheckChordStructure"
  - Count the steps number and print each step, by "FoundResource"
  - Call the printer to print the final result and average number of hops

- Random Controller Actor:
   - Get stabilize message and stabilize
   - Get fix finger table messge and fix the finger table.
    

- Print Actor:
  - Match the mailbox massage with the functions
  - Call the corresponding actions or actors to operate
  - Stableize: call the assistant actor to operate stablize operation
  - Notify
  - Send "StopStabilize" and "StopFixFingerTable" message to its assistant actor
  - Lookup: start lookup mission, Prelookup and Lookup.  
 
![image](https://user-images.githubusercontent.com/28448629/140232556-f96f58b9-c354-49b2-a31c-8d05e09732b3.png)


#### Functionalities

1. Register Account

2. Send tweet

3. Subscribe to user's tweets

4. Re-tweets

5. Allow querying tweets

6. If the user is connected, deliver the above types of tweets live (without querying)




### Chord Algorithm

- Create a chord ring
   -"Program.fs" 
      - Create the first node
      - After the first node, the other nodes join the ring
      - Call stablilize
      - Fix Finger table
   - "ChordNodeActor.fs"
      - Join in a new node
      - Stabilizing every 200 ms
      - Notify

- Find locations of the keys
   - Use hash to map keys by SHA1.
   - Identifiers are ordered on an circle. A key K is assigned to the first node whose identifier is equal to or follows k, which is called the successor node. 

![image](https://user-images.githubusercontent.com/28448629/139670532-43c33549-aa20-4d0f-a225-b081a5c33c4a.png)

   - Each identifier knows its fingertable. Find the query key by jumping to the closest successor of the key in the finger table. If there is not any closest identifier in the finger table, find the closest predecessor.

![image](https://user-images.githubusercontent.com/28448629/139672461-60d82661-d352-42c6-b3dc-5d4941551027.png)



## Experiment

#### Result

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
