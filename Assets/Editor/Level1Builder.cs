using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

// Builds Level 1 into the open scene. The player starts on an empty path,
// walks through the LEVEL 1 gate and then has to platform through:
// steps, breaking platforms, an arrow pad, a dash gap, a moving platform,
// a jump pad, crumbling steps, an arrow bridge and the goal.
// The old arena (floor, its treasure, pufferfish and falling hazards) is
// switched off, not deleted. Tick it back on in the Inspector to use it again.
// Run from the top menu: Tools > Level 1 > Build Level 1
public static class Level1Builder
{
    const string RootName = "Level1";
    const string MaterialFolder = "Assets/Material/Level1";
    const string TreasurePrefabPath = "Assets/Prefabs/Treasure.prefab";
    const string PufferPrefabPath = "Assets/Prefabs/PufferFish.prefab";
    const string SeabedMaterialPath = "Assets/Material/OceanFloor.mat";

    // The whole level is shifted back so the path starts where the player stands
    static readonly Vector3 LevelOrigin = new Vector3(0f, 0f, -40f);
    static readonly Vector3 PlayerStart = new Vector3(0f, 1.1f, 0f);

    static Material solidMat, breakingMat, movingMat, jumpPadMat, dashMat, pathMat, trapMat, postMat, goalMat;
    static GameObject treasurePrefab, pufferPrefab;

    [MenuItem("Tools/Level 1/Build Level 1")]
    public static void Build()
    {
        GameObject existing = GameObject.Find(RootName);
        if (existing != null)
        {
            if (!EditorUtility.DisplayDialog("Rebuild Level 1",
                "Level1 already exists in this scene. Delete it and build it again?",
                "Rebuild", "Cancel"))
                return;
            Undo.DestroyObjectImmediate(existing);
        }

        treasurePrefab = AssetDatabase.LoadAssetAtPath<GameObject>(TreasurePrefabPath);
        pufferPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(PufferPrefabPath);
        CreateMaterials();

        TurnOffArena();

        GameObject root = new GameObject(RootName);
        Undo.RegisterCreatedObjectUndo(root, "Build Level 1");
        root.transform.position = LevelOrigin;

        BuildEntrancePath(root.transform);
        BuildSectionA_Steps(root.transform);
        BuildSectionB_BreakingRow(root.transform);
        BuildSectionC_ArrowPadAndDashGap(root.transform);
        BuildSectionD_MovingAndJumpPad(root.transform);
        BuildSectionE_HighRoute(root.transform);
        BuildSectionF_Crossfire(root.transform);
        BuildSectionG_ArrowBridgeAndGoal(root.transform);
        BuildSeabed(root.transform);
        BuildKillZone(root.transform);

        SetUpPlayer();
        GiveMoreTime();

        EditorSceneManager.MarkSceneDirty(root.scene);
        Selection.activeGameObject = root;
        Debug.Log("Level 1 built. Press Ctrl+S to save the scene.");
    }

    // ---------- Arena ----------

    // Switch off the arena floor, its treasure, its pufferfish and the falling hazards
    static void TurnOffArena()
    {
        int count = 0;
        foreach (GameObject obj in EditorSceneManager.GetActiveScene().GetRootGameObjects())
        {
            if (obj.name == RootName || !obj.activeSelf) continue;

            bool isArena = obj.name == "Ground"
                || obj.GetComponent<HazardSpawner>() != null
                || obj.GetComponentInChildren<TreasurePickup>(true) != null
                || obj.GetComponentInChildren<PufferfishAI>(true) != null;

            if (!isArena) continue;

            Undo.RecordObject(obj, "Turn off arena");
            obj.SetActive(false);
            count++;
        }

        if (count > 0)
            Debug.Log("Switched off " + count + " arena objects (Ground, arena treasure, pufferfish, HazardSpawner). Tick them back on in the Inspector to bring the arena back.");
    }

    // ---------- Sections (positions are inside the Level1 object) ----------

    // Empty walkway the player starts on, leading to the gate
    static void BuildEntrancePath(Transform root)
    {
        Transform section = Group(root, "EntrancePath");
        Platform(section, "Walkway", 0f, 44f, 0f, 4f, 16f, pathMat);

        // Low side walls so it reads as a path
        Block(section, "WallLeft", new Vector3(-2.1f, 0.3f, 44f), new Vector3(0.2f, 0.6f, 16f), postMat);
        Block(section, "WallRight", new Vector3(2.1f, 0.3f, 44f), new Vector3(0.2f, 0.6f, 16f), postMat);

        Sign(section, "Hold W to walk in", new Vector3(0f, 1.5f, 47f), 0.12f);

        // Gate
        Post(section, "GatePostLeft", -2.5f, 52f, 0f, 4f);
        Post(section, "GatePostRight", 2.5f, 52f, 0f, 4f);
        Block(section, "GateTop", new Vector3(0f, 4.2f, 52f), new Vector3(6f, 0.5f, 0.6f), postMat);
        Sign(section, "LEVEL 1", new Vector3(0f, 5.2f, 52f), 0.25f);
    }

    // Start pad and three easy jumps going up
    static void BuildSectionA_Steps(Transform root)
    {
        Transform section = Group(root, "A_Steps");
        Platform(section, "StartPad", 0f, 55f, 0f, 6f, 6f, solidMat);
        Respawn(section, "Respawn_Start", 0f, 55f, 0f, 6f, 6f);
        Sign(section, "Collect all the treasure!\nSpace = Jump   Q / Right click = Dash", new Vector3(0f, 2.2f, 57.5f), 0.12f);

        Platform(section, "Step1", 0f, 61.5f, 0.6f, 3f, 3f, solidMat);
        Platform(section, "Step2", 3f, 66f, 1.2f, 3f, 3f, solidMat);
        Treasure(section, 3f, 66f, 1.2f);
        Platform(section, "Step3", 0f, 70.5f, 1.8f, 3f, 3f, solidMat);
    }

    // Three platforms that crumble after you land on them
    static void BuildSectionB_BreakingRow(Transform root)
    {
        Transform section = Group(root, "B_BreakingPlatforms");
        AddBreakingPlatform(section, "Breaking1", 0f, 75f, 1.8f);
        AddBreakingPlatform(section, "Breaking2", -3f, 79f, 1.8f);
        Treasure(section, -3f, 79f, 1.8f);
        AddBreakingPlatform(section, "Breaking3", 0f, 83f, 1.8f);
    }

    // Safe pad with an arrow trap and a pufferfish, then a gap too wide to jump
    static void BuildSectionC_ArrowPadAndDashGap(Transform root)
    {
        Transform section = Group(root, "C_ArrowsAndDashGap");

        Platform(section, "SafePad1", 0f, 88.5f, 1.8f, 6f, 5f, solidMat);
        Respawn(section, "Respawn_SafePad1", 0f, 88.5f, 1.8f, 6f, 5f);
        Treasure(section, -2f, 89.5f, 1.8f);
        Puffer(section, 1.5f, 87.5f, 1.8f, 1.5f);

        // Trap on a post to the right, firing left across the pad
        Post(section, "TrapPost1", 6.5f, 88.5f, -1f, 3.4f);
        AddArrowTrap(section, "ArrowTrap1", new Vector3(6.5f, 2.7f, 88.5f), -90f, 2.2f, 0f);

        Sign(section, "DASH ACROSS!\nJump, then Q in the air", new Vector3(0f, 3.4f, 91f), 0.13f);

        // 7 unit gap: too far to jump, jump + dash makes it
        Platform(section, "DashLanding", 0f, 100f, 1.8f, 4f, 4f, dashMat);
        Treasure(section, 0f, 100f, 1.8f);
    }

    // Platform sliding side to side, then a jump pad up to a high ledge
    static void BuildSectionD_MovingAndJumpPad(Transform root)
    {
        Transform section = Group(root, "D_MovingAndJumpPad");

        GameObject mover = Platform(section, "MovingPlatform", -4f, 106f, 1.8f, 3f, 3f, movingMat);
        MovingPlatform moving = Undo.AddComponent<MovingPlatform>(mover);
        moving.moveOffset = new Vector3(8f, 0f, 0f);
        moving.speed = 2.5f;

        // Floating treasure the moving platform carries you through
        Treasure(section, 0f, 106f, 1.8f);

        Platform(section, "Ledge", 4f, 111f, 2.2f, 3f, 3f, solidMat);

        GameObject pad = Platform(section, "JumpPad", 4f, 115f, 2.2f, 3f, 3f, jumpPadMat);
        JumpPad jump = Undo.AddComponent<JumpPad>(pad);
        jump.launchSpeed = 13f;
    }

    // High ledge with a guard, then crumbling steps down
    static void BuildSectionE_HighRoute(Transform root)
    {
        Transform section = Group(root, "E_HighRoute");

        Platform(section, "HighLedge", 4f, 120.5f, 6.5f, 5f, 5f, solidMat);
        Respawn(section, "Respawn_HighLedge", 4f, 120.5f, 6.5f, 5f, 5f);
        Treasure(section, 5.5f, 122f, 6.5f);
        Puffer(section, 3f, 120f, 6.5f, 1.5f);

        AddBreakingPlatform(section, "BreakingDown1", 0f, 126f, 6f);
        AddBreakingPlatform(section, "BreakingDown2", -4f, 130f, 5.4f);
        Treasure(section, -4f, 130f, 5.4f);
        AddBreakingPlatform(section, "BreakingDown3", -4f, 134f, 4.8f);
    }

    // Platform with crossfire from two arrow traps
    static void BuildSectionF_Crossfire(Transform root)
    {
        Transform section = Group(root, "F_Crossfire");

        Platform(section, "CrossfirePlatform", -4f, 141f, 4.8f, 8f, 8f, solidMat);
        Respawn(section, "Respawn_Crossfire", -4f, 141f, 4.8f, 8f, 8f);
        Treasure(section, -6.5f, 139f, 4.8f);
        Treasure(section, -1.5f, 143.5f, 4.8f);
        Puffer(section, -4f, 141f, 4.8f, 2f);

        Post(section, "TrapPost2", 2.5f, 139f, 2f, 3.4f);
        AddArrowTrap(section, "ArrowTrap2", new Vector3(2.5f, 5.7f, 139f), -90f, 2f, 0f);

        Post(section, "TrapPost3", -10.5f, 143f, 2f, 3.4f);
        AddArrowTrap(section, "ArrowTrap3", new Vector3(-10.5f, 5.7f, 143f), 90f, 2f, 1f);
    }

    // Narrow bridge with arrows from both sides, then the goal
    static void BuildSectionG_ArrowBridgeAndGoal(Transform root)
    {
        Transform section = Group(root, "G_ArrowBridgeAndGoal");

        Platform(section, "ArrowBridge", -4f, 152.5f, 4.8f, 2f, 15f, pathMat);
        Sign(section, "Time your run!", new Vector3(-4f, 7.5f, 146f), 0.15f);

        // Traps take turns firing across the bridge
        Post(section, "BridgeTrapPostA", 0f, 148f, 2f, 3.4f);
        AddArrowTrap(section, "BridgeTrapA", new Vector3(0f, 5.7f, 148f), -90f, 2.1f, 0f);

        Post(section, "BridgeTrapPostB", -8f, 152f, 2f, 3.4f);
        AddArrowTrap(section, "BridgeTrapB", new Vector3(-8f, 5.7f, 152f), 90f, 2.1f, 0.7f);

        Post(section, "BridgeTrapPostC", 0f, 156f, 2f, 3.4f);
        AddArrowTrap(section, "BridgeTrapC", new Vector3(0f, 5.7f, 156f), -90f, 2.1f, 1.4f);

        GameObject goal = Platform(section, "Goal", -4f, 163f, 4.8f, 6f, 6f, goalMat);
        Undo.AddComponent<LevelGoal>(goal);
        Respawn(section, "Respawn_Goal", -4f, 163f, 4.8f, 6f, 6f);
        Treasure(section, -4f, 164f, 4.8f);
        Sign(section, "END OF LEVEL 1", new Vector3(-4f, 8f, 166f), 0.2f);
    }

    // Seabed far below so the level doesn't float in empty space (no collider)
    static void BuildSeabed(Transform root)
    {
        GameObject seabed = GameObject.CreatePrimitive(PrimitiveType.Plane);
        Undo.RegisterCreatedObjectUndo(seabed, "Seabed");
        Object.DestroyImmediate(seabed.GetComponent<Collider>());
        seabed.name = "Seabed";
        seabed.transform.SetParent(root, false);
        seabed.transform.localPosition = new Vector3(0f, -22f, 100f);
        seabed.transform.localScale = new Vector3(20f, 1f, 20f);

        Material floor = AssetDatabase.LoadAssetAtPath<Material>(SeabedMaterialPath);
        seabed.GetComponent<Renderer>().sharedMaterial = floor != null ? floor : postMat;
    }

    // Falling below the level sends the player back to the last respawn point
    static void BuildKillZone(Transform root)
    {
        GameObject zone = new GameObject("KillZone");
        Undo.RegisterCreatedObjectUndo(zone, "Kill zone");
        zone.transform.SetParent(root, false);
        zone.transform.localPosition = new Vector3(0f, -15f, 100f);

        BoxCollider box = zone.AddComponent<BoxCollider>();
        box.isTrigger = true;
        box.size = new Vector3(200f, 4f, 240f);

        Rigidbody body = zone.AddComponent<Rigidbody>();
        body.isKinematic = true;
        body.useGravity = false;

        zone.AddComponent<KillZone>();
    }

    // ---------- Player and game settings ----------

    static void SetUpPlayer()
    {
        PlayerHealth health = Object.FindFirstObjectByType<PlayerHealth>();
        if (health == null)
        {
            Debug.LogWarning("No player with PlayerHealth found. Add PlayerDash and PlayerPlatformContact to the player by hand.");
            return;
        }

        GameObject player = health.gameObject;
        if (player.GetComponent<PlayerDash>() == null) Undo.AddComponent<PlayerDash>(player);
        if (player.GetComponent<PlayerPlatformContact>() == null) Undo.AddComponent<PlayerPlatformContact>(player);

        // Put the player at the start of the path, and move the camera with them
        Vector3 shift = PlayerStart - player.transform.position;
        Undo.RecordObject(player.transform, "Move player");
        player.transform.position = PlayerStart;

        CameraFollow follow = Object.FindFirstObjectByType<CameraFollow>();
        if (follow != null)
        {
            Undo.RecordObject(follow.transform, "Move camera");
            follow.transform.position += shift;
        }
    }

    // 60 seconds isn't enough for a whole level
    static void GiveMoreTime()
    {
        SurvivalTimer timer = Object.FindFirstObjectByType<SurvivalTimer>();
        if (timer != null && timer.timeRemaining < 300f)
        {
            Undo.RecordObject(timer, "Longer timer");
            timer.timeRemaining = 300f;
            Debug.Log("Survival timer set to 300 seconds for Level 1.");
        }
    }

    // ---------- Building blocks ----------

    static Transform Group(Transform parent, string name)
    {
        GameObject group = new GameObject(name);
        Undo.RegisterCreatedObjectUndo(group, name);
        group.transform.SetParent(parent, false);
        return group.transform;
    }

    // Box platform whose top surface sits at height "top"
    static GameObject Platform(Transform parent, string name, float x, float z, float top,
        float width, float depth, Material material)
    {
        return Block(parent, name,
            new Vector3(x, top - 0.25f, z),
            new Vector3(width, 0.5f, depth),
            material);
    }

    static GameObject Block(Transform parent, string name, Vector3 position, Vector3 size, Material material)
    {
        GameObject block = GameObject.CreatePrimitive(PrimitiveType.Cube);
        Undo.RegisterCreatedObjectUndo(block, name);
        block.name = name;
        block.transform.SetParent(parent, false);
        block.transform.localPosition = position;
        block.transform.localScale = size;
        block.GetComponent<Renderer>().sharedMaterial = material;
        return block;
    }

    static void AddBreakingPlatform(Transform parent, string name, float x, float z, float top)
    {
        GameObject platform = Platform(parent, name, x, z, top, 2.5f, 2.5f, breakingMat);
        Undo.AddComponent<BreakingPlatform>(platform);
    }

    // Vertical post from "bottom" up to "bottom + height"
    static void Post(Transform parent, string name, float x, float z, float bottom, float height)
    {
        Block(parent, name,
            new Vector3(x, bottom + height * 0.5f, z),
            new Vector3(0.8f, height, 0.8f),
            postMat);
    }

    static void AddArrowTrap(Transform parent, string name, Vector3 position, float facingY,
        float interval, float delay)
    {
        GameObject trap = Block(parent, name, position, new Vector3(0.9f, 0.9f, 0.9f), trapMat);
        trap.transform.localRotation = Quaternion.Euler(0f, facingY, 0f);

        // Nozzle so you can see which way it shoots
        GameObject nozzle = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        Undo.RegisterCreatedObjectUndo(nozzle, "Nozzle");
        Object.DestroyImmediate(nozzle.GetComponent<Collider>());
        nozzle.name = "Nozzle";
        nozzle.transform.SetParent(trap.transform, false);
        nozzle.transform.localPosition = new Vector3(0f, 0f, 0.6f);
        nozzle.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
        nozzle.transform.localScale = new Vector3(0.35f, 0.2f, 0.35f);
        nozzle.GetComponent<Renderer>().sharedMaterial = postMat;

        ArrowTrap shooter = Undo.AddComponent<ArrowTrap>(trap);
        shooter.fireInterval = interval;
        shooter.startDelay = delay;
        shooter.arrowSpeed = 9f;
        shooter.damage = 15;
        shooter.activationRange = 25f;
    }

    static void Respawn(Transform parent, string name, float x, float z, float top, float width, float depth)
    {
        GameObject point = new GameObject(name);
        Undo.RegisterCreatedObjectUndo(point, name);
        point.transform.SetParent(parent, false);
        point.transform.localPosition = new Vector3(x, top + 1.2f, z);

        BoxCollider box = point.AddComponent<BoxCollider>();
        box.isTrigger = true;
        box.size = new Vector3(width, 3f, depth);
        box.center = new Vector3(0f, 0.3f, 0f);

        point.AddComponent<RespawnPoint>();
    }

    static void Treasure(Transform parent, float x, float z, float top)
    {
        if (treasurePrefab == null)
        {
            Debug.LogWarning("Treasure prefab not found at " + TreasurePrefabPath);
            return;
        }

        GameObject treasure = (GameObject)PrefabUtility.InstantiatePrefab(treasurePrefab, parent);
        Undo.RegisterCreatedObjectUndo(treasure, "Treasure");
        treasure.transform.localPosition = new Vector3(x, top + 0.7f, z);
    }

    static void Puffer(Transform parent, float x, float z, float top, float patrolRange)
    {
        if (pufferPrefab == null)
        {
            Debug.LogWarning("PufferFish prefab not found at " + PufferPrefabPath);
            return;
        }

        GameObject fish = (GameObject)PrefabUtility.InstantiatePrefab(pufferPrefab, parent);
        Undo.RegisterCreatedObjectUndo(fish, "Pufferfish");
        fish.transform.localPosition = new Vector3(x, top + 1f, z);

        PufferfishAI ai = fish.GetComponent<PufferfishAI>();
        if (ai != null)
        {
            ai.patrolRange = patrolRange;
            ai.moveSpeed = 1.5f;
            PrefabUtility.RecordPrefabInstancePropertyModifications(ai);
        }
    }

    static void Sign(Transform parent, string text, Vector3 position, float size)
    {
        GameObject sign = new GameObject("Sign_" + text.Split('\n')[0]);
        Undo.RegisterCreatedObjectUndo(sign, "Sign");
        sign.transform.SetParent(parent, false);
        sign.transform.localPosition = position;
        // Face the follow camera, which looks in from the south-west
        sign.transform.localRotation = Quaternion.Euler(35f, 45f, 0f);

        TextMesh mesh = sign.AddComponent<TextMesh>();
        mesh.text = text;
        mesh.characterSize = size;
        mesh.fontSize = 48;
        mesh.anchor = TextAnchor.MiddleCenter;
        mesh.alignment = TextAlignment.Center;
        mesh.color = Color.white;

        Font font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        if (font != null)
        {
            mesh.font = font;
            sign.GetComponent<MeshRenderer>().sharedMaterial = font.material;
        }
    }

    // ---------- Materials ----------

    static void CreateMaterials()
    {
        solidMat = GetMaterial("Level1_Solid", new Color(0.35f, 0.45f, 0.55f));
        breakingMat = GetMaterial("Level1_Breaking", new Color(0.85f, 0.7f, 0.45f));
        movingMat = GetMaterial("Level1_Moving", new Color(0.2f, 0.75f, 0.75f));
        jumpPadMat = GetMaterial("Level1_JumpPad", new Color(0.3f, 0.9f, 0.35f));
        dashMat = GetMaterial("Level1_DashLanding", new Color(0.6f, 0.4f, 0.9f));
        pathMat = GetMaterial("Level1_Path", new Color(0.55f, 0.5f, 0.42f));
        trapMat = GetMaterial("Level1_Trap", new Color(0.55f, 0.12f, 0.1f));
        postMat = GetMaterial("Level1_Post", new Color(0.25f, 0.22f, 0.2f));
        goalMat = GetMaterial("Level1_Goal", new Color(0.95f, 0.8f, 0.2f));
    }

    static Material GetMaterial(string name, Color colour)
    {
        if (!AssetDatabase.IsValidFolder(MaterialFolder))
        {
            if (!AssetDatabase.IsValidFolder("Assets/Material"))
                AssetDatabase.CreateFolder("Assets", "Material");
            AssetDatabase.CreateFolder("Assets/Material", "Level1");
        }

        string path = MaterialFolder + "/" + name + ".mat";
        Material material = AssetDatabase.LoadAssetAtPath<Material>(path);
        if (material == null)
        {
            material = new Material(Shader.Find("Standard"));
            material.color = colour;
            AssetDatabase.CreateAsset(material, path);
        }
        return material;
    }
}
