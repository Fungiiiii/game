// Wind shared by the grass and the trees, so both lean the same way at the same time.
#ifndef FUNGIIIII_WIND_INCLUDED
#define FUNGIIIII_WIND_INCLUDED

static const float2 WindDirection = float2(0.8, 0.6);

// How hard the gust heard right now blows, from 0 (none) up to about 1.
// Global, set every frame by WindGustDriver from the gust sounds; 0 when nothing sets it.
float _WindGust;

#endif
