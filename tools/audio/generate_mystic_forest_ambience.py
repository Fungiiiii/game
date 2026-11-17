"""Procedural sound examples for the Mystic Forest. Pure synthesis: noise + sines, no source recording."""
import sys
import numpy as np
from scipy.io import wavfile

SR = 48000
out_dir = sys.argv[1]
rng = np.random.default_rng(20716)


def band_noise(n, centre, width_oct, tilt=0.0, floor=25):
    """Periodic (loopable) noise, Gaussian band in log-frequency, built in the FFT domain."""
    freqs = np.fft.rfftfreq(n, 1 / SR)
    spec = rng.normal(size=freqs.size) + 1j * rng.normal(size=freqs.size)
    f = np.maximum(freqs, 1.0)
    gain = np.exp(-0.5 * (np.log2(f / centre) / width_oct) ** 2) * f ** tilt
    gain[freqs < floor] = 0
    x = np.fft.irfft(spec * gain, n)
    return x / np.sqrt(np.mean(x ** 2))


def slow_env(n, seconds, cycles, depth):
    """Slow periodic modulation: a few sines with whole numbers of cycles, so the loop stays seamless."""
    t = np.arange(n) / SR
    e = np.zeros(n)
    for c in cycles:
        e += np.sin(2 * np.pi * c * t / seconds + rng.uniform(0, 2 * np.pi)) / np.sqrt(len(cycles))
    return np.exp(depth * e)


def wind_bed(seconds, bands, cycles, depth):
    n = int(seconds * SR)
    chans = []
    shared = [slow_env(n, seconds, cycles, depth) for _ in bands]
    for _ in range(2):
        x = np.zeros(n)
        for (centre, width, level), env in zip(bands, shared):
            own = slow_env(n, seconds, cycles, depth * 0.35)
            x += level * band_noise(n, centre, width) * env * own
        chans.append(x)
    left, right = chans
    mid = 0.5 * (left + right)
    return np.stack([0.6 * left + 0.4 * mid, 0.6 * right + 0.4 * mid], axis=1)


def normalise(x, peak_db=-6.0):
    return x * (10 ** (peak_db / 20) / np.max(np.abs(x)))


def fade(x, fade_in, fade_out):
    n_in, n_out = int(fade_in * SR), int(fade_out * SR)
    env = np.ones(len(x))
    env[:n_in] = np.sin(np.linspace(0, np.pi / 2, n_in)) ** 2
    env[-n_out:] = np.cos(np.linspace(0, np.pi / 2, n_out)) ** 2
    return x * (env[:, None] if x.ndim == 2 else env)


def save(name, x):
    x = normalise(x)
    wavfile.write(f"{out_dir}/{name}.wav", SR, np.round(x * 32767).astype(np.int16))
    seam = float(np.max(np.abs(x[0] - x[-1])))
    print(f"{name}: {len(x) / SR:.1f} s, peak {20 * np.log10(np.max(np.abs(x))):.1f} dB, "
          f"rms {20 * np.log10(np.sqrt(np.mean(x ** 2))):.1f} dB, seam step {seam:.4f}")


# --- Zone beds: 60 s loops, stereo -------------------------------------------------------------
# (centre Hz, width in octaves, level)
# The first Zone A attempt is still drawn, then thrown away: it keeps the random sequence that
# produced the Zone B, Zone C and gust files Hugo approved on 17/11.
wind_bed(60, [(180, 1.2, 0.7), (700, 1.0, 0.8), (2400, 0.9, 0.75), (6000, 0.7, 0.45)], [3, 5, 8, 13], 0.55)
save("AMB_MysticForest_ZoneB", wind_bed(60, [
    (110, 1.1, 1.0), (380, 1.0, 0.8), (1100, 0.8, 0.35), (2600, 0.6, 0.08)], [2, 3, 5, 9], 0.45))
save("AMB_MysticForest_ZoneC", wind_bed(60, [
    (60, 0.9, 1.0), (170, 1.0, 0.9), (520, 0.9, 0.4), (1400, 0.7, 0.07)], [1, 2, 3, 5], 0.6))

# --- Gusts: one-shots, stereo -------------------------------------------------------------------
for i, (seconds, rise) in enumerate([(7.0, 2.2), (8.0, 3.0)], start=1):
    n = int(seconds * SR)
    t = np.arange(n) / SR
    chans = []
    for _ in range(2):
        x = np.zeros(n)
        for centre, width, level, lag in [(300, 1.0, 0.8, 0.0), (1200, 0.9, 0.9, 0.3), (4200, 0.8, 0.6, 0.6)]:
            # each band swells a little later: the gust seems to climb into the canopy
            env = np.exp(-0.5 * ((t - rise - lag) / (seconds * 0.17)) ** 2)
            x += level * band_noise(n, centre, width) * env * slow_env(n, seconds, [2, 5], 0.3)
        chans.append(x)
    save(f"SFX_MysticForest_Gust_{i:02d}", fade(np.stack(chans, axis=1), 0.3, 1.0))

# --- Zone A: the Zone B recipe, a little higher and lighter ------------------------------------
# Slower and shallower swells than Zone B: nothing moves fast at the spawn, the trees stand still. The highs stay low in level: loud bright noise
# that swells is what reads as surf.
rng = np.random.default_rng(31001)
save("AMB_MysticForest_ZoneA", wind_bed(60, [
    (150, 1.0, 0.5), (450, 1.0, 0.9), (1200, 0.9, 0.45), (3200, 0.8, 0.08)], [1, 2, 3, 5], 0.28))

# --- Giant tree presence: 32 s loop, mono -------------------------------------------------------
seconds = 32
n = seconds * SR
swell = slow_env(n, seconds, [3, 4, 7], 0.5)             # irregular, like breathing
x = np.zeros(n)
for centre, level in [(62, 1.0), (97, 0.7), (148, 0.45), (230, 0.2)]:
    x += level * band_noise(n, centre, 0.07) * slow_env(n, seconds, [2, 5, 9], 0.35)
x += 0.35 * band_noise(n, 120, 1.0)
save("AMB_MysticForest_GiantTree", x * swell)

# --- Standing stones: 16 s loop, mono -----------------------------------------------------------
seconds = 16
n = seconds * SR
t = np.arange(n) / SR
tone = np.zeros(n)
for ratio, level, beat in [(1.0, 1.0, 1 / 16), (2.76, 0.3, 1 / 8), (5.40, 0.06, 1 / 8)]:
    for detune in (-beat, beat):
        freq = round((98.0 * ratio + detune) * seconds) / seconds
        tone += level * np.sin(2 * np.pi * freq * t + rng.uniform(0, 2 * np.pi))
tone += 0.5 * band_noise(n, 98, 0.05) + 0.25 * band_noise(n, 270, 0.05)
swell = (0.5 - 0.5 * np.cos(2 * np.pi * t / seconds)) ** 2   # rises once, then dies to silence
save("AMB_MysticForest_StandingStones", tone * (0.04 + swell))
