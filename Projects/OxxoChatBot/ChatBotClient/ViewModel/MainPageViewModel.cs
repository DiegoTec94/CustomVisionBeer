using System;
using System.Collections.ObjectModel;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using ChatBotClient.Model.HttpClasses;
using Newtonsoft.Json;
using Xamarin.Forms;

namespace ChatBotClient.ViewModel
{
    public class MainPageViewModel : BaseViewModel
    {

        #region Class properties
        static HttpClient chatClient;
        static HttpClient startConversationClient;
        static PostResult postResult;
        static ActivityToPost activityToPost;
        static GetResult getResult;
        static string botUriStartConversation;
        static string botUriChat;
        static string botSecret;
        static string activity;
        static bool firstMessage;
        static MessageRequest messageRequest;
        static StringContent content;
        #endregion
        #region Properties
        private bool _typingVisibility;
        public bool TypingVisibility
        {
            get { return _typingVisibility; }
            set
            {
                _typingVisibility = value;
                RaisePropertyChanged();
            }
        }

        private string outgoingText;
        public string OutGoingText
        {
            get { return outgoingText; }
            set
            {
                outgoingText = value;
                RaisePropertyChanged();
            }
        }
        #endregion


        #region Commands
        public ICommand SendMessageCommand { get; set; }
        public async void SendMessage()
        {
            try
            {
                if (outgoingText != string.Empty)
                {
                    var msj = OutGoingText;
                    OutGoingText = string.Empty;
                    Messages.Add(new MessageViewModel { Text = msj, IsIncoming = false, MessagDateTime = DateTime.Now.AddMinutes(-25) });
                    TypingVisibility = true;
                    GenerateContent(msj);
                    postResult = JsonConvert.DeserializeObject<PostResult>(await PostAsync(botUriChat, content));
                    if (postResult != null)
                    {
                        if (firstMessage)
                        {
                            getResult = JsonConvert.DeserializeObject<GetResult>(await chatClient.GetStringAsync(botUriChat));
                            firstMessage = false;
                        }

                        else
                        {
                            string jsonResultFromBot = await chatClient.GetStringAsync(botUriChat + "?watermark=" + getResult.watermark);
                            getResult = JsonConvert.DeserializeObject<GetResult>(jsonResultFromBot);
                        }

                        for (int i = 1; i < getResult.activities.Count; i++)
                        {
                            if (getResult != null)
                            {
                                if (getResult.activities[i].attachments != null)
                                {
                                    if (getResult.activities[i].attachments.Count > 0)
                                    {
                                        foreach (var content in getResult.activities[i].attachments)
                                        {
                                            StringBuilder message = new StringBuilder();

                                            message.AppendLine(content.content.text);

                                            foreach (var button in content.content.buttons)
                                            {
                                                message.AppendLine(button.title);
                                            }
                                            Messages.Add(new MessageViewModel { Text = "Oxxo-Bot: " + message.ToString(), IsIncoming = true, MessagDateTime = DateTime.Now.AddMinutes(-25) });

                                        }
                                    }
                                    else
                                    {
                                        Messages.Add(new MessageViewModel { Text = "Oxxo-Bot: " + getResult.activities[i].text, IsIncoming = true, MessagDateTime = DateTime.Now.AddMinutes(-25) });
                                    }
                                }
                            }
                        }
                    }
                    TypingVisibility = false;
                    OutGoingText = string.Empty;
                }
            }
            catch (Exception ex)
            {

                throw;
            }
            
        }
        #endregion
        #region Methods
        public async void StartConversation()
        {
            StringContent content = new StringContent("", Encoding.UTF8, "application/json");
            try
            {
                string result = await PostAsync(botUriStartConversation, content);
                messageRequest = JsonConvert.DeserializeObject<MessageRequest>(result);
                botUriChat = String.Format(botUriChat, messageRequest.conversationId);
                Messages.Add(new MessageViewModel { Text = "Oxxo-Bot: Estoy listo para comenzar", IsIncoming = true, MessagDateTime = DateTime.Now.AddMinutes(-25) });

            }
            catch (Exception ex)
            {
                botSecret = ex.ToString();
            }
        }

        private static async Task<string> PostAsync(string uri, HttpContent content)
        {
            try
            {
                HttpResponseMessage response = await startConversationClient.PostAsync(uri, content);
                var result = await response.Content.ReadAsStringAsync();
                return result;
            }
            catch (Exception ex)
            {
                return ex.ToString();
            }
        }

        private void GenerateContent(string message)
        {
            activityToPost = new ActivityToPost
            {
                type = "message",
                from = new User { id = "user1" },
                text = message,
                locale = "es-MX",
            };
            activity = JsonConvert.SerializeObject(activityToPost);
            content = new StringContent(activity, Encoding.UTF8, "application/json");
        }
        #endregion



        private ObservableCollection<MessageViewModel> messagesList;

        public ObservableCollection<MessageViewModel> Messages
        {
            get { return messagesList; }
            set { messagesList = value; RaisePropertyChanged(); }
        }



        public ICommand SendCommand { get; set; }


        public MainPageViewModel()
        {
            SendMessageCommand = new Command(SendMessage);
            getResult = new GetResult();
            firstMessage = true;

            botUriStartConversation = "https://directline.botframework.com/v3/directline/conversations/";
            botUriChat = "https://directline.botframework.com/v3/directline/conversations/{0}/activities";
            //botSecret = "z0V7vIA8efQ.cwA.f1Y.TLb9RxamcPKYzvfFHVXnjR4CREn_yZwY99WZZbdnzcM";
            botSecret = "XoarI-ZEMUo.cwA.Jes.ox3HroBUIrndyQH15Wn1SbfrxDuGY6_dKkpjPEouyC8";
            chatClient = new HttpClient();
            startConversationClient = new HttpClient();
            chatClient.DefaultRequestHeaders.Add("Authorization", "Bearer " + botSecret);
            startConversationClient.DefaultRequestHeaders.Add("Authorization", "Bearer " + botSecret);

            StartConversation();


            // Initialize with default values
            Messages = new ObservableCollection<MessageViewModel>();

            OutGoingText = null;
            SendCommand = new Command(() =>
            {
                SendMessage();
            });
        }
        

    }
}
