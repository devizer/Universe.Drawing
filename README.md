# Universe.Drawing
Bitmap, Graphics, Bezier, Ellipse, Text, Anti aliasing (SSAA)

## what does work now:
- reading 24 & 32 bit BMP v3 from forward only readonly stream (so seek isnt required)
- writing 24 & 32 bit BMP v3 to forward only writeonly stream (so seek isnt required)
- transparent SSAA x2 - x8 upscale & downscale.
- Ellipses.
- tests

### First demo
AA of Ellipses on a photo. They drawn using Universe.Drawing and stored into bmp-file
![ellipsis](https://github.com/devizer/Universe.Drawing/raw/master/Screenshots/D1-Ellipse-AA8-8W.jpg "Ellipsis")