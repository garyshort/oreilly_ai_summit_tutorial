using Microsoft.Bot.Builder.Dialogs;
using QnAMakerDialog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Dialogs;
using System.Web;

namespace Lab3BotSolution.Dialogs
{
    [Serializable]
    [QnAMakerService("dabc207a592a475cad0d3440a7ad4655", "c4d6de6c-15a3-4b6b-b08f-929b25762a41")]
    public class PersonFAQDialog : QnAMakerDialog<object>
    {
        //Based on http://www.garypretty.co.uk/2017/01/17/qna-maker-dialog-for-bot-framework/ 
        public override async Task DefaultMatchHandler(IDialogContext context, string originalQueryText, QnAMakerResult result)
        {
            var messageActivity = ProcessResultAndCreateMessageActivity(context, ref result);
            messageActivity.Text = $"{result.Answer}";
            await context.PostAsync(messageActivity);

            this.ShowAskAnotherQuestion(context);
        }
        public override async Task NoMatchHandler(IDialogContext context, string originalQueryText)
        {
            await context.PostAsync($"Sorry, I couldn't find an answer for '{originalQueryText}'.");
            this.ShowAskAnotherQuestion(context);
        }

        private void ShowAskAnotherQuestion(IDialogContext context)
        {
            PromptDialog.Choice(context, resume: AfterAskQuestion, prompt: "Would you like to ask another question?",
                        options: new string[] { "Yes, please", "No, thanks" });
        }
        public async Task AfterAskQuestion(IDialogContext context, IAwaitable<string> message)
        {

            var pageAllYes = (await message).Contains("Yes");

            if (pageAllYes)
            {
                context.Wait(MessageReceived);
            }
            else
            {
                context.Done("done");
            }
        }
    }
}