using System;
using System.Threading.Tasks;
using System.Windows;
using System.Text;
using System.Windows.Controls;
using MQTTnet.Client;
using MQTTnet;
using MQTTnet.Packets;
using MQTTnet.Protocol;
using System.Windows.Media;
using System.Collections.Generic;

namespace MqttManager
{
    public partial class Mqtt
    {
        #region Attributes;

        private string ClientId;
        private string MyTopic;
        private string OtherTopic;

        private IMqttClient Client = null;
        private MqttClientOptions Options = null;
        private StackPanel ChatBox = null;
        private TextBlock MessageError = null;
        private List<Message> ListMessagesNotReceived = new List<Message>();
        private List<TextBlock> TextBlocksNotConfirmed = new List<TextBlock>();

        #endregion

        static void Main() { }

        #region Constructor;

        public Mqtt(
            string clientId,
            string myTopic,
            string otherTopic,
            StackPanel chatBox,
            TextBlock messageError
        )
        {
            ClientId = clientId;
            MyTopic = myTopic;
            OtherTopic = otherTopic;
            ChatBox = chatBox;
            MessageError = messageError;

            SetClientAndOptions();
            SetClientEvents();

            TryConnect();
        }

        private void SetClientAndOptions()
        {
            MqttFactory mqttFactory = new MqttFactory();
            Client = mqttFactory.CreateMqttClient();
            Options = new MqttClientOptionsBuilder()
                            .WithClientId(ClientId)
                            .WithTcpServer("test.mosquitto.org", 1883)
                            .WithCleanSession()
                            .Build();
        }

        private void SetClientEvents()
        {
            Client.ConnectedAsync += async e =>
            {
                Console.WriteLine("Connected to the broker successfully");

                MqttTopicFilter topicFilter = new MqttTopicFilterBuilder()
                                                    .WithTopic(MyTopic)
                                                    .Build();

                _ = await Client.SubscribeAsync(topicFilter);

                PublishReconnectConfirmation();
            };

            Client.DisconnectedAsync += async e =>
            {
                Console.WriteLine("The Broker is not connected.");

                await Task.Delay(2000);

                TryConnect();
            };

            Client.ApplicationMessageReceivedAsync += e =>
            {
                Console.WriteLine("Message received succesfully");

                string completeText = Encoding.UTF8.GetString(e.ApplicationMessage.Payload);
                Message message = Message.FromCompleteText(completeText);

                ReceivementManager(message);

                return Task.CompletedTask;
            };
        }

        #endregion;

        #region Receivement;

        private void ReceivementManager(Message message)
        {
            if (message.IsConfirmation)
            {
                ReceiveConfirmation(message.Id);
            }
            else if (message.IsReconnect)
            {
                RePublish();
            }
            else
            {
                ReceiveMessage(message);
            }
        }

        private void ReceiveMessage(Message message)
        {
            ShowMessage(message, RoleEnum.Receiver);
            PublicConfirmation(message.Id);
        }

        private void ReceiveConfirmation(string id)
        {
            ListMessagesNotReceived.RemoveAll(message => message.Id == id);

            for (int i = 0; i < TextBlocksNotConfirmed.Count; i++)
            {
                TextBlock item = TextBlocksNotConfirmed[i];

                item.Dispatcher.Invoke(new Action(() =>
                {
                    if (item.Uid == id)
                    {
                        item.Foreground = Brushes.Black;
                        TextBlocksNotConfirmed.RemoveAt(i);
                    }
                }
                ));
            }
        }

        #endregion;

        #region MessageExibition;

        private TextBlock ShowMessage(Message message, RoleEnum role)
        {
            TextBlock Text = null;

            ChatBox.Dispatcher.Invoke(new Action(() =>
            {
                Text = new TextBlock
                {
                    Text = message.Text,
                    Margin = new Thickness(10.0, 10.0, 10.0, 0),
                    TextWrapping = TextWrapping.Wrap,
                    Uid = message.Id
                };

                DefineParticularCaracteristics(Text, role);

                _ = ChatBox.Children.Add(Text);
            }
            ));

            return Text;
        }

        #endregion;

        #region Publishment;

        private async void PublishReconnectConfirmation()
        {
            try
            {
                Message message = Message.OfReconnection();
                _ = await PublishMessageAsync(message);
            }
            catch (Exception)
            {
                SetMessageErrorVisibility(Visibility.Visible);
            }
        }

        public async void PublicMessage(string text)
        {
            try
            {
                Message message = new Message(text);
                TextBlock Text = null;

                MqttClientPublishResult result = await PublishMessageAsync(message);

                if (result.IsSuccess)
                {
                    Text = ShowMessage(message, RoleEnum.Sender);
                }

                ListMessagesNotReceived.Add(message);
                TextBlocksNotConfirmed.Add(Text);
            }
            catch (Exception)
            {
                SetMessageErrorVisibility(Visibility.Visible);
            }
        }

        private async void PublicConfirmation(string id)
        {
            try
            {
                Message message = Message.OfConfirmation(id);
                _ = await PublishMessageAsync(message);
            }
            catch (Exception)
            {
                SetMessageErrorVisibility(Visibility.Visible);
            }
        }

        private void RePublish()
        {
            try
            {
                ListMessagesNotReceived.ForEach(async message =>
                {
                    _ = await PublishMessageAsync(message);
                });
            }
            catch (Exception)
            {
                SetMessageErrorVisibility(Visibility.Visible);
            }
        }

        private async Task<MqttClientPublishResult> PublishMessageAsync(Message message)
        {
            MqttApplicationMessage messageToSend = new MqttApplicationMessageBuilder()
                                                .WithTopic(OtherTopic)
                                                .WithResponseTopic(MyTopic)
                                                .WithPayload(message.CompleteText)
                                                .WithQualityOfServiceLevel(MqttQualityOfServiceLevel.AtLeastOnce)
                                                .Build();


            Console.WriteLine(message.CompleteText);
            return await Client.PublishAsync(messageToSend);
        }

        #endregion;

        private void DefineParticularCaracteristics(TextBlock text, RoleEnum role)
        {
            if (role == RoleEnum.Sender)
            {
                text.Foreground = Brushes.LightGray;
                text.HorizontalAlignment = HorizontalAlignment.Right;
            }
            else
            {
                text.Foreground = Brushes.Black;
                text.HorizontalAlignment = HorizontalAlignment.Left;
            }
        }

        private async void TryConnect()
        {
            SetMessageErrorVisibility(Visibility.Visible);

            try
            {
                _ = await Client.ConnectAsync(Options);

                SetMessageErrorVisibility(Visibility.Collapsed);
            }
            catch (Exception)
            {
                SetMessageErrorVisibility(Visibility.Visible);
            }
        }

        private void SetMessageErrorVisibility(Visibility visibility)
        {
            if (MessageError.Visibility != visibility)
            {
                MessageError.Dispatcher.Invoke(new Action(() =>
                {
                    MessageError.Visibility = visibility;
                }
                ));
            }
        }
    }
}
