using System;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Connector;
using Microsoft.Azure.Documents;
using System.Collections.Generic;
using System.Threading;

namespace Lab3BotSolution.Dialogs
{
    [Serializable]
    public class RootDialog : IDialog<object>
    {
        private enum SearchBy { Person, Location, Organization };
        private Dictionary<string, SearchBy> searchOptions = new Dictionary<string, SearchBy>() {
            {"By Person", SearchBy.Person },
            {"By Location", SearchBy.Location},
            {"By Organization", SearchBy.Organization} };
        
        public async Task StartAsync(IDialogContext context)
        {
            context.Wait(MessageReceivedAsync);
        }
        
        private async Task MessageReceivedAsync(IDialogContext context, IAwaitable<object> result)
        {
            var activity = await result as Activity;

            var message = "Hi, I'm GraphBot. I'm here to help. I may not answer all your questions, but I can help you to explore Leo Tolstoy's 'Anna Karenina' Graph Model created in Lab 2 and search for people, " +
                "locations, or organizations mentioned there.";
            await context.PostAsync(message);
            
            this.ShowStartOptions(context);
        }

        private void ShowStartOptions(IDialogContext context)
        {
            PromptDialog.Choice(context,
                    OnSearchOptionSelected,
                    searchOptions.Keys,
                    "Explore graph data",
                    "Not a valid option", 3);
        }

        private async Task OnSearchOptionSelected(IDialogContext context, IAwaitable<string> result)
        {
            try
            {
                string searchDialogOption = await result;

                SearchBy optionSearchBy = SearchBy.Location;
                searchOptions.TryGetValue(searchDialogOption, out optionSearchBy);

                switch (optionSearchBy)
                {
                    case SearchBy.Person:
                        context.Call(new PersonDialog(), this.ResumeAfterSearchOptionDialog);
                        break;

                    case SearchBy.Location:
                    case SearchBy.Organization:
                        throw new NotImplementedException();


                }
            }
            catch (TooManyAttemptsException ex)
            {
                await context.PostAsync($"Ooops! Too many attemps :(. But don't worry, I'm handling that exception and you can try again!");

                context.Wait(this.MessageReceivedAsync);
            }
        }

        private async Task ResumeAfterSearchOptionDialog(IDialogContext context, IAwaitable<string> result)
        {
            try
            {
                var x = await result;
            }
            catch (Exception ex)
            {
                await context.PostAsync($"Failed with message: {ex.Message}");
            }
            finally
            {
                this.ShowStartOptions(context);
            }
        }
    }
}