# UniToon

ver 0.21.1-alpha

Physically-based Toon Shader for game applications. Compliant with Unity standard rendering functions. It is not targeted to be multifunctional in order to keep performance, but to be universally usable.

![img001.png](https://user-images.githubusercontent.com/8346139/161423546-9cccd29b-3015-4109-960f-900093c6c9ab.png)

## Documents

[https://yoship1639.github.io/UniToon](https://yoship1639.github.io/UniToon)

## Features
* Both Toon and PBR expressions
* Blends in with the environment
* No custom shader keywords
* No goal of multifunctionality

## Compatible

UniToon supports any light environment implemented by Unity.
* Light (Directional, Point, Spot, Area)
* Light Intensity, Indirect Multipiler
* Baked Global Illmination
* Realtime Global Illmination
* Environment Lighting (Skybox, Gradient, Color)
* Reflection Probes
* Light Probes

The following shader passes are also supported for situations where these are required
* ShadowCaster
* DepthOnly
* DepthNormals
* Meta

Several rendering modes are also supported
* Forward
* Deferred

## Install

* Unity Package Manager (UPM)

You can import packages from the path below

`https://github.com/yoship1639/UniToon.git`

## Support Status

|  Version      | Builtin   | URP       | HDRP      |
| ------------- | --------- | --------- | --------- |
|  Unity2019.3  | Not yet   | Yes       | Not yet   |
|  Unity2019.4  | Not yet   | Yes       | Not yet   |
|  Unity2020.1  | Not yet   | Yes       | Not yet   |
|  Unity2020.2  | Not yet   | Yes       | Not yet   |
|  Unity2020.3  | Not yet   | Yes       | Not yet   |
|  Unity2021.1  | Not yet   | Yes       | Not yet   |
|  Unity2021.2  | Not yet   | Yes       | Not yet   |

## Gallery

Red, Green and Blue lights

![img006.png](https://user-images.githubusercontent.com/8346139/161423583-88898963-4395-40e2-98af-3a0b82c9ca69.png)
