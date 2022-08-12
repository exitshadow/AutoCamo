using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [SerializeField] private float speed = 5f;

    private Vector2 move;

    private void Update()
    {
        move.x = Input.GetAxis("Horizontal");
        move.y = Input.GetAxis("Vertical");

        if (move.y != 0 || move.x != 0)
        {
            transform.position += new Vector3(  move.x * Time.deltaTime * speed,
                                                0,
                                                move.y * Time.deltaTime * speed);
        }
    }
}
