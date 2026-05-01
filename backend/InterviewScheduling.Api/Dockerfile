FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY InterviewScheduling.Api.csproj .
RUN dotnet restore InterviewScheduling.Api.csproj
COPY . .
RUN dotnet publish InterviewScheduling.Api.csproj -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
ENV ASPNETCORE_URLS=http://+:8080
COPY --from=build /app/publish .
EXPOSE 8080
ENTRYPOINT ["dotnet", "InterviewScheduling.Api.dll"]
