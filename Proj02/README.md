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

5. Input the number of workers and rumor limit time

![image](https://user-images.githubusercontent.com/28448629/136636718-408eb9ba-714b-4ce3-8c69-57cc84fa0bd2.png)

6. The result prints the running time and rounds number it takes to achieve the convergence of the algorithm.


### Result Description

- The real time it takes to get the convergence of the algorithm is printed as "".
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

![image](https://user-images.githubusercontent.com/28448629/136637454-8b9b6d5f-e7de-41cd-ae5a-693d1a6d8d55.png)


### Two Algorithms And Four Network Topologies

- Gossip and Push-sum
   - Gossip is that a worker selects one of its neighbors based on the topology of network to send the rumor when the worker receives one rumor.
   - Push-sum is that each time the worker send the rumor, it sends the pair (1/2sr, 1/2wr) in the last round of its total received sr and wr to itself in this round, and the other half to the neighbor it will select.

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

The program is running on two virtual machines of Linux Ubuntu systems to get the cpu time, since linux supports better in multi-core and multi-thread operations. Each machine sets two cores at the beginning to do the paralleling computing.
1. Main Application:
   -IP: 10.0.2.15
   -Port: 10010
   -Running the command "dotnet run 10.0.2.15 10010 y 10"
   -Input the prefix "yingjie.chen"
   -Input the leading 0
   
   ![image](images/009.png)

2. Sub Application:
   -IP: 10.0.2.4
   -Port: 10010
   -Running the command "dotnet run 10.0.2.4 10010 n 10"
   -Input the Main application IP: 10.0.2.15
   -Input the Main application Port: 10010
   
   ![image](images/010.png)

Outcome of leading 0 = 4

   ![image](images/011.png)

   ![image](images/012.png)

   ![image](images/013.png)

   ![image](images/014.png)

The list will extend unless pressing the "Enter" button to stop.

#### CPU Usage

The real time takes the system time of Linux. The CPU time can be shown synchronizingly using Linux conmmands which are listed below. CPU time and CPU usage is the summary time and usage of the two cores allocated to each machine.
- After the Main Application begins calculating, input "ps -aux" in a new terminal window to acquire PID of the process
- Input "top -t #PID" to get the dynamic corresponding CPU usage situation

1. Main Application
-Start: REAL TIME is 10:22
-After a time interval:
   - Real time: 10:29
   - CPU time: 11:27.94
   - CPU time > Real time (10:29 - 10:22), proving that this actor model is CPU bound.
   - Ratio of CPU time / Real time = 11.5 / 7 = 1.6429 (as there is only two cores)
   - CPU Usage: 172.3%. Average: 170%

Start Picture:

   ![image](images/015.png)

Later Picture:

   ![image](images/016.png)

2. Sub Application
-Start: REAL TIME is 10:23
-After a time interval:
   - Real time: 10:27
   - CPU time: 6:36.53
   - CPU time > Real time (10:27 - 10:22), proving that this actor model is CPU bound.
   - Ratio of CPU time / Real time = 6.6 / 5 = 1.32 (as there is only two cores)
   - CPU Usage: 171.3%. Average: 169%

Start Picture:

   ![image](images/017.png)

Later Picture:

   ![image](images/018.png)

#### Coins with most 0s

Coins with nine 0s is the most out program can find. When 0s = 10, the program need a long time to compute the result and that time has proved to be longer than 1 hour.


#### Scalability

After starting one Main Application, we can start a number of Sub Application in the same machine or other machines to join in computation for finding the results with specific length of leading zeros and other results with different length of leading zeros.





