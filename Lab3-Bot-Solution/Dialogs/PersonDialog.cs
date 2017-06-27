using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Connector;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

namespace Lab3BotSolution.Dialogs
{
    [Serializable]
    public class PersonDialog : IDialog<string>
    {
        private enum searchByPerson { HowMany, ShowTopOverall, ShowBottomOverall, Other, Exit };
        private Dictionary<string, searchByPerson> searchPersonOptions =
                new Dictionary<string, searchByPerson>() {
                    {"How many characters are in the book?", searchByPerson.HowMany },
                    {"Who is the most mentioned character?", searchByPerson.ShowTopOverall},
                    {"Who is the least mentioned character?", searchByPerson.ShowBottomOverall},
                    {"Ask about something else", searchByPerson.Other },
                    {"Exit", searchByPerson.Exit }};

        private int resultHowMany = 0;
        private int pageNum = 0;
        private int pageSize = 10;

        public async Task StartAsync(IDialogContext context)
        {
            this.ShowStartOptions(context);
        }

        private void ShowStartOptions(IDialogContext context)
        {
            PromptDialog.Choice(
                context: context,
                resume: RunSearchByPerson,
                options: searchPersonOptions.Keys,
                prompt: "Most popular queries are:",
                retry: "I didn't understand. Please try again.");
        }


        public async Task RunSearchByPerson(IDialogContext context, IAwaitable<string> argument)
        {
            var searchByPersonDialogOption = await argument;
            searchByPerson searchByPersonVal;
            searchPersonOptions.TryGetValue(searchByPersonDialogOption, out searchByPersonVal);
            
            switch (searchByPersonVal)
            {
                case searchByPerson.HowMany:
                    resultHowMany = await CosmosDBRepository.CountNodes("Person");
                    await context.PostAsync(String.Format("I found {0} characters in the book", resultHowMany));

                    pageNum = 0;
                    PromptDialog.Choice(context, resume: GetPersonByPage, prompt: "Would you like to know more about them?", 
                        options: new string[]{ "Yes, please", "No, thanks"});
                    break;

                case searchByPerson.ShowTopOverall:
                    var resultTopOverall = await CosmosDBRepository.GetTop1Person(true);
                    await context.PostAsync(String.Format("The most mentioned character is *{0}*", resultTopOverall));
                    ShowStartOptions(context);

                    break;

                case searchByPerson.ShowBottomOverall:
                    var resultBottomOverall = await CosmosDBRepository.GetTop1Person(false);
                    await context.PostAsync(String.Format("The least mentioned character is *{0}*", resultBottomOverall));
                    ShowStartOptions(context);

                    break;

                case searchByPerson.Other:
                    await context.PostAsync("What do you want to know?");
                    context.Call(new PersonFAQDialog(), this.ResumeAfterQnADialog);

                    break;
                case searchByPerson.Exit:
                    context.Done("done");
                    break;
            }
        }


        public async Task GetPersonByPage(IDialogContext context, IAwaitable<string> message)
        {
            var pageAllYes = (await message).Contains("Yes");

            if (pageAllYes)
            {
                var result = await CosmosDBRepository.GetNodesPage("Person", pageNum);
                await context.PostAsync(String.Format("They are: {0}{1} ", Environment.NewLine, result)); // replace

                pageNum++;
                if ((pageSize * pageNum) <= resultHowMany)
                {
                    PromptDialog.Choice(context, resume: GetPersonByPage, prompt: "Would you like to see next 10 characters?",
                        options: new string[] { "Yes, please", "No, thanks" });
                }
                else
                {
                    ShowStartOptions(context);
                }
            }
            else
            {
                ShowStartOptions(context);
            }
        }

        public async Task ResumeAfterQnADialog(IDialogContext context, IAwaitable<object> message)
        {
            ShowStartOptions(context);
        }
    }

    
}