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

namespace MqttManager
{
    public partial class Mqtt
    {
        #region Attributes;

        string MyTopic;
        string OtherTopic;
        string MessageConfirmation = "MessageConfirmation";

        IMqttClient Client = null;
        MqttClientOptions Options = null;
        StackPanel ChatBox = null;
        TextBlock MessageError = null;
        TextBlock LastChatChildren = null;

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
            MyTopic = myTopic;
            OtherTopic = otherTopic;
            ChatBox = chatBox;
            MessageError = messageError;

            SetClientAndOptions(clientId);
            SetClientEvents();

            TryConnect();
        }

        private void SetClientAndOptions(string clientId)
        {
            MqttFactory mqttFactory = new MqttFactory();
            Client = mqttFactory.CreateMqttClient();
            Options = new MqttClientOptionsBuilder()
                            .WithClientId(clientId)
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

                ReceivementManager(e.ApplicationMessage.Payload);

                return Task.CompletedTask;
            };
        }

        #endregion;

        #region Receivement;

        private void ReceivementManager(byte[] mqttResponse)
        {
            string message = Encoding.UTF8.GetString(mqttResponse);

            if (message == MessageConfirmation)
            {
                LastChatChildren.Dispatcher.Invoke(new Action(() =>
                {
                    LastChatChildren.Foreground = Brushes.Black;
                }
                ));
            }
            else
            {
                ShowMessage(message, RoleEnum.Receiver);

                Public(MessageConfirmation);
            }
        }

        #endregion;

        #region MessageExibition;

        private void ShowMessage(string message, RoleEnum role)
        {
            if (message == MessageConfirmation)
            {
                return;
            }

            ChatBox.Dispatcher.Invoke(new Action(() =>
            {
                TextBlock Text = new TextBlock
                {
                    Foreground = role == RoleEnum.Sender
                        ? Brushes.LightGray
                        : Brushes.Black,
                    Text = message,
                    Margin = new Thickness(10.0, 10.0, 10.0, 0),
                    TextWrapping = TextWrapping.Wrap,
                    HorizontalAlignment = role == RoleEnum.Sender
                        ? HorizontalAlignment.Right
                        : HorizontalAlignment.Left
                };

                _ = ChatBox.Children.Add(Text);

                if (role == RoleEnum.Sender)
                {
                    LastChatChildren = Text;
                }
            }
            ));
        }

        #endregion;

        #region Publishment;

        public async void Public(string message)
        {
            try
            {
                await PublishMessageAsync(message);
            }
            catch (Exception)
            {
                SetMessageErrorVisibility(Visibility.Visible);
            }
        }

        private async Task PublishMessageAsync(string messagePayload)
        {
            MqttApplicationMessage message = new MqttApplicationMessageBuilder()
                                                .WithTopic(OtherTopic)
                                                .WithResponseTopic(MyTopic)
                                                .WithPayload(messagePayload)
                                                .WithQualityOfServiceLevel(MqttQualityOfServiceLevel.AtLeastOnce)
                                                .Build();

            MqttClientPublishResult result = await Client.PublishAsync(message);

            if (result.IsSuccess)
            {
                ShowMessage(messagePayload, RoleEnum.Sender);
            }
        }

        #endregion;

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
