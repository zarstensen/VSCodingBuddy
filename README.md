# VSCodingBuddy

This is an Visual Studio 2022 extension that makes use of the [OpenAI API](https://platform.openai.com/docs/api-reference),
to generate and read out loud (un)helpful messages,
whenever a project fails to build, or a runtime exception is hit.

## Features

### GPT-3 Messages

Generate messages about build errors and exceptions whilst coding, using OpenAIs GPT-3.5-turbo model.
Uses code snippets and error messages for increased context.

### TTS

Read aloud the generated messages, using built in Windows Text To Speech.

### Customization

Customize the personality of the messages with the personality editor.
Specify a prompt for both Build errors and Exceptions.
A default "Rude" personality is included in the extension

The editor is found under:

Tools > Options > VSCodingBuddy > Personalities

### Token limiter

Limit number of tokens used using a number of strategies.
- Random chance for message to be generated.
- Limit max number of tokens used pr. message.
- Avoid repeat error scenarios, where the error has already occured.

## Setup

In order to use this extension, and OpenAI API key is needed.

For a guide to retrieve this key, please see the OpenAI Setup section.

### OpenAI Setup
An OpenAI key is needed in order to use the OpenAI API.
This is not free, however OpenAI gives you a free 5$ for the first 3 months.
The pricing is also in general extremely cheap (<0.002$ for a messages). This number can vary depending on the Token limiter strategies used.

To retrieve an OpenAI key follow the steps below.

- Go to [OpenAI developer page](https://platform.openai.com/overview)
- Create account or login.
- Go to View API keys under the profile menu in the top right corner.
- Press "Create new secret key"
- Store for next steps in the setup guide.

### VSCodingBuddy Setup
The VSCodingBuddy extension needs the OpenAI key in order to function.
To inform the extension about the key, follow the below steps.

- Go to Tools > Options > VSCodingBuddy > General
- Put the OpenAI key value under the OpenAI > OpenAI API Key field.

The extension is now setup and should function normally.
Additional options can be found under the General options page.

## Built With

- [Betalgo.OpenAI](https://betalgo.github.io/openai/) (OpenAI API)
- [System.Speech](https://www.nuget.org/packages/System.Speech/) (Text To Speech)
- See visual studio solution in the github repository for all dependencies

## Github Repository

Right here -> https://github.com/karstensensensen/VSCodingBuddy

## License
See LICENSE.TXT

