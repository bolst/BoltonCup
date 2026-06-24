FROM python:3.12-slim

# spotDL shells out to ffmpeg for audio conversion.
RUN apt-get update \
    && apt-get install -y --no-install-recommends ffmpeg \
    && rm -rf /var/lib/apt/lists/*

WORKDIR /app

COPY requirements.txt .
RUN pip install --no-cache-dir -r requirements.txt

COPY fetch_music.py .

# One-shot: cron (host crontab or a sidecar) drives the cadence.
ENTRYPOINT ["python", "fetch_music.py"]
