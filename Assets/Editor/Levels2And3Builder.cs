using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

// Builds Level 2 (Kelp Caves) and Level 3 (The Shipwreck) straight onto the
// end of Level 1, so the player walks from one level into the next with no
// loading screen. Each level starts with a gate and a banner (plus bonus time).
// The Level Complete menu now only appears at the end of Level 3.
//
// Level 2 introduces jellyfish (time your jumps), chargers (dash to defeat)
// and a lift. Level 3 adds spitters (dash to pop their bubbles), two moving
// platforms passing each other, and a shipwreck deck before the goal.
//
// Build Level 1 first, then run: Tools > Levels 2 and 3 > Build Levels 2 and 3
public static class Levels2And3Builder
{
    const string RootName = "Levels2And3";
    const string Level1RootName = "Level1";
    const string MaterialFolder = "Assets/Material/Level1";
    const string TreasurePrefabPath = "Assets/Prefabs/Treasure.prefab";
    const string PufferPrefabPath = "Assets/Prefabs/PufferFish.prefab";
    const string SeabedMaterialPath = "Assets/Material/OceanFloor.mat";

    // Where Level 3 starts, measured from the start of Level 2
    static readonly Vector3 Level3Offset = new Vector3(0f, 4f, 101.5f);

    static Material solidMat, breakingMat, movingMat, jumpPadMat, dashMat, trapMat, postMat, goalMat;
    static Material kelpMat, kelpPathMat, woodMat, hullMat;
    static Material chargerMat, eyeMat, jellyMat, spitterMat, mouthMat;
    static GameObject treasurePrefab, pufferPrefab;

    [MenuItem("Tools/Levels 2 and 3/Build Levels 2 and 3")]
    public static void Build()
    {
        GameObject level1 = GameObject.Find(Level1RootName);
        if (level1 == null)
        {
            EditorUtility.DisplayDialog("Level 1 missing",
                "Build Level 1 first (Tools > Level 1 > Build Level 1), then run this again.", "OK");
            return;
        }

        GameObject existing = GameObject.Find(RootName);
        if (existing != null)
        {
            if (!EditorUtility.DisplayDialog("Rebuild Levels 2 and 3",
                "Levels 2 and 3 already exist in this scene. Delete them and build them again?",
                "Rebuild", "Cancel"))
                return;
            Undo.DestroyObjectImmediate(existing);
        }

        treasurePrefab = AssetDatabase.LoadAssetAtPath<GameObject>(TreasurePrefabPath);
        pufferPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(PufferPrefabPath);
        CreateMaterials();

        // Level 2 starts at the front edge of Level 1's goal platform
        Vector3 level2Start = FindLevel1End(level1);

        GameObject root = new GameObject(RootName);
        Undo.RegisterCreatedObjectUndo(root, "Build Levels 2 and 3");

        ConnectLevel1(root.transform, level1);

        Transform level2 = Group(root.transform, "Level2");
        level2.position = level2Start;
        BuildLevel2(level2);

        Transform level3 = Group(root.transform, "Level3");
        level3.position = level2Start + Level3Offset;
        BuildLevel3(level3);

        BuildSeabedAndKillZone(root.transform, level2Start);

        EditorSceneManager.MarkSceneDirty(root.scene);
        Selection.activeGameObject = root;
        Debug.Log("Levels 2 and 3 built. Press Ctrl+S to save the scene.");
    }

    // ---------- Joining onto Level 1 ----------

    static Vector3 FindLevel1End(GameObject level1)
    {
        Transform goal = FindChild(level1.transform, "Goal");
        if (goal == null)
        {
            Debug.LogWarning("Couldn't find Level 1's Goal platform. Starting Level 2 at the default spot.");
            return new Vector3(-4f, 4.8f, 126f);
        }

        float top = goal.position.y + goal.lossyScale.y * 0.5f;
        float front = goal.position.z + goal.lossyScale.z * 0.5f;
        return new Vector3(goal.position.x, top, front);
    }

    // Level 1's goal no longer ends the game, and its sign points onwards
    static void ConnectLevel1(Transform root, GameObject level1)
    {
        Transform goal = FindChild(level1.transform, "Goal");
        if (goal != null)
        {
            LevelGoal oldGoal = goal.GetComponent<LevelGoal>();
            if (oldGoal != null) Undo.DestroyObjectImmediate(oldGoal);
        }

        foreach (TextMesh sign in level1.GetComponentsInChildren<TextMesh>(true))
        {
            if (sign.text == "END OF LEVEL 1")
            {
                Undo.RecordObject(sign, "Update sign");
                sign.text = "LEVEL 2 AHEAD";
            }
        }

        // Banner for Level 1 too, the moment the player reaches its start pad
        Transform start = FindChild(level1.transform, "Respawn_Start");
        if (start != null)
        {
            GameObject zone = new GameObject("LevelZone_Level1");
            Undo.RegisterCreatedObjectUndo(zone, "Level 1 zone");
            zone.transform.SetParent(root, false);
            zone.transform.position = start.position;

            BoxCollider box = zone.AddComponent<BoxCollider>();
            box.isTrigger = true;
            box.size = new Vector3(6f, 3f, 6f);
            box.center = new Vector3(0f, 0.3f, 0f);

            LevelZone levelZone = zone.AddComponent<LevelZone>();
            levelZone.levelTitle = "LEVEL 1";
            levelZone.levelName = "THE SHALLOWS";
            levelZone.bonusTime = 0f;
        }
    }

    // ---------- Level 2: Kelp Caves ----------
    // Positions are inside the Level2 object. Heights are measured from
    // Level 1's goal, so 0 means level with it.

    static void BuildLevel2(Transform level)
    {
        // Entrance
        Transform entrance = Group(level, "Entrance");
        Platform(entrance, "KelpPath", 0f, 5f, 0f, 3f, 10f, kelpPathMat);
        Kelp(entrance, -2.6f, 2f, -2f, 4f);
        Kelp(entrance, 2.6f, 4.5f, -2f, 5f);
        Kelp(entrance, -2.8f, 7.5f, -2f, 4.5f);
        Gate(entrance, 10f, 0f, "LEVEL 2\nKELP CAVES", kelpMat);

        Platform(entrance, "StartPad", 0f, 13f, 0f, 6f, 6f, kelpMat);
        Zone(entrance, "LevelZone_Level2", 0f, 13f, 0f, 6f, 6f, "LEVEL 2", "KELP CAVES", 150f);
        Sign(entrance, "Jellyfish drift up and down.\nTime your jumps!", new Vector3(0f, 2.4f, 15.5f), 0.12f);

        // A: hops past jellyfish floating in the gaps
        Transform hops = Group(level, "A_JellyfishHops");
        Platform(hops, "Hop1", 0f, 19.5f, 0f, 3f, 3f, solidMat);
        Jelly(hops, 0f, 17f, 1.2f, 1.6f, 0.6f, 0f);
        Platform(hops, "Hop2", 2f, 24f, 0.5f, 3f, 3f, solidMat);
        Treasure(hops, 2f, 24f, 0.5f);
        Jelly(hops, 1f, 21.75f, 1.7f, 1.6f, 0.6f, 0.5f);
        Platform(hops, "Hop3", 0f, 28.5f, 1f, 3f, 3f, solidMat);

        // B: wide hall with a charger guarding it
        Transform hall = Group(level, "B_ChargerHall");
        Platform(hall, "Hall", 0f, 36f, 1f, 8f, 10f, kelpMat);
        Respawn(hall, "Respawn_Hall", 0f, 36f, 1f, 8f, 10f);
        Sign(hall, "Chargers flash red, then rush you.\nDash into them to defeat them!", new Vector3(0f, 3.6f, 32f), 0.11f);
        Charger(hall, -3f, 38.5f, 1f, new Vector3(6f, 0f, 0f), 7f);
        Treasure(hall, -3f, 33f, 1f);
        Treasure(hall, 3f, 40f, 1f);
        Kelp(hall, -4.6f, 34f, -1f, 5f);
        Kelp(hall, 4.6f, 38f, -1f, 6f);

        // C: lift up to a high ledge
        Transform lift = Group(level, "C_Lift");
        GameObject elevator = Platform(lift, "Lift", 0f, 43.5f, 1f, 3f, 3f, movingMat);
        MovingPlatform up = Undo.AddComponent<MovingPlatform>(elevator);
        up.moveOffset = new Vector3(0f, 5f, 0f);
        up.speed = 1.2f;
        Sign(lift, "Ride the lift up!", new Vector3(2.6f, 3.4f, 41f), 0.12f);

        Platform(lift, "HighLedge", 0f, 48.5f, 6f, 4f, 4f, solidMat);
        Respawn(lift, "Respawn_HighLedge", 0f, 48.5f, 6f, 4f, 4f);
        Treasure(lift, 0f, 49f, 6f);

        // D: crumbling platforms with a jellyfish in the way
        Transform crumble = Group(level, "D_Crumble");
        BreakingPlatform(crumble, "Crumble1", 0f, 53f, 6f);
        BreakingPlatform(crumble, "Crumble2", -3f, 57f, 6f);
        Treasure(crumble, -3f, 57f, 6f);
        BreakingPlatform(crumble, "Crumble3", 0f, 61f, 6f);
        Jelly(crumble, -1.5f, 59f, 7.2f, 1.2f, 0.7f, 0.25f);

        // E: dash gap
        Transform dashGap = Group(level, "E_DashGap");
        Platform(dashGap, "SafePad", 0f, 66.5f, 6f, 5f, 5f, solidMat);
        Respawn(dashGap, "Respawn_SafePad", 0f, 66.5f, 6f, 5f, 5f);
        Sign(dashGap, "DASH ACROSS!", new Vector3(0f, 8.4f, 69f), 0.15f);
        Platform(dashGap, "DashLanding", 0f, 78f, 6f, 4f, 4f, dashMat);
        Treasure(dashGap, 0f, 78f, 6f);

        // F: kelp corridor with a second charger
        Transform corridor = Group(level, "F_Corridor");
        Platform(corridor, "Corridor", 0f, 87f, 6f, 5f, 10f, kelpMat);
        Respawn(corridor, "Respawn_Corridor", 0f, 87f, 6f, 5f, 10f);
        Charger(corridor, -2f, 88.5f, 6f, new Vector3(4f, 0f, 0f), 6f);
        Treasure(corridor, 1.5f, 91f, 6f);
        Kelp(corridor, -2.9f, 84f, 3f, 5f);
        Kelp(corridor, 2.9f, 89f, 3f, 6f);

        // G: steps down to the shipwreck
        Transform exit = Group(level, "G_Exit");
        Platform(exit, "StepDown1", 0f, 95.5f, 5f, 3f, 3f, solidMat);
        Platform(exit, "StepDown2", 0f, 100f, 4f, 3f, 3f, solidMat);
        Sign(exit, "LEVEL 3 AHEAD", new Vector3(0f, 7f, 101f), 0.18f);
    }

    // ---------- Level 3: The Shipwreck ----------

    static void BuildLevel3(Transform level)
    {
        // Entrance
        Transform entrance = Group(level, "Entrance");
        Platform(entrance, "Gangway", 0f, 4f, 0f, 3f, 8f, woodMat);
        Gate(entrance, 8f, 0f, "LEVEL 3\nTHE SHIPWRECK", hullMat);

        Platform(entrance, "StartPad", 0f, 11f, 0f, 6f, 6f, woodMat);
        Zone(entrance, "LevelZone_Level3", 0f, 11f, 0f, 6f, 6f, "LEVEL 3", "THE SHIPWRECK", 150f);
        Sign(entrance, "Spitters shoot bubbles.\nDash to pop them!", new Vector3(0f, 2.4f, 13.5f), 0.12f);

        // A: hops while a spitter shoots from the side
        Transform alley = Group(level, "A_SpitterAlley");
        Platform(alley, "Hop1", 0f, 17.5f, 0f, 3f, 3f, solidMat);
        Platform(alley, "Hop2", -2f, 22f, 0.5f, 3f, 3f, solidMat);
        Treasure(alley, -2f, 22f, 0.5f);
        Platform(alley, "Hop3", 0f, 26.5f, 1f, 3f, 3f, solidMat);
        Post(alley, "SpitterPost1", 6.5f, 22f, -2f, 3.5f);
        Spitter(alley, 6.5f, 22f, 2.2f, 13f, 0.3f);

        // B: two moving platforms passing each other in the middle
        Transform movers = Group(level, "B_TwoMovers");
        GameObject moverA = Platform(movers, "MoverA", -4f, 31f, 1f, 3f, 3f, movingMat);
        MovingPlatform a = Undo.AddComponent<MovingPlatform>(moverA);
        a.moveOffset = new Vector3(8f, 0f, 0f);
        a.speed = 2f;
        GameObject moverB = Platform(movers, "MoverB", 4f, 35.5f, 1f, 3f, 3f, movingMat);
        MovingPlatform b = Undo.AddComponent<MovingPlatform>(moverB);
        b.moveOffset = new Vector3(-8f, 0f, 0f);
        b.speed = 2f;
        Sign(movers, "Jump across when they meet\nin the middle", new Vector3(3f, 3.2f, 28f), 0.11f);
        Treasure(movers, 0f, 33.25f, 1.2f);

        Platform(movers, "Landing", 0f, 41.5f, 1f, 5f, 5f, woodMat);
        Respawn(movers, "Respawn_Landing", 0f, 41.5f, 1f, 5f, 5f);
        Puffer(movers, 1.5f, 42f, 1f, 1.5f);

        // C: crumbling climb with a spitter on the left
        Transform climb = Group(level, "C_CrumbleClimb");
        BreakingPlatform(climb, "Climb1", 0f, 46.5f, 1.6f);
        BreakingPlatform(climb, "Climb2", 2.5f, 50f, 2.2f);
        BreakingPlatform(climb, "Climb3", 0f, 53.5f, 2.8f);
        Treasure(climb, 0f, 53.5f, 2.8f);
        BreakingPlatform(climb, "Climb4", -2.5f, 57f, 3.4f);
        Post(climb, "SpitterPost2", -8f, 51f, -1f, 4.5f);
        Spitter(climb, -8f, 51f, 4.2f, 13f, 1.2f);

        // D: jump pad up onto the ship's deck
        Transform deck = Group(level, "D_Deck");
        GameObject pad = Platform(deck, "JumpPad", 0f, 61f, 3.4f, 3f, 3f, jumpPadMat);
        JumpPad jump = Undo.AddComponent<JumpPad>(pad);
        jump.launchSpeed = 14f;

        Platform(deck, "Deck", 0f, 71.5f, 8f, 7f, 12f, woodMat);
        Respawn(deck, "Respawn_Deck", 0f, 71.5f, 8f, 7f, 12f);
        Sign(deck, "Almost there!", new Vector3(0f, 10.4f, 66.5f), 0.15f);
        Charger(deck, -2.5f, 72f, 8f, new Vector3(5f, 0f, 0f), 6f);
        Spitter(deck, 2.5f, 76.5f, 9.1f, 12f, 0.8f);
        Treasure(deck, -2.5f, 67.5f, 8f);
        Treasure(deck, 2.5f, 74.5f, 8f);

        // Shipwreck look: mast and low side rails (no collision on the mast)
        Decor(deck, "Mast", new Vector3(-2.8f, 11f, 69.5f), new Vector3(0.4f, 6f, 0.4f), hullMat);
        Decor(deck, "MastBeam", new Vector3(-2.8f, 12.5f, 69.5f), new Vector3(3f, 0.25f, 0.25f), hullMat);
        Block(deck, "RailLeft", new Vector3(-3.6f, 8.3f, 71.5f), new Vector3(0.2f, 0.6f, 12f), hullMat);
        Block(deck, "RailRight", new Vector3(3.6f, 8.3f, 71.5f), new Vector3(0.2f, 0.6f, 12f), hullMat);
        Decor(deck, "BrokenHullLeft", new Vector3(-4.5f, 5.5f, 70f), new Vector3(0.5f, 5f, 9f), hullMat);
        Decor(deck, "BrokenHullRight", new Vector3(4.5f, 5f, 73f), new Vector3(0.5f, 4f, 7f), hullMat);

        // E: final dash gap to the treasure room
        Transform finish = Group(level, "E_Finish");
        Sign(finish, "DASH ACROSS!", new Vector3(0f, 10.4f, 77.2f), 0.15f);

        GameObject goal = Platform(finish, "Goal", 0f, 87.5f, 8f, 6f, 6f, goalMat);
        LevelGoal levelGoal = Undo.AddComponent<LevelGoal>(goal);
        levelGoal.titleTop = "YOU";
        levelGoal.titleBottom = "ESCAPED!";
        levelGoal.description = "You made it out of the shipwreck with the treasure. Thanks for playing!";
        levelGoal.continueLabel = "BACK TO MAIN MENU";
        levelGoal.nextSceneName = "MainMenu";

        Respawn(finish, "Respawn_Goal", 0f, 87.5f, 8f, 6f, 6f);
        Treasure(finish, 0f, 88.5f, 8f);
        Sign(finish, "TREASURE ROOM", new Vector3(0f, 11.5f, 90.5f), 0.2f);
    }

    // Seabed far below, and a kill zone so falling sends you back
    static void BuildSeabedAndKillZone(Transform root, Vector3 level2Start)
    {
        Vector3 centre = new Vector3(level2Start.x, 0f, level2Start.z + 100f);

        GameObject seabed = GameObject.CreatePrimitive(PrimitiveType.Plane);
        Undo.RegisterCreatedObjectUndo(seabed, "Seabed");
        Object.DestroyImmediate(seabed.GetComponent<Collider>());
        seabed.name = "Seabed";
        seabed.transform.SetParent(root, false);
        seabed.transform.position = new Vector3(centre.x, -22f, centre.z);
        seabed.transform.localScale = new Vector3(22f, 1f, 22f);
        Material floor = AssetDatabase.LoadAssetAtPath<Material>(SeabedMaterialPath);
        seabed.GetComponent<Renderer>().sharedMaterial = floor != null ? floor : postMat;

        GameObject zone = new GameObject("KillZone");
        Undo.RegisterCreatedObjectUndo(zone, "Kill zone");
        zone.transform.SetParent(root, false);
        zone.transform.position = new Vector3(centre.x, level2Start.y - 20f, centre.z);

        BoxCollider box = zone.AddComponent<BoxCollider>();
        box.isTrigger = true;
        box.size = new Vector3(200f, 4f, 260f);

        Rigidbody body = zone.AddComponent<Rigidbody>();
        body.isKinematic = true;
        body.useGravity = false;

        zone.AddComponent<KillZone>();
    }

    // ---------- Enemies ----------

    // Swims side to side, flashes red, then charges at the player
    static void Charger(Transform parent, float x, float z, float top, Vector3 patrol, float detectRange)
    {
        GameObject fish = new GameObject("ChargerFish");
        Undo.RegisterCreatedObjectUndo(fish, "Charger");
        fish.transform.SetParent(parent, false);
        fish.transform.localPosition = new Vector3(x, top + 1f, z);

        Part(fish.transform, "Body", PrimitiveType.Capsule,
            Vector3.zero, new Vector3(90f, 0f, 0f), new Vector3(0.7f, 0.8f, 0.7f), chargerMat);
        Part(fish.transform, "Tail", PrimitiveType.Cube,
            new Vector3(0f, 0f, -0.95f), Vector3.zero, new Vector3(0.1f, 0.6f, 0.5f), chargerMat);
        Part(fish.transform, "Fin", PrimitiveType.Cube,
            new Vector3(0f, 0.45f, 0f), Vector3.zero, new Vector3(0.1f, 0.4f, 0.5f), chargerMat);
        Part(fish.transform, "EyeLeft", PrimitiveType.Sphere,
            new Vector3(-0.25f, 0.15f, 0.6f), Vector3.zero, Vector3.one * 0.15f, eyeMat);
        Part(fish.transform, "EyeRight", PrimitiveType.Sphere,
            new Vector3(0.25f, 0.15f, 0.6f), Vector3.zero, Vector3.one * 0.15f, eyeMat);

        EnemyBody(fish, 0.8f);

        ContactDamage hurt = fish.AddComponent<ContactDamage>();
        hurt.damage = 20;
        DashKillable killable = fish.AddComponent<DashKillable>();
        killable.popColour = new Color(0.6f, 0.7f, 0.8f);

        ChargerFish ai = fish.AddComponent<ChargerFish>();
        ai.patrolOffset = patrol;
        ai.detectRange = detectRange;
    }

    // Floats up and down in a gap
    static void Jelly(Transform parent, float x, float z, float y, float bobHeight, float bobSpeed, float phase)
    {
        GameObject jelly = new GameObject("Jellyfish");
        Undo.RegisterCreatedObjectUndo(jelly, "Jellyfish");
        jelly.transform.SetParent(parent, false);
        jelly.transform.localPosition = new Vector3(x, y, z);

        Part(jelly.transform, "Bell", PrimitiveType.Sphere,
            new Vector3(0f, 0.15f, 0f), Vector3.zero, new Vector3(1f, 0.6f, 1f), jellyMat);
        for (int i = 0; i < 4; i++)
        {
            float sx = (i % 2 == 0) ? -0.25f : 0.25f;
            float sz = (i < 2) ? -0.25f : 0.25f;
            Part(jelly.transform, "Tentacle" + i, PrimitiveType.Cylinder,
                new Vector3(sx, -0.45f, sz), Vector3.zero, new Vector3(0.06f, 0.35f, 0.06f), jellyMat);
        }

        EnemyBody(jelly, 0.55f);

        ContactDamage hurt = jelly.AddComponent<ContactDamage>();
        hurt.damage = 15;
        DashKillable killable = jelly.AddComponent<DashKillable>();
        killable.popColour = new Color(1f, 0.6f, 0.85f);

        Jellyfish bob = jelly.AddComponent<Jellyfish>();
        bob.bobHeight = bobHeight;
        bob.bobSpeed = bobSpeed;
        bob.phase = phase;
    }

    // Turns to face the player and spits bubbles
    static void Spitter(Transform parent, float x, float z, float y, float range, float startDelay)
    {
        GameObject fish = new GameObject("SpitterFish");
        Undo.RegisterCreatedObjectUndo(fish, "Spitter");
        fish.transform.SetParent(parent, false);
        fish.transform.localPosition = new Vector3(x, y, z);

        Part(fish.transform, "Body", PrimitiveType.Sphere,
            Vector3.zero, Vector3.zero, Vector3.one * 0.9f, spitterMat);
        Part(fish.transform, "Mouth", PrimitiveType.Cylinder,
            new Vector3(0f, -0.05f, 0.45f), new Vector3(90f, 0f, 0f), new Vector3(0.3f, 0.12f, 0.3f), mouthMat);
        Part(fish.transform, "EyeLeft", PrimitiveType.Sphere,
            new Vector3(-0.22f, 0.2f, 0.35f), Vector3.zero, Vector3.one * 0.14f, eyeMat);
        Part(fish.transform, "EyeRight", PrimitiveType.Sphere,
            new Vector3(0.22f, 0.2f, 0.35f), Vector3.zero, Vector3.one * 0.14f, eyeMat);
        Part(fish.transform, "TailFin", PrimitiveType.Cube,
            new Vector3(0f, 0f, -0.55f), Vector3.zero, new Vector3(0.1f, 0.45f, 0.35f), spitterMat);

        EnemyBody(fish, 0.55f);

        DashKillable killable = fish.AddComponent<DashKillable>();
        killable.popColour = new Color(1f, 0.6f, 0.3f);

        SpitterFish ai = fish.AddComponent<SpitterFish>();
        ai.range = range;
        ai.startDelay = startDelay;
    }

    // Trigger hitbox + kinematic Rigidbody so touches and dashes register
    static void EnemyBody(GameObject enemy, float radius)
    {
        SphereCollider hitbox = enemy.AddComponent<SphereCollider>();
        hitbox.isTrigger = true;
        hitbox.radius = radius;

        Rigidbody body = enemy.AddComponent<Rigidbody>();
        body.isKinematic = true;
        body.useGravity = false;
    }

    // Visual piece of an enemy, with its collider removed
    static GameObject Part(Transform parent, string name, PrimitiveType shape,
        Vector3 position, Vector3 rotation, Vector3 scale, Material material)
    {
        GameObject part = GameObject.CreatePrimitive(shape);
        Undo.RegisterCreatedObjectUndo(part, name);
        Object.DestroyImmediate(part.GetComponent<Collider>());
        part.name = name;
        part.transform.SetParent(parent, false);
        part.transform.localPosition = position;
        part.transform.localRotation = Quaternion.Euler(rotation);
        part.transform.localScale = scale;
        part.GetComponent<Renderer>().sharedMaterial = material;
        return part;
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

    // Scenery with no collision
    static void Decor(Transform parent, string name, Vector3 position, Vector3 size, Material material)
    {
        GameObject block = Block(parent, name, position, size, material);
        Object.DestroyImmediate(block.GetComponent<Collider>());
    }

    // Tall thin kelp strand (scenery)
    static void Kelp(Transform parent, float x, float z, float bottom, float height)
    {
        Decor(parent, "Kelp", new Vector3(x, bottom + height * 0.5f, z), new Vector3(0.3f, height, 0.3f), kelpMat);
    }

    static void BreakingPlatform(Transform parent, string name, float x, float z, float top)
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

    // Two posts and a crossbar with the level name on top
    static void Gate(Transform parent, float z, float top, string title, Material material)
    {
        Block(parent, "GatePostLeft", new Vector3(-2.2f, top + 2f, z), new Vector3(0.6f, 4f, 0.6f), material);
        Block(parent, "GatePostRight", new Vector3(2.2f, top + 2f, z), new Vector3(0.6f, 4f, 0.6f), material);
        Block(parent, "GateTop", new Vector3(0f, top + 4.2f, z), new Vector3(5f, 0.5f, 0.6f), material);
        Sign(parent, title, new Vector3(0f, top + 5.6f, z), 0.2f);
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

    // Start-of-level area: banner, bonus time and respawn point
    static void Zone(Transform parent, string name, float x, float z, float top, float width, float depth,
        string title, string levelName, float bonusTime)
    {
        GameObject point = new GameObject(name);
        Undo.RegisterCreatedObjectUndo(point, name);
        point.transform.SetParent(parent, false);
        point.transform.localPosition = new Vector3(x, top + 1.2f, z);

        BoxCollider box = point.AddComponent<BoxCollider>();
        box.isTrigger = true;
        box.size = new Vector3(width, 3f, depth);
        box.center = new Vector3(0f, 0.3f, 0f);

        LevelZone zone = point.AddComponent<LevelZone>();
        zone.levelTitle = title;
        zone.levelName = levelName;
        zone.bonusTime = bonusTime;

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
        sign.transform.rotation = Quaternion.Euler(35f, 45f, 0f);

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

    static Transform FindChild(Transform parent, string name)
    {
        foreach (Transform child in parent.GetComponentsInChildren<Transform>(true))
            if (child.name == name) return child;
        return null;
    }

    // ---------- Materials ----------

    static void CreateMaterials()
    {
        // Shared with Level 1, so the platform colours mean the same thing everywhere
        solidMat = GetMaterial("Level1_Solid", new Color(0.35f, 0.45f, 0.55f));
        breakingMat = GetMaterial("Level1_Breaking", new Color(0.85f, 0.7f, 0.45f));
        movingMat = GetMaterial("Level1_Moving", new Color(0.2f, 0.75f, 0.75f));
        jumpPadMat = GetMaterial("Level1_JumpPad", new Color(0.3f, 0.9f, 0.35f));
        dashMat = GetMaterial("Level1_DashLanding", new Color(0.6f, 0.4f, 0.9f));
        trapMat = GetMaterial("Level1_Trap", new Color(0.55f, 0.12f, 0.1f));
        postMat = GetMaterial("Level1_Post", new Color(0.25f, 0.22f, 0.2f));
        goalMat = GetMaterial("Level1_Goal", new Color(0.95f, 0.8f, 0.2f));

        // New for Levels 2 and 3
        kelpMat = GetMaterial("Level2_Kelp", new Color(0.2f, 0.5f, 0.3f));
        kelpPathMat = GetMaterial("Level2_KelpPath", new Color(0.3f, 0.42f, 0.32f));
        woodMat = GetMaterial("Level3_Wood", new Color(0.45f, 0.32f, 0.2f));
        hullMat = GetMaterial("Level3_Hull", new Color(0.28f, 0.2f, 0.14f));

        chargerMat = GetMaterial("Enemy_Charger", new Color(0.4f, 0.48f, 0.58f));
        eyeMat = GetMaterial("Enemy_Eye", new Color(0.9f, 0.15f, 0.1f));
        jellyMat = GetMaterial("Enemy_Jellyfish", new Color(0.95f, 0.55f, 0.85f));
        spitterMat = GetMaterial("Enemy_Spitter", new Color(0.95f, 0.55f, 0.2f));
        mouthMat = GetMaterial("Enemy_Mouth", new Color(0.2f, 0.08f, 0.05f));
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
