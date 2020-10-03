using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;

namespace BetterTravel.Bot.Bots
{
    public interface IStateService
    {
        IStatePropertyAccessor<DialogState> DialogStateAccessor { get; }

        Task SaveUserStateChangesAsync(ITurnContext turnContext, CancellationToken cancellationToken);
        Task SaveConversationStateChangesAsync(ITurnContext turnContext, CancellationToken cancellationToken);
    }

    public class StateService : IStateService
    {
        private readonly UserState _userState;
        private readonly ConversationState _conversationState;

        public StateService(UserState userState, ConversationState conversationState)
        {
            _userState = userState;
            _conversationState = conversationState;

            InitializeAccessors();
        }

        private void InitializeAccessors()
        {
            DialogStateAccessor = _conversationState.CreateProperty<DialogState>($"{nameof(StateService)}.{nameof(DialogState)}");
        }

        public IStatePropertyAccessor<DialogState> DialogStateAccessor { get; private set; }

        public async Task SaveUserStateChangesAsync(ITurnContext turnContext, CancellationToken cancellationToken) => 
            await _userState.SaveChangesAsync(turnContext, true, cancellationToken);

        public async Task SaveConversationStateChangesAsync(ITurnContext turnContext, CancellationToken cancellationToken) => 
            await _conversationState.SaveChangesAsync(turnContext, true, cancellationToken);
    }
}