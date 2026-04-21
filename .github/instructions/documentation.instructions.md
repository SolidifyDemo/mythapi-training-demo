---
name: Documentation Instructions
description: Our documentation standards and guidelines for the project.
applyTo: '**/*.cs, **/*.js, **/*.ts'
---

# Documentation Instructions

Make sure that the project is documented properly and follows the guidelines below:

- Explain arguments in detail
- Make sure to explain the purpose of the function and its return value
- Add all cases and edge cases to the documentation

## API Endpoints

- Add proper HTTP documentation
- Ensure compatibility with Swagger
- Place existing openapi definition in root of project
	- Make sure an existing API.md exists
	- Use instruction file as in [api.instructions.md](api.instructions.md)
	- The file adds documentation instructions to update the `openapi.yaml` and the `API.md` file with each change in the API endpoints. The API.md is updated with the structure given by a template