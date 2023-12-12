using System.Text;

namespace BlazorChatWeb
{
    public class NotyficationServiceChatServer
    {
    }

    public class NotyficationChannels
    {
        public event Action OnChange;
    
        public void NotifyStateChanged() => OnChange?.Invoke();
    }
}
