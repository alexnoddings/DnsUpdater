FROM mcr.microsoft.com/dotnet/runtime:5.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:5.0 AS build
WORKDIR /src
COPY ["Directory.Build.props", ""]
COPY ["Host/Host.csproj", "Host/"]
COPY ["DnsUpdaters.Azure/DnsUpdaters.Azure.csproj", "DnsUpdaters.Azure/"]
COPY ["Core/Core.csproj", "Core/"]
COPY ["DnsUpdaters.Google/DnsUpdaters.Google.csproj", "DnsUpdaters.Google/"]
COPY ["IpResolvers.Ipify/IpResolvers.Ipify.csproj", "IpResolvers.Ipify/"]
COPY ["DnsUpdaters.Cloudflare/DnsUpdaters.Cloudflare.csproj", "DnsUpdaters.Cloudflare/"]
RUN dotnet restore "Host/Host.csproj"
COPY . .
WORKDIR "/src/Host"
RUN dotnet build "Host.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Host.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "DnsUpdater.dll"]
