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
    public class AIModel:XPObject
    {
        public AIModel(Session s):base(s)
        {
                
        }


        [XafDisplayName("名称")]
        [ModelDefault("PredefinedValues", "gpt-3.5-turbo-1106;gpt-3.5-turbo;gpt-3.5-turbo-instruct;gpt-4;gpt-4-32k;gpt-4-1106-preview;gpt-4-1106-vision-preview")]
        public string Name
        {
            get { return GetPropertyValue<string>(nameof(Name)); }
            set { SetPropertyValue(nameof(Name), value); }
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

        [XafDisplayName("说明")]
        public string Description
        {
            get { return GetPropertyValue<string>(nameof(Description)); }
            set { SetPropertyValue(nameof(Description), value); }
        }

        [XafDisplayName("默认")]
        public bool Default
        {
            get { return GetPropertyValue<bool>(nameof(Default)); }
            set { SetPropertyValue(nameof(Default), value); }
        }

    }
}
