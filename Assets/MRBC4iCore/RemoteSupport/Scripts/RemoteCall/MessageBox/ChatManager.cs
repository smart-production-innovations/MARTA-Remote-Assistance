using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// chat message type
/// </summary>
public enum ChatMessageType
{
    Message,
    Debug,
    Status,
    Command
}

/// <summary>
/// Container item for chat messages
/// </summary>
public struct ChatMessage
{
    public ChatMessageType ChatMessageType { get; set; }
    public string Text { get; set; }
    public TextAnchor Align { get; set; }
    public LogType LogType { get; set; }
    public CommandMsgType CmdType { get; set; }

    /// <summary>
    /// initialize a new chat message
    /// </summary>
    /// <param name="type">message type</param>
    /// <param name="textMessage">message text</param>
    public ChatMessage(ChatMessageType type, string textMessage)
    {
        LogType = LogType.Log;
        Align = TextAnchor.UpperLeft;
        CmdType = CommandMsgType.NoCommand;
        Text = textMessage;
        ChatMessageType = type;
    }
}

/// <summary>
/// Manages the chat box communication between the devices
/// </summary>
public class ChatManager : AManager<ChatManager>
{
    #region properties
    [Header("Chat panel elements")]
    /// <summary>
    /// Input field to enter a new message.
    /// </summary>
    public InputField uMessageInputField;

    /// <summary>
    /// Output message list to show incoming and sent messages + output messages of the
    /// system itself.
    /// </summary>
    public ChatMessageList uMessageOutput;

    /// <summary>
    /// Send button.
    /// </summary>
    public Button uSendMessageButton;

    public GameObject uToggleUI;
    public SnackbarController uChatMessageSnackbar;

    public bool addInfoTextToSnackbar = true;

    [SerializeField]
    private bool interactable = true;
    /// <summary>
    /// This determines if this component will accept input. When it is set to false interaction is disabled and the transition state will be set to the disabled state.
    /// </summary>
    public bool Interactable
    {
        get { return interactable; }
        set
        {
            interactable = value;
            var controlles = GetComponentsInChildren<Selectable>();

            foreach (var item in controlles)
            {
                item.interactable = interactable;
            }
        }
    }

    /// <summary>
    /// toggle chat box on or off
    /// </summary>
    public bool ChatBoxActive
    {
        get
        {
            if (uToggleUI)
            {
                return uToggleUI.activeSelf;
            }
            return gameObject.activeSelf;
        }
        set
        {
            if (uToggleUI)
            {
                uToggleUI.SetActive(value);
            }
            else
            {
                gameObject.SetActive(value);
            }
        }
    }
    #endregion

    #region unity loop
    protected override void Awake()
    {
        base.Awake();

        ActionEventManager.Subscribe<string>(EventName.Append, Append);
        ActionEventManager.Subscribe<string>(EventName.AppendDebug, AppendDebug);
        ActionEventManager.Subscribe<ChatMessage>(EventName.Append, Append);
        Application.logMessageReceived += Log;
    }

    void OnDestroy()
    {
        ActionEventManager.Unsubscribe<string>(EventName.Append, Append);
        ActionEventManager.Unsubscribe<string>(EventName.AppendDebug, AppendDebug);
        ActionEventManager.Unsubscribe<ChatMessage>(EventName.Append, Append);
        Application.logMessageReceived -= Log;
    }
    #endregion

    #region init
    /// <summary>
    /// Shows the setup screen or the chat + video
    /// </summary>
    /// <param name="showSetup">true Shows the setup. False hides it.</param>
    public void SetGuiState(bool showSetup)
    {
        uSendMessageButton.interactable = !showSetup;
        uMessageInputField.interactable = !showSetup;
    }

    /// <summary>
    /// set default message input field text
    /// </summary>
    /// <param name="text"></param>
    public void SetDefaultMessage(string text)
    {
        uMessageInputField.text = text;
    }
    #endregion

    #region append message
    /// <summary>
    /// Adds a new message to the message view
    /// </summary>
    /// <param name="text">text message</param>
    /// <param name="align">text align</param>
    public void Append(string text, TextAnchor align)
    {
        if (uMessageOutput != null)
        {
            uMessageOutput.AddTextEntry(text, align);
        }
    }

    /// <summary>
    /// Adds a new message to the message view
    /// </summary>
    /// <param name="text">text message</param>
    public void Append(string text)
    {
        if (uMessageOutput != null)
        {
            uMessageOutput.AddTextEntry(text, TextAnchor.UpperLeft);
        }
    }

    /// <summary>
    /// Adds a new message to the message view
    /// </summary>
    /// <param name="msg">text message with parameters</param>
    public void Append(ChatMessage msg)
    {
        switch (msg.ChatMessageType)
        {
            case ChatMessageType.Message:
                Append(msg.Text, msg.Align);
                break;
            case ChatMessageType.Debug:
                AppendDebug(msg.LogType, msg.Text);
                break;
            case ChatMessageType.Status:
                AppendStatus(msg.Text);
                break;
            case ChatMessageType.Command:
                AppendCommand(msg.CmdType, msg.Text);
                break;
            default:
                break;
        }
    }

    /// <summary>
    /// Adds a new debug message to the message view
    /// </summary>
    /// <param name="logType">debug type (error, warning, log)</param>
    /// <param name="text">text message</param>
    public void AppendDebug(LogType logType, string text)
    {
        if (uMessageOutput != null)
        {
            uMessageOutput.AddLogTextEntry(logType, text);
        }
    }

    /// <summary>
    /// Adds a new debug message of type log to the message view
    /// </summary>
    /// <param name="text">text message</param>
    public void AppendDebug(string text)
    {
        if (uMessageOutput != null)
        {
            uMessageOutput.AddLogTextEntry(LogType.Log, text);
        }
    }

    /// <summary>
    /// Adds a new status message to the message view
    /// </summary>
    /// <param name="text">text message</param>
    public void AppendStatus(string text)
    {
        if (uMessageOutput != null)
        {
            uMessageOutput.AddStatusTextEntry(text);
        }
    }

    /// <summary>
    /// Adds a new command message to the message view
    /// </summary>
    /// <param name="cmdType">command message type</param>
    /// <param name="text">text message</param>
    public void AppendCommand(CommandMsgType cmdType, string text)
    {
        if (uMessageOutput != null)
        {
            uMessageOutput.AddCommandTextEntry(cmdType, text);
        }
    }

    /// <summary>
    /// delete all messages
    /// </summary>
    public void ClearMessageBox()
    {
        if (uMessageOutput != null)
        {
            uMessageOutput.ClearAllMessages();
        }
    }

    /// <summary>
    /// Adds a new debug message to the message view
    /// </summary>
    /// <param name="logString">text message</param>
    /// <param name="stackTrace">debug stack</param>
    /// <param name="type">debug type (error, warning, log)</param>
    public void Log(string logString, string stackTrace, LogType type)
    {
        AppendDebug(type, type.ToString() + ": " + logString + (type == LogType.Exception ? " ---- " + stackTrace : ""));
    }

    /// <summary>
    /// Notify the user of a new chat message with a snack bar hint when the chat is hidden.
    /// </summary>
    /// <param name="msg">new chat message</param>
    public void MessageHint(string msg)
    {
        if (!ChatBoxActive && uChatMessageSnackbar)
        {
            if (addInfoTextToSnackbar)
                uChatMessageSnackbar.Text = "New chat message: " + msg;
            else
                uChatMessageSnackbar.Text = msg;
            uChatMessageSnackbar.gameObject.SetActive(true);
        }
    }
    #endregion

    #region send message
    /// <summary>
    /// This is called if the send button
    /// </summary>
    public void SendButtonPressed()
    {
        //get the message written into the text field
        string msg = uMessageInputField.text;
        SendMsg(msg);
    }

    /// <summary>
    /// Sends a message to the other end
    /// </summary>
    /// <param name="msg"></param>
    public void SendMsg(string msg)
    {
        if (String.IsNullOrEmpty(msg))
        {
            //never send null or empty messages. webrtc can't deal with that
            return;
        }

        Append(msg, TextAnchor.UpperRight);
        ActionEventManager.SendEvent<string>(EventName.WebRTCSend, msg);

        //reset UI
        uMessageInputField.text = "";
        if (!Application.isMobilePlatform)
            uMessageInputField.Select();
        else
            uSendMessageButton.Select();
    }

    /// <summary>
    /// receive a message from the other end
    /// </summary>
    /// <param name="msg"></param>
    public void ReceiveMsg(string msg)
    {
        if (!msg.StartsWith("@"))
        {
            Append(msg, TextAnchor.UpperLeft);
        }
    }
    #endregion
}
