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
using System.Speech.Synthesis;

namespace TTS
{
    public class Speaker
    {
        public Speaker(string api_key, string prompt, string prompt_file)
        {
            open_ai_service = new OpenAIService(new OpenAiOptions()
            {
                ApiKey = api_key
            });

            this.prompt = prompt;
            this.prompt_file = prompt_file;

            speech_synth.SetOutputToDefaultAudioDevice();
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

        public void speakResponse(string message)
        {
            string speak_ssml = File.ReadAllText(prompt_file).Replace("[RESPONSE]", message);
            speech_synth.SpeakSsmlAsync(speak_ssml);
        }

        IOpenAIService open_ai_service;
        SpeechSynthesizer speech_synth = new();
        string prompt;
        string prompt_file;
    }
}
