using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;
using System.Linq;

/// <summary>
/// define container entry for status command messages between devices
/// </summary>
public struct CommandMsg
{
    public CommandMsgType Command;
    public string Message;
    public int Block;

    public CommandMsg(CommandMsgType command, string message)
    {
        Command = command;
        Message = message;
        Block = -1;
    }

    public CommandMsg(CommandMsgType command, string message, int block) : this (command, message)
    {
        Block = block;
    }
}

/// <summary>
/// types of status command messages between devices
/// </summary>
public enum CommandMsgType
{
    NoCommand,
    VideoSize,
    MouseDown,
    ImageData,
    AnchorState,
    DataReceived,
    DeleteAnchor,
    CancelAnchor,
    VideoFrameReceived,
    BandwidthOptions,
    AnchorId,
    CallForAnchorData,
    CallForNextAnchorData,
    CallForPreviousAnchorData,
    ResendBlock,
    ConvertInto3DMousePosition,
    NoAnchorDataFound,
    CallForAnchorSnapshotData,
    CallForGalleryItems,
    GalleryItems,
	GalleryOpen,
    GalleryClose,
    GallerySwitchToId,
    ProjectionPlaneSelected,
    ARMode,
    CalculationMode,
    NewGalleryItem,
    SupportsARMode,
    StatusProperty,
    IsSmartGlassCommunication,
    DeviceResolution,
    DrawingColor,
    DrawingPoint,
    ParticleAnnotationType,
    ClearAllParticle,
    StopParticleAnnotation,
    ARFieldOfView
}

/// <summary>
/// serialize and deserialize status command messages between devices in the same defined way on each device
/// </summary>
public static class Commands
{
    private static IFormatProvider iFormatProvider = iFormatProvider = new CultureInfo("de-AT");

    /// <summary>
    /// parse command parameter from command string
    /// </summary>
    /// <param name="cmd">full command text string</param>
    /// <param name="commandName">name of the command</param>
    /// <returns></returns>
    public static string getCommendParam(string cmd, CommandMsgType commandName)
    {
        return cmd.Substring(commandName.ToString().Length + 1);
    }

    /// <summary>
    /// is message string a command of given command type
    /// </summary>
    /// <param name="cmd">message string</param>
    /// <param name="commandName">name of the command which should be checked</param>
    /// <param name="param">return command parameter</param>
    /// <returns>is message string a command of given command type</returns>
    public static bool isCommand(string cmd, CommandMsgType commandName, out string param)
    {
        if (cmd.StartsWith(commandName.ToString()))
        {
            param = getCommendParam(cmd, commandName);
            return true;
        }
        else
        {
            param = "";
            return false;
        }
    }

    /// <summary>
    /// convert string to vector2 coordinates
    /// </summary>
    /// <param name="param">coordinates string in format x/y</param>
    /// <returns>vector2 coordinates</returns>
    public static Vector2 parseCoordinates(string param)
    {
        Vector2 coord = Vector2.zero;
        var size = param.Split('/');
        coord.x = float.Parse(size[0], iFormatProvider);
        coord.y = float.Parse(size[1], iFormatProvider);
        return coord;
    }

    /// <summary>
    /// convert string to vector3 coordinates
    /// </summary>
    /// <param name="param">coordinates string in format x/y</param>
    /// <returns>vector3 coordinates</returns>
    public static Vector3 parseVector3(string param)
    {
        Vector3 coord = Vector3.zero;
        var size = param.Split('/');
        coord.x = float.Parse(size[0], iFormatProvider);
        coord.y = float.Parse(size[1], iFormatProvider);
        coord.z = float.Parse(size[2], iFormatProvider);
        return coord;
    }

    /// <summary>
    /// convert string to color
    /// </summary>
    /// <param name="param">color string in format r/g/b</param>
    /// <returns>color</returns>
    public static Color parseColor(string param)
    {
        Color color = Color.black;
        var size = param.Split('/');
        color.r = float.Parse(size[0], iFormatProvider);
        color.g = float.Parse(size[1], iFormatProvider);
        color.b = float.Parse(size[2], iFormatProvider);
        return color;
    }

    /// <summary>
    /// Converts the string representation of a float number using the format iFormatProvider.
    /// </summary>
    /// <param name="param">string value</param>
    /// <returns>float value</returns>
    public static float parseFloat(string param)
    {
        return float.Parse(param, iFormatProvider);
    }

    /// <summary>
    /// Converts the array representation of a ;-separated string to a List.
    /// </summary>
    /// <typeparam name="T">type of the list entry</typeparam>
    /// <param name="param">string value</param>
    /// <returns>list</returns>
    public static List<T> parseArray<T>(string param)
    {
        var elements = param.Split(';');
        var list = new List<T>();
        foreach (var item in elements)
        {
            try
            {
                if (!string.IsNullOrEmpty(item))
                {
                    var val = (T)Convert.ChangeType(item, typeof(T));
                    list.Add(val);
                }
            }
            catch (FormatException e) { }
        }
        return list;
    }

    /// <summary>
    /// convert coordinates to string
    /// </summary>
    /// <param name="x"></param>
    /// <param name="y"></param>
    /// <returns>coordinates string</returns>
    public static string getCoordinatesString(int x, int y)
    {
        return x + "/" + y;
    }

    /// <summary>
    /// convert the bandwidth option values to a string
    /// </summary>
    /// <param name="quality"></param>
    /// <param name="fps"></param>
    /// <param name="supportType"></param>
    /// <returns></returns>
    public static string getBandwidthOptionsString(int quality = -1, int fps = -1, int supportType = -1)
    {
        string options = "";
        if (quality > 0) options += "Quality=" + quality + ";";
        else if (quality == 0) options += "Quality=Auto;";
        if (fps > 0) options += "FPS=" + fps + ";";
        else if (fps == 0) options += "FPS=Auto;";
        if (Enum.GetValues(typeof(SupportModeType)).Cast<int>().Any(x => x == supportType))
            options += "Mode=" + ((SupportModeType)supportType).ToString() + ";";

        return options;
    }

    /// <summary>
    /// convert bandwidth parameter string to out values.
    /// </summary>
    /// <param name="param">; separated bandwidth options. possible parameters Quality=;FPS=;Mode=</param>
    /// <param name="quality">out parameter quality</param>
    /// <param name="fps">out parameter fps</param>
    /// <param name="supportType">out parameter mode</param>
    public static void parseBandwidthOptionsString(string param, out int quality, out int fps, out int supportType)
    {
        quality = -1;
        fps = -1;
        supportType = -1;

        var options = param.Split(';');
        foreach (var option in options)
        {
            var split = option.Split('=');
            if (split.Length > 1)
            {
                int val = 0;
                int.TryParse(split[1], out val);
                if (option.StartsWith("Quality=")) quality = val;
                if (option.StartsWith("FPS=")) fps = val;
                if (option.StartsWith("Mode=")) supportType = (int)Enum.Parse(typeof(SupportModeType), split[1]);
            }
        }
    }

    /// <summary>
    /// convert coordinates to string
    /// </summary>
    /// <param name="x"></param>
    /// <param name="y"></param>
    /// <returns>coordinates string</returns>
    public static string getCoordinatesString(float x, float y)
    {
        return x.ToString(iFormatProvider) + "/" + y.ToString(iFormatProvider);
    }

    /// <summary>
    /// convert coordinates to string
    /// </summary>
    /// <param name="vector"></param>
    /// <returns>coordinates string</returns>
    public static string getCoordinatesString(Vector3 vector)
    {
        return vector.x.ToString(iFormatProvider) + "/" + vector.y.ToString(iFormatProvider) + "/" + vector.z.ToString(iFormatProvider);
    }

    /// <summary>
    /// Converts a float number to a string using the format iFormatProvider.
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    public static string getFloatString(float value)
    {
        return value.ToString(iFormatProvider);
    }

    /// <summary>
    /// Converts a array to a ;-separated string.
    /// </summary>
    /// <typeparam name="T">type of the list entry</typeparam>
    /// <param name="value">array which should be converted</param>
    /// <returns>;-separated string.</returns>
    public static string getArrayString<T>(this List<T> value)
    {
        string itemsString = "";
        foreach (var item in value)
        {
            if (itemsString.Length > 0) itemsString += ";";
            itemsString += item.ToString();
        }
        return itemsString;
    }

    /// <summary>
    /// convert color to string
    /// </summary>
    /// <param name="color"></param>
    /// <returns>coordinates string</returns>
    public static string getColorString(Color color)
    {
        return color.r.ToString(iFormatProvider) + "/" + color.g.ToString(iFormatProvider) + "/" + color.b.ToString(iFormatProvider);
    }

    /// <summary>
    /// convert new command parameter to defined command string
    /// </summary>
    /// <param name="command">command name</param>
    /// <param name="msg">command parameter</param>
    /// <param name="block">commands have a maximal length to could be send by webrtd. Send long command in several blocks</param>
    /// <returns></returns>
    public static string getCommandString(CommandMsgType command, string msg, int block = -1)
    {
        if (block >= 0)
        {
            return "@" + command + "=" + block.ToString("0000") + msg;
        }
        return "@" + command + "=" + msg;
    }
}
