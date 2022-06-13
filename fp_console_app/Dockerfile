FROM mcr.microsoft.com/dotnet/runtime:6.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["fp_console_app/fp_console_app.csproj", "fp_console_app/"]
RUN dotnet restore "fp_console_app/fp_console_app.csproj"
COPY . .
WORKDIR "/src/fp_console_app"
RUN dotnet build "fp_console_app.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "fp_console_app.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "fp_console_app.dll"]
