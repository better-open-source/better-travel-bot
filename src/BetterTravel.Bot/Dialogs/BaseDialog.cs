using Microsoft.Bot.Builder.Dialogs;

namespace BetterTravel.Bot.Dialogs
{
    public abstract class BaseDialog : ComponentDialog
    {
        protected abstract void InitializeDialog();
    }
}
