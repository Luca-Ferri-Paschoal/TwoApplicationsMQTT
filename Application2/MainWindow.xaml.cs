using System;
using System.Threading.Tasks;
using System.Windows;
using System.Text;
using System.Windows.Controls;
using MQTTnet.Client;
using MQTTnet;
using MQTTnet.Packets;
using MQTTnet.Protocol;

namespace Application2
{
    public partial class MainWindow : Window
    {
        IMqttClient Client = null;
        MqttClientOptions Options = null;
        string ClientId = $"Application2/{Guid.NewGuid()}";
        string TopicReceiver = "Application1LFP/Message";
        string TopicSender = "Application2LFP/Message";

        private void ApplicationLoaded(object sender, EventArgs args)
        {
            MqttFactory mqttFactory = new MqttFactory();
            Client = mqttFactory.CreateMqttClient();
            Options = new MqttClientOptionsBuilder()
                            .WithClientId(ClientId)
                            .WithTcpServer("test.mosquitto.org", 1883)
                            .WithCleanSession()
                            .Build();

            Client.ConnectedAsync += async e =>
            {
                Console.WriteLine("Connected to the broker successfully");

                MqttTopicFilter topicFilter = new MqttTopicFilterBuilder()
                                                    .WithTopic(TopicReceiver)
                                                    .Build();

                _ = await Client.SubscribeAsync(topicFilter);
            };

            Client.DisconnectedAsync += async e =>
            {
                Console.WriteLine("The Broker was disconnected.");
                
                await Task.Delay(2000);

                TryConnect();
            };

            Client.ApplicationMessageReceivedAsync += e =>
            {
                Console.WriteLine("Message received succesfully");

                ShowMessage(e.ApplicationMessage.Payload);

                return Task.CompletedTask;
            };

            TryConnect();
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

        private async void TryConnect()
        {
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

        private void ShowMessage(byte[] mqttResponse)
        {
            AddMessageInTextBlock(Encoding.UTF8.GetString(mqttResponse), RoleEnum.Receiver);
        }

        private void ShowMessage(string text)
        {
            AddMessageInTextBlock(text, RoleEnum.Sender);
        }

        private void AddMessageInTextBlock(string message, RoleEnum role)
        {
            ListOfResponses.Dispatcher.Invoke(new Action(() =>
            {
                TextBlock Text = new TextBlock
                {
                    Text = message,
                    Margin = new Thickness(10.0, 10.0, 10.0, 0),
                    TextWrapping = TextWrapping.Wrap,
                    HorizontalAlignment = role == RoleEnum.Sender
                        ? HorizontalAlignment.Right
                        : HorizontalAlignment.Left
                };

                _ = ListOfResponses.Children.Add(Text);
            }
            ));
        }

        private async void PublishButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                await PublishMessageAsync();
            }
            catch
            {

            }
        }

        private async Task PublishMessageAsync()
        {
            string messagePayload = InputField.Text;

            MqttApplicationMessage message = new MqttApplicationMessageBuilder()
                                                .WithTopic(TopicSender)
                                                .WithPayload(messagePayload)
                                                .WithQualityOfServiceLevel(MqttQualityOfServiceLevel.AtLeastOnce)
                                                .Build();

            MqttClientPublishResult result = await Client.PublishAsync(message);

            if (result.IsSuccess)
            {
                ShowMessage(messagePayload);
            }

            InputField.Text = "";
        }
    }
}
