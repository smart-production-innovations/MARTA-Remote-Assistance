using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System;

/// <summary>
/// Data block structure for sending data over the network.
/// </summary>
[Serializable]
public class CommunicationData
{
    /// <summary>
    /// Unique ID of the data. All data blocks that belong together have the same ID.
    /// </summary>
    public int ID;
    /// <summary>
    /// data of the block
    /// </summary>
    public byte[] Data;

    /// <summary>
    /// The data channel of webRTC can only send a special amount of bytes at once. If the required bytes exceed the maximum number of bytes, the data is sent in blocks. 
    /// </summary>
    public int TotalDataLength;
    public int StartIndexBlock;

    /// <summary>
    /// initialize a new data block
    /// </summary>
    /// <param name="id"></param>
    /// <param name="data"></param>
    /// <param name="startIndexBlock"></param>
    /// <param name="totalDataLength"></param>
    public CommunicationData(int id, byte[] data, int startIndexBlock = 0, int totalDataLength = -1)
    {
        this.ID = id;
        this.Data = data;
        this.StartIndexBlock = startIndexBlock;
        this.TotalDataLength = totalDataLength;
    }
}

/// <summary>
/// Manages the data queues of the data to be sent and the received data blocks that have not yet been completely reconstructed
/// </summary>
public static class CommunicationMonitor
{
    private const float blockRecallTime = 2;
    private static int nextKey = 0;
    /// <summary>
    /// output queue
    /// </summary>
    private static Dictionary<int, byte[]> dataQueue = new Dictionary<int, byte[]>();
    /// <summary>
    /// input queue
    /// </summary>
    private static Dictionary<int, BlockReconstruction> receivedData = new Dictionary<int, BlockReconstruction>();

    /// <summary>
    /// Add data to be sent to the output queue.
    /// </summary>
    /// <param name="data">Data to be sent</param>
    /// <returns>data blocks</returns>
    public static CommunicationData[] AddToQueue(byte[] data)
    {
        nextKey++;
        dataQueue.Add(nextKey, data);
        return GetDataPackages(nextKey);
    }

    /// <summary>
    /// Converts the data from the output queue into data blocks.
    /// </summary>
    /// <param name="id">Unique data ID used to find the data in the output queue.. All data blocks that belong together have the same ID.</param>
    /// <param name="blockIndex">If not all data blocks of the data ID are required, individual block numbers can be requested.</param>
    /// <returns>data blocks</returns>
    public static CommunicationData[] GetDataPackages(int id, int[] blockIndex = null)
    {
        var result = new List<CommunicationData>();
        var data = dataQueue[id];
        for (int i = 0; i < BlockReconstruction.blockCount(data.Length); i++)
        {
            if (blockIndex != null && !blockIndex.Contains(i)) continue;

            int startIndex = i * CommunicationConstants.MaxDataSizeBlock;

            int blockSize = data.Length - startIndex;
            if (blockSize > CommunicationConstants.MaxDataSizeBlock) blockSize = CommunicationConstants.MaxDataSizeBlock;

            var block = data.SubArray(startIndex, blockSize);
            result.Add(new CommunicationData(id, block, startIndex, data.Length));
        }
        return result.ToArray();
    }

    /// <summary>
    /// Add data received from the network to the input queue.
    /// </summary>
    /// <param name="data">received data block</param>
    /// <returns>Were all blocks needed for the reconstruction of the data received?</returns>
    public static bool DataReceived(CommunicationData data)
    {
        if (!receivedData.ContainsKey(data.ID))
            receivedData.Add(data.ID, new BlockReconstruction(data.Data, data.StartIndexBlock, data.TotalDataLength));
        else
            receivedData[data.ID].addData(data.Data, data.StartIndexBlock);

        return receivedData[data.ID].IsReconstructed;
    }

    /// <summary>
    /// reconstructed data from all received data blocks
    /// </summary>
    /// <param name="id">unique data ID</param>
    /// <returns>reconstructed data</returns>
    public static byte[] GetReceivedData(int id)
    {
        if (receivedData[id].IsReconstructed)
        {
            var data = receivedData[id].result;
            receivedData.Remove(id);
            return data;
        }
        return null;
    }

    /// <summary>
    /// When all data blocks have been successfully transmitted, the data can be deleted from the output queue.
    /// </summary>
    /// <param name="id"></param>
    public static void DataReceiveCompletet(int id)
    {
        dataQueue.Remove(id);
    }

    /// <summary>
    /// Request resending of lost blocks.
    /// </summary>
    public static void ResendMissingBlocks()
    {
        foreach (var item in receivedData)
        {
            var id = item.Key;
            var data = item.Value;
            if (data.lastBlockDataReceived + blockRecallTime < Time.time)
            {
                data.lastBlockDataReceived = Time.time;

                var missing = data.MissingBlocks;
                string missingString = "";
                foreach (var missingIndex in missing)
                {
                    missingString += "/" + missingIndex;
                }
                CommunicationManager.Instance.SendCommandMsg(new CommandMsg(CommandMsgType.ResendBlock, id + missingString));
            }
        }
    }
}


/// <summary>
/// The data channel of webRTC can only send a special amount of bytes at once. If the required bytes exceed the maximum number of bytes, the data is sent in blocks. 
/// The receiver must reconstruct the received blocks.
/// Data structure for the reconstruction of data with the same unique ID.
/// </summary>
public class BlockReconstruction
{
    public byte[] result;
    public int reconstructionCount;
    private bool[] blockReconstructed;
    public float lastBlockDataReceived;

    /// <summary>
    /// Calculates the block index depending to the start index.
    /// The data channel of webRTC can only send a special amount of bytes at once. If the required bytes exceed the maximum number of bytes, the data is sent in blocks. 
    /// The receive has to reconstruct the revived blocks by ordering them correctly.
    /// </summary>
    /// <param name="startIndex">index of the first byte in this block in whole dataset</param>
    /// <returns>block index</returns>
    public static int blockIndex(int startIndex)
    {
        return startIndex / CommunicationConstants.MaxDataSizeBlock;
    }

    /// <summary>
    /// how many block are necessary to send the whole dataset
    /// </summary>
    /// <param name="totalLength">total length of the whole dataset</param>
    /// <returns>total block count</returns>
    public static int blockCount(int totalLength)
    {
        int count = (int)Mathf.Ceil((float)totalLength / CommunicationConstants.MaxDataSizeBlock);
        return count;
    }

    /// <summary>
    /// Initiation of a new reconstruction request.
    /// </summary>
    /// <param name="block">received data</param>
    /// <param name="startIndex">index of the first byte in this block in whole dataset</param>
    /// <param name="totalLength">Total number of bytes of data to be reconstructed.</param>
    public BlockReconstruction(byte[] block, int startIndex, int totalLength)
    {
        result = new byte[totalLength];
        blockReconstructed = Enumerable.Repeat(false, blockCount(totalLength)).ToArray();
        reconstructionCount = 0;
        addData(block, startIndex);
    }

    /// <summary>
    /// add received data blocks to the reconstruction process
    /// </summary>
    /// <param name="block">received data</param>
    /// <param name="startIndex">index of the first byte in this block in whole dataset</param>
    public void addData(byte[] block, int startIndex)
    {
        reconstructionCount += block.Length;

        Array.Copy(block, 0, result, startIndex, block.Length);

        blockReconstructed[blockIndex(startIndex)] = true;
        lastBlockDataReceived = Time.time;
    }

    /// <summary>
    /// is reconstruction finished
    /// </summary>
    public bool IsReconstructed
    {
        get
        {
            bool isDone = blockReconstructed.Where(x => !x).Count() == 0;
            return isDone;
        }
    }

    /// <summary>
    /// which blocks get lost by the sending process
    /// </summary>
    public int[] MissingBlocks
    {
        get
        {
            return blockReconstructed.Select((x, i) => new { Value = x, Index = i }).Where(x => !x.Value).Select(x => x.Index).ToArray();
        }
    }
}


/// <summary>
/// Constant values
/// </summary>
public static class CommunicationConstants
{
    /// <summary>
    /// the data channel of webRTC can only send a special amount of bytes at once. 
    /// </summary>
    public const int MaxDataSizeBlock = 60000;
}
