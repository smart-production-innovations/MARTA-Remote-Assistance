# MARTA-Remote-Assistance

MARTA (Mixed reAlity RemoTe Assistance) is a mixed reality supported remote assistance application for PC and smartphone/tablet. Our solution connects a portable device of an on-site worker with an remote expert using a desktop application. The on-site worker starts the call by sharing a unique connection key with the remote expert, e.g. by e-mail. The remote expert uses this key to establish the connection with the on-site worker. Remote experts and on-site workers see a live video recorded by the device of the on-site worker and can extend this live video with AR annotations in 3D space to facilitate communication with visual labels 
 
If you find this code useful in your research, please consider citing:
 
Andrea Aschauer, Irene Reisner-Kollmann, Josef Wolfartsberger, Creating an Open-Source Augmented Reality Remote Support Tool for Industry: Challenges and Learnings, Procedia Computer Science, Volume 180, 2021, Pages 269-279.
 
https://www.sciencedirect.com/science/article/pii/S1877050921002040
https://doi.org/10.1016/j.procs.2021.01.164
 
Note: You have to buy WebRTC first to use this package, see https://assetstore.unity.com/packages/tools/network/webrtc-video-chat-68030
 
By default, the code "FHOÖ" is used to establish a connection between a client and a server. For productive use, however, unique keys must be sent. To do this, a value must be changed on the smartphone. Connect the smartphone to the PC. Open this file:
 
Android/data/com.mrbc4i.RemoteSupport/files/config.xml
 
And change the third last line to:
<GenerateUniqueKey>True</GenerateUniqueKey>
