using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

// Builds Level 4: "The Abyss", a dark trench below the shipwreck.
//
// Levels 1-3 run straight into each other in Main.unity. Level 4 is its own
// scene: finishing Level 3 opens the Level Complete menu, and its Continue
// button loads Level4.
//
// Tools > Level 4 > Create Level 4 Scene
//   Copies Main.unity to Level4.unity (so the player, camera, HUD and managers
//   come along), removes Levels 1-3 from the copy, builds Level 4, saves it and
//   adds it to File > Build Settings.
//
// Tools > Level 4 > Rebuild Level 4 In Open Scene
//   Deletes and rebuilds just the Level4 object in the open scene, for when
//   you've tweaked numbers in this file.
//
// New challenges:
//   slippery wobbling slime rocks, sticky trash that weighs you down,
//   a rising rock lift, exploding fish, a secret dash-only ledge, and at the
//   end the oxygen tank breaks and you cross using air bubbles.
public static class Level4Builder
{
    const string RootName = "Level4";
    static readonly string[] OtherLevelRoots = { "Level1", "Levels2And3" };
    const string MainScenePath = "Assets/Scenes/Main.unity";
    const string Level4ScenePath = "Assets/Scenes/Level4.unity";
    const string MaterialFolder = "Assets/Material/Level4";
    const string TreasurePrefabPath = "Assets/Prefabs/Treasure.prefab";
    const string SeabedMaterialPath = "Assets/Material/OceanFloor.mat";

    // Same start as Level 1, so the camera offset in Main.unity still works
    static readonly Vector3 LevelOrigin = new Vector3(0f, 0f, -40f);
    static readonly Vector3 PlayerStart = new Vector3(0f, 1.1f, 0f);

    static Material rockMat, ledgeMat, wallMat, slimeMat, crumbleMat, movingMat, pathMat,
        trapMat, postMat, goalMat, trashMat, trashBagMat, fishMat, finMat, bubbleMat, warningMat, crystalMat;
    static GameObject treasurePrefab;

    // ---------- Menu items ----------

    [MenuItem("Tools/Level 4/Create Level 4 Scene")]
    public static void CreateScene()
    {
        if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;

        if (AssetDatabase.LoadAssetAtPath<SceneAsset>(MainScenePath) == null)
        {
            EditorUtility.DisplayDialog("Level 4", "Couldn't find " + MainScenePath, "OK");
            return;
        }

        if (AssetDatabase.LoadAssetAtPath<SceneAsset>(Level4ScenePath) != null)
        {
            if (!EditorUtility.DisplayDialog("Create Level 4 Scene",
                "Level4.unity already exists. Replace it with a fresh copy of Main.unity and rebuild Level 4?\n\n" +
                "Anything you changed by hand in Level4.unity will be lost. " +
                "(Use 'Rebuild Level 4 In Open Scene' to only rebuild the level.)",
                "Replace", "Cancel"))
                return;
            AssetDatabase.DeleteAsset(Level4ScenePath);
        }

        if (!AssetDatabase.CopyAsset(MainScenePath, Level4ScenePath))
        {
            Debug.LogError("Couldn't copy " + MainScenePath + " to " + Level4ScenePath);
            return;
        }

        Scene scene = EditorSceneManager.OpenScene(Level4ScenePath, OpenSceneMode.Single);

        // Levels 1-3 don't belong in this scene
        foreach (GameObject obj in scene.GetRootGameObjects())
            if (System.Array.IndexOf(OtherLevelRoots, obj.name) >= 0) Object.DestroyImmediate(obj);

        BuildLevel();

        EditorSceneManager.SaveScene(scene);
        AddToBuildSettings();

        Debug.Log("Level 4 created at " + Level4ScenePath + " and added to Build Settings. " +
                  "Finishing Level 3 now continues to Level 4.");
    }

    [MenuItem("Tools/Level 4/Rebuild Level 4 In Open Scene")]
    public static void RebuildInOpenScene()
    {
        Scene scene = EditorSceneManager.GetActiveScene();
        if (scene.path != Level4ScenePath &&
            !EditorUtility.DisplayDialog("Rebuild Level 4",
                "The open scene isn't Level4.unity. Build Level 4 into '" + scene.name + "' anyway?",
                "Build here", "Cancel"))
            return;

        GameObject existing = GameObject.Find(RootName);
        if (existing != null) Undo.DestroyObjectImmediate(existing);

        BuildLevel();
        EditorSceneManager.MarkSceneDirty(scene);
        Debug.Log("Level 4 rebuilt. Press Ctrl+S to save the scene.");
    }

    // ---------- Build ----------

    static void BuildLevel()
    {
        treasurePrefab = AssetDatabase.LoadAssetAtPath<GameObject>(TreasurePrefabPath);
        CreateMaterials();

        GameObject root = new GameObject(RootName);
        Undo.RegisterCreatedObjectUndo(root, "Build Level 4");
        root.transform.position = LevelOrigin;

        BuildEntrancePath(root.transform);
        BuildSectionA_SlimeSteps(root.transform);
        BuildSectionB_TrashShelf(root.transform);
        BuildSectionC_Lift(root.transform);
        BuildSectionD_FishRidge(root.transform);
        BuildSectionE_CrumblingDescent(root.transform);
        BuildSectionF_TankBreaks(root.transform);
        BuildSectionG_BubbleCrossing(root.transform);
        BuildTrenchDecor(root.transform);
        BuildSeabed(root.transform);
        BuildKillZone(root.transform);

        SetUpPlayer();
        SetTimer(360f);

        Selection.activeGameObject = root;
    }

    static void AddToBuildSettings()
    {
        var scenes = new List<EditorBuildSettingsScene>(EditorBuildSettings.scenes);
        foreach (EditorBuildSettingsScene s in scenes)
            if (s.path == Level4ScenePath) { s.enabled = true; EditorBuildSettings.scenes = scenes.ToArray(); return; }

        scenes.Add(new EditorBuildSettingsScene(Level4ScenePath, true));
        EditorBuildSettings.scenes = scenes.ToArray();
    }

    // ---------- Sections (positions are inside the Level4 object) ----------

    static void BuildEntrancePath(Transform root)
    {
        Transform section = Group(root, "EntrancePath");
        Platform(section, "Walkway", 0f, 44f, 0f, 4f, 16f, pathMat);
        Block(section, "WallLeft", new Vector3(-2.1f, 0.3f, 44f), new Vector3(0.2f, 0.6f, 16f), wallMat);
        Block(section, "WallRight", new Vector3(2.1f, 0.3f, 44f), new Vector3(0.2f, 0.6f, 16f), wallMat);

        Sign(section, "Below the shipwreck, the trench drops into darkness...", new Vector3(0f, 1.5f, 47f), 0.12f);

        Post(section, "GatePostLeft", -2.5f, 52f, 0f, 4f);
        Post(section, "GatePostRight", 2.5f, 52f, 0f, 4f);
        Block(section, "GateTop", new Vector3(0f, 4.2f, 52f), new Vector3(6f, 0.5f, 0.6f), postMat);
        Sign(section, "LEVEL 4", new Vector3(0f, 5.2f, 52f), 0.25f);
    }

    // Three slippery, wobbling slime-covered rocks going up
    static void BuildSectionA_SlimeSteps(Transform root)
    {
        Transform section = Group(root, "A_SlimeSteps");
        Platform(section, "StartPad", 0f, 55f, 0f, 6f, 6f, rockMat);
        Respawn(section, "Respawn_Start", 0f, 55f, 0f, 6f, 6f);
        LevelBanner(section, 0f, 55f, 0f, 6f, 6f);
        Sign(section, "THE ABYSS\nGlowing slime is slippery - don't slide off!",
            new Vector3(0f, 2.2f, 57.5f), 0.12f);

        Slippery(section, "Slime1", 0f, 62f, 0.4f, 3.5f, 3.5f);
        Slippery(section, "Slime2", 2.5f, 67.5f, 0.8f, 3.5f, 3.5f);
        Treasure(section, 2.5f, 67.5f, 0.8f);
        Slippery(section, "Slime3", 0f, 73f, 1.2f, 3.5f, 3.5f);
    }

    // Rock shelf where rubbish from the surface has piled up. Getting stuck
    // stops you jumping high enough to climb onto the next step.
    static void BuildSectionB_TrashShelf(Transform root)
    {
        Transform section = Group(root, "B_TrashShelf");

        Platform(section, "Shelf", 0f, 80.5f, 1.2f, 8f, 7f, ledgeMat);
        Respawn(section, "Respawn_Shelf", -2.5f, 78.2f, 1.2f, 3f, 2.4f);
        Block(section, "ShelfWallLeft", new Vector3(-4.1f, 1.8f, 80.5f), new Vector3(0.2f, 1.2f, 7f), wallMat);
        Block(section, "ShelfWallRight", new Vector3(4.1f, 1.8f, 80.5f), new Vector3(0.2f, 1.2f, 7f), wallMat);

        // Patches leave a zig-zag path: up the left, across, then up the middle-right lane
        Trash(section, "TrashA", 1.5f, 78.5f, 1.2f, 5f, 1.4f);
        Trash(section, "TrashB", -1.75f, 81.1f, 1.2f, 4.5f, 1.4f);
        Trash(section, "TrashC", 3f, 83.4f, 1.2f, 2f, 1.2f);

        // Treasure sitting in the rubbish - worth getting stuck for?
        Treasure(section, -3f, 81.1f, 1.2f);

        Sign(section, "Avoid the sticky trash!\nIt slows you and weighs down your jumps",
            new Vector3(0f, 3.8f, 84.5f), 0.12f);

        // Step too high to reach while stuck
        Platform(section, "RaisedStep", -0.5f, 87f, 2.4f, 3f, 3f, rockMat);
    }

    // Rising rock carrying you up to a lookout
    static void BuildSectionC_Lift(Transform root)
    {
        Transform section = Group(root, "C_Lift");

        GameObject lift = Platform(section, "RisingRock", -0.5f, 92f, 2.4f, 3f, 3f, movingMat);
        MovingPlatform mover = Undo.AddComponent<MovingPlatform>(lift);
        mover.moveOffset = new Vector3(0f, 4f, 0f);
        mover.speed = 1.5f;
        Sign(section, "Ride the rising rock up", new Vector3(2.8f, 4.4f, 90f), 0.12f);

        Platform(section, "Lookout", -0.5f, 98f, 6.4f, 5f, 5f, ledgeMat);
        Respawn(section, "Respawn_Lookout", -0.5f, 98f, 6.4f, 5f, 5f);
        Crystal(section, "LookoutCrystal", new Vector3(-2.4f, 6.4f, 99.8f), 2.2f, 15f);
        Treasure(section, 1f, 99f, 6.4f);
    }

    // Narrow rock ridge guarded by two exploding fish, plus a secret ledge
    static void BuildSectionD_FishRidge(Transform root)
    {
        Transform section = Group(root, "D_FishRidge");

        Platform(section, "Ridge", -0.5f, 106f, 6.4f, 2f, 10f, ledgeMat);
        Sign(section, "EXPLODING FISH!\nDash through them, or set them off and back away",
            new Vector3(-0.5f, 9.4f, 101.5f), 0.12f);

        Fish(section, "ExplodingFish1", new Vector3(-2f, 7.4f, 104f), new Vector3(3f, 0f, 0f), 1.4f);
        Fish(section, "ExplodingFish2", new Vector3(1f, 7.4f, 108.5f), new Vector3(-3f, 0f, 0f), 1.8f);

        // Hidden treasure: too far to jump, jump + dash makes it (and back)
        Platform(section, "SecretLedge", 6.5f, 106f, 6.4f, 2.5f, 2.5f, rockMat);
        Treasure(section, 6.5f, 106f, 6.4f);
    }

    // Crumbling rocks down to the trench floor ledge, with an arrow trap
    static void BuildSectionE_CrumblingDescent(Transform root)
    {
        Transform section = Group(root, "E_CrumblingDescent");

        Breaking(section, "CrumblingRock1", -0.5f, 113.5f, 5.8f);
        Breaking(section, "CrumblingRock2", -3.5f, 117.5f, 5.2f);
        Treasure(section, -3.5f, 117.5f, 5.2f);
        Breaking(section, "CrumblingRock3", -3.5f, 121.5f, 4.6f);

        Platform(section, "TrenchLedge", -3.5f, 128f, 4.6f, 6f, 8f, ledgeMat);
        Respawn(section, "Respawn_TrenchLedge", -3.5f, 125.5f, 4.6f, 6f, 3f);
        Treasure(section, -5.5f, 127f, 4.6f);

        // Old booby trap left by earlier treasure hunters
        Post(section, "TrapPost", 3f, 128.5f, 1.7f, 3.4f);
        AddArrowTrap(section, "BoobyTrap", new Vector3(3f, 5.5f, 128.5f), -90f, 2f, 0f);
    }

    // The tank breaks at the far end of the trench ledge
    static void BuildSectionF_TankBreaks(Transform root)
    {
        Transform section = Group(root, "F_TankBreaks");

        GameObject zone = new GameObject("OxygenBreakZone");
        Undo.RegisterCreatedObjectUndo(zone, "Oxygen break zone");
        zone.transform.SetParent(section, false);
        zone.transform.localPosition = new Vector3(-3.5f, 4.6f + 1.5f, 130.5f);
        BoxCollider box = zone.AddComponent<BoxCollider>();
        box.isTrigger = true;
        box.size = new Vector3(6f, 3f, 3f);
        zone.AddComponent<OxygenBreakZone>();

        // Red strip on the floor so the spot is visible
        Block(section, "DangerStrip", new Vector3(-3.5f, 4.61f, 130.5f), new Vector3(6f, 0.02f, 3f), warningMat, false);

        Sign(section, "CRUSHING DEPTHS AHEAD\nFollow the air bubbles!", new Vector3(-3.5f, 8f, 131f), 0.13f);
    }

    // Small platforms with air bubbles all the way to the goal
    static void BuildSectionG_BubbleCrossing(Transform root)
    {
        Transform section = Group(root, "G_BubbleCrossing");

        Bubble(section, -3.5f, 131.5f, 4.6f);

        Platform(section, "Rock1", -3.5f, 136f, 4.6f, 2.5f, 2.5f, rockMat);
        Bubble(section, -3.5f, 136f, 4.6f);

        Platform(section, "Rock2", 0f, 140.5f, 5f, 2.5f, 2.5f, rockMat);
        Bubble(section, 0f, 140.5f, 5f);

        GameObject mover = Platform(section, "Drifter", 0f, 146f, 5f, 3f, 3f, movingMat);
        MovingPlatform moving = Undo.AddComponent<MovingPlatform>(mover);
        moving.moveOffset = new Vector3(6f, 0f, 0f);
        moving.speed = 2f;
        Bubble(section, 3f, 146f, 5f); // the drifter carries you through it

        Platform(section, "RestRock", 6f, 151.5f, 5.4f, 4f, 4f, rockMat);
        Respawn(section, "Respawn_RestRock", 6f, 151.5f, 5.4f, 4f, 4f);
        Treasure(section, 7f, 152.5f, 5.4f);
        Bubble(section, 5f, 151f, 5.4f);

        Slippery(section, "SlimeRock", 6f, 157.5f, 5.4f, 3f, 3.5f);
        Bubble(section, 6f, 157.5f, 5.4f);

        Breaking(section, "CrumblingRock4", 3f, 162f, 5.8f);

        Platform(section, "Rock3", -0.5f, 166.5f, 6.2f, 2.5f, 2.5f, rockMat);
        Bubble(section, -0.5f, 166.5f, 6.2f);

        GameObject goal = Platform(section, "Goal", -0.5f, 173f, 6.2f, 6f, 6f, goalMat);
        LevelGoal levelGoal = Undo.AddComponent<LevelGoal>(goal);
        levelGoal.titleTop = "YOU";
        levelGoal.titleBottom = "ESCAPED!";
        levelGoal.description = "You climbed out of the abyss on your last breath of air. Thanks for playing!";
        levelGoal.continueLabel = "BACK TO MAIN MENU";
        levelGoal.nextSceneName = "MainMenu";
        Respawn(section, "Respawn_Goal", -0.5f, 173f, 6.2f, 6f, 6f);
        Bubble(section, -2.5f, 171f, 6.2f);
        Treasure(section, -0.5f, 174f, 6.2f);
        Sign(section, "END OF LEVEL 4", new Vector3(-0.5f, 9.4f, 176f), 0.2f);
    }

    // Trench walls on both sides and glowing crystals (no colliders)
    static void BuildTrenchDecor(Transform root)
    {
        Transform section = Group(root, "TrenchDecor");

        for (int i = 0; i < 6; i++)
        {
            float z = 60f + i * 22f;
            float lean = (i % 2 == 0) ? 8f : -6f;

            GameObject left = Block(section, "TrenchWallLeft" + i, new Vector3(-17f - (i % 3), -4f, z),
                new Vector3(5f, 30f + (i % 3) * 4f, 22f), wallMat, false);
            left.transform.localRotation = Quaternion.Euler(0f, lean, -10f);

            GameObject right = Block(section, "TrenchWallRight" + i, new Vector3(17f + ((i + 1) % 3), -4f, z + 8f),
                new Vector3(5f, 28f + ((i + 2) % 3) * 4f, 22f), wallMat, false);
            right.transform.localRotation = Quaternion.Euler(0f, -lean, 10f);
        }

        Crystal(section, "Crystal1", new Vector3(-12f, -6f, 75f), 7f, -20f);
        Crystal(section, "Crystal2", new Vector3(11f, -8f, 100f), 9f, 25f);
        Crystal(section, "Crystal3", new Vector3(-11f, -5f, 135f), 6f, 15f);
        Crystal(section, "Crystal4", new Vector3(12f, -7f, 160f), 8f, -30f);
        Crystal(section, "Crystal5", new Vector3(-9f, -4f, 178f), 5f, 10f);
    }

    // Tall glowing crystal leaning a little (scenery, no collider)
    static void Crystal(Transform parent, string name, Vector3 basePosition, float height, float lean)
    {
        GameObject crystal = Block(parent, name, basePosition + Vector3.up * height * 0.5f,
            new Vector3(height * 0.18f, height, height * 0.18f), crystalMat, false);
        crystal.transform.localRotation = Quaternion.Euler(lean * 0.5f, 45f, lean);
    }

    // Shows the LEVEL 4 banner (LevelHUD) when the player reaches the start pad
    static void LevelBanner(Transform parent, float x, float z, float top, float width, float depth)
    {
        GameObject zone = new GameObject("LevelZone_Level4");
        Undo.RegisterCreatedObjectUndo(zone, "Level 4 zone");
        zone.transform.SetParent(parent, false);
        zone.transform.localPosition = new Vector3(x, top + 1.2f, z);

        BoxCollider box = zone.AddComponent<BoxCollider>();
        box.isTrigger = true;
        box.size = new Vector3(width, 3f, depth);
        box.center = new Vector3(0f, 0.3f, 0f);

        LevelZone levelZone = zone.AddComponent<LevelZone>();
        levelZone.levelTitle = "LEVEL 4";
        levelZone.levelName = "THE ABYSS";
        levelZone.bonusTime = 0f;
    }

    static void BuildSeabed(Transform root)
    {
        GameObject seabed = GameObject.CreatePrimitive(PrimitiveType.Plane);
        Undo.RegisterCreatedObjectUndo(seabed, "Seabed");
        Object.DestroyImmediate(seabed.GetComponent<Collider>());
        seabed.name = "Seabed";
        seabed.transform.SetParent(root, false);
        seabed.transform.localPosition = new Vector3(0f, -22f, 110f);
        seabed.transform.localScale = new Vector3(22f, 1f, 22f);

        Material floor = AssetDatabase.LoadAssetAtPath<Material>(SeabedMaterialPath);
        seabed.GetComponent<Renderer>().sharedMaterial = floor != null ? floor : postMat;
    }

    static void BuildKillZone(Transform root)
    {
        GameObject zone = new GameObject("KillZone");
        Undo.RegisterCreatedObjectUndo(zone, "Kill zone");
        zone.transform.SetParent(root, false);
        zone.transform.localPosition = new Vector3(0f, -15f, 110f);

        BoxCollider box = zone.AddComponent<BoxCollider>();
        box.isTrigger = true;
        box.size = new Vector3(200f, 4f, 260f);

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
            Debug.LogWarning("No player with PlayerHealth found. Add PlayerDash, PlayerPlatformContact and OxygenSupply to the player by hand.");
            return;
        }

        GameObject player = health.gameObject;
        if (player.GetComponent<PlayerDash>() == null) Undo.AddComponent<PlayerDash>(player);
        if (player.GetComponent<PlayerPlatformContact>() == null) Undo.AddComponent<PlayerPlatformContact>(player);
        if (player.GetComponent<OxygenSupply>() == null) Undo.AddComponent<OxygenSupply>(player);

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

    static void SetTimer(float seconds)
    {
        SurvivalTimer timer = Object.FindFirstObjectByType<SurvivalTimer>();
        if (timer == null) return;
        Undo.RecordObject(timer, "Level 4 timer");
        timer.timeRemaining = seconds;
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

    static GameObject Block(Transform parent, string name, Vector3 position, Vector3 size,
        Material material, bool keepCollider = true)
    {
        GameObject block = GameObject.CreatePrimitive(PrimitiveType.Cube);
        Undo.RegisterCreatedObjectUndo(block, name);
        if (!keepCollider) Object.DestroyImmediate(block.GetComponent<Collider>());
        block.name = name;
        block.transform.SetParent(parent, false);
        block.transform.localPosition = position;
        block.transform.localScale = size;
        block.GetComponent<Renderer>().sharedMaterial = material;
        return block;
    }

    static void Slippery(Transform parent, string name, float x, float z, float top, float width, float depth)
    {
        GameObject platform = Platform(parent, name, x, z, top, width, depth, slimeMat);
        Undo.AddComponent<SlipperyPlatform>(platform);

        // A few glowing slime blobs on top (no colliders)
        for (int i = 0; i < 3; i++)
        {
            float sx = (i - 1) * width * 0.3f;
            float sz = (i % 2 == 0 ? 0.25f : -0.25f) * depth;
            GameObject strand = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            Undo.RegisterCreatedObjectUndo(strand, "Slime");
            Object.DestroyImmediate(strand.GetComponent<Collider>());
            strand.name = "SlimeBlob";
            strand.transform.SetParent(platform.transform, false);
            // Child of a flattened cube, so undo the parent's scale
            strand.transform.localScale = new Vector3(0.12f / width, 0.6f, 0.12f / depth);
            strand.transform.localPosition = new Vector3(sx / width, 0.5f + 0.3f / 0.5f, sz / depth);
            strand.GetComponent<Renderer>().sharedMaterial = slimeMat;
        }
    }

    static void Breaking(Transform parent, string name, float x, float z, float top)
    {
        GameObject platform = Platform(parent, name, x, z, top, 2.5f, 2.5f, crumbleMat);
        Undo.AddComponent<BreakingPlatform>(platform);
    }

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

    // Flat sticky patch with some rubbish on it, and a trigger above it
    static void Trash(Transform parent, string name, float x, float z, float top, float width, float depth)
    {
        GameObject patch = new GameObject(name);
        Undo.RegisterCreatedObjectUndo(patch, name);
        patch.transform.SetParent(parent, false);
        patch.transform.localPosition = new Vector3(x, top, z);

        Block(patch.transform, "Goo", new Vector3(0f, 0.04f, 0f), new Vector3(width, 0.08f, depth), trashMat, false);

        // Bags and cans scattered across the patch
        int bags = Mathf.Max(2, Mathf.RoundToInt(width));
        for (int i = 0; i < bags; i++)
        {
            float bx = Mathf.Lerp(-width * 0.4f, width * 0.4f, bags == 1 ? 0.5f : i / (float)(bags - 1));
            float bz = (i % 2 == 0 ? 0.2f : -0.2f) * depth;
            GameObject bag = GameObject.CreatePrimitive(i % 3 == 2 ? PrimitiveType.Cylinder : PrimitiveType.Sphere);
            Undo.RegisterCreatedObjectUndo(bag, "Trash bag");
            Object.DestroyImmediate(bag.GetComponent<Collider>());
            bag.name = "Rubbish";
            bag.transform.SetParent(patch.transform, false);
            bag.transform.localPosition = new Vector3(bx, 0.2f, bz);
            bag.transform.localScale = i % 3 == 2 ? new Vector3(0.25f, 0.2f, 0.25f) : new Vector3(0.55f, 0.4f, 0.5f);
            bag.transform.localRotation = Quaternion.Euler(0f, i * 47f, i % 3 == 2 ? 90f : 0f);
            bag.GetComponent<Renderer>().sharedMaterial = trashBagMat;
        }

        BoxCollider box = patch.AddComponent<BoxCollider>();
        box.isTrigger = true;
        box.center = new Vector3(0f, 0.6f, 0f);
        box.size = new Vector3(width, 1.2f, depth);
        patch.AddComponent<StickyTrash>();
    }

    static void Fish(Transform parent, string name, Vector3 position, Vector3 swim, float speed)
    {
        GameObject fish = new GameObject(name);
        Undo.RegisterCreatedObjectUndo(fish, name);
        fish.transform.SetParent(parent, false);
        fish.transform.localPosition = position;

        SphereCollider hitbox = fish.AddComponent<SphereCollider>();
        hitbox.isTrigger = true;
        hitbox.radius = 0.55f;

        GameObject body = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        Undo.RegisterCreatedObjectUndo(body, "Fish body");
        Object.DestroyImmediate(body.GetComponent<Collider>());
        body.name = "Body";
        body.transform.SetParent(fish.transform, false);
        body.transform.localScale = new Vector3(0.9f, 0.8f, 0.8f);
        body.GetComponent<Renderer>().sharedMaterial = fishMat;

        // Tail points away from the swim direction
        float side = swim.x >= 0f ? -1f : 1f;
        GameObject tail = GameObject.CreatePrimitive(PrimitiveType.Cube);
        Undo.RegisterCreatedObjectUndo(tail, "Fish tail");
        Object.DestroyImmediate(tail.GetComponent<Collider>());
        tail.name = "Tail";
        tail.transform.SetParent(fish.transform, false);
        tail.transform.localPosition = new Vector3(0.55f * side, 0f, 0f);
        tail.transform.localRotation = Quaternion.Euler(0f, 0f, 45f);
        tail.transform.localScale = new Vector3(0.35f, 0.35f, 0.08f);
        tail.GetComponent<Renderer>().sharedMaterial = finMat;

        // Little spikes so it reads as dangerous
        for (int i = 0; i < 4; i++)
        {
            GameObject spike = GameObject.CreatePrimitive(PrimitiveType.Cube);
            Undo.RegisterCreatedObjectUndo(spike, "Spike");
            Object.DestroyImmediate(spike.GetComponent<Collider>());
            spike.name = "Spike";
            spike.transform.SetParent(fish.transform, false);
            float angle = i * 90f + 45f;
            Vector3 dir = Quaternion.Euler(angle, 0f, 0f) * Vector3.up;
            spike.transform.localPosition = dir * 0.42f;
            spike.transform.localRotation = Quaternion.Euler(angle, 0f, 0f);
            spike.transform.localScale = new Vector3(0.08f, 0.22f, 0.08f);
            spike.GetComponent<Renderer>().sharedMaterial = finMat;
        }

        ExplodingFish ai = fish.AddComponent<ExplodingFish>();
        ai.swimOffset = swim;
        ai.swimSpeed = speed;
    }

    static void Bubble(Transform parent, float x, float z, float top)
    {
        GameObject bubble = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        Undo.RegisterCreatedObjectUndo(bubble, "Air bubble");
        bubble.name = "AirBubble";
        bubble.transform.SetParent(parent, false);
        bubble.transform.localPosition = new Vector3(x, top + 1.3f, z);
        bubble.transform.localScale = Vector3.one * 0.9f;
        bubble.GetComponent<Renderer>().sharedMaterial = bubbleMat;

        SphereCollider col = bubble.GetComponent<SphereCollider>();
        col.isTrigger = true;
        col.radius = 0.8f; // a bit bigger than it looks, so it's easy to grab

        bubble.AddComponent<AirBubble>();
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

    static void Sign(Transform parent, string text, Vector3 position, float size)
    {
        GameObject sign = new GameObject("Sign_" + text.Split('\n')[0]);
        Undo.RegisterCreatedObjectUndo(sign, "Sign");
        sign.transform.SetParent(parent, false);
        sign.transform.localPosition = position;
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
        rockMat = GetMaterial("Level4_Rock", new Color(0.24f, 0.26f, 0.38f));
        ledgeMat = GetMaterial("Level4_Ledge", new Color(0.32f, 0.29f, 0.42f));
        wallMat = GetMaterial("Level4_TrenchWall", new Color(0.1f, 0.1f, 0.18f));
        slimeMat = GetMaterial("Level4_GlowSlime", new Color(0.35f, 0.95f, 0.55f), true);
        crumbleMat = GetMaterial("Level4_CrumblingRock", new Color(0.58f, 0.46f, 0.52f));
        movingMat = GetMaterial("Level4_Moving", new Color(0.2f, 0.75f, 0.75f));
        pathMat = GetMaterial("Level4_Path", new Color(0.4f, 0.4f, 0.5f));
        trapMat = GetMaterial("Level4_Trap", new Color(0.55f, 0.12f, 0.1f));
        postMat = GetMaterial("Level4_Post", new Color(0.18f, 0.17f, 0.22f));
        goalMat = GetMaterial("Level4_Goal", new Color(0.95f, 0.8f, 0.2f));
        trashMat = GetMaterial("Level4_TrashGoo", new Color(0.32f, 0.3f, 0.1f));
        trashBagMat = GetMaterial("Level4_TrashBag", new Color(0.12f, 0.13f, 0.12f));
        fishMat = GetMaterial("Level4_ExplodingFish", new Color(1f, 0.55f, 0.15f));
        finMat = GetMaterial("Level4_FishFin", new Color(0.95f, 0.88f, 0.7f));
        warningMat = GetMaterial("Level4_Warning", new Color(0.8f, 0.15f, 0.1f));
        crystalMat = GetMaterial("Level4_Crystal", new Color(0.45f, 0.75f, 1f), true);
        bubbleMat = GetTransparentMaterial("Level4_AirBubble", new Color(0.7f, 0.95f, 1f, 0.45f));
    }

    static void EnsureFolder()
    {
        if (AssetDatabase.IsValidFolder(MaterialFolder)) return;
        if (!AssetDatabase.IsValidFolder("Assets/Material"))
            AssetDatabase.CreateFolder("Assets", "Material");
        AssetDatabase.CreateFolder("Assets/Material", "Level4");
    }

    // glow = also light up in the dark (emission), for slime and crystals
    static Material GetMaterial(string name, Color colour, bool glow = false)
    {
        EnsureFolder();
        string path = MaterialFolder + "/" + name + ".mat";
        Material material = AssetDatabase.LoadAssetAtPath<Material>(path);
        if (material == null)
        {
            material = new Material(Shader.Find("Standard"));
            material.color = colour;
            if (glow)
            {
                material.EnableKeyword("_EMISSION");
                material.SetColor("_EmissionColor", colour * 0.6f);
                material.globalIlluminationFlags = MaterialGlobalIlluminationFlags.RealtimeEmissive;
            }
            AssetDatabase.CreateAsset(material, path);
        }
        return material;
    }

    // Standard shader in Transparent mode, for the see-through bubbles
    static Material GetTransparentMaterial(string name, Color colour)
    {
        EnsureFolder();
        string path = MaterialFolder + "/" + name + ".mat";
        Material material = AssetDatabase.LoadAssetAtPath<Material>(path);
        if (material == null)
        {
            material = new Material(Shader.Find("Standard"));
            material.color = colour;
            material.SetFloat("_Mode", 3f);
            material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.One);
            material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            material.SetInt("_ZWrite", 0);
            material.DisableKeyword("_ALPHATEST_ON");
            material.DisableKeyword("_ALPHABLEND_ON");
            material.EnableKeyword("_ALPHAPREMULTIPLY_ON");
            material.SetFloat("_Glossiness", 0.9f);
            material.renderQueue = 3000;
            AssetDatabase.CreateAsset(material, path);
        }
        return material;
    }
}
