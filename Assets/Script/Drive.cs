using UnityEngine;
using UnityEngine.InputSystem;   

public class Drive : MonoBehaviour
{
    public float speed = 10f;
    public float rotationSpeed = 100f;
    public float currentSpeed = 0;

    void Update()
    {
        float vertical = 0f;
        float horizontal = 0f;

        var kb = Keyboard.current; // shorter access reference

        // Vertical (Forward/Back)  — W/S + Up/Down arrows
        if (kb.wKey.isPressed || kb.upArrowKey.isPressed) vertical += 1f;
        if (kb.sKey.isPressed || kb.downArrowKey.isPressed) vertical -= 1f;

        // Horizontal (Rotate Left/Right) — A/D + Left/Right arrows
        if (kb.aKey.isPressed || kb.leftArrowKey.isPressed) horizontal -= 1f;
        if (kb.dKey.isPressed || kb.rightArrowKey.isPressed) horizontal += 1f;

        // Apply same logic as before
        float translation = vertical * speed * Time.deltaTime;
        float rotation = horizontal * rotationSpeed * Time.deltaTime;

        transform.Translate(0, 0, translation);
        currentSpeed = translation;
        transform.Rotate(0, rotation, 0);
    }
}
