#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/azure-functions/dotnet:4 AS base
WORKDIR /home/site/wwwroot
EXPOSE 80

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["Fathym.LCU.Mortgage.Calculator.StateAPI/Fathym.LCU.Mortgage.Calculator.StateAPI.csproj", "Fathym.LCU.Mortgage.Calculator.StateAPI/"]
RUN dotnet restore "Fathym.LCU.Mortgage.Calculator.StateAPI/Fathym.LCU.Mortgage.Calculator.StateAPI.csproj"
COPY . .
WORKDIR "/src/Fathym.LCU.Mortgage.Calculator.StateAPI"
RUN dotnet build "Fathym.LCU.Mortgage.Calculator.StateAPI.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Fathym.LCU.Mortgage.Calculator.StateAPI.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /home/site/wwwroot
COPY --from=publish /app/publish .
ENV AzureWebJobsScriptRoot=/home/site/wwwroot \
    AzureFunctionsJobHost__Logging__Console__IsEnabled=true