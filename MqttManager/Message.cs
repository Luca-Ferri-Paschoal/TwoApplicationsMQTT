using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MqttManager
{
    class Message
    {
        public string Text;
        public readonly string Id;
        static private string ConfirmationCode = $"2cgd56vcfvbmaqw1dlaodcv69cxjsi321cs";
        static private string ReconnectionCode = $"bdsthjbv359ckjs12948cxkjsfi29djghvd";

        public MessageTypeEnum Type;

        public Message(string text)
        {
            Text = text;
            Id = $"{Guid.NewGuid():N}";
            Type = MessageTypeEnum.Default;
        }

        public Message(string text, string id)
        {
            Text = text;
            Id = id;
            Type = MessageTypeEnum.Default;
        }

        private Message(string text, MessageTypeEnum type)
        {
            Text = text;
            Id = $"{Guid.NewGuid():N}";
            Type = type;
        }
    
        private Message(string text, string id, MessageTypeEnum type)
        {
            Text = text;
            Type = type;
            Id = id;
        }

        static public Message FromCompleteText(string completeText)
        {
            string text = completeText.Substring(33);
            string id = completeText.Remove(32);
            MessageTypeEnum type = FindType(text)
            ;

            return new Message(
                text,
                id,
                type
            );
        }

        static public Message OfConfirmation(string id)
        {
            return new Message(
                ConfirmationCode,
                id,
                MessageTypeEnum.Confirmation
            );
        }

        static public Message OfReconnection()
        {
            return new Message(
                ReconnectionCode,
                MessageTypeEnum.Reconnect
            );
        }

        static private MessageTypeEnum FindType(string text)
        {
            if (text == ConfirmationCode) return MessageTypeEnum.Confirmation;

            else if (text == ReconnectionCode) return MessageTypeEnum.Reconnect;

            else return MessageTypeEnum.Default;
        }

        public string CompleteText
        {
            get
            {
                return $"{Id} {Text}";
            }

            private set { }
        }

        public bool IsConfirmation 
        {   
            get
            {
                return Type == MessageTypeEnum.Confirmation;
            }

            private set { }
        }

        public bool IsDefault
        {
            get
            {
                return Type == MessageTypeEnum.Default;
            }

            private set { }
        }

        public bool IsReconnect
        {
            get
            {
                return Type == MessageTypeEnum.Reconnect;
            }

            private set { }
        }
    }
}
