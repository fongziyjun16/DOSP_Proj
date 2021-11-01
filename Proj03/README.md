# DOSP_Proj01

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

- The real time it takes to get the convergence of the algorithm is printed as "real time -- minutes: 0 seconds: 0 milliseconds: 396 \nrun time -- minutes: 0 seconds: 0 milliseconds: 396".

![image](https://user-images.githubusercontent.com/28448629/136638555-ae23bf61-68e3-447a-ae6f-37d4f11dfaf8.png)

- The round it takes is printed as "rounds:6". Only the "push-sum" algorithm prints the number of rounds.

![image](https://user-images.githubusercontent.com/28448629/136675776-97a3adf6-cab0-46ff-8bbd-0aebea7ac98c.png)


- There is the possibility that not all workers can receive the rumor. This observation is especially obvious in the topology "line" in gossip algorithm. 


## Architecture

There are 6 files in total in this project.

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
   - Inherit from actor, a print actor to print the sender address and received message    
   
- "ChordManagerActor.fs"
   - Module ChordManagerActor
   - An actor. Test the correctness of algorithm
   
- "ChordNodeActor.fs"
   - Module ChordNodeActor
   - The real actor in the chord.
   - 
- "Programs.fs" is the running entry of the application
   - Generate number of "numberOfNodes" identifiers
   - Defines nodes broad cast router
   - Check if the structure is completed

### Actors Function

- Printer Actor in Module PrinterActor:
  - Print the addresses of an identifier and its next predecessor and successor.

- Manage Actor in module Manager Actor: 
  - Destination actor of the messages, with messages processing
  - To verify the correctness of structure of the chord
  
- Node Actor in module ChordNodeActor:
  - Generate one random resource
  - Find the successor: judge if the current identifier's code is same to or in the range of the successor. If it is, change the variant "isInScope" to true. If not find, search from index=159 down to 0 in the finger table.
  - Update the successor in fingertable
  - Update predecessor in the fingertable
  - Prepare and add identifiers in the fingertable
  - Match the mailbox massage with the functions

### Chord Algorithm

- Gossip and Push-sum
   - Gossip is that a worker selects one of its neighbors based on the topology of network to send the rumor when the worker receives one rumor.
   - Push-sum is that each time the worker send the rumor, it sends the pair (1/2sr, 1/2wr) in the last round of its total received sr and wr to itself in this round, and the other half to the neighbor it will select. When the ratio of sr/wr of a worker is changed under 10 with an exponent -10, the worker stops.

- Full, 3D, Line, Imp3D
   -  Full: A worker is connected to all the other workers.

![image](https://user-images.githubusercontent.com/28448629/136637868-d80e096d-4a36-4c63-8e73-acc2e52791b7.png)


   -  3D: A worker is connnected to the other 6 neighbors in a 3D grid.
   
![image](https://user-images.githubusercontent.com/28448629/136637886-7697284f-cdc9-4ce0-b23a-8dd81fbcd5d6.png)

   
   -  Line: The workers are arranged in a line, with the begining and the end worker reaches only one neighbor and the middle workers reaches the other two neighbors. 
   
![image](https://user-images.githubusercontent.com/28448629/136638073-52923c05-0a33-4b9d-a74b-0248f0efbe64.png)

   
   -  Imp3D: A worker is connected to another random neighbor selected from all the workers on the fundation of the network of 3D.


## Experiment

#### Result

1. GOSSIP-FULL: The largest actor amount tested in the project is 100000. It can be more there but is not verified.
   -The algorithm convergence time for different scales of actors is saved in the file "gossipfull.txt", under the rumor limitation of 20.
   
![image](https://user-images.githubusercontent.com/28448629/136712241-56fb92bf-7a85-448e-a19d-a191afa3dbac.png)

   
2. GOSSIP-LINE: The largest actor amount tested in the project is 1000. When there is 2000, it is blocked between 80%~100% that not all workers can receive the rumor. 
   -The algorithm convergence time for different scales of actors is saved in the file "gossipline.txt", under the rumor limitation of 20.
   
![image](https://user-images.githubusercontent.com/28448629/136712254-38e3eb1d-d162-4ba0-b491-49f5d277a019.png)

   
3. GOSSIP-3D: The largest actor amount tested in the project is 100000. It can be more there but is not verified. 
   -The algorithm convergence time for different scales of actors is saved in the file "gossip3D.txt", under the rumor limitation of 20.

![image](https://user-images.githubusercontent.com/28448629/136712270-d834855c-fa86-4516-bcdf-5fceb49466ef.png)


4. GOSSIP-IMP3D: The largest actor amount tested in the project is 100000. It can be more there but is not verified. 
   -The algorithm convergence time for different scales of actors is saved in the file "gossipimp3D.txt", under the rumor limitation of 20.

![image](https://user-images.githubusercontent.com/28448629/136712284-b4bf18d0-739b-498b-9ef7-95130c6679cc.png)


The convergence time of gossip topologies draws in one picture.

![image](https://user-images.githubusercontent.com/28448629/136712325-b1e12a17-3025-4b62-b04b-b93d774a4327.png)

![image](https://user-images.githubusercontent.com/28448629/136712346-3b6f91ca-0937-46aa-847c-6ff56c121675.png)


5. PUSH_SUM-FULL: The largest actor amount tested in the project is 100000. When the ratio of s/w changes less than 10 with exponents -10 in 3 consecutive rounds, the actor stops. It can be more actors there but is not verified.
   -The algorithm convergence time for different scales of actors is saved in the file "pushsumfull.txt".
   
![image](https://user-images.githubusercontent.com/28448629/136712393-c0bb4fb7-fd0f-4d4c-a1eb-bd34e5790c95.png)


6. PUSH_SUM-LINE: The largest actor amount tested in the project is 100000. It can be more there but is not verified.
   -The algorithm convergence time for different scales of actors is saved in the file "pushsumline.txt".

![image](https://user-images.githubusercontent.com/28448629/136712402-bfc513d4-bc71-4be8-ab82-c9e6881ffa97.png)


7. PUSH_SUM-3D: The largest actor amount tested in the project is 100000. It can be more there but is not verified. 
   -The algorithm convergence time for different scales of actors is saved in the file "pushsum3D.txt".

![image](https://user-images.githubusercontent.com/28448629/136712409-1dbcf2a9-6aaf-43d3-a04a-33f0c0baec36.png)


8. PUSH_SUM-IMP3D: The largest actor amount tested in the project is 100000. It can be more there but is not verified.
   -The algorithm convergence time for different scales of actors is saved in the file "pushsumimp3D.txt".
   
![image](https://user-images.githubusercontent.com/28448629/136712416-a230db17-94d2-4343-80c0-0a8e027f8fc3.png)


The convergence time of push-sum topologies draws in one picture.

![image](https://user-images.githubusercontent.com/28448629/136712424-5f12a9d1-84e1-4292-b060-679efcf676f7.png)
