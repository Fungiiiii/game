// Wind shared by the grass, the trees and the undergrowth, so everything leans the
// same way and the same blow is seen passing from one plant to the next.
#ifndef FUNGIIIII_WIND_INCLUDED
#define FUNGIIIII_WIND_INCLUDED

static const float2 WindDirection = float2(0.8, 0.6);

// How hard the gust heard right now blows, from 0 (none) up to about 1.
// Global, set every frame by WindGustDriver from the gust sounds; 0 when nothing sets it.
float _WindGust;

// Waves of wind travel downwind at about 9 m/s. The short one is what is seen crossing
// the canopy; the long one makes lulls and stronger spells, so the forest never sways
// like a clock.
static const float WindWaveLength = 46.0;
static const float WindWaveRate = 1.2;
static const float WindSpellLength = 170.0;
static const float WindSpellRate = 0.33;

// Strength of the wind at a place on the ground, from 0 (lull) to about 1.
// Depends on the place: neighbours are out of step, and a blow reaches them in turn.
float WindStrengthAt(float2 positionXZ)
{
    float along = dot(positionXZ, WindDirection);
    float across = dot(positionXZ, float2(-WindDirection.y, WindDirection.x));

    // Fronts are bent, and the bends drift: no straight line sweeps the forest.
    float bend = 1.4 * sin(across * 0.045 + _Time.y * 0.13) + 1.9 * sin(across * 0.013 - _Time.y * 0.07);

    float wavePhase = _Time.y * WindWaveRate - along * (6.2832 / WindWaveLength) + bend;
    float wave = 0.5 + 0.32 * sin(wavePhase) + 0.18 * sin(wavePhase * 2.3 + 1.7);

    float spellPhase = _Time.y * WindSpellRate - along * (6.2832 / WindSpellLength) + bend * 0.4;
    float spell = 0.5 + 0.5 * sin(spellPhase);

    return saturate(wave) * (0.3 + 0.7 * spell);
}

#endif
