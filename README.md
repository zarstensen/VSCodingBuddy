<div align="center">
  <h1 align="center">VSCodingBuddy</h1>
  <img src=VSCodingBuddy/Resources/Icon.svg alt="Icon" width="160" height="160"/>
</div>

This is an Visual Studio 2022 extension that makes use of the [OpenAI API](https://platform.openai.com/docs/api-reference),
to generate and read out loud messages with a custom personality,
whenever a project fails to build, or a runtime exception is hit.

- [Features](#features)
- [Built With](#built-with)
- [Setup](#setup)
- [Configuration](#configuration)
- [Links](#links)
- [License](#license)

## Features

### GPT-3 Messages

Generate messages about build errors and exceptions whilst coding, using OpenAIs GPT-3.5-turbo model.
Uses code snippets and error messages for increased context.

### TTS

Read aloud the generated messages, using built in Windows Text To Speech.

### Customization

Customize the personality of the messages with the personality editor.
Specify a prompt for both Build errors and Exceptions.
A default "Rude" personality is included in the extension, as well as the "Helpful" and "Maid" personality.

The editor is found under:

Tools > Options > VSCodingBuddy > Personalities

### Token limiter

Limit number of tokens used using a number of strategies.
- Random chance for errors to trigger a speech.
- Limit max number of tokens used pr. message.
- Avoid repeat error scenarios, where the error has already occurred.
- Truncate code snippets and error messages.

## Built With

- [OpenAI Chat Completion](https://platform.openai.com/docs/guides/chat) (GPT-3)
- [Betalgo.OpenAI](https://betalgo.github.io/openai/) (OpenAI API Library)
- [System.Speech](https://www.nuget.org/packages/System.Speech/) (Text To Speech)
- See visual studio solution in the github repository for all dependencies.

## Setup

In order to use this extension, an OpenAI API key is needed.

For a guide to retrieve this key, please see the [OpenAI Setup] section.

### OpenAI Setup
An OpenAI key is needed in order to use the OpenAI API.
This is not free, however OpenAI gives you a free 5$ for the first 3 months of usage.
The pricing is also in general extremely cheap (<0.002$ for a messages). This number can vary depending on the Token limiter strategies used.

To retrieve an OpenAI key follow the steps below.

- Go to the [OpenAI developer page](https://platform.openai.com/overview)
- Create account or login.
- Go to "View API Keys" under the profile menu, in the top right corner.
- Press "Create new secret key"
- Store the key for the next steps in the setup guide.

### VSCodingBuddy Setup
The VSCodingBuddy extension needs the OpenAI key in order to function.
To inform the extension about this key, follow the below steps.

- Go to Tools > Options > VSCodingBuddy > General
- Put the OpenAI key value under the OpenAI > OpenAI API Key field.

The extension is now setup and should function normally.
Additional options can be found under the General and Personalities options page.

## Configuration

All configurable options can be found under the Tools > Options > VSCodingBuddy page.

### General

Each option under the general page contains a brief description, which can be referred to.

### Personalities

This menu should be used, when editing, adding or deleting personalities.

#### Editing
Whenever the name or prompt fields are edited, the save button can be used to override the selected personalities values,
or the new button can be used, to create a new personality, with the current name and prompts.

#### Compressing
prompts can optionally be compressed, by pressing the compress button.
This works by telling a GPT-3 model to summerise the prompt, which leads to a smaller number of tokens for the prompt.
This compression strategy does not always produce a desired result, so make sure to make a backup of the original prompt.

## Links

Github -> https://github.com/karstensensensen/VSCodingBuddy

Visual Studio Marketplace -> https://marketplace.visualstudio.com/items?itemName=zarstensen.vscb-1

## License
See LICENSE.TXT

