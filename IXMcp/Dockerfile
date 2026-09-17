FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src
COPY IXMcp/IXMcp.csproj IXMcp/
RUN dotnet restore IXMcp/IXMcp.csproj
COPY IXMcp/ IXMcp/
RUN dotnet publish IXMcp/IXMcp.csproj -c Release --no-restore -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app
RUN adduser --disabled-password --gecos "" --uid 10001 ixmcp
USER 10001
COPY --from=build /app/publish .
ENV ASPNETCORE_URLS=http://+:8080
ENV IXMcp__ContractDirectory=/contracts
EXPOSE 8080
ENTRYPOINT ["dotnet", "IAX.IXMcp.dll"]
