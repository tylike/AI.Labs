using BingChat;
using DevExpress.ExpressApp.DC;
using DevExpress.Persistent.Base;
using DevExpress.Xpo;

namespace AI.Labs.Module.BusinessObjects
{
    [NavigationItem]
    public class BingChat:XPObject
    {
        public BingChat(Session s):base(s)
        {
                
        }

        [Action]
        public async void Chat()
        {
            // Construct the chat client
            var client = new BingChatClient(new BingChatClientOptions
            {
                // Tone used for conversation
                Tone = BingChatTone.Balanced,
                CookieU = "1DNMmXAknvYw9XY6ytC7n6T0_LvNoPkzj4N5HatzrqK19XJmBc8HZ7Ni8pVnL7gHDdWqSUC8LJJQoqZgKlShwXE5J3cxBjv_ZISrFEP0dvQXIEnI-viLQDLj57UqsIRbcck0e0YmwlGH4ValWfO2mdbnhhfK2GhaYMZZOKHNOPzniKgJmTq8dQRL5Zn7w22xFHGnap32sGLVIWa9ZbDj7uQ"
            });

            var message = "Do you like cats?";
            var answer = await client.AskAsync(message);

            Console.WriteLine($"Answer: {answer}");
        }
    }
}
