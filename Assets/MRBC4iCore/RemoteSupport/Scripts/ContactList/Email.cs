using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Mail;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using UnityEngine;

/// <summary>
/// class for sending emails
/// https://stackoverflow.com/questions/43720786/how-to-send-an-email-from-inside-application-in-unity
/// </summary>
public class Email : MonoBehaviour
{
    [Tooltip("the email address to send from")]
    public string fromAddress;
    [Tooltip("the password for the email address above")]
    public string fromPassword;
    [Tooltip("e.g. for gmail: smtp.gmail.com")]
    public string smtpServerAddress;
    [Tooltip("e.g. for gmail: 587")]
    public int smtpPort;

    private static Email instance;


    private void Awake()
    {
        instance = this;
    }

    /// <summary>
    /// Send an email
    /// </summary>
    /// <param name="toAddress">the recipients address</param>
    /// <param name="subject">the subject of the email</param>
    /// <param name="content">the content of the email</param>
    public static void Send(string toAddress, string subject, string content)
    {
        MailMessage mail = new MailMessage();
        mail.From = new MailAddress(instance.fromAddress);
        mail.To.Add(toAddress);

        mail.Subject = subject;
        mail.Body = content;

        SmtpClient smtpServer = new SmtpClient(instance.smtpServerAddress);
        smtpServer.Port = instance.smtpPort;
        smtpServer.Credentials = new System.Net.NetworkCredential(instance.fromAddress, instance.fromPassword) as ICredentialsByHost;
        smtpServer.EnableSsl = true;
        ServicePointManager.ServerCertificateValidationCallback =
        delegate (object s, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
        { return true; };
        smtpServer.Send(mail);
        Debug.Log("email sent to " + toAddress);
    }
}
