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
        // Messages sent to the user.
        private const string WelcomeMessage = "This is a simple Welcome Bot sample. This bot will introduce you " +
                                                "to welcoming and greeting users. You can say 'intro' to see the " +
                                                "introduction card. If you are running this bot in the Bot Framework " +
                                                "Emulator, press the 'Start Over' button to simulate user joining " +
                                                "a bot or a channel";

        private const string InfoMessage = "You are seeing this message because the bot received at least one " +
                                            "'ConversationUpdate' event, indicating you (and possibly others) " +
                                            "joined the conversation. If you are using the emulator, pressing " +
                                            "the 'Start Over' button to trigger this event again. The specifics " +
                                            "of the 'ConversationUpdate' event depends on the channel. You can " +
                                            "read more information at: " +
                                            "https://aka.ms/about-botframework-welcome-user";

        private const string LocaleMessage = "You can use the activity's 'GetLocale()' method to welcome the user " +
                                             "using the locale received from the channel. " +
                                             "If you are using the Emulator, you can set this value in Settings.";


        private const string PatternMessage = "It is a good pattern to use this event to send general greeting" +
                                              "to user, explaining what your bot can do. In this example, the bot " +
                                              "handles 'hello', 'hi', 'help' and 'intro'. Try it now, type 'hi'";

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

        /*
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
        */

        protected override async Task OnMembersAddedAsync(
            IList<ChannelAccount> membersAdded, 
            ITurnContext<IConversationUpdateActivity> turnContext,
            CancellationToken cancellationToken)
        {
            foreach (var member in membersAdded)
            {
                if (member.Id != turnContext.Activity.Recipient.Id)
                {
                    await turnContext.SendActivityAsync($"Hi there - {member.Name}. {WelcomeMessage}", cancellationToken: cancellationToken);
                    await turnContext.SendActivityAsync(InfoMessage, cancellationToken: cancellationToken);
                    await turnContext.SendActivityAsync($"{LocaleMessage} Current locale is '{turnContext.Activity.GetLocale()}'.", cancellationToken: cancellationToken);
                    await turnContext.SendActivityAsync(PatternMessage, cancellationToken: cancellationToken);
                }
            }
        }

        /*
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
        */
        protected override async Task OnMessageActivityAsync(ITurnContext<IMessageActivity> turnContext, CancellationToken cancellationToken)
        {
            var didBotWelcomeUser = await _stateService.WelcomeUserStateAccessor.GetAsync(turnContext, () => new WelcomeUserState(), cancellationToken);

            if (didBotWelcomeUser.DidBotWelcomeUser == false)
            {
                didBotWelcomeUser.DidBotWelcomeUser = true;

                // the channel should sends the user name in the 'From' object
                var userName = turnContext.Activity.From.Name;

                await turnContext.SendActivityAsync("You are seeing this message because this was your first message ever to this bot.", cancellationToken: cancellationToken);
                await turnContext.SendActivityAsync($"It is a good practice to welcome the user and provide personal greeting. For example, welcome {userName}.", cancellationToken: cancellationToken);
            }
            else
            {
                // This example hardcodes specific utterances. You should use LUIS or QnA for more advance language understanding.
                var text = turnContext.Activity.Text.ToLowerInvariant();
                switch (text)
                {
                    case "hello":
                    case "hi":
                        await turnContext.SendActivityAsync($"You said {text}.", cancellationToken: cancellationToken);
                        break;
                    case "intro":
                    case "help":
                        await SendIntroCardAsync(turnContext, cancellationToken);
                        break;
                    default:
                        await turnContext.SendActivityAsync(WelcomeMessage, cancellationToken: cancellationToken);
                        break;
                }
            }

            // Save any state changes.
            await _stateService.SaveUserStateChangesAsync(turnContext, cancellationToken);
        }

        /*
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
        */

        private static async Task SendIntroCardAsync(ITurnContext turnContext, CancellationToken cancellationToken)
        {
            var card = new HeroCard
            {
                Title = "Welcome to Bot Framework!",
                Text = "Welcome to Welcome Users bot sample! " +
                       "This Introduction card is a great way to introduce your Bot to the user and suggest some things to get them started. " +
                       "We use this opportunity to recommend a few next steps for learning more creating and deploying bots.",
                Images = new List<CardImage> { new CardImage("https://github.com/itkerry/better-travel/raw/master/icon.png") },
                Buttons = new List<CardAction>
                {
                    new CardAction(
                        ActionTypes.OpenUrl, 
                        "Get an overview", null, 
                        "Get an overview", 
                        "Get an overview", 
                        "https://docs.microsoft.com/en-us/azure/bot-service/?view=azure-bot-service-4.0"),
                    new CardAction(
                        ActionTypes.OpenUrl, 
                        "Ask a question", null, 
                        "Ask a question", 
                        "Ask a question", 
                        "https://stackoverflow.com/questions/tagged/botframework"),
                    new CardAction(
                        ActionTypes.OpenUrl, 
                        "Learn how to deploy", null, 
                        "Learn how to deploy", 
                        "Learn how to deploy", 
                        "https://docs.microsoft.com/en-us/azure/bot-service/bot-builder-howto-deploy-azure?view=azure-bot-service-4.0"),
                }
            };

            var response = MessageFactory.Attachment(card.ToAttachment());
            await turnContext.SendActivityAsync(response, cancellationToken);
        }
    }

    public class WelcomeUserState
    {
        public bool DidBotWelcomeUser { get; set; }
    }
}