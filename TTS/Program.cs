using TTS;

//var chat_gpt = new Speaker("",
//    "eeeeeee", "test");


throw new Exception("");

#if False
// catch exception

// generate text with chatgpt.

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
#endif