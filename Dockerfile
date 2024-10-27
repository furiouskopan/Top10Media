# Step 1: Use the official ASP.NET Core runtime image for the base image
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

# Copy the HTTPS certificate
COPY Certs/localhostCert.pfx /https/localhostCert.pfx

# Set the environment variables for ASP.NET Core to use the HTTPS certificate
ENV ASPNETCORE_Kestrel__Certificates__Default__Path=/https/localhostCert.pfx
ENV ASPNETCORE_Kestrel__Certificates__Default__Password=Packaras1

# Step 2: Use the official .NET SDK image for building the application
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["Top10MediaApi.csproj", "./"]
RUN dotnet restore

# Step 3: Copy the rest of the application and build it
COPY . .
WORKDIR "/src/"
RUN dotnet build "Top10MediaApi.csproj" -c Release -o /app/build

# Step 4: Publish the application (this creates the final output of your build)
FROM build AS publish
RUN dotnet publish "Top10MediaApi.csproj" -c Release -o /app/publish

# Step 5: Create the runtime image using the build output
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Top10MediaApi.dll"]
