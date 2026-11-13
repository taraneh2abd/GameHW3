using System;
using UnityEngine;

public enum ChickenState
{
    Waiting,
    Entering,
    InFormation,
    Diving
}

[RequireComponent(typeof(Chicken))]
public class ChickenMover : MonoBehaviour
{
    private Chicken chicken;
    private Vector3[] path;
    private Vector3[] returnPath;
    private int nextPoint;
    private float speed;
    private Vector3 slot;

    public event Action<ChickenMover> Settled;

    public ChickenState State { get; private set; } = ChickenState.Waiting;
    public Vector3 Slot => slot;

    private void Awake()
    {
        chicken = GetComponent<Chicken>();
    }

    public void BeginEntry(Vector3[] entryPath, Vector3 formationSlot, float entrySpeed)
    {
        path = entryPath;
        slot = formationSlot;
        speed = entrySpeed;
        nextPoint = 1;
        transform.position = path[0];
        State = ChickenState.Entering;
        chicken.PlayWalk();
    }

    public void BeginDive(Vector3[] divePath, Vector3[] comeBackPath, float diveSpeed)
    {
        path = divePath;
        returnPath = comeBackPath;
        speed = diveSpeed;
        nextPoint = 1;
        transform.position = path[0];
        State = ChickenState.Diving;
        chicken.PlayAngry();
    }

    private void Update()
    {
        if (State == ChickenState.Entering || State == ChickenState.Diving)
        {
            FollowPath();
        }
        else if (State == ChickenState.InFormation)
        {
            transform.position = slot + Vector3.right * Formation.SwayOffset;
        }
    }

    private void FollowPath()
    {
        float step = speed * Time.deltaTime;
        Vector3 position = transform.position;

        while (step > 0f && nextPoint < path.Length)
        {
            Vector3 target = path[nextPoint];
            float distance = Vector3.Distance(position, target);

            if (distance <= step)
            {
                position = target;
                step -= distance;
                nextPoint++;
            }
            else
            {
                position = Vector3.MoveTowards(position, target, step);
                step = 0f;
            }
        }

        transform.position = position;

        if (nextPoint < path.Length)
        {
            return;
        }

        if (State == ChickenState.Diving)
        {
            BeginReturn();
        }
        else
        {
            Settle();
        }
    }

    private void BeginReturn()
    {
        path = returnPath;
        nextPoint = 1;
        transform.position = path[0];
        State = ChickenState.Entering;
        chicken.PlayWalk();
    }

    private void Settle()
    {
        transform.position = slot;
        State = ChickenState.InFormation;
        chicken.PlayIdle();
        Settled?.Invoke(this);
    }
}
