# Auth Service

## Overview

`Auth Service` is a microservice that handles authentication and authorization for the system. It uses Kafka as a messaging system, which is deployed on an external network (`microservices-network`). This service is designed to ensure secure user access, token management, and communication with other system components via asynchronous messaging.
This service is combined with UserService for faster development.

---

## Features and Functionality

1. **User Authentication**
   - Handles user login using credentials or tokens.
   - Supports session management and secure handling of user access tokens.

2. **Authorization**
   - Provides role-based access control to enforce permissions.
   - Ensures users have valid permissions for requested operations.

3. **Token Management**
   - Generates and validates JWT or Refresh tokens for authentication.
   - Includes token expiration policies and automatic refresh functionality.

4. **Integration With Kafka**
   - Publishes authentication-related events (e.g., user logged in, logged out, session expired) to Kafka topics.
   - Subscribes to relevant Kafka topics for further processing or reacting to external events.

5. **Microservice Network Communication**
   - Encapsulated to work within the `microservices-network`.
   - Facilitates communication with other microservices through Kafka and other interfaces.

6. **Databases**
   - Redis for denylist tokens
   - Postgres for users
---

## Architecture

Using clean architecture, CQRS for handling controllers.

---

## Kafka Integration Details

- **External Kafka Network**: The service relies on a Kafka instance running on the `microservices-network`. This ensures scalability and decoupling of services.
- **Kafka Topics**:
  - Authentication events are published to designated topics.
  - The service can consume or react to messages from other services by subscribing to these topics.
  - Topics are defined in package https://github.com/worldDevourer2009/TaskHandler.Shared/pkgs/nuget/worldDevourer2009.TaskHandler.Shared

---

## How to Use

### Steps to Run
1. Configure `.env` with required variables.
2. Start the services using Docker Compose:
   ```bash
   docker-compose -f compose.yml up --build
   ```
3. The API will be available at `http://localhost:9000`.


### Prerequisites
- A running instance of Kafka on `microservices-network`.
- Necessary environment variables (e.g., for Kafka connection, secrets for token signing / secrets from docker compose).
---