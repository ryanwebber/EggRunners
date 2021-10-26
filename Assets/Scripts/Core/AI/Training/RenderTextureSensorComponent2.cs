using UnityEngine;
using System.Collections;
using Unity.MLAgents.Sensors;
using System;

public class RenderTextureSensorComponent2 : SensorComponent, IDisposable
{
    RenderTextureSensor2 m_Sensor;

    /// <summary>
    /// The [RenderTexture](https://docs.unity3d.com/ScriptReference/RenderTexture.html) instance
    /// that the associated <see cref="RenderTextureSensor"/> wraps.
    /// </summary>
    [SerializeField]
    RenderTexture m_RenderTexture;

    /// <summary>
    /// Stores the [RenderTexture](https://docs.unity3d.com/ScriptReference/RenderTexture.html)
    /// associated with this sensor.
    /// </summary>
    public RenderTexture RenderTexture
    {
        get { return m_RenderTexture; }
        set { m_RenderTexture = value; }
    }

    [SerializeField]
    string m_SensorName = "RenderTextureSensor";

    /// <summary>
    /// Name of the generated <see cref="RenderTextureSensor"/>.
    /// Note that changing this at runtime does not affect how the Agent sorts the sensors.
    /// </summary>
    public string SensorName
    {
        get { return m_SensorName; }
        set { m_SensorName = value; }
    }

    [SerializeField]
    bool m_Grayscale;

    /// <summary>
    /// Whether the RenderTexture observation should be converted to grayscale or not.
    /// Note that changing this after the sensor is created has no effect.
    /// </summary>
    public bool Grayscale
    {
        get { return m_Grayscale; }
        set { m_Grayscale = value; }
    }

    [SerializeField]
    [Range(1, 50)]
    [Tooltip("Number of frames that will be stacked before being fed to the neural network.")]
    int m_ObservationStacks = 1;

    [SerializeField]
    SensorCompressionType m_Compression = SensorCompressionType.PNG;

    /// <summary>
    /// Compression type for the render texture observation.
    /// </summary>
    public SensorCompressionType CompressionType
    {
        get { return m_Compression; }
        set { m_Compression = value; UpdateSensor(); }
    }

    /// <summary>
    /// Whether to stack previous observations. Using 1 means no previous observations.
    /// Note that changing this after the sensor is created has no effect.
    /// </summary>
    public int ObservationStacks
    {
        get { return m_ObservationStacks; }
        set { m_ObservationStacks = value; }
    }

    private void OnPostRender()
    {
        Debug.Log("Will update texture...");
        Debug.Break();
        if (m_Sensor != null)
            m_Sensor.UpdateObservationAfterFrameRender();
    }

    /// <inheritdoc/>
    public override ISensor[] CreateSensors()
    {
        Dispose();
        m_Sensor = new RenderTextureSensor2(RenderTexture, Grayscale, SensorName, m_Compression);
        if (ObservationStacks != 1)
        {
            return new ISensor[] { new StackingSensor(m_Sensor, ObservationStacks) };
        }
        return new ISensor[] { m_Sensor };
    }

    /// <summary>
    /// Update fields that are safe to change on the Sensor at runtime.
    /// </summary>
    internal void UpdateSensor()
    {
        if (m_Sensor != null)
        {
            m_Sensor.CompressionType = m_Compression;
        }
    }

    /// <summary>
    /// Clean up the sensor created by CreateSensors().
    /// </summary>
    public void Dispose()
    {
        if (!ReferenceEquals(null, m_Sensor))
        {
            m_Sensor.Dispose();
            m_Sensor = null;
        }
    }
}
