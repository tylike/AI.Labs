using DevExpress.ExpressApp.Model;
using DevExpress.Persistent.Base;
using DevExpress.Xpo;
//using System.Speech.Recognition;
using DevExpress.ExpressApp;
using DevExpress.ExpressApp.Actions;
using System.Diagnostics;
using DevExpress.Persistent.BaseImpl;

namespace AI.Labs.Module.BusinessObjects.STT
{
    [NavigationItem("STT")]
    public class AzureMicrophoneInput : VoiceContentBase
    {
        public AzureMicrophoneInput(Session s) : base(s)
        {

        }
    }

    /// <summary>
    /// 会议记录
    /// </summary>
    [NavigationItem("STT")]
    public class MeetingNotes : VoiceContentBase, ICanRealTimeSpeechRecognition
    {
        public MeetingNotes(Session s) : base(s)
        {
        }

        /// <summary>
        /// 会议标题：记录会议的主题或名称。
        /// </summary>
        [Size(200)]
        public string Title
        {
            get { return GetPropertyValue<string>(nameof(Title)); }
            set { SetPropertyValue(nameof(Title), value); }
        }

        /// <summary>
        /// 开始时间:会议实际发生的日期。会议开始和结束的具体时间。
        /// </summary>
        public DateTime BeginTime
        {
            get { return GetPropertyValue<DateTime>(nameof(BeginTime)); }
            set { SetPropertyValue(nameof(BeginTime), value); }
        }

        /// <summary>
        /// 结束时间
        /// </summary>
        public DateTime EndDateTime
        {
            get { return GetPropertyValue<DateTime>(nameof(EndDateTime)); }
            set { SetPropertyValue(nameof(EndDateTime), value); }
        }

        /// <summary>
        /// 会议地点：会议举行的地点或平台（如线上会议工具的名称）。
        /// </summary>
        public string MeetingLocation
        {
            get { return GetPropertyValue<string>(nameof(MeetingLocation)); }
            set { SetPropertyValue(nameof(MeetingLocation), value); }
        }

        /// <summary>
        /// 会议内容摘要
        /// </summary>
        [Size(3000)]
        public string Memo
        {
            get { return GetPropertyValue<string>(nameof(Memo)); }
            set { SetPropertyValue(nameof(Memo), value); }
        }

        /// <summary>
        /// 列出所有出席会议的人员名单，可能包括内部员工和外部参与者。
        /// 参与者
        /// </summary>
        [Size(1000)]
        public string Attendees
        {
            get { return GetPropertyValue<string>(nameof(Attendees)); }
            set { SetPropertyValue(nameof(Attendees), value); }
        }

        /// <summary>
        /// 缺习者：列出被邀请但未能出席会议的人员名单。
        /// </summary>
        [Size(1000)]
        public string Absentees
        {
            get { return GetPropertyValue<string>(nameof(Absentees)); }
            set { SetPropertyValue(nameof(Absentees), value); }
        }

        /// <summary>
        /// 主持人：负责引导会议进程的人。
        /// </summary>
        public string Chairperson
        {
            get { return GetPropertyValue<string>(nameof(Chairperson)); }
            set { SetPropertyValue(nameof(Chairperson), value); }
        }

        /// <summary>
        /// 记录人：负责记录会议内容的人。
        /// </summary>
        public string Recorder
        {
            get { return GetPropertyValue<string>(nameof(Recorder)); }
            set { SetPropertyValue(nameof(Recorder), value); }
        }

        /// <summary>
        /// 会议中的重要关键词
        /// </summary>
        public string Keywords
        {
            get { return GetPropertyValue<string>(nameof(Keywords)); }
            set { SetPropertyValue(nameof(Keywords), value); }
        }

        /// <summary>
        /// 决定事项：会议中做出的决策。
        /// </summary>
        [Size(-1)]
        public string DecisionsMade
        {
            get { return GetPropertyValue<string>(nameof(DecisionsMade)); }
            set { SetPropertyValue(nameof(DecisionsMade), value); }
        }

        /// <summary>
        /// 行动计划：会议中确定的后续行动项，包括负责人和预期完成日期。
        /// </summary>
        [Size(-1)]
        public string ActionItems
        {
            get { return GetPropertyValue<string>(nameof(ActionItems)); }
            set { SetPropertyValue(nameof(ActionItems), value); }
        }

        /// <summary>
        /// 文件附件：会议中提到或使用的所有相关文档、演示或资料的链接或附件。
        /// </summary>
        [Association,Aggregated]
        public XPCollection<MeetingNotesAttachment> Attachments
        {
            get
            {
                return GetCollection<MeetingNotesAttachment>(nameof(Attachments));
            }
        }

        /// <summary>
        /// 问题和关注点：会议过程中提出的未解决的问题或需要注意的事项。
        /// </summary>
        [Size(-1)]
        public string Issues_Concerns
        {
            get { return GetPropertyValue<string>(nameof(Issues_Concerns)); }
            set { SetPropertyValue(nameof(Issues_Concerns), value); }
        }

        /// <summary>
        /// 其他备注：其他需要记录的信息，如特别通知、下一次会议的安排等。
        /// </summary>
        public string Remarks
        {
            get { return GetPropertyValue<string>(nameof(Remarks)); }
            set { SetPropertyValue(nameof(Remarks), value); }
        }

        //讨论内容：针对每个议程项目的详细讨论内容。
        [Association, Aggregated]
        public XPCollection<TextItem> Items
        {
            get
            {
                return GetCollection<TextItem>();
            }
        }

        [Size(-1)]
        public string ContentText
        {
            get { return GetPropertyValue<string>(nameof(ContentText)); }
            set { SetPropertyValue(nameof(ContentText), value); }
        }


        void ICanRealTimeSpeechRecognition.AddSegment(TimeSpan begin, TimeSpan end, string text)
        {
            Items.Add(new TextItem(Session) { Begin = begin, End = end, Text = text });
        }
    }

    public class MeetingNotesAttachment : FileData
    {
        public MeetingNotesAttachment(Session s):base(s)
        {
            
        }

        [Association]
        public MeetingNotes MeetingNotes
        {
            get { return GetPropertyValue<MeetingNotes>(nameof(MeetingNotes)); }
            set { SetPropertyValue(nameof(MeetingNotes), value); }
        }

    }
    public class TextItem : XPObject
    {
        public TextItem(Session s) : base(s)
        {

        }
        public override void AfterConstruction()
        {
            base.AfterConstruction();
            Time = DateTime.Now;
        }

        [Association]
        public MeetingNotes MeetingNotes
        {
            get { return GetPropertyValue<MeetingNotes>(nameof(MeetingNotes)); }
            set { SetPropertyValue(nameof(MeetingNotes), value); }
        }

        [ModelDefault("DisplayFormat", "yyyy-MM-dd HH:mm:ss.fff")]
        public DateTime Time
        {
            get { return GetPropertyValue<DateTime>(nameof(Time)); }
            set { SetPropertyValue(nameof(Time), value); }
        }

        public TimeSpan Begin
        {
            get { return GetPropertyValue<TimeSpan>(nameof(Begin)); }
            set { SetPropertyValue(nameof(Begin), value); }
        }

        public TimeSpan End
        {
            get { return GetPropertyValue<TimeSpan>(nameof(End)); }
            set { SetPropertyValue(nameof(End), value); }
        }

        public string Spreaker
        {
            get { return GetPropertyValue<string>(nameof(Spreaker)); }
            set { SetPropertyValue(nameof(Spreaker), value); }
        }

        [Size(-1)]
        public string Text
        {
            get { return GetPropertyValue<string>(nameof(Text)); }
            set { SetPropertyValue(nameof(Text), value); }
        }

    }
}
