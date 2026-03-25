---
name: Document Class
description: This prompt is used to generate documentation for a class in a code file.
agent: Doc Writer
model: GPT-4.1 (copilot)
argument-hint: Provide the reference to the class you want to document
tools: [read/readFile, edit/editFiles, vscode/askQuestions]
---

Add documentation to provided {given selection} and ensure that:

- Explain arguments in detail
- Add proper HTTP documentation
- Ensure compatibility with Swagger