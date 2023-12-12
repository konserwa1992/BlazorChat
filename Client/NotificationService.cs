namespace BlazorChatWeb.Client
{
    public class NotificationService
    {
        public event Action OnChange;

        public void NotifyStateChanged() => OnChange?.Invoke();
    }
}
