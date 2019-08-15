FROM mcr.microsoft.com/dotnet/core/aspnet:2.2-stretch-slim AS base
#FROM mcr.microsoft.com/dotnet/core/aspnet AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/core/sdk:2.2-stretch AS build
#FROM mcr.microsoft.com/dotnet/core/sdk AS build
WORKDIR /src
COPY ["SignalRApp/SignalRApp.csproj", "SignalRApp/"]
RUN dotnet restore "SignalRApp/SignalRApp.csproj"
COPY . .
WORKDIR "/src/SignalRApp"
RUN dotnet build "SignalRApp.csproj" -c Release -o /app

FROM build AS publish
RUN dotnet publish "SignalRApp.csproj" -c Release -o /app

FROM base AS final
WORKDIR /app
COPY --from=publish /app .
ENTRYPOINT ["dotnet", "SignalRApp.dll"]