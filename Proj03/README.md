# DOSP_Proj03

## Group Members

1. UFID: 83981600, Email: yingjie.chen@ufl.edu
2. UFID: 84714011, Email: wei.he@ufl.edu

## How to Run

### Windows 10

1. Environment Setup: 

   ​	.NET SDKs

   ​	.NET runtimes

2. Decompressing the .RAR file: .rar 

3. Go to ChordP2PSimulator to input the "number of nodes" and "number of each node request" 

![image](https://user-images.githubusercontent.com/28448629/139657413-001c5cd4-e638-42d8-b5b3-ed0e51aeed98.png)

4. Observe the result

### Result Description

- The avrage number of hops it takes is printed as "the average number of hops is 4.243333333333333" in the end.

![image](https://user-images.githubusercontent.com/28448629/140201373-fd624d07-59c1-496e-9e41-63eca27b1ae9.png)

- The round it takes is printed as "rounds:6". Only the "push-sum" algorithm prints the number of rounds.

![image](https://user-images.githubusercontent.com/28448629/136675776-97a3adf6-cab0-46ff-8bbd-0aebea7ac98c.png)


- There is the possibility that not all workers can receive the rumor. This observation is especially obvious in the topology "line" in gossip algorithm. 


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
   - The operation for information of fingertables a node in the chord.

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

- Info Actor in module ChordNodeInfo:
  - Initialize and set the predecessor and successor
  - Reset fingertable
  - Find the successor in fingertable: judge if the current identifier's code is same to or in the range of the successor. If it is, change the variant "found" to be true. If not find, search from index=159 down to 0 in the finger table.
  - Update the fingertable
  - Get or set prececessor and successor and their codes

- Node Actor in module ChordNodeActor:
  - Match the mailbox massage with the functions
  - Call the corresponding actions or actors to operate
  - Stableize: call the assistant actor to operate stablize operation
  - Notify
  - Send "StopStabilize" and "StopFixFingerTable" message to its assistant actor
  - Lookup: start lookup mission, Prelookup and Lookup.  
 
![image](https://user-images.githubusercontent.com/28448629/140198834-e7253847-170a-4be0-a983-193aea53201a.png)


### Attention

Their was an deadlock problem we met when imployment the network.
When their are two nodes A and B. A sends out a message to query B the successor or predecessor, while B also sends out a message to A's mailbox and is waiting for A's answer but the message is listed in the back of the mailbox of A. In such a condition, A will be hindered in a waiting status and B is the same hindered in a waiting status. The program is trapped in a deadlock.

![image](https://user-images.githubusercontent.com/28448629/140204445-efea5e7b-0733-4fc6-8e72-4b2c1f26cdea.png)

Such a situation happens also in a larger scale of situation, like "A->B->C->D->E->A". The solution of this problem is to clean up the mailbox of one actor periodically. After we added an clean-up function, the problem is solved.




### Chord Algorithm

- Find locations of the keys
   - Use hash to map keys by SHA1.
   - Identifiers are ordered on an circle. A key K is assigned to the first node whose identifier is equal to or follows k, which is called the successor node. 

![image](https://user-images.githubusercontent.com/28448629/139670532-43c33549-aa20-4d0f-a225-b081a5c33c4a.png)

   - Each identifier knows its fingertable. Find the query key by jumping to the closest successor of the key in the finger table. If there is not any closest identifier in the finger table, find the closest predecessor.

![image](https://user-images.githubusercontent.com/28448629/139672461-60d82661-d352-42c6-b3dc-5d4941551027.png)


## Experiment

#### Result

1. There listed several results and their average number of hops below.

![image](https://user-images.githubusercontent.com/28448629/140200345-2c0faae2-98b6-42da-931b-e3d11cb134f3.png)


2. As we tested in the program, 30 nodes performs well in this chord algorithm and theoritically it can afford way much more scale of network to realize chord algorithm. There is no up limitation when the program is implemented in a real network unless bigger than 2^160. Limited to the running time and a single machine to do the experiments, there are only results of limited scales.

And When deploymented in a solo machine, a mailbox of an actor may receive too much messages so that leads to delay of network building or finger table updating. In a real multi-machine network, there will not be such problems.   