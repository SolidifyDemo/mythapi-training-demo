---
name: API Develeopment Instructions
description: Guilde developers on best practices for API development in the project.
applyTo: '**Endpoints/**/*.cs'
---

# ASP.NET REST API Development Guidelines

## Code Style

- Use RESTful conventions and meaningful resource naming for endpoints.
- Implement proper input validation and error handling with standardized responses.
- Secure APIs with authentication and authorization (e.g., JWT, OAuth).
- Version your API to support future changes without breaking existing clients.
- Document your API using tools like Swagger/OpenAPI for easy client integration.

## Documentation

- Use Swagger for API documentation
- Ensure all endpoints are documented with appropriate HTTP methods, parameters, and responses
- Always update the `openapi.yaml` with the latest changes of the API, create it if it doesn't exist, and make sure it is placed in the root of the project
- Always update the `API.md`  with the latest changes of the API and make sure that:
  - Each method uses the structure of [](./api-documentation-template.md) template for documentation
  - Explain arguments in detail
  - Add proper HTTP documentation
  - Provide examples for each endpoint
  - Provide detailes error messages and their meanings

