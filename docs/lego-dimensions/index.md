---
title: LEGO Dimensions
description: Use LEGO Dimensions toy pads and tags from .NET on Windows and Linux.
---

# LEGO Dimensions

The LEGO Dimensions project provides a .NET implementation for using compatible toy pads and tags outside the original game. It can control portal lights, detect tags, and support automation projects on Windows or Linux.

## Where to start

- Browse the [minifigure details](wiki/All-known-characters.md) for character IDs, worlds, and abilities.
- Browse the [vehicle details](wiki/All%20known%20vehicles.md) for vehicle IDs, rebuilds, worlds, and abilities.
- Read the [communication protocol](content/LegoDimensionsProtocol.md) to understand the USB messages exchanged with the toy pad.
- Browse the [.NET implementation and samples](https://github.com/Ellerbach/LegoDimensions).
- Review the platform-specific USB setup in the repository before connecting a portal.

The documented USB protocol applies to non-Xbox portals with vendor ID `0x0E6F` and product ID `0x0241`.
