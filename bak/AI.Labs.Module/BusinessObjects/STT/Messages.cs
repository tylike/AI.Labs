#define FixDesigner
using DevExpress.ExpressApp.DC;
using DevExpress.ExpressApp.Utils;

namespace AI.Labs;

public enum Messages
{
    [XafDisplayName("STT Service Started! ElapsedTime:{0}(ms)")]
    STTServiceStarted,
    [XafDisplayName("STT Service Stopped!")]
    STTServiceStopped
}

//不用有地方用到，但为了让模型发现命名用了messages这个枚举
[DomainComponent]
public class MessagesRef
{
    public Messages Messages { get; set; }
}

public static class MessageHelper
{
    public static string RemoveJsonRem(this string json)
    {
        if (string.IsNullOrEmpty(json))
            return string.Empty;
        if (json.StartsWith("```json"))
        {
            json = json["```json".Length..];
        }

        if (json.StartsWith("```"))
        {
            json = json["```".Length..];
        }

        if (json.EndsWith("```"))
        {
            json = json[..^3];
        }
        return json;
    }

    /// <summary>
    /// 这么干就是为了实现中英文显示文字的不同
    /// </summary>
    /// <param name="msg"></param>
    /// <returns></returns>
    public static string GetDisplayText(this Messages msg)
    {
        return CaptionHelper.GetDisplayText(msg);
    }
}
