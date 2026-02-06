#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.IO;

namespace Overun.Utils
{
    public class VFXGenerator : MonoBehaviour
    {
        [MenuItem("Overun/Generate Elemental VFX")]
        public static void GenerateVFX()
        {
            string path = "Assets/_Project/Prefabs/VFX";
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }

            CreateFireVFX(path);
            CreateIceVFX(path);
            CreateLightningVFX(path);
            CreatePoisonVFX(path);
            
            AssetDatabase.Refresh();
            Debug.Log($"[VFXGenerator] Generated VFX prefabs at {path}");
        }

        private static void CreateFireVFX(string path)
        {
            GameObject go = new GameObject("VFX_Fire");
            ParticleSystem ps = go.AddComponent<ParticleSystem>();
            var main = ps.main;
            var emission = ps.emission;
            var shape = ps.shape;
            var col = ps.colorOverLifetime;
            var size = ps.sizeOverLifetime;

            // Optimization
            main.maxParticles = 50;
            
            // Main settings
            main.startLifetime = 0.8f;
            main.startSpeed = 1.5f;
            main.startSize = new ParticleSystem.MinMaxCurve(0.2f, 0.5f);
            main.startColor = new Color(1f, 0.4f, 0.1f, 1f); // Orange
            main.simulationSpace = ParticleSystemSimulationSpace.World;
            
            // Emission
            emission.rateOverTime = 15f;
            
            // Shape
            shape.shapeType = ParticleSystemShapeType.Cone;
            shape.angle = 15f;
            shape.radius = 0.3f;
            
            // Color over lifetime
            col.enabled = true;
            Gradient grad = new Gradient();
            grad.SetKeys(
                new GradientColorKey[] { new GradientColorKey(Color.yellow, 0.0f), new GradientColorKey(Color.red, 0.5f), new GradientColorKey(Color.gray, 1.0f) },
                new GradientAlphaKey[] { new GradientAlphaKey(1.0f, 0.0f), new GradientAlphaKey(0.0f, 1.0f) }
            );
            col.color = grad;

            // Size over lifetime
            size.enabled = true;
            AnimationCurve curve = new AnimationCurve();
            curve.AddKey(0.0f, 0.5f);
            curve.AddKey(1.0f, 0.0f);
            size.size = new ParticleSystem.MinMaxCurve(1.0f, curve);

            SavePrefab(go, path);
        }

        private static void CreateIceVFX(string path)
        {
            GameObject go = new GameObject("VFX_Ice");
            ParticleSystem ps = go.AddComponent<ParticleSystem>();
            var main = ps.main;
            var emission = ps.emission;
            var shape = ps.shape;
            var col = ps.colorOverLifetime;
            var vel = ps.velocityOverLifetime;

            main.maxParticles = 30;
            main.startLifetime = 1.5f;
            main.startSpeed = 0.2f;
            main.startSize = 0.3f;
            main.startColor = new Color(0.5f, 0.9f, 1f, 0.8f); // Cyan
            main.gravityModifier = 0.1f; // Fall slowly
            
            emission.rateOverTime = 8f;
            
            shape.shapeType = ParticleSystemShapeType.Box;
            shape.scale = new Vector3(0.5f, 0.5f, 0.5f);

            col.enabled = true;
            Gradient grad = new Gradient();
            grad.SetKeys(
                new GradientColorKey[] { new GradientColorKey(Color.white, 0.0f), new GradientColorKey(Color.cyan, 1.0f) },
                new GradientAlphaKey[] { new GradientAlphaKey(0.0f, 0.0f), new GradientAlphaKey(1.0f, 0.2f), new GradientAlphaKey(0.0f, 1.0f) }
            );
            col.color = grad;
            
            SavePrefab(go, path);
        }

        private static void CreateLightningVFX(string path)
        {
            GameObject go = new GameObject("VFX_Lightning");
            ParticleSystem ps = go.AddComponent<ParticleSystem>();
            var main = ps.main;
            var emission = ps.emission;
            var shape = ps.shape;
            var noise = ps.noise;

            main.maxParticles = 20;
            main.startLifetime = 0.3f;
            main.startSpeed = 0f;
            main.startSize = 0.4f;
            main.startColor = Color.yellow;
            
            emission.rateOverTime = 10f;
            
            shape.shapeType = ParticleSystemShapeType.Sphere;
            shape.radius = 0.5f;
            
            noise.enabled = true;
            noise.strength = 5f;
            noise.frequency = 10f;
            
            // Add sparks sub-emitter if possible, but keeping it simple for generated script
            
            SavePrefab(go, path);
        }

        private static void CreatePoisonVFX(string path)
        {
            GameObject go = new GameObject("VFX_Poison");
            ParticleSystem ps = go.AddComponent<ParticleSystem>();
            var main = ps.main;
            var emission = ps.emission;
            var shape = ps.shape;
            var col = ps.colorOverLifetime;
            var size = ps.sizeOverLifetime;

            main.maxParticles = 40;
            main.startLifetime = 2.0f;
            main.startSpeed = 0.5f;
            main.startSize = 0.3f;
            main.startColor = new Color(0.2f, 1f, 0.2f); // Green
            
            emission.rateOverTime = 10f;
            
            shape.shapeType = ParticleSystemShapeType.Circle;
            shape.radius = 0.4f;
            shape.rotation = new Vector3(-90f, 0f, 0f); // Horizontal circle

            col.enabled = true;
            Gradient grad = new Gradient();
            grad.SetKeys(
                new GradientColorKey[] { new GradientColorKey(new Color(0.2f, 0.8f, 0.2f), 0f), new GradientColorKey(new Color(0.6f, 0f, 0.8f), 1f) },
                new GradientAlphaKey[] { new GradientAlphaKey(0f, 0f), new GradientAlphaKey(0.8f, 0.2f), new GradientAlphaKey(0f, 1f) }
            );
            col.color = grad;

            SavePrefab(go, path);
        }

        private static void SavePrefab(GameObject go, string dirPath)
        {
            string localPath = dirPath + "/" + go.name + ".prefab";
            
            // Make unique path
            localPath = AssetDatabase.GenerateUniqueAssetPath(localPath);
            
            PrefabUtility.SaveAsPrefabAsset(go, localPath);
            GameObject.DestroyImmediate(go);
        }
    }
}
#endif
