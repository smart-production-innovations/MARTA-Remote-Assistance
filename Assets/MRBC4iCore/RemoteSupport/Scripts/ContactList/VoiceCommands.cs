using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using WearHFPlugin;

public class VoiceCommands : MonoBehaviour
{
    public List<string> quitCommands;

    private WearHF wearHf;

    private void Awake()
    {
        wearHf = FindObjectOfType<WearHF>();

        wearHf.ClearCommands();
        foreach(string cmd in quitCommands)
        {
            wearHf.AddVoiceCommand(cmd, QuitVoiceCommandCallback);
        }
    }

    private void OnDestroy()
    {
        wearHf.ClearCommands();
    }

    private void QuitVoiceCommandCallback(string voiceCommand)
    {
        wearHf.ClearCommands();
        Application.Quit();
    }
}
