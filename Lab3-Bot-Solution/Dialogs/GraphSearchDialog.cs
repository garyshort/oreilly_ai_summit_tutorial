namespace Lab3BotSolution.Dialogs
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Microsoft.Bot.Builder.Dialogs;
    using Microsoft.Bot.Connector;
    using System.Threading.Tasks;
    
    public class GraphSearchDialog : IDialog<string>
    {
       
        public Task StartAsync(IDialogContext context)
        {
            return null;
            //return this.InitialPrompt(context);
        }
        /*
        protected Task InitialPrompt(IDialogContext context)
        {
            string prompt = "Who would you like to search for?";

            PromptDialog.Text(context, this.Search, prompt);
            return Task.CompletedTask;
        }

        public async Task Search(IDialogContext context, IAwaitable<string> input)
        {
            //string text = input != null ? await input : null;
            searchText = input != null ? await input : null;

            var response = await ExecuteSearchAsync(searchText);

            if (response.Count() == 0)
            {
                await this.NoResultsConfirmRetry(context);
            }
            else
            {
                //await context.PostAsync("This is what I found about this person");


                PPSearchResult searchResults = new PPSearchResult();
                searchResults.results = response.ToList();
                searchResults.searchText = searchText;

                var message = context.MakeMessage();

                styler.Apply(
                    ref message,
                    String.Format("Here are a few entries I found ({0}):", response.Count()),
                    new List<PPSearchResult>() { searchResults });

                await context.PostAsync(message);
                await context.PostAsync("Do you want to learn *more* about this person? Or do you want to search *again*?");

                context.Wait(this.ActOnSearchResults);
            }

        }

        protected virtual Task NoResultsConfirmRetry(IDialogContext context)
        {
            PromptDialog.Confirm(context, this.ShouldRetry, "Sorry, I didn't find any matches. Do you want to retry your search?");
            return Task.CompletedTask;
        }

        private async Task ShouldRetry(IDialogContext context, IAwaitable<bool> input)
        {
            try
            {
                bool retry = await input;
                if (retry)
                {
                    await this.InitialPrompt(context);
                }
                else
                {
                    context.Done<IList<SearchHit>>(null);
                }
            }
            catch (TooManyAttemptsException)
            {
                context.Done<IList<SearchHit>>(null);
            }
        }

        private async Task ActOnSearchResults(IDialogContext context, IAwaitable<IMessageActivity> input)
        {
            var activity = await input;
            var choice = activity.Text;

            switch (choice.ToLowerInvariant())
            {
                case "yes":
                case "again":
                case "reset":
                    //this.QueryBuilder.Reset();
                    await this.InitialPrompt(context);
                    break;

                case "more":
                    var bingSearch = new BingSearch();
                    var imageSearchMessage = await bingSearch.GetBingSearchResultAsync(context.MakeMessage(), searchText);

                    await context.PostAsync(imageSearchMessage);
                    await context.PostAsync("Do you want to search *again*?");
                    context.Wait(this.ActOnSearchResults);

                    break;

                //case "refine":
                //    this.SelectRefiner(context);
                //    break;

                //case "list":
                //    await this.ListAddedSoFar(context);
                //    context.Wait(this.ActOnSearchResults);
                //    break;

                case "done":
                case "no":
                    context.Done(context);
                    break;

                    //default:
                    //    await this.AddSelectedItem(context, choice);
                    //    break;
            }
        }
        protected async Task<IEnumerable<PPResult>> ExecuteSearchAsync(string searchText)
        {
            return await this.SearchClient.SearchAsync(searchText);
        }

        [Serializable]
        public class PanamaPapersResultsStyler : PromptStyler
        {
            public string SearchText { get; set; }


            public override void Apply<T>(ref IMessageActivity message, string prompt, IList<T> options)
            {

                var searchResults = ((IList<PPSearchResult>)options).First();

                var searchText = searchResults.searchText;
                var imageSearch = searchResults.imageSearch;
                var hits = searchResults.results;

                var cards = hits.Select(h => new HeroCard
                {
                    Title = String.Format("Name: {0}", h.Person.name),

                    Text = String.Format("{0} (based in {1})", h.Company.name, h.Company.country_codes),
                    Subtitle = String.Format("Role: {0}",
                        (h.IsDirector ? "Director" : (h.IsBeneficiary ? "Beneficiary" : "Shareholder"))),

                    //Images = new [] {new CardImage(imageSearch.contentUrl)},

                    //Buttons = new[] { new CardAction(ActionTypes.ImBack, "Read more", value: imageSearch.contentUrl) },
                });

                message.AttachmentLayout = AttachmentLayoutTypes.Carousel;
                message.Attachments = cards.Select(c => c.ToAttachment()).ToList();
                message.Text = prompt;
            }
        }
        */
    }
}
