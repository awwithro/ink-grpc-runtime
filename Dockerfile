FROM mcr.microsoft.com/dotnet/aspnet:3.1

COPY bin/Release/netcoreapp3.1/publish/ App/
RUN mkdir /Stories
COPY The-Intercept.json /Stories
WORKDIR /App
EXPOSE 50051
ENTRYPOINT ["dotnet", "ink-runtime-grpc.dll"]
