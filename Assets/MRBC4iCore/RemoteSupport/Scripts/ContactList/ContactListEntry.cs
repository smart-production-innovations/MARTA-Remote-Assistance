using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

/// <summary>
/// Contains name and address of a contact and references to all components needed by <see cref="ContactList"/>
/// Attached to each entry of the contact list
/// </summary>
public class ContactListEntry : MonoBehaviour
{
    [Tooltip("name of the contact")]
    public string contactName;
    [Tooltip("mail address of the contact")]
    public string contactAddress;
    [Tooltip("RectTransform of this GameObject")]
    public RectTransform entry;
    [Tooltip("RectTransform of the Panel showing the name/address")]
    public RectTransform textPanel;
    [Tooltip("RectTransform of the Panel showing the entry number")]
    public RectTransform numberPanel;
    [Tooltip("Text showing the name/address")]
    public TextMeshProUGUI text;
    [Tooltip("Text showing the entry number")]
    public TextMeshProUGUI number;
}
