# UniToon

ver 0.3.0

A universally usable toon shader. compliant with Unity standard rendering functions, aiming to create a shader that can withstand avatar use across platforms, such as VRM, which is expected to grow in the future.

![img001.png](./Documents/img/img001.png)

## Compatible

UniToon supports any light environment implemented by Unity.
* Light(Directional, Point, Spot, Area)
* Light Intensity, Indirect Multipiler
* Baked Global Illmination
* Realtime Global Illmination
* Environment Lighting (Skybox, Gradient, Color)
* Reflection Probes

The following shader paths are also supported for situations where these are required
* ShadowCaster
* DepthOnly
* DepthNormals
* Meta

## Install

* Unity Package Manager (UPM)

You can import packages from the path below

`https://github.com/yoship1639/UniToon.git`

## Support Status

Render Pipeline

|  Builtin  |  URP      | HDRP      |
| --------- | --------  | --------  |
|  Not yet  |  Support  | Not yet   |

Unity Version

|  Version      | Support           |
| ------------- | ------------      |
|  Unity2019.x  | Not yet           |
|  Unity2020.x  | Not yet           |
|  Unity2021.x  | Yes (2021.2 only) |

## Gallery

Light intensity

![img002.png](./Documents/img/img002.png)

Light indirect multipiler

![img003.png](./Documents/img/img003.png)

Sky light intensity

![img004.png](./Documents/img/img004.png)

Red point light

![img005.png](./Documents/img/img005.png)

Red, Green and Blue lights

![img006.png](./Documents/img/img006.png)