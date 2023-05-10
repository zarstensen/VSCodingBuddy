using Microsoft.CognitiveServices.Speech;
using Newtonsoft.Json.Linq;
using System.Speech.Synthesis;
using TTS;
using StandardSpeechSynthesis = System.Speech.Synthesis.SpeechSynthesizer;
using NeuralSpeechSynthesis = Microsoft.CognitiveServices.Speech.SpeechSynthesizer;

// catch exception

// generate text with chatgpt.

var chat_gpt = new ChatGPT("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",
    "Please explain the following exception error messages in a very rude and condescending way. Keep the responses to no more than 1000 characters, preferably less.");

string response = await chat_gpt.generateResponse("Unhandled exception. System.ArgumentOutOfRangeException: Index was out of range. Must be non-negative and less than the size of the collection. (Parameter 'index') at Program.<Main>(String[] args)");

// convert to tts and read out loud.


// See https://aka.ms/new-console-template for more information
Console.WriteLine("Hello, World!");

var synth = new StandardSpeechSynthesis();
synth.SetOutputToDefaultAudioDevice();


AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;

var creds = JObject.Parse(await File.ReadAllTextAsync("creeds.json"));

var speech_config = SpeechConfig.FromSubscription(creds["SPEECH_KEY"].ToString(), creds["SPEECH_REGION"].ToString());

speech_config.SpeechSynthesisVoiceName = "en-US-JennyNeural";

using (var speech_synth = new NeuralSpeechSynthesis(speech_config))
{
    var result = await speech_synth.SpeakSsmlAsync(File.ReadAllText("Phrase.ssml"));
}

var l = new List<int>();

l[25] += 2;

void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
{
    synth.SpeakSsmlAsync(File.ReadAllText("Phrase.ssml").Replace("[RESPONSE]", chat_gpt.generateResponse(e.ExceptionObject.ToString()).Result));
};
