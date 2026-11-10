# Build
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY Admission.Api/Admission.Api.csproj Admission.Api/
RUN dotnet restore Admission.Api/Admission.Api.csproj

COPY Admission.Api/ Admission.Api/
RUN dotnet publish Admission.Api/Admission.Api.csproj -c Release -o /app --no-restore

# Run
FROM mcr.microsoft.com/dotnet/aspnet:10.0
WORKDIR /app
COPY --from=build /app .

ENV ASPNETCORE_HTTP_PORTS=8080
EXPOSE 8080
ENTRYPOINT ["dotnet", "Admission.Api.dll"]
