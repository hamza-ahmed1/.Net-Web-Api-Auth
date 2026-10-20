  #!/usr/bin/env bash
# Stops any running instance of the .NET app, then starts it again.

# ---- CONFIG (edit these) ----
PROJECT_DIR="."          # folder containing your .csproj
PROJECT_NAME="Auth"             # name of your app/dll (used to find running processes)
PORTS=(5000)                # ports your app listens on
# -----------------------------

echo "Stopping existing instances of $PROJECT_NAME..."

# Kill by process name/command line
pkill -f "dotnet.*$PROJECT_NAME" 2>/dev/null
pkill -f "$PROJECT_NAME" 2>/dev/null

# Kill anything still holding the ports
for PORT in "${PORTS[@]}"; do
  PIDS=$(lsof -ti tcp:"$PORT" 2>/dev/null)
  if [ -n "$PIDS" ]; then
    echo "Freeing port $PORT (PID: $PIDS)"
    kill -9 $PIDS 2>/dev/null
  fi
done

sleep 1

echo "Starting $PROJECT_NAME..."
cd "$PROJECT_DIR" || { echo "Folder not found: $PROJECT_DIR"; exit 1; }
dotnet run
