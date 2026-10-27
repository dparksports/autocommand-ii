# AutoCommand II v2026.10.27.3 — the phase 3 build, first with a shippable zip

This is the same AutoCommand you know — same windows, same behavior, same
"see what your PC is really doing" — rebuilt on a restructured foundation:
the engines (the parts that watch, capture, classify, and stop) now live in
separate core libraries instead of being compiled into the UI executable.
For you, nothing changes in how you use the app. For the project, it means
every engine is now tested as the exact library that ships, the blocking
and undo path carries its own offline proof suite, and the codebase is
ready to grow new faces (a headless AI-access host is next).

## What is inside

- **Watch**: process monitor, phone home, spawns, traces, BITS transfers,
  traffic patterns, recorded transmissions — all powered by the extracted
  NetWatch and Intercept cores.
- **Contain**: firewall blocks with one-click undo and the Restore Internet
  panic button, telemetry opt-outs with proof, the sinkhole receipts
  listener, Defender tools — the Contain core, extracted **test-first**:
  a new 28-check offline suite proved the blocks store, the opt-out rule
  engine, and the firewall rule builders before a single file moved, and
  again against the shipped library after.
- **System**: AI access (Be My Guest), privacy, settings — unchanged.

## Verification behind this build

All five offline self-test suites (NetWatch, BITS trace, Python trace,
site doctor, contain) build and pass against the exact libraries in this
zip. The app builds clean in Debug and Release.

## Install — identical to AutoCommand

1. Unzip anywhere.
2. Right-click `AutoCommand.exe` -> **Run as administrator**.
3. If Windows shows the blue "protected your PC" screen: **More info ->
   Run anyway** (the app is not from a big company; it is open source and
   talks to no one). If the zip will not open or the exe will not start at
   all, run the `unblock-autocommand.ps1` asset below once.

Deleting the folder still uninstalls it completely. Apache 2.0. The
original repository (dparksports/autocommand-windows) keeps shipping its
own releases until this one replaces it.
