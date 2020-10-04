using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Logging;

namespace BetterTravel.Bot.Bots
{
    public sealed class DialogBot<T> : ActivityHandler where T : Dialog
    {
        private readonly Dialog _dialog;
        private readonly ILogger _logger;
        private readonly IStateService _stateService;
        private readonly ConcurrentDictionary<string, ConversationReference> _conversationReferences;

        public DialogBot(
            ILogger<DialogBot<T>> logger, 
            Dialog dialog, 
            IStateService stateService, 
            ConcurrentDictionary<string, ConversationReference> conversationReferences)
        {
            _logger = logger;
            _dialog = dialog;
            _stateService = stateService;
            _conversationReferences = conversationReferences;
        }

        protected override async Task OnMembersAddedAsync(
            IList<ChannelAccount> membersAdded, 
            ITurnContext<IConversationUpdateActivity> turnContext, 
            CancellationToken cancellationToken)
        {
            const string welcomeText = "Hello and welcome!";
            foreach (var member in membersAdded)
            {
                if (member.Id != turnContext.Activity.Recipient.Id)
                {
                    await turnContext.SendActivityAsync(
                        MessageFactory.Text(welcomeText, welcomeText), 
                        cancellationToken);
                }
            }
        }

        protected override Task OnConversationUpdateActivityAsync(ITurnContext<IConversationUpdateActivity> turnContext, CancellationToken cancellationToken)
        {
            AddConversationReference(turnContext.Activity as Activity);

            return base.OnConversationUpdateActivityAsync(turnContext, cancellationToken);
        }

        public override async Task OnTurnAsync(ITurnContext turnContext, CancellationToken cancellationToken = default)
        {
            await base.OnTurnAsync(turnContext, cancellationToken);

            await _stateService.SaveConversationStateChangesAsync(turnContext, cancellationToken);
            await _stateService.SaveUserStateChangesAsync(turnContext, cancellationToken);
        }

        protected override async Task OnMessageActivityAsync(ITurnContext<IMessageActivity> turnContext, CancellationToken cancellationToken)
        {
            AddConversationReference(turnContext.Activity as Activity);

            _logger.LogInformation("Running dialog with Message Activity.");
            await _dialog.RunAsync(turnContext, _stateService.DialogStateAccessor, cancellationToken);
        }

        private void AddConversationReference(IActivity activity)
        {
            var conversationReference = activity.GetConversationReference();
            _conversationReferences.AddOrUpdate(conversationReference.User.Id, conversationReference,
                (key, newValue) => conversationReference);
        }
    }
}