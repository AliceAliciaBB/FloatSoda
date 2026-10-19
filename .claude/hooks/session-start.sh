#!/bin/bash
# Claude Code のクラウドセッションで .NET 10 SDK を用意し、パッケージを復元する。
set -euo pipefail

if [ "${CLAUDE_CODE_REMOTE:-}" != "true" ]; then
  exit 0
fi

DOTNET_ROOT="$HOME/.dotnet"

if ! "$DOTNET_ROOT/dotnet" --list-sdks 2>/dev/null | grep -q '^10\.'; then
  installer="$(mktemp)"
  curl -sSL https://dot.net/v1/dotnet-install.sh -o "$installer"
  bash "$installer" --channel 10.0 --install-dir "$DOTNET_ROOT"
  rm -f "$installer"
fi

export DOTNET_ROOT
export PATH="$DOTNET_ROOT:$PATH"
export DOTNET_CLI_TELEMETRY_OPTOUT=1
export DOTNET_NOLOGO=1

if [ -n "${CLAUDE_ENV_FILE:-}" ]; then
  {
    echo "export DOTNET_ROOT=\"$DOTNET_ROOT\""
    echo "export PATH=\"$DOTNET_ROOT:\$PATH\""
    echo 'export DOTNET_CLI_TELEMETRY_OPTOUT=1'
    echo 'export DOTNET_NOLOGO=1'
  } >> "$CLAUDE_ENV_FILE"
fi

cd "$CLAUDE_PROJECT_DIR"
dotnet restore
