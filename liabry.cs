using System;
using Telegram.Bot.Types;

/// <summary>
/// Summary description for Class1
/// </summary>
///

public struct Schdule
#nullable enable
{

    public string description { get; set; }
    public string pubDate { get; set; }
}
public struct process_body
{
    public Message m;
    public string received_text;
    public Schdule schdule;
}