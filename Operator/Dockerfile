FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["Operator/Operator.csproj", "Operator/"]
COPY ["EventsLib/EventsLib.csproj", "EventsLib/"]
RUN dotnet restore "Operator/Operator.csproj"
COPY . .
WORKDIR "/src/Operator"
RUN dotnet build "Operator.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "Operator.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/runtime:latest AS final
WORKDIR /app
COPY --from=publish /app/publish .

RUN addgroup k8s-operator && useradd -G k8s-operator operator-user
RUN chown operator-user:k8s-operator -R .

USER operator-user

ENTRYPOINT ["dotnet", "Operator.dll"]
