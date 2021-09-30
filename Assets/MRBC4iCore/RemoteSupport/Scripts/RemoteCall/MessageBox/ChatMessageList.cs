using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using System.Linq;

/// <summary>
/// container object for ChatMessageList
/// </summary>
public struct MessageEntry
{
    public bool isLogMessage;
    public bool isCommandMessage;
    public bool isStatusMessage;
    public LogType logType;
    public CommandMsgType cmdType;
    public GameObject entry;

    /// <summary>
    /// initialize a new chat message
    /// </summary>
    /// <param name="entry">game object that displays the chat message</param>
    public MessageEntry(GameObject entry) : this(false, entry)
    {
    }

    /// <summary>
    /// initialize a new chat message
    /// </summary>
    /// <param name="isStatusMessage">is the chat entry a status message</param>
    /// <param name="entry">game object that displays the chat message</param>
    public MessageEntry(bool isStatusMessage, GameObject entry) : this(false, false, isStatusMessage, LogType.Log, CommandMsgType.NoCommand, entry)
    {
    }

    /// <summary>
    /// initialize a new chat message
    /// </summary>
    /// <param name="isLogMessage">is the chat entry a log message</param>
    /// <param name="logType">type of the log message</param>
    /// <param name="entry">game object that displays the chat messag</param>
    public MessageEntry(bool isLogMessage, LogType logType, GameObject entry) : this(isLogMessage, false, false, logType, CommandMsgType.NoCommand, entry)
    {
    }

    /// <summary>
    /// initialize a new chat message
    /// </summary>
    /// <param name="isCommandMessage">is the chat entry a command message</param>
    /// <param name="cmdType">type of the command message</param>
    /// <param name="entry">game object that displays the chat messag</param>
    public MessageEntry(bool isCommandMessage, CommandMsgType cmdType, GameObject entry) : this(isCommandMessage, isCommandMessage, false, LogType.Log, cmdType, entry)
    {
    }

    /// <summary>
    /// initialize a new chat message
    /// </summary>
    /// <param name="isLogMessage">is the chat entry a log message</param>
    /// <param name="isCommandMessage">is the chat entry a command message</param>
    /// <param name="isStatusMessage">is the chat entry a status message</param>
    /// <param name="logType">type of the log message</param>
    /// <param name="cmdType">type of the command message</param>
    /// <param name="entry">game object that displays the chat messag</param>
    private MessageEntry(bool isLogMessage, bool isCommandMessage, bool isStatusMessage, LogType logType, CommandMsgType cmdType, GameObject entry)
    {
        this.isLogMessage = isLogMessage;
        this.isCommandMessage = isCommandMessage;
        this.isStatusMessage = isStatusMessage;
        this.logType = logType;
        this.cmdType = cmdType;
        this.entry = entry;
    }
}

/// <summary>
/// Shows a list of a text prefab.
/// Used to show the messages that are sent/received in the ChatApp.
/// </summary>
public class ChatMessageList : MonoBehaviour
{
    #region properties
    /// <summary>
    /// References to the "Text" prefab.
    /// Needs to contain RectTransform and Text element.
    /// </summary>
    public GameObject uEntryPrefab;

    /// <summary>
    /// Reference to the own RectTransform
    /// </summary>
    private RectTransform mOwnTransform;
    private RectTransform MOwnTransform
    {
        get
        {
            if (mOwnTransform == null)
            {
                mOwnTransform = this.GetComponent<RectTransform>();
            }
            return mOwnTransform;
        }
    }

    /// <summary>
    /// Number of messages until the older messages will be deleted.
    /// </summary>
    public int mMaxMessages = 50;

    private int mCounter = 0;
    private bool newEntryAdded = false;

    /// <summary>
    /// container for chat messages
    /// </summary>
    private List<MessageEntry> messageEntries;
    private List<MessageEntry> MessageEntries
    {
        get
        {
            if (messageEntries == null) messageEntries = new List<MessageEntry>();
            return messageEntries;
        }
    }
    #endregion

    #region unity loop
    private void Awake()
    {
        ActionEventManager.Subscribe(EventName.ClearMessageBox, ClearAllMessages);
    }

    void OnDestroy()
    {
        ActionEventManager.Unsubscribe(EventName.ClearMessageBox, ClearAllMessages);
    }

    private void Start()
    {
        foreach(var v in MOwnTransform.GetComponentsInChildren<RectTransform>())
        {
            if(v != MOwnTransform)
            {
                v.name = "Element " + mCounter;
                mCounter++;
            }
        }
    }

    /// <summary>
    /// Destroys old messages if needed and repositions the existing messages.
    /// </summary>
    private void Update()
    {
        if (!debug) ClearLogMessages(MessageEntries.Count - mMaxMessages);
        ClearMessages(MessageEntries.Count - mMaxMessages);

        if (newEntryAdded)
        {
            newEntryAdded = false;
            ShowDebug(debug);
        }
    }
    #endregion

    #region edit
    /// <summary>
    /// Allows the chat to add new entires to the list
    /// </summary>
    /// <param name="text">Text to be added</param>
    public void AddTextEntry(string text, TextAnchor align)
    {
        text = text.Trim();
        if (text.Length == 0) return;

        var item = new MessageEntry(CreateTextEntry(text, Color.black, align));
        MessageEntries.Add(item);
        newEntryAdded = true;
    }

    /// <summary>
    /// Allows the chat to add new status entires to the list
    /// </summary>
    /// <param name="text">Text to be added</param>
    public void AddStatusTextEntry(string text)
    {
        text = text.Trim();
        if (text.Length == 0) return;

        var item = new MessageEntry(true, CreateTextEntry(text, Color.gray, TextAnchor.UpperLeft));
        MessageEntries.Add(item);
        newEntryAdded = true;
    }

    /// <summary>
    /// Allows the chat to add new debug log entires to the list
    /// </summary>
    /// <param name="logType">debug type (error, warning, log)</param>
    /// <param name="text">Text to be added</param>
    public void AddLogTextEntry(LogType logType, string text)
    {
        text = text.Trim();
        if (text.Length == 0) return;

        Color color = Color.gray;
        switch (logType)
        {
            case LogType.Error:
                color = Color.red;
                break;
            case LogType.Assert:
                color = Color.red;
                break;
            case LogType.Warning:
                color = Color.yellow;
                break;
            case LogType.Log:
                color = Color.green;
                break;
            case LogType.Exception:
                color = Color.red;
                break;
            default:
                break;
        }

        var item = new MessageEntry(true, logType, CreateTextEntry(text, color, TextAnchor.UpperLeft));
        MessageEntries.Add(item);
        newEntryAdded = true;
    }

    /// <summary>
    /// Allows the chat to add new command entires to the list
    /// </summary>
    /// <param name="cmdType">command message type</param>
    /// <param name="text">Text to be added</param>
    public void AddCommandTextEntry(CommandMsgType cmdType, string text)
    {
        text = text.Trim();
        if (text.Length == 0) return;

        Color color = Color.gray;
        var item = new MessageEntry(true, cmdType, CreateTextEntry(Commands.getCommandString(cmdType, text), color, TextAnchor.UpperLeft));
        MessageEntries.Add(item);
        newEntryAdded = true;
    }

    /// <summary>
    /// create a new text entry game object from prefab
    /// </summary>
    /// <param name="text">>text message<</param>
    /// <param name="color">text color</param>
    /// <param name="align">text align</param>
    /// <returns></returns>
    private GameObject CreateTextEntry(string text, Color color, TextAnchor align)
    {
        GameObject ngp = Instantiate(uEntryPrefab);
        Text t = ngp.GetComponentInChildren<Text>();
        t.text = text;
        t.color = color;
        t.alignment = align;

        RectTransform transform = ngp.GetComponent<RectTransform>();
        transform.SetParent(MOwnTransform, false);

        GameObject go = transform.gameObject;
        go.name = "Element " + mCounter;
        mCounter++;

        return transform.gameObject;
    }

    /// <summary>
    /// delete all messages in UI
    /// </summary>
    public void ClearAllMessages()
    {
        ClearMessages(MessageEntries.Count);
    }

    /// <summary>
    /// delete only log messages
    /// </summary>
    /// <param name="range">how many messages should be deleted</param>
    public void ClearLogMessages(int range)
    {
        var logMsg = MessageEntries.Where(x => x.isLogMessage).ToArray();

        for (int i = 0; i < range; i++)
        {
            var msg = logMsg[i];
            MessageEntries.Remove(msg);
            Destroy(msg.entry);
        }
    }

    /// <summary>
    /// delete messages in UI
    /// </summary>
    /// <param name="range">how many messages should be deleted</param>
    public void ClearMessages(int range)
    {
        for (int i = 0; i < range; i++)
        {
            Destroy(MessageEntries[0].entry);
            MessageEntries.RemoveAt(0);
        }
    }

    private bool debug = false;
    /// <summary>
    /// set the chat message visible in UI
    /// </summary>
    /// <param name="item">chat message</param>
    private void setVisible(MessageEntry item)
    {
        try
        {
            bool showEntry = (debug || !item.isLogMessage);
            item.entry.SetActive(showEntry);
        }
        catch(System.Exception e)
        {

        }
    }

    /// <summary>
    /// should debug messages be displayed in UI
    /// </summary>
    /// <param name="debug">if true display debug messages</param>
    public void ShowDebug(bool debug)
    {
        this.debug = debug;
        foreach (var item in MessageEntries)
        {
            setVisible(item);
        }
    }
    #endregion

}
