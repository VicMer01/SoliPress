FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY . .

RUN dotnet restore "PROYECTOS/DocumentApprovalSystem.sln"
RUN dotnet publish "PROYECTOS/DocumentApprovalSystem/DocumentApprovalSystem.csproj" -c Release -o /app

FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build /app .

EXPOSE 80
ENTRYPOINT ["dotnet", "DocumentApprovalSystem.dll"]
