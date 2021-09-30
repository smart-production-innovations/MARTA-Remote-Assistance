using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;

public class UIFunctions : MonoBehaviour
{
    public ToggleEvent arToggle;
    public ToggleEvent muteToggle;
    public ToggleEvent volumeToggle;

    private class InitObjectState
    {
        public GameObject GameObject { get; set; }
        public bool active { get; set; }
    }
    public Sprite arrowImage;
    public ParticleAnnotationContainer localParticleAnnotation;

    private void Awake()
    {
        StatusProperties.OnPropertiesLoaded += StatusProperties_OnPropertiesLoaded;
    }

    private void OnDestroy()
    {
        StatusProperties.OnPropertiesLoaded -= StatusProperties_OnPropertiesLoaded;
    }

    private void Start()
    {
        if (!statusPropertiesLoaded && StatusProperties.Values.IsLoaded)
            StatusProperties_OnPropertiesLoaded();
    }

    private bool statusPropertiesLoaded = false;
    private void StatusProperties_OnPropertiesLoaded()
    {
        statusPropertiesLoaded = true;
        if (StatusProperties.Values.isServer)
        {
            if (arToggle) arToggle.isOn = StatusProperties.Values.ARActive;
            if (muteToggle) muteToggle.isOn = !StatusProperties.Values.DefaultExpertMicrophoneOn;
            if (volumeToggle) volumeToggle.isOn = !StatusProperties.Values.DefaultExpertSpeakerOn;
        }
        else
        {
            if (muteToggle) muteToggle.isOn = !StatusProperties.Values.DefaultClientMicrophoneOn;
            if (volumeToggle) volumeToggle.isOn = !StatusProperties.Values.DefaultClientSpeakerOn;
        }
    }

    private List<InitObjectState> initObjectStateList;
    private void OnEnable()
    {
        if (initObjectStateList == null)
        {
            initObjectStateList = new List<InitObjectState>();
            var allChildren = GetComponentsInChildren<Transform>(includeInactive: true);
            foreach (var item in allChildren)
            {
                initObjectStateList.Add(new InitObjectState() { GameObject = item.gameObject, active = item.gameObject.activeSelf }); 
            }
        }
        else
        {
            foreach (var item in initObjectStateList)
            {
                if (item.GameObject.activeSelf != item.active)
                {
                    item.GameObject.SetActive(item.active);
                }
            }
        }
    }

    private ParticleAnnotationType particleAnnotationType = ParticleAnnotationType.crosshair;
    public ParticleAnnotationType ParticleAnnotationType
    {
        get { return particleAnnotationType; }
        set
        {
            particleAnnotationType = value;
            if (localParticleAnnotation)
                localParticleAnnotation.setParticleAnnotationType(value);
            else
            {
                var typeName = value.ToString();
                EventNameManager.SendEventCommandMsg(new CommandMsg(CommandMsgType.ParticleAnnotationType, typeName));
            }
        }
    }

    public void SetParticleAnnotationTypeCrosshair()
    {
        ParticleAnnotationType = ParticleAnnotationType.crosshair;
    }

    public void SetParticleAnnotationTypeLine()
    {
        ParticleAnnotationType = ParticleAnnotationType.line;
    }

    public void SetParticleAnnotationTypeArrow()
    {
        ParticleAnnotationType = ParticleAnnotationType.arrow;
    }

    public void ClearParticles()
    {
        if (localParticleAnnotation)
        {
            var particleAnnotations = SearchHelper.FindSceneObjectsOfTypeAll<ParticleAnnotationContainer>();
            foreach (var particleAnnotation in particleAnnotations)
            {
                particleAnnotation.clearAll();
            }
        }
        else
            EventNameManager.SendEventCommandMsg(new CommandMsg(CommandMsgType.ClearAllParticle, ""));
    }

    /// <summary>
    /// toggle speaker on or off
    /// </summary>
    public void SetLoudspeaker(bool value)
    {
        CallSettings.Instance.SetLoudspeaker(value);
    }

    /// <summary>
    /// toggle microphone on or off
    /// </summary>
    public void SetMute(bool value)
    {
        CallSettings.Instance.SetMute(value);
    }

    public void DeleteImage()
    {
        if (StatusProperties.Values.isServer)
        {
            DrawingRemoteManager.Instance.ResetAll();
        }
        else
        {
            DrawingAnnotationManager.Instance.ResetAll();
        }
    }

    public void SetColor(Color color)
    {
        if (StatusProperties.Values.isServer)
            EventNameManager.SendEventCommandMsg(new CommandMsg(CommandMsgType.DrawingColor, Commands.getColorString(color)));
    }

    public void SaveFrame()
    {
        StartCoroutine(AsyncSaveFrame());
    }

    IEnumerator AsyncSaveFrame()
    {
        var video = SearchHelper.FindSceneObjectOfType<VideoStream>();
        if (video)
        {
            var videoImage = video.GetComponent<RawImage>();
            var tex = (Texture2D)videoImage.texture;
            tex = ARPlaneDisplayManager.FlipTexture(tex, ARPlaneDisplayManager.flipDirection.vertical);
            byte[] bytes = tex.EncodeToJPG();

            var imagePath = Environment.GetFolderPath(Environment.SpecialFolder.MyPictures);
            var dir = Path.Combine(imagePath, "MRBC4i", RemoteCallManager.Instance.UniqueID);
            if (!Directory.Exists(dir))
                Directory.CreateDirectory(dir);
            var path = Path.Combine(dir, DateTime.Now.ToString("yyMMddHHmmssfff") + ".jpg");
            File.WriteAllBytes(path, bytes);
        }
        yield return null;
    }
}
