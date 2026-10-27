# AutoCommand II v2026.10.27.5 — phase 5: the site-doctor core

Same app, same behavior. The "Why is this site slow?" pipeline — passive
fingerprinting from recorded traffic, the dual-shape TLS probes, the
plain-English verdict rules, the dependency-block diagnosis, and the
service outage map — is now its own tested library
(AutoCommand.Sitedoctor.Core), the fifth and final engine cluster extracted
from the UI executable ahead of the agent work.

## Verification

All six offline self-test suites build and pass against the exact libraries
shipped in this zip. The app builds clean in Debug and Release.

## Install — identical to AutoCommand

1. Unzip anywhere.
2. Right-click `AutoCommand.exe` -> **Run as administrator**.
3. Blue "protected your PC" screen: **More info -> Run anyway**. If the zip
   will not open or the exe will not start, run `unblock-autocommand.ps1`.

Deleting the folder uninstalls it completely. Apache 2.0.
