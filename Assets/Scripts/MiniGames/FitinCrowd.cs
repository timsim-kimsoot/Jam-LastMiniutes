using UnityEngine;

public class FitInCrowd : MinigameBase
{
    [SerializeField] GameObject hitboxPrefab;
    [SerializeField] GameObject playerBallPrefab;
    [SerializeField] Transform hitboxStart;
    [SerializeField] Transform playerStart;

    private Bus_Crowd hitbox;
    private Bus_Char ball;

    public override void Init(float difficulty)
    {
        base.Init(difficulty);

        hitbox = Instantiate(hitboxPrefab, hitboxStart.position, Quaternion.identity).GetComponent<Bus_Crowd>();
        ball = Instantiate(playerBallPrefab, playerStart.position, Quaternion.identity).GetComponent<Bus_Char>();

        hitbox.StartMoving();
    }

    void Update()
    {
        base.Update();

        if (running && Input.GetMouseButtonDown(0))
        {
            hitbox.StopMoving();
            ball.FallTo(hitbox.transform.position.y, () => CheckFit());
        }
    }

    void CheckFit()
    {
        float distance = Mathf.Abs(hitbox.transform.position.x - ball.transform.position.x);
        float tolerance = hitbox.Width / 2f;

        if (distance <= tolerance)
            Win();
        else
            Fail();
    }
}
