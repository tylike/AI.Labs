using DevExpress.ExpressApp;
using AI.Labs.Module.BusinessObjects;
using AI.Labs.Module.BusinessObjects.ChatInfo;

namespace AI.Labs.Module.DatabaseUpdate;

public class DefaultAssistantUpdate
{
    IObjectSpace os;

    public DefaultAssistantUpdate(IObjectSpace os)
    {
        this.os = os;
        this.Roles = os.GetObjectsQuery<PredefinedRole>().ToList();
        Create();

    }

    void Create()
    {
        CreateDefaultRole("总结", typeof(WordDocumentRecord), "你是一个写作助手,为用户提供的内容进行总结,尽量精简.", "要续写的内容:\n{T}", "Weather_Umbrella");
        CreateDefaultRole("翻译为英语", typeof(WordDocumentRecord), "你是一个写作助手,为用户提供的内容翻译为英文.", "将以下内容翻译为英语:{T}", "Action_Translate");
        CreateDefaultRole("翻译为日语", typeof(WordDocumentRecord), "你是一个写作助手,为用户提供的内容翻译为日语.", "将以下内容翻译为日语:{T}", "ModelEditor_Localization");
        CreateDefaultRole("翻译为韩语", typeof(WordDocumentRecord), "你是一个写作助手,为用户提供的内容翻译为韩语.", "将以下内容翻译为韩语:{T}", "BO_Localization");
        CreateDefaultRole("翻译为中文", typeof(WordDocumentRecord), "你是一个写作助手,为用户提供的内容翻译为中文.", "将以下内容翻译为中文:{T}", "Action_StateMachine");

        CreateDefaultRole("解释公式", typeof(SpreadSheetDocument), "你精通excel公式的用法,为用户提供的内容进行解释.", "用户要解释的公式是:{T}", "BindingEditorHelpIcon");
        CreateDefaultRole("帮写公式", typeof(SpreadSheetDocument), "你精通excel公式的用法,为用户提供的自然语言内容转换为公式,不要解释,直接给出公式结果,用户将复制到excel的单元格公式处直接应用.", "将这个自然语言描写成公式:{T}", "ShowFormulas");

        //续写:
        //总结：Weather_Umbrella
        //翻译为英语：
        //翻译为日语：
        //翻译为韩语：
        //翻译为中文：
        //解释公式：
        //帮助公式：

    }

    List<PredefinedRole> Roles;
    void CreateDefaultRole(string caption, Type type, string systemPrompt, string userTemplate, string imageName)
    {
        var name = type.Name + "." + caption;
        if (!Roles.Any(t => t.Name == name))
        {
            var role = os.CreateObject<PredefinedRole>();
            role.ShortcutCaption = caption;
            role.Business = type.FullName;
            role.Name = name;
            role.ShortcutImageName = imageName;
            role.ShortcutMessageTemplate = userTemplate;
            var prompt = os.CreateObject<Prompt>();
            prompt.Message = systemPrompt;
            prompt.ChatRole = ChatRole.system;
            role.Prompts.Add(prompt);
            Roles.Add(role);
        }
    }
}