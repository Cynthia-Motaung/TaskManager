FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY TaskManager.sln ./
COPY TaskManager/TaskManager.csproj TaskManager/
COPY TaskManager.Tests/TaskManager.Tests.csproj TaskManager.Tests/
RUN dotnet restore TaskManager/TaskManager.csproj

COPY . .
RUN dotnet publish TaskManager/TaskManager.csproj -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app

COPY --from=build /app/publish ./

EXPOSE 10000
ENTRYPOINT ["sh", "-c", "dotnet TaskManager.dll --urls http://0.0.0.0:${PORT:-10000}"]
