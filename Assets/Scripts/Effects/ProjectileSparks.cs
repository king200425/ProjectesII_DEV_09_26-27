using UnityEngine;
using UnityEngine.Rendering;

internal static class ProjectileSparks
{
    private const float Pixel = 0.055f;
    private const float BurstDuration = 0.24f;

    private static readonly Gradient Fade = CreateFade();
    private static readonly System.Random Random = new();

    public static void PlayMuzzle(Vector3 position, Vector2 forward, Material material, Color color, Color accent)
    {
        ParticleSystem particles = CreateBurst("Muzzle Sparks", position, material);
        Vector2 side = new(-forward.y, forward.x);
        Color hot = Color.Lerp(color, Color.white, 0.83f);

        Emit(particles, forward * Pixel, forward, new Vector2(4f, 2f), hot, 0.065f);
        Emit(particles, forward * Pixel * 3.8f, forward * 1.6f, new Vector2(3f, 1f), color, 0.081f);
        Emit(particles, forward * Pixel + side * Pixel * 1.5f, side * 0.9f, new Vector2(2f, 1f), color, 0.068f);
        Emit(particles, forward * Pixel - side * Pixel * 1.5f, -side * 0.9f, new Vector2(2f, 1f), color, 0.068f);

        EmitFan(particles, forward, color, accent, 6, 47f, 1.8f, 4f);
    }

    public static void PlayImpact(Vector3 position, Vector2 normal, Material material, Color color, Color accent)
    {
        ParticleSystem particles = CreateBurst("Impact Sparks", position, material);
        Vector2 side = new(-normal.y, normal.x);
        Color hot = Color.Lerp(color, Color.white, 0.89f);

        Emit(particles, normal * Pixel, normal * 0.6f, new Vector2(2f, 2f), hot, 0.085f);
        Emit(particles, normal * Pixel * 3f, normal * 1.2f, new Vector2(2f, 2f), color, 0.10f);
        Emit(particles, normal * Pixel * 0.5f + side * Pixel * 2.1f, side * 1.3f, Vector2.one, hot, 0.11f);
        Emit(particles, normal * Pixel * 0.5f - side * Pixel * 2.1f, -side * 1.3f, Vector2.one, hot, 0.11f);

        EmitFan(particles, normal, color, accent, 11, 79f, 1.9f, 4.5f);
    }

    private static ParticleSystem CreateBurst(string name, Vector3 position, Material material)
    {
        GameObject burst = new(name);
        burst.SetActive(false);
        burst.transform.position = position;
        ParticleSystem particles = burst.AddComponent<ParticleSystem>();

        ParticleSystem.MainModule main = particles.main;
        main.loop = false;
        main.playOnAwake = false;
        main.maxParticles = 18;
        main.duration = BurstDuration;
        main.startSize3D = true;
        main.cullingMode = ParticleSystemCullingMode.AlwaysSimulate;

        ParticleSystem.EmissionModule emission = particles.emission;
        emission.enabled = false;
        ParticleSystem.ShapeModule shape = particles.shape;
        shape.enabled = false;
        ParticleSystem.ColorOverLifetimeModule color = particles.colorOverLifetime;
        color.enabled = true;
        color.color = Fade;

        ParticleSystemRenderer renderer = burst.GetComponent<ParticleSystemRenderer>();
        renderer.sharedMaterial = material;
        renderer.renderMode = ParticleSystemRenderMode.Billboard;
        renderer.alignment = ParticleSystemRenderSpace.World;
        renderer.sortingOrder = 15;
        renderer.shadowCastingMode = ShadowCastingMode.Off;
        renderer.receiveShadows = false;

        burst.SetActive(true);
        particles.Play();
        Object.Destroy(burst, BurstDuration);
        return particles;
    }

    private static void EmitFan(ParticleSystem particles, Vector2 direction, Color color, Color accent,
                                int count, float spread, float minSpeed, float maxSpeed)
    {
        Vector2 side = new(-direction.y, direction.x);
        for (int i = 0; i < count; i++)
        {
            float fraction = (i + 0.15f + (float)Random.NextDouble() * 0.7f) / count;
            float angle = Mathf.Lerp(-spread, spread, fraction) * Mathf.Deg2Rad;
            Vector2 ray = direction * Mathf.Cos(angle) + side * Mathf.Sin(angle);
            float speed = Mathf.Lerp(minSpeed, maxSpeed, (float)Random.NextDouble());
            float life = Mathf.Lerp(0.10f, BurstDuration, (float)Random.NextDouble());
            Color sparkColor = i % 3 == 0 ? accent : Color.Lerp(color, Color.white, (i % 3) * 0.23f);
            Vector2 size = i % 4 == 1 ? new Vector2(2f, 1f) : Vector2.one;
            Emit(particles, direction * Pixel * 0.8f, ray * speed, size, sparkColor, life);
        }
    }

    private static void Emit(ParticleSystem particles, Vector2 position, Vector2 velocity, Vector2 size,
                             Color color, float lifetime)
    {
        ParticleSystem.EmitParams spark = new()
        {
            position = position,
            velocity = velocity,
            startColor = color,
            startSize3D = new Vector3(size.x * Pixel, size.y * Pixel, Pixel),
            startLifetime = lifetime
        };
        particles.Emit(spark, 1);
    }

    private static Gradient CreateFade()
    {
        Gradient fade = new();
        fade.SetKeys(new[] { new GradientColorKey(Color.white, 0f), new GradientColorKey(Color.white, 1f) },
                     new[] { new GradientAlphaKey(1f, 0f), new GradientAlphaKey(1f, 0.55f), new GradientAlphaKey(0f, 1f) });
        return fade;
    }
}
