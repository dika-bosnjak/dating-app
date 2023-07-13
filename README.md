# dating-app


# Dating Application

This is a dating application built with C# .NET and Angular framework. The application provides a platform for users to connect and find potential matches based on their preferences. It leverages PostgreSQL as the backend database for efficient data storage and incorporates SignalR for real-time chat functionality.


# Features

User Registration and Authentication: Users can create accounts and authenticate themselves to access the application's features.

Profile Creation: Users can create detailed profiles, including personal information, interests, and photos.

Matching Algorithm: The application utilizes a matching algorithm to suggest potential matches based on user preferences and compatibility.

Real-Time Chat: Users can communicate with their matches in real time using the integrated chat functionality powered by SignalR.

Search and Filters: Users can search for other users based on various criteria and apply filters to find specific matches.

Privacy and Security: The application ensures the privacy and security of user data by implementing authentication mechanisms and data encryption.



# Technologies

The application utilizes the following technologies:

C# .NET: A versatile programming language and framework for building robust web applications.

Angular: A popular JavaScript framework for building dynamic and responsive user interfaces.

PostgreSQL: An open-source, highly scalable, and reliable relational database management system.

SignalR: A real-time messaging library that enables bi-directional communication between clients and servers.

HTML/CSS: Standard web technologies for structuring and styling the user interface.

JavaScript/TypeScript: Programming languages used for client-side logic and interactivity.




# Configuration

Before running the application, you need to configure the database connection and any necessary API keys.

Create a PostgreSQL database to store the application data.

Open the backend/appsettings.json file and update the following configuration settings:

"ConnectionStrings:DefaultConnection": Replace with the connection string for your PostgreSQL database.

Open the frontend/src/environments/environment.ts file and update the following configuration settings:

apiUrl: Replace with the URL where the backend API is hosted.

If necessary, configure the SignalR hub in the backend/Startup.cs file.


# Usage
To start the Dating application, follow these steps:

Start the .NET backend server. Open a terminal, navigate to the backend directory, and run: dotnet run

Start the Angular frontend. Open a new terminal, navigate to the frontend directory, and run: ng serve

Access the application by opening a web browser and visiting http://localhost:4200.



# Database Setup
To set up the PostgreSQL database for the application, follow these steps:

Create a new PostgreSQL database.

Run the database migration scripts provided in the backend/Migrations directory to create the necessary tables and schema.

dotnet ef database update

The database is now ready for use with the Dating application.


