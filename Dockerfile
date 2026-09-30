FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["NHIGIA.Modern/NHIGIA.Modern.csproj", "NHIGIA.Modern/"]
RUN dotnet restore "NHIGIA.Modern/NHIGIA.Modern.csproj"
COPY NHIGIA.Modern/ NHIGIA.Modern/
WORKDIR /src/NHIGIA.Modern
RUN dotnet publish "NHIGIA.Modern.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
RUN apt-get update && apt-get install -y --no-install-recommends python3 python3-venv ca-certificates \
    && rm -rf /var/lib/apt/lists/* \
    && python3 -m venv /opt/opencv
COPY NHIGIA.Modern/OpenCv/requirements.txt /tmp/opencv-requirements.txt
RUN /opt/opencv/bin/pip install --no-cache-dir -r /tmp/opencv-requirements.txt
EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080
COPY --from=build /app/publish .
RUN /opt/opencv/bin/python OpenCv/download_models.py --model-dir /opt/opencv/models
ENV HRM_OPENCV_ENABLED=true \
    HRM_OPENCV_PYTHON=/opt/opencv/bin/python \
    HRM_OPENCV_MODEL_PATH=/opt/opencv/models \
    OPENBLAS_NUM_THREADS=1 \
    OMP_NUM_THREADS=1
ENTRYPOINT ["dotnet", "NHIGIA.Modern.dll"]
