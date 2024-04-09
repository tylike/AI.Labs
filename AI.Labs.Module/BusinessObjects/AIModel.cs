using DevExpress.Xpo;
using DevExpress.Persistent.Base;
using DevExpress.ExpressApp.DC;
using DevExpress.ExpressApp.Model;
using DevExpress.Utils.Design;
using DevExpress.XtraRichEdit.Import.Doc;

namespace AI.Labs.Module.BusinessObjects
{


    [XafDisplayName("模型")]
    [NavigationItem()]
    public class AIModel : XPObject
    {
        public AIModel(Session s) : base(s)
        {

        }
        public string Title
        {
            get { return GetPropertyValue<string>(nameof(Title)); }
            set { SetPropertyValue(nameof(Title), value); }
        }

        protected override void OnSaving()
        {
            base.OnSaving();
            if (string.IsNullOrEmpty(Title))
            {
                Title = Name;
            }
        }
        public override void AfterConstruction()
        {
            base.AfterConstruction();
            this.LocalServerArgument = " --embeddings";
        }

        [XafDisplayName("名称")]
        [ModelDefault("PredefinedValues", "gpt-3.5-turbo-1106;gpt-3.5-turbo;gpt-3.5-turbo-instruct;gpt-4;gpt-4-32k;gpt-4-1106-preview;gpt-4-1106-vision-preview")]
        public string Name
        {
            get { return GetPropertyValue<string>(nameof(Name)); }
            set { SetPropertyValue(nameof(Name), value); }
        }

        [XafDisplayName("默认")]
        public bool IsDefault
        {
            get { return GetPropertyValue<bool>(nameof(IsDefault)); }
            set { SetPropertyValue(nameof(IsDefault), value); }
        }

        [Size(200)]
        public string ApiKey
        {
            get { return GetPropertyValue<string>(nameof(ApiKey)); }
            set { SetPropertyValue(nameof(ApiKey), value); }
        }

        [Size(-1)]
        public string ApiUrlBase
        {
            get { return GetPropertyValue<string>(nameof(ApiUrlBase)); }
            set { SetPropertyValue(nameof(ApiUrlBase), value); }
        }

        [Size(-1)]
        [ModelDefault("RowCount", "1")]
        public string Host
        {
            get { return GetPropertyValue<string>(nameof(Host)); }
            set { SetPropertyValue(nameof(Host), value); }
        }


        [XafDisplayName("说明")]
        public string Description
        {
            get { return GetPropertyValue<string>(nameof(Description)); }
            set { SetPropertyValue(nameof(Description), value); }
        }

        [Size(-1)]
        [XafDisplayName("服务程序路径")]
        [ToolTip("如llama.cpp的server.exe程序所在的路径")]
        [ModelDefault("RowCount", "1")]
        public string ServerProgramFilePath
        {
            get { return GetPropertyValue<string>(nameof(ServerProgramFilePath)); }
            set { SetPropertyValue(nameof(ServerProgramFilePath), value); }
        }

        [XafDisplayName("说明")]
        [Size(-1)]
        public string ModelFilePath
        {
            get { return GetPropertyValue<string>(nameof(ModelFilePath)); }
            set { SetPropertyValue(nameof(ModelFilePath), value); }
        }

        [XafDisplayName("本地服务端口")]
        public int LocalServicePort
        {
            get { return GetPropertyValue<int>(nameof(LocalServicePort)); }
            set { SetPropertyValue(nameof(LocalServicePort), value); }
        }

        [XafDisplayName("模型分类")]
        [ToolTip("决定了如何是在本地启动服务器还是直接使用openai风格的api")]
        public ModelCategory ModelCategory
        {
            get { return GetPropertyValue<ModelCategory>(nameof(ModelCategory)); }
            set { SetPropertyValue(nameof(ModelCategory), value); }
        }

        [XafDisplayName("加载到GPU层数")]
        [Size(-1)]
        [ToolTip("llama.cpp中的ngl参数")]
        public int LoadGpuLayer
        {
            get { return GetPropertyValue<int>(nameof(LoadGpuLayer)); }
            set { SetPropertyValue(nameof(LoadGpuLayer), value); }
        }

        [Size(-1)]
        [XafDisplayName("附加参数")]
        //[ModelDefault("RowCount", "1")]
        public string LocalServerArgument
        {
            get { return GetPropertyValue<string>(nameof(LocalServerArgument)); }
            set { SetPropertyValue(nameof(LocalServerArgument), value); }
        }
    }

    //server -m D:\llm\text-generation-webui-2023-11-12\models\MistralAI\8x7B\qwen1_5-14b-chat-q5_0.gguf --port 8000 -ngl 41

    public enum ModelCategory
    {
        OpenAILike,
        LlamaCPPLocalServer,
    }
}
