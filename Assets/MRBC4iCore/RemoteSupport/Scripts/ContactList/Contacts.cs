using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

/// <summary>
/// Read email contacts from a textfile
/// Each line contains one contact e.g.: name {separator} address
/// Ignores empty lines
/// Ignores white spaces at the beginning and end of name and address
/// </summary>
public class Contacts : MonoBehaviour
{
    [Tooltip("name of the file containing the contacts; on HMT1 it has to be located in 'Android\\data\\com.mrbc4i.RemoteSupport\\files'")]
    public string fileName;

    [Tooltip("separator between name and address in contacts-file")]
    public char separator;

    [Tooltip("List containing all the contacts (name and email address)")]
    public List<Contact> entries;

    public struct Contact
    {
        public string name;
        public string address;

        public Contact(string name, string address)
        {
            this.name = name;
            this.address = address;
        }
    }

    private string regexEol = @"\r\n?|\n";      //Regex for detecting end of line
    private string regexSpace = @"^\s+|\s+$";   //Regex for detecting all white spaces at the beginning an end of a string

    /// <summary>
    /// load contacts from file with name <see cref="fileName"/>
    /// contacts file structure:
    ///     one contact per line
    ///     ignores white spaces at start/end and empty lines
    ///     line structure: name, then email address, separated by character <see cref="separator"/>
    ///     e.g. 'Name; name@gmail.com'
    /// </summary>
    /// <returns>-1 if file was not found or else the number of contacts found in the file</returns>
    public int LoadFromFile()
    {
        string filePath = GetFilePath();

        if (!File.Exists(filePath))
        {
            Debug.LogWarning("file for contacts not found");
            return -1;
        }
        else
        {
            Debug.Log("file for contacts found");
        }

        entries = new List<Contact>();

        StreamReader sr = new StreamReader(filePath);
        string line;
        string[] separatedLine;
        while ( (line = sr.ReadLine()) != null)
        {
            line = Regex.Replace(line, regexEol, ""); //remove end of line characters
            separatedLine = line.Split(separator);
            if (separatedLine.Length == 2)
            {
                //remove white spaces at start and end of the name/address:
                string name = Regex.Replace(separatedLine[0], regexSpace, "");
                string address = Regex.Replace(separatedLine[1], regexSpace, "");

                entries.Add(new Contact(name, address));
            }
        }

        return entries.Count;
    }

    /// <summary>
    /// get the persistent data path where the contacts file will be stored
    /// </summary>
    /// <returns>path as a string</returns>
    public string GetPath()
    {
        return Application.persistentDataPath;
    }

    /// <summary>
    /// full path of the contacts file
    /// </summary>
    /// <returns>file path as a string</returns>
    public string GetFilePath()
    {
        return GetPath() + "/" + fileName;
    }

    /// <summary>
    /// Check, if there are contact entries or not
    /// </summary>
    /// <returns></returns>
    public bool IsEmpty()
    {
        return entries == null || entries.Count <= 0;
    }
    
}
