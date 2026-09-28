dotnet build -c Release

scp `
    TCGProfiler\bin\Release\netstandard2.1\TCGProfiler.dll `
    deck@steamdeck:"/home/deck/.steam/steam/steamapps/common/TCG Card Shop Simulator/BepInEx/plugins/TCGProfiler.dll"
