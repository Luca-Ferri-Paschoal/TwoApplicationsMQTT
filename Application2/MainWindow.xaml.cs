using System;
using System.Windows;
using MqttManager;

namespace Application2
{
    public partial class MainWindow : Window
    {
        string ClientId = $"Application2/{Guid.NewGuid()}";
        string MyTopic = "Application1LFP/Message";
        string OtherTopic = "Application2LFP/Message";
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
