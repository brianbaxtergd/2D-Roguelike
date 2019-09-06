using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class MovingObject : MonoBehaviour
{
    public float moveTime = 0.1f;
    public LayerMask blockingLayer;

    private BoxCollider2D boxCollider;
    private Rigidbody2D rb2D;
    private float inverseMoveTime;


    protected virtual void Start() // Protected virtual functions can be overwritten by their inheriting classes.
    {
        boxCollider = GetComponent<BoxCollider2D>();
        rb2D = GetComponent<Rigidbody2D>();
        inverseMoveTime = 1.0f / moveTime; // By storing the inverse of move time, we can use it by multiplying instead of dividing which is more efficient.
    }

    protected bool Move(int xDir, int yDir, out RaycastHit2D hit) // out keyword causes args to be passed by reference.
    {
        Vector2 start = transform.position;
        Vector2 end = start + new Vector2(xDir, yDir);

        boxCollider.enabled = false;
        hit = Physics2D.Linecast(start, end, blockingLayer);
        boxCollider.enabled = true;

        // Check if anything was hit.
        if (hit.transform == null)
        {
            StartCoroutine(SmoothMovement(end));
            // Move was successful.
            return true;
        }

        // Move was unsuccessful.
        return false;
    }

    protected IEnumerator SmoothMovement (Vector3 end)
    {
        float sqrRemainingDistance = (transform.position - end).sqrMagnitude; // Sq mag. is computationally cheaper than mag.

        while(sqrRemainingDistance > float.Epsilon)
        {
            Vector3 newPosition = Vector3.MoveTowards(rb2D.position, end, inverseMoveTime * Time.deltaTime);
            rb2D.MovePosition(newPosition);
            // Recalculate remaining distance after we've moved.
            sqrRemainingDistance = (transform.position - end).sqrMagnitude;
            // Wait for a frame before re-evaluating the condition of our loop.
            yield return null;
        }
    }

    protected virtual void AttemptMove<T>(int xDir, int yDir)
        where T : Component
    {
        RaycastHit2D hit;
        bool canMove = Move(xDir, yDir, out hit);

        if (hit.transform == null)
            return;

        T hitComponent = hit.transform.GetComponent<T>();

        if (!canMove && hitComponent != null)
            OnCantMove(hitComponent);
    }

    protected abstract void OnCantMove <T> (T component)
        where T : Component;
}
