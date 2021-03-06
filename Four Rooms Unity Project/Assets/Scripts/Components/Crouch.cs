using UnityEngine;

public class Crouch : MonoBehaviour
{
    public KeyCode key = KeyCode.LeftControl;

    [Header("Low Head")]
    [Tooltip("Head to lower when crouched.")]
    public Transform headToLower;
    [HideInInspector]
    public float? defaultHeadYLocalPosition;
    public float crouchYHeadPosition = 1;
    
    [Tooltip("Collider to lower when crouched.")]
    public CapsuleCollider colliderToLower;
    [HideInInspector]
    public float? defaultColliderHeight;

    public bool IsCrouched { get; private set; }
    public static bool CrouchedForSpeed = false;
    public event System.Action CrouchStart, CrouchEnd;

    void LateUpdate()
    {
        if (Input.GetKey(key))
        {
            // Enforce a low head.
            if (headToLower)
            {
                // If we don't have the defaultHeadYLocalPosition, get it now.
                if (!defaultHeadYLocalPosition.HasValue)
                {
                    defaultHeadYLocalPosition = headToLower.localPosition.y;
                }

                // Lower the head.
                headToLower.localPosition = Vector3.Lerp(headToLower.localPosition,
                    new Vector3(headToLower.localPosition.x, crouchYHeadPosition, headToLower.localPosition.z),
                    Time.deltaTime * 5);
                //headToLower.localPosition = new Vector3(headToLower.localPosition.x, crouchYHeadPosition, headToLower.localPosition.z);
            }

            // Enforce a low colliderToLower.
            if (colliderToLower)
            {
                // If we don't have the defaultColliderHeight, get it now.
                if (!defaultColliderHeight.HasValue)
                {
                    defaultColliderHeight = colliderToLower.height;
                }

                // Get lowering amount.
                float loweringAmount;
                if(defaultHeadYLocalPosition.HasValue)
                {
                    loweringAmount = defaultHeadYLocalPosition.Value - crouchYHeadPosition;
                }
                else
                {
                    loweringAmount = defaultColliderHeight.Value * .5f;
                }

                // Lower the colliderToLower.
                colliderToLower.height = Mathf.Max(defaultColliderHeight.Value - loweringAmount, 0);
                colliderToLower.center = Vector3.up * colliderToLower.height * .5f;
            }

            // Set IsCrouched state.
            if (!IsCrouched)
            {
                IsCrouched = true;
                CrouchedForSpeed = IsCrouched;
                // SetSpeedOverrideActive(true);
                CrouchStart?.Invoke();
            }
        }
        else
        {
            if (IsCrouched)
            {
                // Rise the head back up.
                if (headToLower)
                {
                    //headToLower.localPosition = new Vector3(headToLower.localPosition.x, defaultHeadYLocalPosition.Value, headToLower.localPosition.z);
                    headToLower.localPosition = Vector3.Lerp(headToLower.localPosition,
                        new Vector3(headToLower.localPosition.x, defaultHeadYLocalPosition.Value, headToLower.localPosition.z),
                        Time.deltaTime * 5);
                }

                // Reset the colliderToLower's height.
                if (colliderToLower)
                {
                    colliderToLower.height = defaultColliderHeight.Value;
                    colliderToLower.center = Vector3.up * colliderToLower.height * .5f;
                }

                // Reset IsCrouched.
                IsCrouched = false;
                CrouchedForSpeed = IsCrouched;
                // SetSpeedOverrideActive(false);
                CrouchEnd?.Invoke();
            }
        }
    }
}
