using Microsoft.Bot.Builder.Dialogs;

namespace BetterTravel.Bot.Dialogs
{
    public abstract class BaseDialog : ComponentDialog
    {
        protected BaseDialog()
        {
            InitializeDialog();
        }

        protected abstract void InitializeDialog();
    }
}
