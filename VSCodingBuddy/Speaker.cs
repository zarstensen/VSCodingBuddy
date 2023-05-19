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
using System.IO;

namespace VSCodingBuddy
{
    /// <summary>
    /// Class for handling an OpenAIService and SpeechSynthesizer instance.
    /// </summary>
    public class Speaker
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="api_key"> openai api key </param>
        /// <param name="prompt_file">
        /// ssml file that will be used to generate the final text to speech, text.
        /// The file should contain a [RESPONSE] field, which will be replaced by the openai prompt later.
        /// </param>

        public Speaker(string api_key)
        {
            m_open_ai_service = new OpenAIService(new OpenAiOptions()
            {
                ApiKey = api_key
            });

            m_speech_synth.SetOutputToDefaultAudioDevice();
            m_speech_synth.SelectVoiceByHints(VoiceGender.Male);
        }

        /// <summary>
        /// makes use of the gpt-3.5-turbo model, to reduce the number of tokens in the passed prompt.
        /// this should in turn reduce the token / cost pr. error message handled.
        /// </summary>
        /// <returns> compressed / shortened prompt </returns>
        public async Task<string> compressPrompt(string prompt)
        {
            var result = await m_open_ai_service.ChatCompletion.CreateCompletion(
                new()
                {
                    Messages = new List<ChatMessage>
                    {
                        ChatMessage.FromSystem("Compress the following prompt using as few tokens as possible, whilst the original intent of the prompt is still comprehendible by the gpt-3.5-turbo openai model. The response should only contain the compressed prompt, and nothing else."),
                        ChatMessage.FromUser($"[PROMPT]\n{prompt}\n[END PROMPT]")
                    },
                    Model = Models.ChatGpt3_5Turbo
                });

            return result.Choices.First().Message.Content;
        }
        
        /// <summary>
        /// generate a response from the given system prompt and user message.
        /// </summary>
        /// <returns> the first message in the generated messages collection. </returns>
        public async Task<string> generateResponse(string prompt, string message, int max_tokens)
        {
            var result = await m_open_ai_service.ChatCompletion.CreateCompletion(
                new()
                {
                    Messages = new List<ChatMessage>
                    {
                        ChatMessage.FromSystem(prompt),
                        ChatMessage.FromUser(message)
                    },
                    Model = Models.ChatGpt3_5Turbo,
                    MaxTokens=max_tokens
                });

            return result.Choices.First().Message.Content;
        }

        /// <summary>
        /// reads out loud the passed string, using SpeechSynthesis.
        /// </summary>
        public void speakMessage(string message)
        {
            m_speech_synth.SpeakAsyncCancelAll();
            m_speech_synth.SpeakAsync(message);
        }

        /// <summary>
        /// combines generateResponse and speakResponse.
        /// </summary>
        public async Task speakResponse(string prompt, string message, int max_tokens)
        {
            speakMessage(await generateResponse(prompt, message, max_tokens));
        }

        IOpenAIService m_open_ai_service;
        SpeechSynthesizer m_speech_synth = new();
    }
}
