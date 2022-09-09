# Examples

## launchSettings.json
Example configuration of environment variables during development. The application relies on these for its runtime configuration.

## mqttloggerlite.service
Allow the application to run as a background service on linux. This needs to be copied into */lib/systemd/system/* or wherever systemd config files are.

Tested on Raspberry Pi (debian 10). It should be self-explanatory, but pay attention to these:
- assuming dotnet is installed in **/opt/dotnet/**
- assuming application is installed in **/home/pi/apps/mqttloggerlite/**

### Snippets:
```
First start
$ sudo systemctl daemon-reload
$ sudo systemctl start mqttloggerlite

See the logs
$ sudo journalctl -fu mqttloggerlite

Start on boot
$ sudo systemctl enable mqttloggerlite
```

