using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using WearHFPlugin;
using System.Text.RegularExpressions;
using UnityEngine.Android;

/// <summary>
/// List of email contacts on HMT1 for sending the key for connecting to this client
/// Create/show/hide the contactList
/// Scroll the list with head movement and select/send to a contact by voice command on the HMT1
/// </summary>
public class ContactList : MonoBehaviour
{
    [Tooltip("the contacts the list should show")]
    public Contacts contacts;
    [Tooltip("the RectTransform which should contain the list")]
    public RectTransform container;
    [Tooltip("hint for voice commands")]
    public RectTransform hint;
    [Tooltip("activated, if contacts could not be read from file")]
    public RectTransform warning;
    [Tooltip("Prefab for one list entry")]
    public ContactListEntry listEntryPrefab;
    [Tooltip("sensitivity factor for scrolling the list with head movement")]
    public float sensitivity;

    [Header("for testing only:")]
    [Tooltip("scroll the list in the editor (instead of head movement)")]
    public float offset;

    private string messageToSend;
    private List<ContactListEntry> contactList;
    private WearHF wearHf;

    private static ContactList _instance;
    public static ContactList instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindObjectOfType<ContactList>();
            }
            return _instance;
        }
    }

    /// <summary>
    /// Set the message to send
    /// </summary>
    /// <param name="message">the message to send to a contact (the key for connecting)</param>
    public void SendMessageToContact(string message)
    {
        Debug.Log("SendMessageToContact");
        messageToSend = message;
        this.gameObject.SetActive(true);
    }

    private void Awake()
    {
        _instance = this;

        AskForPermissions();
    }

    private void AskForPermissions()
    {
#if PLATFORM_ANDROID
        if (!Permission.HasUserAuthorizedPermission(Permission.Camera))
        {
            Permission.RequestUserPermission(Permission.Camera);
        }
        if (!Permission.HasUserAuthorizedPermission(Permission.Microphone))
        {
            Permission.RequestUserPermission(Permission.Microphone);
        }
#endif
    }

    /// <summary>
    /// Create and show the contacts list (or the warning in case of an error reading the contacts file)
    /// </summary>
    private void OnEnable()
    {
        HideWarning();

        if (contacts == null)
        {
            Debug.LogError("Contacts in ContactList not defined");
            return;
        }
        else
        {
            int result = contacts.LoadFromFile();
            if (result < 0)
            {
                Debug.LogWarning("contacts file not found");
                ShowWarningNotFound();
            }
            else if (result == 0)
            {
                Debug.LogWarning("contacts file empty");
                ShowWarningEmpty();
            }
            else
            {
                ShowContacts();
            }
        }
    }

    private void OnDisable()
    {
        HideContacts();
        HideWarning();
    }

    private void Update()
    {
        if (contactList != null && contactList.Count > 0)
        {
            UpdateEntries();
        }
    }

    /// <summary>
    /// Update the position of each list entry
    /// If the contacts fit into the view, place them centered
    /// If the contacts do not fit into the view, place them aligned to the top and enable scrolling
    /// Scoll with Gyro input (head movement on the hmt1)
    /// </summary>
    private void UpdateEntries()
    {
        float entryHeight = listEntryPrefab.entry.sizeDelta.y;
        float height = entryHeight * contactList.Count;
        float maxHeight = container.rect.height - 20;
        float cOffset;
        if (height > maxHeight) //align entries to top and enable scrolling
        {
            cOffset = 0.5f * (maxHeight - entryHeight);

            Input.gyro.enabled = true;
            float scrollDelta = -Input.gyro.rotationRate.y;

            offset += sensitivity * scrollDelta;
            offset = Mathf.Clamp(offset, 0, (height - maxHeight) / entryHeight);
        }
        else //center entries vertically
        {
            cOffset = 0.5f * (height - entryHeight);
            offset = 0;
        }

        for (int i = 0; i < contactList.Count; i++)
        {
            float position = i - offset;

            contactList[i].entry.localPosition = (cOffset - position * contactList[i].entry.sizeDelta.y) * Vector3.up;
        }
    }

    /// <summary>
    /// Callback for detected voicecommands
    /// Send an email to the selected contact
    /// </summary>
    /// <param name="voiceCommand">the detected voicecommand</param>
    private void SendVoiceCommandCallback(string voiceCommand)
    {
        //Check for contact name:
        for (int i = 0; i < contactList.Count; i++)
        {
            if (voiceCommand == contactList[i].contactName)
            {
                SendToContact(i);
                return;
            }
        }

        //Check for id:
        int id = int.Parse(Regex.Match(voiceCommand, @"\d+").Value);
        SendToContact(id - 1);
    }

    /// <summary>
    /// Send an email with the remote support key to the selected contact
    /// </summary>
    /// <param name="index">position in the contact list of the selected contact (0...length-1)</param>
    public void SendToContact(int index)
    {
        string receiverName = contactList[index].contactName;
        string receiverAddress = contactList[index].contactAddress;

        Debug.Log(string.Format("Entry {0} selected: {1} - {2} ", index, receiverName, receiverAddress));

        string subject = "MRBC4i Remote Support request";

        Email.Send(receiverAddress, subject, messageToSend);

        messageToSend = "";
        this.gameObject.SetActive(false);
    }

    /// <summary>
    /// Add <see cref="Contacts"/> to the list
    /// Add voicecommands for all entries in the list
    /// Enable the list and voice command hint
    /// </summary>
    private void ShowContacts()
    {
        if (contactList == null)
        {
            AddContacts();
        }

        AddVoiceCommands();

        container.gameObject.SetActive(true);
        hint.gameObject.SetActive(true);
    }

    /// <summary>
    /// Remove voice commands and disable the list and voice command hint
    /// </summary>
    private void HideContacts()
    {
        RemoveVoiceCommands();

        container.gameObject.SetActive(false);
        hint.gameObject.SetActive(false);
    }

    /// <summary>
    /// Add all contacts to the list and set the voice command hint
    /// </summary>
    private void AddContacts()
    {
        contactList = new List<ContactListEntry>();

        for (int i = 0; i < contacts.entries.Count; i++)
        {
            string name = contacts.entries[i].name;
            string address = contacts.entries[i].address;
            AddContact(i + 1, name, address);
        }

        TextMeshProUGUI tmpHint = hint.GetComponentInChildren<TextMeshProUGUI>();
        tmpHint.text = string.Format("Objekt 1...{0}", contactList.Count);
    }

    /// <summary>
    /// Add one contact to the list
    /// </summary>
    /// <param name="id">position of the entry in the list</param>
    /// <param name="name">name of the contact</param>
    /// <param name="address">mail address of the contact</param>
    private void AddContact(int id, string name, string address)
    {
        ContactListEntry entry = Instantiate(listEntryPrefab, container);
        entry.gameObject.SetActive(true);

        entry.contactName = name;
        entry.contactAddress = address;
        entry.text.text = string.Format("{0} ({1})", name, address);
        entry.number.text = string.Format("{0}.", id);

        contactList.Add(entry);
    }

    /// <summary>
    /// Remove all entries from the contact list
    /// </summary>
    private void RemoveContacts()
    {
        if (contactList != null)
        {
            foreach (ContactListEntry cle in contactList)
            {
                Destroy(cle.gameObject);
            }
            contactList = null;
        }
    }

    /// <summary>
    /// Add two voice commands for each list entry:
    /// 1. Objekt {id}
    /// 2. name of the contact
    /// </summary>
    private void AddVoiceCommands()
    {
        if (wearHf == null)
        {
            wearHf = FindObjectOfType<WearHF>();
        }
        if (wearHf == null)
        {
            Debug.LogWarning("Could not add voice commands");
            return;
        }

        for (int i = 0; i < contactList.Count; i++)
        {
            string voiceCommand = string.Format("Objekt {0}", i + 1);
            wearHf.AddVoiceCommand(voiceCommand, SendVoiceCommandCallback);
            voiceCommand = contactList[i].contactName;
            wearHf.AddVoiceCommand(voiceCommand, SendVoiceCommandCallback);
        }
    }

    /// <summary>
    /// Remove voice commands for selecting a list entry:
    /// </summary>
    private void RemoveVoiceCommands()
    {
        if (wearHf == null)
        {
            wearHf = FindObjectOfType<WearHF>();
        }
        if (wearHf == null)
        {
            Debug.LogWarning("Could not remove voice commands");
            return;
        }

        for (int i = 0; i < contactList.Count; i++)
        {
            string voiceCommand = string.Format("Objekt {0}", i + 1);
            wearHf.RemoveVoiceCommand(voiceCommand);
            voiceCommand = contactList[i].contactName;
            wearHf.RemoveVoiceCommand(voiceCommand);
        }
    }

    /// <summary>
    /// Show a warning message instead of the contact list (if contacts file could not be read)
    /// </summary>
    /// <param name="message"></param>
    private void ShowWarning(string message)
    {
        warning.gameObject.SetActive(true);
        TextMeshProUGUI tmp = warning.GetComponentInChildren<TextMeshProUGUI>();
        tmp.text = message;
    }

    /// <summary>
    /// Hide the warning
    /// </summary>
    private void HideWarning()
    {
        warning.gameObject.SetActive(false);
    }

    /// <summary>
    /// Show this message, if the contacts file was not found
    /// </summary>
    private void ShowWarningNotFound()
    {
        string message = string.Format("Contacts file not found.\nCreate a text file called '{0}' in '{1}'.\nIn the file add one contact per line, e.g. 'Name {2} name@gmail.com'", contacts.fileName, contacts.GetPath(), contacts.separator);
        ShowWarning(message);
    }

    /// <summary>
    /// Show this message, if the contacts file is empty
    /// </summary>
    private void ShowWarningEmpty()
    {
        string message = string.Format("Contacts file is empty.\nAdd contacts in '{0}'.\nOne contact per line, e.g. 'Name {1} name@gmail.com'", contacts.GetFilePath(), contacts.separator);
        ShowWarning(message);
    }

}
