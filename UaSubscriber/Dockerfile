FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# enforce net8 on stack dependencies when VisualStudioVersion defaults to net9 in container
ARG CUSTOM_TEST_TARGET=net8.0
ENV CustomTestTarget=${CUSTOM_TEST_TARGET}

# copy full repository into container (safe from arbitrary build context path)
COPY . .

# restore and publish using local project references
RUN dotnet restore UaSubscriber/UaSubscriber.csproj /p:CustomTestTarget=net8.0
RUN dotnet publish UaSubscriber/UaSubscriber.csproj -c Release -o /app/publish --no-restore /p:CustomTestTarget=net8.0

FROM mcr.microsoft.com/dotnet/runtime:8.0
WORKDIR /app
COPY --from=build /app/publish .

ENV BROKER_HOST=mqtt-broker-local
ENV BROKER_PORT=1883
ENV BROKER_TLS=false
ENV BROKER_USER=iopuser
ENV BROKER_PASS=iop-opc
ENV TOPIC_PREFIX=opcua-kit
ENV PUBLISHER_ID=opcf-iiot-kit-requestor

ENTRYPOINT ["dotnet", "UaSubscriber.dll"]
