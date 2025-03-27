# MqttLoggerLite

This application saves MQTT messages coming from a Sonoff POW (flashed with Tasmota) in MongoDB.

## Background
To log how much electricity is being used by different devices around the house, and see the profile of usage.

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
Nothing complicated, just running *mosquitto broker* on a Linux server.

## The application
The application runs on the same server as it has plenty of resources available, so why not. Developed in dotnet for fun. The messages could be stored in any DB or even as files, but wanted to play with MongoDB.

It's running as background service. See the [examples](examples) folder for more info.

## Changes
- Mar 2025: It has been running fine since the last update without missing a beat. Just updated the dotnet version and dependencies.
- Nov 2023: Moving away from Raspberry Pi for now, see how it works on a Linux box, should still work, but not tested.

## Disclaimer
This is not production ready code, not even close. It was created for fun and for a specific use case and most likely full of bugs. ;)
Still, I hope it will be of some use to others.
