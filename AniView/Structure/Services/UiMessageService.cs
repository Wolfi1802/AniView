using MudBlazor;

namespace AniView.Structure.Services
{
    public class UiMessageService
    {
        private readonly ISnackbar _snackbar;

        public UiMessageService(ISnackbar snackbar)
        {
            _snackbar = snackbar;
        }

        public void ShowSnackbar(
            string message,
            Severity severity = Severity.Normal)
        {
            _snackbar.Add(message, severity);
        }
    }
}