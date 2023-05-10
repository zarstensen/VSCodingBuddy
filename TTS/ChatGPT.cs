using OpenAI.GPT3.Managers;
using OpenAI.GPT3;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenAI.GPT3.ObjectModels.RequestModels;
using OpenAI.GPT3.Interfaces;
using OpenAI.GPT3.ObjectModels;

namespace TTS
{
    public class ChatGPT
    {
        public ChatGPT(string api_key, string prompt)
        {
            open_ai_service = new OpenAIService(new OpenAiOptions()
            {
                ApiKey = api_key
            });

            this.prompt = prompt;
        }

        public async Task<string> generateResponse(string message)
        {
            var result = await open_ai_service.ChatCompletion.CreateCompletion(
                new()
                {
                    Messages = new List<ChatMessage>
                    {
                        ChatMessage.FromSystem(prompt),
                        ChatMessage.FromUser(message)
                    },
                    Model = Models.ChatGpt3_5Turbo,
                    MaxTokens=200
                });

            return result.Choices.First().Message.Content;
        }

        IOpenAIService open_ai_service;
        string prompt;
    }
}
