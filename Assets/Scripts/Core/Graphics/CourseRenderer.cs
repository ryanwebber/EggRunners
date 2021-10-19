using UnityEngine;
using System.Collections;

public class CourseRenderer : MonoBehaviour
{
    [System.Serializable]
    struct ClippingParemeter
    {
        [SerializeField]
        private ClippingPlane chasePlane;

        public void UpdateMaterial(Material mat)
        {
            mat.SetInt("_Clip", 1);
            mat.SetVector("_NearClippingPlane", chasePlane.PlanarData);
        }
    }

    [SerializeField]
    private Material courseMaterial;

    [Header("Material Parameters")]

    [SerializeField]
    private ClippingParemeter clippingParameter;

    private void LateUpdate()
    {
        clippingParameter.UpdateMaterial(courseMaterial);
    }
}
