🗂️ Task Management System

A full-stack Task Management System built with ASP.NET Core Web API (C#) for the backend and React (JavaScript) for the frontend.
This system allows you to manage projects, tasks, assignments, priorities, dependencies, comments, and attachments.

🚀 Features

Users: Create and manage users

Projects: Organize tasks into projects

Tasks: Assign priorities, dependencies, deadlines

Assignments: Assign tasks to users

Dependencies: Track relationships between tasks

Comments: Add comments on tasks

Attachments: Upload and manage file attachments

Swagger UI for API documentation and testing

React Frontend with project/task browsing

🛠️ Tech Stack

Backend: ASP.NET Core 8 Web API

Database: SQL Server (Entity Framework Core ORM)

Frontend: React + Axios

Tools: Swagger, Postman, Visual Studio / VS Code

📂 Project Structure
TaskManagementAPI/        → Backend API (ASP.NET Core)
  Controllers/            → API Controllers
  Models/                 → Entity Models
  Migrations/             → EF Core Migrations
  appsettings.json        → DB connection strings

task-management-frontend/ → React Frontend
  src/
    components/           → React components (ProjectList, TaskList, etc.)
    api.js                → Axios API service

⚡ Getting Started
1️⃣ Clone the Repository
git clone https://github.com/your-username/task-management-system.git
cd task-management-system

2️⃣ Backend Setup (ASP.NET Core)
cd TaskManagementAPI
dotnet restore
dotnet ef database update   # Run migrations and create DB
dotnet run                  # Run the API


API runs at: https://localhost:5001

Swagger UI: https://localhost:5001/swagger

3️⃣ Frontend Setup (React)
cd task-management-frontend
npm install
npm start


Frontend runs at: http://localhost:3000

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

Use Swagger UI to explore API endpoints

Use Postman for API testing

Use the React frontend for end-to-end interaction

🎯 Future Improvements

Authentication & Authorization (JWT)

File storage for attachments (Azure Blob / AWS S3)

Notifications & Reminders

Kanban board UI

User roles & permissions

📜 License

This project is licensed under the MIT License.
