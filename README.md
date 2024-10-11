# Kanji Teacher Backend

Welcome to the **Kanji Teacher Backend**! This application is a C#/.NET 8 backend service for managing user interactions and data in the Kanji learning application. The backend uses **ASP.NET Core**, **Entity Framework Core**, **SQLite** for database management, and **Firebase** for user authentication. This README provides an overview of the setup, endpoints, and core services.

## Table of Contents

1. [Getting Started](#getting-started)
2. [Environment Variables](#environment-variables)
3. [Endpoints](#endpoints)
4. [Database Context](#database-context)
5. [Services](#services)
6. [Running the Application](#running-the-application)

## Getting Started

### Prerequisites

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- [SQLite](https://www.sqlite.org/download.html)
- [Firebase Admin SDK](https://firebase.google.com/docs/admin/setup)

### Installation

1. Clone this repository.
   ```sh
   git clone <repo-url>
   cd kanji-teacher-backend
   ```
2. Install the necessary dependencies.
   ```sh
   dotnet restore
   ```
3. Ensure you have set up all required **environment variables** (see below).

## Environment Variables

To configure the backend, create a `.env` file in the root directory with the following environment variables:

- **ALLOWED_ORIGIN**: URL of the allowed origin for CORS (default: `https://kanji-teacher-backend.onrender.com`).
- **DATABASE_LOCATION**: Path to the SQLite database (default: `/var/data/KanjiTeacherDatabase.db`).
- **GOOGLE_AUTH_JSON**: Base64-encoded Google Service Account credentials for Firebase.
- **FIREBASE_APIKEY**: Firebase API key.
- **FIREBASE_AUTHDOMAIN**: Firebase Auth domain.
- **FIREBASE_PROJECTID**: Firebase Project ID.
- **FIREBASE_STORAGEBUCKET**: Firebase Storage bucket.
- **FIREBASE_MESSAGINGSENDERID**: Firebase Messaging sender ID.
- **FIREBASE_APPID**: Firebase App ID.
- **PORT**: Port for hosting the application (default: `5000`).

## Endpoints

### FlashCardDataController

**Base Route**: `/api`

- **GET /getFlashCard**

  - **Description**: Retrieves flashcard data for a specific user.
  - **Query Parameters**:
    - `progress` (optional): Boolean value indicating if the user wants to progress.
  - **Returns**: JSON object with flashcard information.

- **GET /validateAnswer**

  - **Description**: Validates a user's answer for a specific flashcard.
  - **Query Parameters**:
    - `id`: ID of the flashcard relation.
    - `answer`: The user's submitted answer.
  - **Returns**: JSON object indicating if the answer is correct and the relevant character information.

- **GET /userinfo**

  - **Description**: Retrieves user information including stats and progress.
  - **Returns**: JSON object containing user data.

### ConfigController

**Base Route**: `/api`

- **GET /config**
  - **Description**: Retrieves Firebase configuration details.
  - **Returns**: JSON object containing Firebase configuration settings.

## Database Context

The backend uses **Entity Framework Core** with **SQLite** as the database. The **KTContext** (`Kanji_teacher_backend.dbContext.KTContext`) manages the following tables:

- **Characters**: Stores Kanji character data.
- **Users**: Stores user data.
- **UserCharacterRelations**: Stores relationships between users and Kanji characters.

The database location is configurable through the `DATABASE_LOCATION` environment variable.

## Services

### FirebaseService

- This service is used for validating Firebase authentication tokens.
- Credentials are loaded from the environment variable `GOOGLE_AUTH_JSON`.

### KawazuConverter

- This singleton service converts Kanji readings into Romaji using the **Kawazu** package, which provides functionality for handling Japanese text.

## Running the Application

To run the backend locally:

```sh
dotnet run
```

The application listens on the port defined by the `PORT` environment variable (default is `5000`).

Swagger documentation is available in **development** mode at `/swagger`.

### Building for Production

To build the application for production:

```sh
dotnet publish -c Release
```

Ensure your environment variables are set up correctly for the production environment. Use **Docker** or your preferred deployment method to host the application.

## CORS Configuration

CORS is configured to allow requests from specific origins. The allowed origins include:

- The default origin (`ALLOWED_ORIGIN` environment variable)
- `https://www.kanjiteacher.com`
- `https://kanjiteacher.com`

You can modify these settings in the `Program.cs` file to accommodate your specific requirements.

## License

This project is licensed under the [MIT License](LICENSE).

## Contributing

If you would like to contribute, please fork the repository and submit a pull request. Any contributions are greatly appreciated!

## Contact

For questions or support, please reach out to the project maintainer.
