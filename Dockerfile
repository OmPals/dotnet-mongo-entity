FROM mcr.microsoft.com/dotnet/core/sdk:3.1 AS build-env
COPY ./ /app
WORKDIR /app
EXPOSE 8080
ENV ASPNETCORE_URLS=http://*:8080
RUN dotnet publish -c Release
WORKDIR /app/Project1/bin/Release/netcoreapp3.1/publish/
#ENTRYPOINT ["dotnet", "Project1.dll"]
CMD ASPNETCORE_URLS=http://*:$PORT 
CMD dotnet Project1.dll
ENV ASPNETCORE_ENVIRONMENT=Development