using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "TrainingCourse_01", menuName = "Custom/Training/Training Course")]
public class TrainingCourse : ScriptableObject
{
    [SerializeField]
    public CourseChunk[] chunks;
}
