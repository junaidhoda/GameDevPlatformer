using UnityEngine;

// Invisible area at the start of each level. The first time the player walks
// in, it shows the level banner, adds bonus time and sets the respawn point.
// Levels follow straight on from each other, so there's no loading screen.
public class LevelZone : MonoBehaviour
{
    public string levelTitle = "LEVEL 2";
    public string levelName = "KELP CAVES";
    public float bonusTime = 150f;

    private bool entered;

    void OnTriggerEnter(Collider other)
    {
        if (entered) return;
        if (other.GetComponent<CharacterController>() == null) return;
        entered = true;

        if (bonusTime > 0f)
        {
            SurvivalTimer timer = FindFirstObjectByType<SurvivalTimer>();
            if (timer != null) timer.timeRemaining += bonusTime;
        }

        KillZone.SetRespawn(transform.position);

        LevelHUD hud = LevelHUD.Get();
        if (hud != null) hud.ShowBanner(levelTitle, levelName, bonusTime);
    }
}
