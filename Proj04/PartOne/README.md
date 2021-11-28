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

- When a node finds a key in ith steps, it is printed as "176_55_59_235_16731 found key 9IOm2wtz5D5XdKY6MtP in 1 steps".

![image](https://user-images.githubusercontent.com/28448629/140211783-0fc7665b-cb24-4997-8769-8f6cba63f4c5.png)

The other messages include:
- The simulated IP_address and its key.

![image](https://user-images.githubusercontent.com/28448629/140214848-93afcebf-a4a9-4608-ae3c-45926ee9c033.png)

- A file of statistic data of number of subscribers and tweets amounts in folder "output" is newly created, named "statistic.txt" each time restarting the programming. 

<img width="467" alt="image" src="https://user-images.githubusercontent.com/28448629/143749845-f55efb96-ed65-4b10-bf29-bdf5e33ec250.png">


   - In this txt file, one line of the output result shows like:

<img width="562" alt="image" src="https://user-images.githubusercontent.com/28448629/143752091-3efa1053-5fc5-490a-89b9-6a8782714f8a.png">

   - ID number is listed in the left, followed with the user names, number of followers, and tweet posting rates.
   - There only screen shotting a counting of the first 20 users. The other users are listed below but not screen shotting in.



## Architecture

The project use model of MVC, which contains data layer, 

### The structure of the source code

- "Msgs.fs" defines different types of messages
   - Module Msgs

- "Tools.fs" defines the tools to test the project
   - Module ToolsKit
   - Generate and transform different IP address and ports to simulate the chord algorithm
      - static let generateOndNodeIdentifier()
   - Encode Key by SHA1. 
      - Attention: Here, it should add a "0" at the begining of the key to convert it into a positive integer of a Hexadecimal form
      - static member encodeBySHA1(key: string)
   - Generate nodes identifier randomly
      - static member generateNodeIdentifier()
   - Generate the chord of the identifiers
      - static member getCorrectIdentifiers()

- "PrinterActor.fs"
   - Module PrinterActor
   - Inherit from actor, a print actor to print message   
   
- "ChordManagerActor.fs"
   - Module ChordManagerActor
   - An actor. Test the correctness of the chord and print the result using the printor actor
   
- "ChordNodeInfo.fs"
   - Module ChordNodeInfo
   - Stores the information (fingertable) of a node in the chord. Can be accessed by actor and assistant actor.
   - Initialize and set the predecessor and successor
   - Reset fingertable
   - Find the successor in fingertable: judge if the current identifier's code is same to or in the range of the successor. If it is, change the variant "found" to be true. If not find, search from index=159 down to 0 in the finger table.
   - Update the fingertable
   - Get or set prececessor and successor and their codes

- "ChordNodeActor.fs"
   - Module ChordNodeInfo
   - The realization of operations of a node in the chord, including "Lookup"

- "ChordNodeAssitantActor.fs"
   - Module ChordNodeAssitantActor
   - Stabilize and fix finger table for each node.
   
- "Programs.fs" is the running entry of the application
   - Generate number of "numberOfNodes" identifiers
      - create first node
      - After the first node, the other nodes join the network
   - Check the node of chord structure every 500 ms
   - Defines nodes broad cast router
   - Check if the structure is completed
   - If the chord is completed, start the mission

### Actors of A Node

- Printer Actor in Module PrinterActor:
  - Print the addresses of the sender and its successor.
  - When the searching is down, be called and print out the result massage

- Manage Actor in module ChordManagerActor: 
  - Destination actor of the messages, with messages processing
  - To verify the correctness of structure of the chord, by "CheckChordStructure"
  - Count the steps number and print each step, by "FoundResource"
  - Call the printer to print the final result and average number of hops

- Assistant Actor in module ChordNodeAssitantActor
   - Get stabilize message and stabilize
   - Get fix finger table messge and fix the finger table.
      - The operations of stabilizing and fixing finger table are seperated out to the Assistant actor so that an actor can no longer wait for these messages and doing corresponding operations that hinder other operations. The assistant actor will do these issues, and the efficient of and an actor of a node improves.

- Node Actor in module ChordNodeActor:
  - Match the mailbox massage with the functions
  - Call the corresponding actions or actors to operate
  - Stableize: call the assistant actor to operate stablize operation
  - Notify
  - Send "StopStabilize" and "StopFixFingerTable" message to its assistant actor
  - Lookup: start lookup mission, Prelookup and Lookup.  
 
![image](https://user-images.githubusercontent.com/28448629/140232556-f96f58b9-c354-49b2-a31c-8d05e09732b3.png)



### Attention

Their was an deadlock problem we met when imployment the network.
When their are two nodes A and B. A sends out a message to query B the successor or predecessor, while B also sends out a message to A's mailbox and is waiting for A's answer but the message is listed in the back of the mailbox of A. In such a condition, A will be hindered in a waiting status and B is the same hindered in a waiting status. The program is trapped in a deadlock.

![image](https://user-images.githubusercontent.com/28448629/140204445-efea5e7b-0733-4fc6-8e72-4b2c1f26cdea.png)

Such a situation happens also in a larger scale of situation, like "A->B->C->D->E->A". The solution of this problem is to do overtime processing periodically. After we added a judging step that set the flag to be "false" for a searching failure when one node waits for another until overtime, the problem is solved. The failure flag will stimulate the sender of "Findsuccessor" to do an corresponding processing.

![image](https://user-images.githubusercontent.com/28448629/140238116-4011ac75-9521-4c4d-84b0-cc39dfd33d05.png)



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

2. If a user queries one hashtag, mentioned ID, or subscribed tweet, the results will be printed on the screen. The form { 30(5), 18 } means that there are two results corresponding to the query, one is the post tweet 30 which is a retweet of tweet 5, the other is tweet 18. "[akka://TweetSimulator/user/NfSWWTUVYv4h62F]" at the beginning shows the querying operation by user who.
- Query tweets subscribed to:
<img width="730" alt="image" src="https://user-images.githubusercontent.com/28448629/143774584-a29036b8-0d6e-43d0-b122-64f6ad5c04f0.png">

- Query a special hashtag:
<img width="743" alt="image" src="https://user-images.githubusercontent.com/28448629/143774554-c38a91e8-b2ca-4ef5-b5b3-678be42b4ecf.png">

- Query mentioned tweets:
<img width="555" alt="image" src="https://user-images.githubusercontent.com/28448629/143774638-30f14b64-9340-4dcb-bb2d-02687b5c27cb.png">

3. A Zipf distribution on the number of subscribers and more subscribers, more tweets and retweets. The count of number of subscribers and tweet proportion of all users are listed in the path "output\\statistics.txt". One line in the file gives the user ID, user name, number of followers and how much proportion his number of tweets occupies in the total tweets.

<img width="571" alt="image" src="https://user-images.githubusercontent.com/28448629/143775072-6d4480f5-38f0-48e3-b632-ccc0b2a82245.png">
