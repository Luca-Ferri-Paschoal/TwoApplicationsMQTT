using System;
using System.Windows;
using MqttManager;

namespace Application1
{
    public partial class MainWindow : Window
    {
        string ClientId = $"Application1/{Guid.NewGuid()}";
        string MyTopic = "Application2LFP/Message";
        string OtherTopic = "Application1LFP/Message";
        Mqtt MqttManager = null;

        private void ApplicationLoaded(object sender, EventArgs args)
        {
            MqttManager = new Mqtt(
                ClientId,
                MyTopic,
                OtherTopic,
                ListOfResponses,
                MessageError
            );
        }

        private void PublishButton_Click(object sender, RoutedEventArgs e)
        {
            MqttManager.PublicMessage(InputField.Text);

            InputField.Text = "";
        }
    }
}
