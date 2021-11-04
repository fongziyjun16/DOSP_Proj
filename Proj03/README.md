# DOSP_Proj03

## Group Members

1. UFID: 83981600, Email: yingjie.chen@ufl.edu
2. UFID: 84714011, Email: wei.he@ufl.edu

## How to Run

### Windows 10

1. Environment Setup: 

   ​	.NET SDKs

   ​	.NET runtimes

2. Decompressing the .RAR file: ChordP2PSystemSimulate.rar

3. Opening Command Prompt and Going to the folder obtained from the second step

4. Run the command line "dotnet run [numberOfNodes] [numberOfEachNodeRequest]"
   - suggest to input numberOfNodes under 30, for it needs more time to run the project as the scale of network growing larger

![image](https://user-images.githubusercontent.com/28448629/140233504-4a9c6459-d8e5-4ef0-acdf-74ceb15d94b6.png)


5. Wait and observe the result 


### Result Description

- The avrage number of hops it takes is printed as "the average number of hops is 4.243333333333333" in the end.

![image](https://user-images.githubusercontent.com/28448629/140201373-fd624d07-59c1-496e-9e41-63eca27b1ae9.png)

- When a node finds a key in ith steps, it is printed as "176_55_59_235_16731 found key 9IOm2wtz5D5XdKY6MtP in 1 steps".

![image](https://user-images.githubusercontent.com/28448629/140211783-0fc7665b-cb24-4997-8769-8f6cba63f4c5.png)

The other messages include:
- The simulated IP_address and its key.

![image](https://user-images.githubusercontent.com/28448629/140214848-93afcebf-a4a9-4608-ae3c-45926ee9c033.png)

- The first identifier

![image](https://user-images.githubusercontent.com/28448629/140212788-a376e3a4-b8f9-4fc3-8e5f-eee11cee1296.png)

   - Updating of a node of in its finger table of its predecessor and successor.

![image](https://user-images.githubusercontent.com/28448629/140209336-20b38e3d-9196-4662-a75c-490bc8c840bc.png)

   - Stabilize message

![image](https://user-images.githubusercontent.com/28448629/140209847-34cc7ba6-d47d-4b8f-ad95-fd05d6335602.png)

   - Fix Finger Table message

![image](https://user-images.githubusercontent.com/28448629/140209866-1e1493ed-e72c-4075-a9da-78f78a07be85.png)

   -  Checking structure

![image](https://user-images.githubusercontent.com/28448629/140210013-ac1eb365-e16e-4732-b5a6-9dad9603eed4.png)

   -  Sends out all lookup

![image](https://user-images.githubusercontent.com/28448629/140212219-1016b627-aaf8-428f-a131-c8f42972dc5a.png)




## Architecture

There are 8 files in total in this project.

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

Such a situation happens also in a larger scale of situation, like "A->B->C->D->E->A". The solution of this problem is to clean up the mailbox of one actor periodically. After we added a judging step, the problem is solved.


### Chord Algorithm

- Find locations of the keys
   - Use hash to map keys by SHA1.
   - Identifiers are ordered on an circle. A key K is assigned to the first node whose identifier is equal to or follows k, which is called the successor node. 

![image](https://user-images.githubusercontent.com/28448629/139670532-43c33549-aa20-4d0f-a225-b081a5c33c4a.png)

   - Each identifier knows its fingertable. Find the query key by jumping to the closest successor of the key in the finger table. If there is not any closest identifier in the finger table, find the closest predecessor.

![image](https://user-images.githubusercontent.com/28448629/139672461-60d82661-d352-42c6-b3dc-5d4941551027.png)



## Experiment

#### Result

1. There listed several results and their average number of hops below. The number of hops is counted using variant "totalNumberOfSteps" in ChordManagerActor.fs and each time it receives a message the variant will add the steps number up. Finally devided by totalNumberOfRequest to get the average hop number.

![image](https://user-images.githubusercontent.com/28448629/140229296-afd4b925-6a08-4760-b318-a3135bbc1923.png)

- numberOfNodes: 20   numberOfEachNodeRequests: 10

![image](https://user-images.githubusercontent.com/28448629/140234197-d98f9331-c1f9-49c1-8978-ad93d8e4d1b2.png)

![image](https://user-images.githubusercontent.com/28448629/140234164-f0de9601-1a1e-4583-b1d2-bd0b6f815456.png)

![image](https://user-images.githubusercontent.com/28448629/140234367-b31b994a-48dc-421f-b0bf-30141785c4eb.png)

- numberOfNodes: 30   numberOfEachNodeRequests: 10

![image](https://user-images.githubusercontent.com/28448629/140234398-28bdc64f-5bbd-4597-8805-61bb0e7fdaf3.png)

![image](https://user-images.githubusercontent.com/28448629/140200345-2c0faae2-98b6-42da-931b-e3d11cb134f3.png)



2. As we tested in the program, 50 nodes performs well in this chord algorithm and theoritically it can afford way much more scale of network to realize chord algorithm. There is no up limitation when the program is implemented in a real network unless bigger than 2^160. Limited to the running time and a single machine to do the experiments, there are only results of limited scales. 

   And When deploymented in a solo machine, a mailbox of an actor may receive too much messages so that leads to delay of network building or finger table updating. In a real multi-machine network, there will not be such problems.   
