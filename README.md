🗂️ Task Management API (Backend)

A Task Management System API built with ASP.NET Core 8 Web API and Entity Framework Core.
This backend provides endpoints for managing projects, tasks, users, assignments, dependencies, comments, and attachments.

🚀 Features

Users: Create and manage users

Projects: Organize tasks into projects

Tasks: Add deadlines, priorities, and statuses

Assignments: Assign tasks to users

Dependencies: Define relationships between tasks

Comments: Allow collaboration through comments

Attachments: Upload and manage file attachments

Swagger UI for API documentation and testing

🛠️ Tech Stack

Framework: ASP.NET Core 8 Web API

Database: SQL Server (via EF Core ORM)

Migrations: Entity Framework Core

Testing: Swagger / Postman

📂 Project Structure
TaskManagementAPI/
 ┣ Controllers/          → API Controllers
 
 ┣ Models/               → Entity Models (Users, Tasks, Projects, etc.)
 
 ┣ Data/                 → EF Core DbContext
 
 ┣ Migrations/           → Database migrations
 
 ┣ Program.cs            → Application entrypoint
 
 ┣ appsettings.json      → DB connection string & configuration

⚡ Getting Started
1️⃣ Clone the Repository
git clone https://github.comCynthia-Motaung/TaskManager.git
cd task-management-api/TaskManagementAPI

2️⃣ Database Setup

Update your appsettings.json with your SQL Server connection string:

"ConnectionStrings": {
  "DefaultConnection": "Server=localhost;Database=TaskManagementDB;User Id=your_user;Password=your_password;TrustServerCertificate=True;"
}


Run migrations and update the database:

dotnet ef database update

3️⃣ Run the API
dotnet run


The API will run at:

API: https://localhost:5001

Swagger UI: https://localhost:5001/swagger

🔗 API Endpoints
Resource	Endpoint	Methods
Users	/api/users	GET, POST, PUT, DELETE
Projects	/api/projects	GET, POST, PUT, DELETE
Tasks	/api/tasks	GET, POST, PUT, DELETE
Assignments	/api/taskassignments	GET, POST, DELETE
Dependencies	/api/taskdependencies	GET, POST, DELETE
Comments	/api/comments	GET, POST, DELETE
Attachments	/api/attachments	GET, POST, DELETE

🧪 Testing

Open Swagger UI: https://localhost:5001/swagger

Or test endpoints with Postman

🎯 Future Improvements

✅ JWT Authentication & Authorization

✅ File storage integration (Azure Blob / AWS S3)

✅ Notification system for deadlines

✅ Role-based access (Admin, Manager, User)

📜 License

This project is licensed under the MIT License.
