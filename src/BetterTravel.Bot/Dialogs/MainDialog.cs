using System.Threading;
using System.Threading.Tasks;
using BetterTravel.Bot.Bots;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;

namespace BetterTravel.Bot.Dialogs
{
    public sealed class MainDialog : BaseDialog
    {
        private readonly IStateService _stateService;

        public MainDialog(IStateService stateService)
        {
            _stateService = stateService;
        }

        protected override void InitializeDialog()
        {
            var waterfallSteps = new WaterfallStep[]
            {
                InitialStepAsync,
                FinalStepAsync
            };

            AddDialog(new WaterfallDialog($"{nameof(MainDialog)}.mainFlow", waterfallSteps));

            InitialDialogId = $"{nameof(MainDialog)}.mainFlow";
        }

        private async Task<DialogTurnResult> InitialStepAsync(
            WaterfallStepContext stepContext,
            CancellationToken cancellationToken)
        {
            var replyText = $"Echo: {stepContext.Context.Activity.Text}";
            await stepContext.Context.SendActivityAsync(MessageFactory.Text(replyText, replyText), cancellationToken);
            return await stepContext.NextAsync(null, cancellationToken);
        }

        private static async Task<DialogTurnResult> FinalStepAsync(
            WaterfallStepContext stepContext,
            CancellationToken cancellationToken)
        {
            return await stepContext.EndDialogAsync(cancellationToken: cancellationToken);
        }
    }
}