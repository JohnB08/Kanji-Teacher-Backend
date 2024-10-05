# Use the official Microsoft .NET SDK image for building the project
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build

# Set the working directory inside the container
WORKDIR /app

# Copy the project files to the container
COPY *.csproj ./
RUN dotnet restore

# Copy the rest of the application code to the container
COPY . ./

# Build the application
RUN dotnet publish -c Release -o out

# Use a lightweight runtime image for running the app
FROM mcr.microsoft.com/dotnet/aspnet:7.0 AS runtime

# Set the working directory in the runtime container
WORKDIR /app

# Copy the built application from the build container to the runtime container
COPY --from=build /app/out .

# Expose the port on which your app will run (e.g., 80 or any other port)
EXPOSE 80

# Define the entry point for the container
ENTRYPOINT ["dotnet", "kanji_teacher_backend.dll"]