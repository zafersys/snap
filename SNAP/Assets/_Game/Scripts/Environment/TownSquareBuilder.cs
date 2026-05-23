using UnityEngine;
using GPOyun.Core;
using GPOyun.NPC;
using GPOyun;

namespace GPOyun.Environment
{
    /// <summary>
    /// Utility script to build a Mediterranean Town Square out of Unity Primitives.
    /// Uses 'Compound Primitives' to create a stylized, low-poly aesthetic.
    /// </summary>
    public class TownSquareBuilder : MonoBehaviour
    {
        [Header("Settings")]
        public float scaleFactor = 1.0f;
        
        [ContextMenu("Build Mediterranean Square")]
        public void Build()
        {
            // Reset self-alignment to prevent "Crushed" layout
            transform.localScale = Vector3.one;
            transform.position = Vector3.zero;

            Debug.Log("[TownSquareBuilder] Building Santorini-styled Square...");

            // 1. CLEAR EXISTING
            for (int i = transform.childCount - 1; i >= 0; i--)
            {
                #if UNITY_EDITOR
                if (!Application.isPlaying) DestroyImmediate(transform.GetChild(i).gameObject);
                else Destroy(transform.GetChild(i).gameObject);
                #else
                Destroy(transform.GetChild(i).gameObject);
                #endif
            }

            // 2. GROUND (Defined as Layer 6)
            GameObject ground = CreatePrimitive(PrimitiveType.Plane, "Ground", Vector3.zero, new Vector3(20, 1, 20));
            ground.tag = "Finish"; 
            ground.layer = 6; // GROUND LAYER
            VisualUtils.ApplyAesthetic(ground, VisualUtils.SlateGrey, 0.05f);

            // 3. NATURE (Cypress Trees)
            CreateTree(new Vector3(-8, 3, 14));
            CreateTree(new Vector3(8, 3, 14));
            CreateTree(new Vector3(14, 3, -8));
            CreateTree(new Vector3(-14, 3, -8));

            // 4. VILLAGE BUILDINGS (Spread out further)
            CreateHouse("Bakery", new Vector3(-20, 0, 20), new Vector3(8, 6, 8), VisualUtils.StuccoWhite, VisualUtils.Terracotta);
            CreateHouse("Cafe", new Vector3(-20, 0, -20), new Vector3(7, 5, 7), VisualUtils.StuccoWhite, VisualUtils.CobaltBlue);
            CreateHouse("Residence_A", new Vector3(20, 0, 20), new Vector3(6, 7, 6), VisualUtils.StuccoWhite, VisualUtils.Terracotta);
            CreateHouse("Residence_B", new Vector3(25, 0, 0), new Vector3(5, 5, 9), VisualUtils.StuccoWhite, VisualUtils.CobaltBlue);

            // 5. CLOCK TOWER (Landmark)
            GameObject tower = CreatePrimitive(PrimitiveType.Cube, "ClockTower", new Vector3(18, 8f, 18), new Vector3(4f, 16, 4f));
            VisualUtils.ApplyAesthetic(tower, VisualUtils.StuccoWhite);
            GameObject towerTop = CreatePrimitive(PrimitiveType.Cube, "TowerCap", new Vector3(18, 16.5f, 18), new Vector3(4.5f, 1, 4.5f));
            VisualUtils.ApplyAesthetic(towerTop, VisualUtils.CobaltBlue);
            
            var towerSubject = tower.AddComponent<PhotoSubject>();
            towerSubject.PrimaryCategory = GPOyun.Newspaper.NewsCategory.Global;

            GameObject fBase = CreatePrimitive(PrimitiveType.Cylinder, "Fountain_Base", Vector3.zero, new Vector3(5, 0.4f, 5));
            VisualUtils.ApplyAesthetic(fBase, VisualUtils.SlateGrey);
            GameObject water = CreatePrimitive(PrimitiveType.Cylinder, "Water", new Vector3(0, 0.3f, 0), new Vector3(4.5f, 0.1f, 4.5f));
            VisualUtils.ApplyAesthetic(water, VisualUtils.FountainBlue, 0.9f);

            var fountainObstacle = fBase.AddComponent<UnityEngine.AI.NavMeshObstacle>();
            fountainObstacle.shape = UnityEngine.AI.NavMeshObstacleShape.Capsule;
            fountainObstacle.carving = true;
            fountainObstacle.radius = 2.5f;
            fountainObstacle.height = 1f;

            // Benches around fountain
            CreateBench(new Vector3(0, 0.5f, 6.0f), 0);
            CreateBench(new Vector3(0, 0.5f, -6.0f), 0);
            CreateBench(new Vector3(6.0f, 0.5f, 0), 90);
            CreateBench(new Vector3(-6.0f, 0.5f, 0), 90);

            // 6. NEWSPAPER BOARD (Modern accent)
            GameObject board = CreatePrimitive(PrimitiveType.Cube, "NewspaperBoard", new Vector3(10, 1.5f, -6), new Vector3(0.3f, 3f, 5f));
            VisualUtils.ApplyAesthetic(board, VisualUtils.WoodBrown);

            var boardObstacle = board.AddComponent<UnityEngine.AI.NavMeshObstacle>();
            boardObstacle.shape = UnityEngine.AI.NavMeshObstacleShape.Box;
            boardObstacle.carving = true;

            // 7. FLOWER POTS (Scattered life)
            CreateFlowerPot(new Vector3(-8, 0.5f, 8));
            CreateFlowerPot(new Vector3(-9, 0.5f, 8.5f));
            CreateFlowerPot(new Vector3(8, 0.5f, -8));

            // 8. NPC SPAWNERS
            for (int i = 0; i < 10; i++)
            {
                SpawnNPC(i, board.transform);
            }

            Debug.Log("[TownSquareBuilder] Village Overhaul Complete. Sprawl achieved.");
        }

        private void CreateHouse(string name, Vector3 pos, Vector3 size, Color wallCol, Color roofCol)
        {
            GameObject container = new GameObject(name);
            container.transform.SetParent(this.transform);
            container.transform.localPosition = pos;
            container.layer = 7; // COLLISION LAYER

            // Main Walls
            GameObject walls = CreatePrimitive(PrimitiveType.Cube, "Walls", new Vector3(0, size.y / 2f, 0), size, container.transform);
            VisualUtils.ApplyAesthetic(walls, wallCol);

            // Roof
            GameObject roof = CreatePrimitive(PrimitiveType.Cube, "Roof", new Vector3(0, size.y + 0.1f, 0), new Vector3(size.x + 0.5f, 0.4f, size.z + 0.5f), container.transform);
            VisualUtils.ApplyAesthetic(roof, roofCol);

            // Door (Cobalt Blue accent)
            GameObject door = CreatePrimitive(PrimitiveType.Cube, "Door", new Vector3(0, 1.25f, size.z / 2f + 0.05f), new Vector3(1.5f, 2.5f, 0.2f), container.transform);
            VisualUtils.ApplyAesthetic(door, VisualUtils.CobaltBlue);

            // Windows (Simple recessed cubes)
            CreateWindow(container.transform, new Vector3(size.x/3f, size.y*0.6f, size.z/2f + 0.05f));
            CreateWindow(container.transform, new Vector3(-size.x/3f, size.y*0.6f, size.z/2f + 0.05f));

            // Dynamic Collision: Add NavMeshObstacle so NPCs don't walk through walls!
            var nmo = container.AddComponent<UnityEngine.AI.NavMeshObstacle>();
            nmo.shape = UnityEngine.AI.NavMeshObstacleShape.Box;
            nmo.carving = true;
            nmo.size = size;
            nmo.center = new Vector3(0, size.y / 2f, 0);
        }

        private void CreateWindow(Transform parent, Vector3 pos)
        {
            GameObject window = CreatePrimitive(PrimitiveType.Cube, "Window", pos, new Vector3(1f, 1f, 0.1f), parent);
            VisualUtils.ApplyAesthetic(window, VisualUtils.FountainBlue, 0.8f);
        }

        private void CreateBench(Vector3 pos, float rotationY)
        {
            GameObject bench = new GameObject("Bench");
            bench.transform.SetParent(this.transform);
            bench.transform.localPosition = pos;
            bench.transform.localRotation = Quaternion.Euler(0, rotationY, 0);
            
            bench.AddComponent<BenchObject>();

            GameObject seat = CreatePrimitive(PrimitiveType.Cube, "Seat", new Vector3(0, 0.2f, 0), new Vector3(3f, 0.2f, 0.8f), bench.transform);
            VisualUtils.ApplyAesthetic(seat, VisualUtils.WoodBrown);

            GameObject back = CreatePrimitive(PrimitiveType.Cube, "Back", new Vector3(0, 0.6f, 0.4f), new Vector3(3f, 0.8f, 0.1f), bench.transform);
            VisualUtils.ApplyAesthetic(back, VisualUtils.WoodBrown);
        }

        private void CreateFlowerPot(Vector3 pos)
        {
            GameObject container = new GameObject("FlowerPot_Container");
            container.transform.SetParent(this.transform);
            container.transform.localPosition = pos;

            GameObject pot = CreatePrimitive(PrimitiveType.Cylinder, "FlowerPot", Vector3.zero, new Vector3(0.6f, 0.4f, 0.6f), container.transform);
            VisualUtils.ApplyAesthetic(pot, VisualUtils.Terracotta);
            
            GameObject plant = CreatePrimitive(PrimitiveType.Sphere, "Plant", Vector3.up * 0.4f, new Vector3(0.5f, 0.5f, 0.5f), container.transform);
            VisualUtils.ApplyAesthetic(plant, Color.green);
        }

        private void CreateTree(Vector3 pos)
        {
            GameObject tree = new GameObject("CypressTree");
            tree.transform.SetParent(this.transform);
            tree.transform.localPosition = pos;
            tree.layer = 7; // COLLISION LAYER

            // Trunk
            GameObject trunk = CreatePrimitive(PrimitiveType.Cylinder, "Trunk", new Vector3(0, -2.5f, 0), new Vector3(0.5f, 1.5f, 0.5f), tree.transform);
            VisualUtils.ApplyAesthetic(trunk, VisualUtils.WoodBrown);

            // Folliage (Stylized tall cone-like cypress)
            GameObject foliage = CreatePrimitive(PrimitiveType.Cylinder, "Foliage", Vector3.zero, new Vector3(1.2f, 3f, 1.2f), tree.transform);
            VisualUtils.ApplyAesthetic(foliage, VisualUtils.PineGreen);

            // Shorter foliage on top
            GameObject foliageTop = CreatePrimitive(PrimitiveType.Sphere, "FoliageTop", new Vector3(0, 3f, 0), new Vector3(1.0f, 1.0f, 1.0f), tree.transform);
            VisualUtils.ApplyAesthetic(foliageTop, VisualUtils.PineGreen);
        }

        private void SpawnNPC(int id, Transform board)
        {
            // Spawn in quadrants to avoid the central fountain (Radius 5)
            float angle = (id * 60f) * Mathf.Deg2Rad;
            Vector3 spawnPos = new Vector3(Mathf.Cos(angle) * 8f, 1f, Mathf.Sin(angle) * 8f);
            
            GameObject npcGroup = new GameObject($"NPC_{id}");
            npcGroup.transform.SetParent(this.transform);
            npcGroup.transform.localPosition = spawnPos;

            Color[] presetColors = new Color[] {
                VisualUtils.Terracotta,
                VisualUtils.CobaltBlue,
                VisualUtils.PineGreen,
                VisualUtils.WoodBrown,
                VisualUtils.StuccoWhite,
                new Color(1f, 0.55f, 0.35f), // Premium Peach
                new Color(1f, 0.82f, 0.15f), // Sunshine Yellow
                new Color(0.85f, 0.45f, 0.95f), // Soft Lavender
                new Color(0.15f, 0.75f, 0.75f), // Cozy Teal
                new Color(1f, 0.35f, 0.35f)  // Crimson Coral
            };
            Color col = presetColors[id % presetColors.Length];

            GameObject body = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            body.name = "Body";
            body.transform.SetParent(npcGroup.transform);
            body.transform.localPosition = Vector3.zero;
            VisualUtils.ApplyAesthetic(body, col);

            GameObject head = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            head.name = "Head";
            head.transform.SetParent(npcGroup.transform);
            head.transform.localPosition = new Vector3(0, 1.2f, 0);
            head.transform.localScale = new Vector3(0.6f, 0.6f, 0.6f);
            VisualUtils.ApplyAesthetic(head, col);
            
            NPCController controller = npcGroup.AddComponent<NPCController>();
            controller.NpcId = id;
            string[] names = new string[] {
                "Leo", "Zoe", "Max", "Mia", "Eli", "Ava", "Kai", "Ivy", "Rex", "Sol"
            };
            controller.NpcName = names[id % names.Length];
            controller.boardPosition = board;
            
            // Add visual helper
            npcGroup.AddComponent<NPCVisualHelper>();
            
            // Add obstacle avoidance for A1 smooth walking
            npcGroup.AddComponent<ObstacleAvoidance>();
            
            // Add PhotoSubject so the Camera can detect them
            var photoSub = npcGroup.AddComponent<PhotoSubject>();
            photoSub.SubjectName = controller.NpcName;
            photoSub.PrimaryCategory = GPOyun.Newspaper.NewsCategory.Local;
        }

        private GameObject CreatePrimitive(PrimitiveType type, string name, Vector3 pos, Vector3 scale, Transform parentOverride = null)
        {
            GameObject obj = GameObject.CreatePrimitive(type);
            obj.name = name;
            obj.transform.SetParent(parentOverride != null ? parentOverride : this.transform);
            obj.transform.localPosition = pos;
            obj.transform.localScale = scale;
            return obj;
        }
    }
}

