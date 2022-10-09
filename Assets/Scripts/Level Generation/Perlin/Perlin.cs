using UnityEngine;

public class Perlin
{
    private float seed;

    public Perlin(int seed)
    {
        Random.State oldState = Random.state;
        SetPerlinSeed(seed);
        Random.state = oldState;
    }

    public Perlin()
    {
        Random.State oldState = Random.state;
        int seed = Mathf.FloorToInt(Random.value * 10000000);
        SetPerlinSeed(seed);
        Random.state = oldState;
    }

    public float SimpleNoise2D(float x, float y)
    {
        return Noise2D(x, y, 1f, 1f, 1);
    }

    public float Noise2D(float x, float y, float frequency, float persistence, int octave)
    {
        return Noise3D(x, y, 0f, frequency, persistence, octave);
    }

    public float SimpleNoise3D(float x, float y, float z)
    {
        return Noise3D(x, y, z, 1f, 1f, 1);
    }

    public float Noise3D(float x, float y, float z, float frequency, float persistence, int octave)
    {
        float amplitude = 1f;
        float noise = 0.0f;
        const float perlinScaler = 0.15f;

        for (int i = 0; i < octave; ++i)
        {
            // Get all permutations of noise for each individual axis
            float noiseXY = Mathf.PerlinNoise((x * frequency + seed) * perlinScaler, (y * frequency + seed) * perlinScaler) * amplitude;
            float noiseXZ = Mathf.PerlinNoise((x * frequency + seed) * perlinScaler, (z * frequency + seed) * perlinScaler) * amplitude;
            float noiseYZ = Mathf.PerlinNoise((y * frequency + seed) * perlinScaler, (z * frequency + seed) * perlinScaler) * amplitude;

            // Reverse of the permutations of noise for each individual axis
            float noiseYX = Mathf.PerlinNoise((y * frequency + seed) * perlinScaler, (x * frequency + seed) * perlinScaler) * amplitude;
            float noiseZX = Mathf.PerlinNoise((z * frequency + seed) * perlinScaler, (x * frequency + seed) * perlinScaler) * amplitude;
            float noiseZY = Mathf.PerlinNoise((z * frequency + seed) * perlinScaler, (y * frequency + seed) * perlinScaler) * amplitude;

            // Use the average of the noise functions
            noise += (noiseXY + noiseXZ + noiseYZ + noiseYX + noiseZX + noiseZY) / 6.0f;

            amplitude *= persistence;
            frequency *= 2.0f;
        }

        // Use the average of all octaves
        return noise / octave;
    }

    private void SetPerlinSeed(int seed)
    {
        Random.InitState(seed);
        this.seed = Random.value * 100000;
    }
}
