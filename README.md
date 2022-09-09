# MqttLoggerLite

This application saves MQTT messages coming from a Sonoff POW (flashed with Tasmota) in a simple DB.

## Background
Just wanted to know how much electricity is being used by different devices around the house.

## The device
[Tasmota](https://tasmota.github.io/docs/) is an awesome firmware developed for 'smart' devices, including power meters. I happened to have an old [Sonoff POW](https://templates.blakadder.com/sonoff_Pow.html). It will send (publish) readings every 5 mins to an MQTT broker. The message looks like this:
```
{
    "Time": "2022-09-09T08:29:21",
    "ENERGY": {
        "TotalStartTime": "2022-09-06T17:53:07",
        "Total": 2.537,
        "Yesterday": 1.045,
        "Today": 0.089,
        "Period": 9,
        "Power": 113,
        "ApparentPower": 137,
        "ReactivePower": 78,
        "Factor": 0.82,
        "Voltage": 241,
        "Current": 0.569
    }
}
```

## The MQTT server
Nothing complicated, just running *mosquitto broker* on a Raspberry Pi.

## The application
The application runs on the same Rpi as it has plenty of resources available, so why not. Developed in dotnet 6 for fun. The messages could be stored in any DB or even as files, but LiteDB provides plenty of features and simplicity for this use case. Plus, I never tried it before and it's fun to play with new stuff. :)

It's running as background service. See the [examples](examples) folder for more info.

## Disclaimer
This is not production ready code, not even close. It was created for fun and for a specific use case and most likely full of bugs. ;)
Still, I hope it will be of some use to others.