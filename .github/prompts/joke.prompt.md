---
name: Joke Generator
description: This prompt is used to generate a joke based on a given topic.
argument-hint: "Please provide a topic for the joke"
agent: agent
model: GPT-4.1 (copilot)
tools: [vscode/askQuestions]
---

Generate a joke based on the following topic: $ARGUMENT.

If the user does not provide a topic, ask for the topic before generating the joke.