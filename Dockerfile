FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY . .

RUN dotnet restore "Proyectos con ia/DocumentApprovalSystem.sln"
RUN dotnet publish "Proyectos con ia/DocumentApprovalSystem/DocumentApprovalSystem.csproj" -c Release -o /app

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=build /app .

ENTRYPOINT ["dotnet", "DocumentApprovalSystem.dll"]
