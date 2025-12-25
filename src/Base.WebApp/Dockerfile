# Use the official image as a build stage  
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build  
WORKDIR /app  

# Copy the project files and restore dependencies  
COPY *.csproj ./  
RUN dotnet restore  

# Copy the rest of the application and build  
COPY . ./  
RUN dotnet publish -c Release -o out  

# Use the official runtime image  
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime  
WORKDIR /app  
COPY --from=build /app/out .  

# Expose the port your app will run on  
EXPOSE 80  

# Define the entry point  
ENTRYPOINT ["dotnet", "Base.WebApp.dll"] 