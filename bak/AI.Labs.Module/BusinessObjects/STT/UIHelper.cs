using DevExpress.ExpressApp;

//using TranscribeCS;
public static class UIHelper
{
    public static Action<Action> UIInvoke { get; set; }
    public static Action UIDoEvents { get; set; }
    public static void UIThreadInvoke(this XafApplication app, Action action)
    {
        if (UIInvoke != null)
        {
            UIInvoke(action);
        }
    }
    public static void UIThreadDoEvents(this XafApplication app)
    {
        if(UIDoEvents != null)
        {
            UIDoEvents();
        }
    }
}
