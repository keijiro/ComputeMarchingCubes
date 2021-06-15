ComputeMarchingCubes
====================

![gif](https://user-images.githubusercontent.com/343936/121985718-420d6d80-cdd0-11eb-9f2f-04ed15d925c5.gif)
![gif](https://user-images.githubusercontent.com/343936/121985721-43d73100-cdd0-11eb-89e4-3801387ac88d.gif)

**ComputeMarchingCubes** is a Unity sample project that reconstructs
isosurfaces of scalar volume data using a compute shader and the [new mesh API]
graphics buffer direct access).

[new mesh API]:
  https://docs.google.com/document/d/1_YrJafo9_ZsFm4-8K2QlD0k3RgwZ_49tSA84paobfcY/edit#heading=h.cvw3aojqmyd2

It uses the classic [marching cubes] algorithm for isosurface reconstruction.
The implementation is based on Paul Bourke's article but partially modified for
GPU optimization.

[marching cubes]: https://en.wikipedia.org/wiki/Marching_cubes
[Paul Bourke's article]: http://paulbourke.net/geometry/polygonise/

System requirements
-------------------

- Unity 2021.2.0 a19 or later
- Compute shader capable system

What's inside
-------------

This project contains two sample scenes:

- **NoiseField**: Visualizes a simple animating noise field.
- **VolumeData**: Visualizes a CT scan dataset from the
  [Stanford volume data archive].

[Stanford volume data archive]: https://graphics.stanford.edu/data/voldata/

Note that the configuration of the volume data is hardcoded in the sample
script/shader. You have to implement a data parser to support a specific volume
data format.
