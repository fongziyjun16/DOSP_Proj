# DOSP_Proj01

## Group Members

1. UFID: 83981600, Email: yingjie.chen@ufl.edu
2. UFID: 84714011, Email: wei.he@ufl.edu

## How to Run

### Windows 10

1. Environment Setup: 

   ​	.NET SDKs

   ​	.NET runtimes

2. Decompressing the .RAR file: G_PS_Topology.rar 

3. Opening Command Prompt and Going to the folder obtained from the second step

4. Run the command line "dotnet run [topology] [algorithm]"
   - selections for [topology]: full, 3D, line, imp3D
   - selections for [algorithm]: gossip, push-sum

![image](https://user-images.githubusercontent.com/28448629/136636560-4d0c46a8-5745-49db-aa68-d3e9eaf75a25.png)

5. Input the number of workers or the (length, width, height) for 3D, and rumor limit time

![image](https://user-images.githubusercontent.com/28448629/136636718-408eb9ba-714b-4ce3-8c69-57cc84fa0bd2.png)

![image](https://user-images.githubusercontent.com/28448629/136666905-907ba5d8-4962-4d66-916c-2fd7c8925227.png)


6. The result prints the running time and rounds number it takes to achieve the convergence of the algorithm.


### Result Description

- The real time it takes to get the convergence of the algorithm is printed as "real time -- minutes: 0 seconds: 0 milliseconds: 396 \nrun time -- minutes: 0 seconds: 0 milliseconds: 396".

![image](https://user-images.githubusercontent.com/28448629/136638555-ae23bf61-68e3-447a-ae6f-37d4f11dfaf8.png)

- The round it takes is printed as "".



- There is the possibility that not all workers can receive the rumor. This observation is especially obvious in the topology "line". 


## Architecture

### The structure of the source code

- "Msgs.fs" defines different kinds of messages
- "Actors.fs" defines different kinds of actors
- "Programs.fs" is the running entry of the application

### Actors Function

- Printer Actor:
  - Print the messages.
- Recorder Actor: 
  - Supervise all the worker actors about the times they recieve the rumors
  - When the network starts, choose a random worker to begin with sending the rumor.
  - Counts the rumors, and reports the percentage of workers that have gotten the rumor. Every 20%, it reports once.
  - Report the real time that used for the rumor to spread over the network
- Worker Actor:
  - Send out the rumor message and wait for one millisecond.
  - Receive the message from its neighbors and account. When the rumors it receives reaches the limitaion, stop action.
  - Report the rumor counts to the recorder actor. 
- Attention:
  - Here is a command "do! Async.Sleep(1)" in the worker actor after sending a message. As there is limited threads in the CPU, it makes the worker gives up the thread and stops for 1 millisecond, when it ends this turn of sending the rumor. Since the number of actors may be huge, this mechanisum assure that the thread can move to other actors but not be occupied by one same actor for a long time.
  - If the gossip is running on different computers, that is to say a real network, there will not be this thread occupying issue.

![image](https://user-images.githubusercontent.com/28448629/136637454-8b9b6d5f-e7de-41cd-ae5a-693d1a6d8d55.png)


### Two Algorithms And Four Network Topologies

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
   
![image](https://user-images.githubusercontent.com/28448629/136675503-112f553a-784c-427d-9b65-99d21e2645aa.png)

   
2. GOSSIP-LINE: The largest actor amount tested in the project is 1000. When there is 2000, it is blocked between 80%~100% that not all workers can receive the rumor. 
   -The algorithm convergence time for different scales of actors is saved in the file "gossipline.txt", under the rumor limitation of 20.
   
![image](https://user-images.githubusercontent.com/28448629/136675509-23e2be14-f142-4a02-b97b-7e69085bf20e.png)

   
3. GOSSIP-3D: The largest actor amount tested in the project is 100000. When there is 2000, it is blocked between 80%~100%. 
   -The algorithm convergence time for different scales of actors is saved in the file "gossip3D.txt", under the rumor limitation of 20.

![image](https://user-images.githubusercontent.com/28448629/136675520-33cc1279-c012-4643-8f94-41917c9c1a53.png)


4. GOSSIP-IMP3D: The largest actor amount tested in the project is 1000. When there is 2000, it is blocked between 80%~100%. 
   -The algorithm convergence time for different scales of actors is saved in the file "gossipimp3D.txt", under the rumor limitation of 20.

![image](https://user-images.githubusercontent.com/28448629/136675528-b40a8f77-dd62-4d65-8102-6e7cfa9c2752.png)


The convergence time of gossip topologies draws in one picture.

![image](https://user-images.githubusercontent.com/28448629/136675571-99f49bc1-1693-4c3b-8606-8b1dbb74cfa2.png)

![image](https://user-images.githubusercontent.com/28448629/136675591-d2c1a171-7c62-47c0-b279-48aa376983de.png)


5. PUSH_SUM-FULL: The largest actor amount tested in the project is 1000. When there is 2000, it is blocked between 80%~100%. 
   -The algorithm convergence time for different scales of actors is saved in the file "pushsumfull.txt", under the rumor limitation of 20.
   
![image](https://user-images.githubusercontent.com/28448629/136675609-b2235506-486f-43ad-b015-37957c9f0adc.png)


6. PUSH_SUM-LINE: The largest actor amount tested in the project is 1000. When there is 2000, it is blocked between 80%~100%. 
   -The algorithm convergence time for different scales of actors is saved in the file "pushsumline.txt", under the rumor limitation of 20.

![image](https://user-images.githubusercontent.com/28448629/136675619-caafe1ae-bcbb-4d4c-9546-e11d3df07045.png)


7. PUSH_SUM-3D: The largest actor amount tested in the project is 1000. When there is 2000, it is blocked between 80%~100%. 
   -The algorithm convergence time for different scales of actors is saved in the file "pushsum3D.txt", under the rumor limitation of 20.

![image](https://user-images.githubusercontent.com/28448629/136675641-d1506b6f-feeb-4e7b-98e4-42b65c49110c.png)


8. PUSH_SUM-IMP3D: The largest actor amount tested in the project is 1000. When there is 2000, it is blocked between 80%~100%. 
   -The algorithm convergence time for different scales of actors is saved in the file "pushsumimp3D.txt", under the rumor limitation of 20.
   
![image](https://user-images.githubusercontent.com/28448629/136675656-74a8f1e2-1619-4f60-af66-e3bdbba22736.png)


The convergence time of push-sum topologies draws in one picture.

![image](https://user-images.githubusercontent.com/28448629/136675693-6417e84e-5075-4f0b-920d-d2113a7a9198.png)
